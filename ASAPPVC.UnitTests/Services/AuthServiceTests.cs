using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Services;
using ASAPPVC.UI.ViewModels.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ASAPPVC.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            _signInManager = new Mock<SignInManager<ApplicationUser>>(
                _userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);

            _sut = new AuthService(_userManager.Object, _signInManager.Object, _userRepo.Object);
        }

        // ---------------- Calls Sign In Manager ----------------

        [Fact]
        public async Task LoginAsync_CallsSignInManager_ReturnsResult()
        {
            // Arrange
            var vm = new LoginViewModel { Email = "a@b.com", Password = "pw", RememberMe = false };
            _signInManager.Setup(s => s.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, false))
                          .ReturnsAsync(SignInResult.Success);

            // Act
            var res = await _sut.LoginAsync(vm);

            // Assert
            res.Succeeded.Should().BeTrue();
            _signInManager.Verify(s => s.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, false), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_CreateFails_ReturnsFailureAndDoesNotAddRole()
        {
            // Arrange
            var vm = new RegisterViewModel { Email = "x@y.com", Password = "pw", Role = UI.Models.Enums.RoleType.Admin };
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), vm.Password))
                        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "err" }));

            // Act
            var res = await _sut.RegisterAsync(vm);

            // Assert
            res.Succeeded.Should().BeFalse();
            _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_CreateSucceeds_AddToRoleSucceeds_ReturnsSuccess()
        {
            // Arrange
            var vm = new RegisterViewModel { Email = "x@y.com", Password = "pw", Role = UI.Models.Enums.RoleType.Admin };
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), vm.Password))
                        .ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), vm.Role.ToString()))
                        .ReturnsAsync(IdentityResult.Success);

            // Act
            var res = await _sut.RegisterAsync(vm);

            // Assert
            res.Succeeded.Should().BeTrue();
            _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), vm.Role.ToString()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_AddRoleThrows_RollsBackAndReturnsFailedWithProfileCreationCode()
        {
            // Arrange
            var vm = new RegisterViewModel { Email = "x@y.com", Password = "pw", Role = UI.Models.Enums.RoleType.Admin };
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), vm.Password))
                        .ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), vm.Role.ToString()))
                        .ThrowsAsync(new System.Exception("boom"));
            _userManager.Setup(u => u.DeleteAsync(It.IsAny<ApplicationUser>()))
                        .ReturnsAsync(IdentityResult.Success);

            // Act
            var res = await _sut.RegisterAsync(vm);

            // Assert
            res.Succeeded.Should().BeFalse();
            res.Errors.Should().Contain(e => e.Code == "ProfileCreationFailed");
            _userManager.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }
    }
}