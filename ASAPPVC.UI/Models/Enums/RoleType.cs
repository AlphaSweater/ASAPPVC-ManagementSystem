using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.Enums
{
    /// <summary>
    /// Application role types.
    /// <br/>
    /// <br/><b>Examples:</b>
    /// <code>
    /// var role = RoleType.WarehouseManager;
    /// role.ToRoleName();    // "WarehouseManager"
    /// role.ToDisplayName(); // "Warehouse Manager"
    /// </code>
    /// </summary>
    public enum RoleType
    {
        [Display(Name = "Admin")]
        Admin,

        [Display(Name = "Warehouse Manager")]
        WarehouseManager,

        // Placeholder default value used in forms when no role selected
        [Display(Name = "Unassigned")]
        Unassigned
    }

    public static class RoleTypeExtensions
    {
        /// <summary>
        /// Returns the role name used with ASP.NET Identity (by default the enum name).
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// RoleType.Admin.ToRoleName();             // "Admin"
        /// RoleType.WarehouseManager.ToRoleName();  // "WarehouseManager"
        /// </code>
        /// </summary>
        public static string ToRoleName(this RoleType role)
        {
            return role.ToString();
        }

        /// <summary>
        /// Returns a friendly display name for the role. If a <see cref="DisplayAttribute"/>
        /// is applied to the enum value that name will be returned; otherwise the enum name.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// RoleType.Admin.ToDisplayName();             // "Admin"
        /// RoleType.WarehouseManager.ToDisplayName();  // "Warehouse Manager"
        /// RoleType.Unassigned.ToDisplayName();        // "Unassigned"
        /// </code>
        /// </summary>
        public static string ToDisplayName(this RoleType role)
        {
            var member = typeof(RoleType).GetMember(role.ToString()).FirstOrDefault();
            var attr = member?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            return attr?.Name ?? role.ToString();
        }
    }
}