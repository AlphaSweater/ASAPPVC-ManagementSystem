using ASAPPVC.UI.Services;
using FluentAssertions;

namespace ASAPPVC.UnitTests.Services
{
    public class UserServiceTests
    {
        // ---------------- UserService: Implements IUserService ----------------
        [Fact]
        public void UserService_Implements_IUserService()
        {
            var svc = new UserService();
            svc.Should().BeAssignableTo<IUserService>();
        }
    }
}