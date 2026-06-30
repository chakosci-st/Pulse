
using Pulse.Auth.Identity.Interfaces;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Auth.Identity.Models
{
    public class CustomClaimsIdentity : ClaimsIdentity, ICustomClaimsIdentity
    {
        public CustomClaimsIdentity(ClaimsIdentity identity) : base(identity) { }
 
        public string EmployeeId => GetClaimValue(CustomClaimTypes.EmployeeId);

        public string Email => GetClaimValue(CustomClaimTypes.Email);

        public string FirstName => GetClaimValue(CustomClaimTypes.FirstName);

        public string LastName => GetClaimValue(CustomClaimTypes.LastName);

        public string STJobFunctionDescription => GetClaimValue(CustomClaimTypes.STJobFunctionDescription);

        public string Department => GetClaimValue(CustomClaimTypes.Department);

        public string Division => GetClaimValue(CustomClaimTypes.Division);

        public string Photo => GetClaimValue(CustomClaimTypes.Photo);

        public string ManagerUsername => GetClaimValue(CustomClaimTypes.ManagerUsername);

        public string ManagerUserId => GetClaimValue(CustomClaimTypes.ManagerUserId);

        public string ManagerFirstName => GetClaimValue(CustomClaimTypes.ManagerFirstName);

        public string ManagerLastName => GetClaimValue(CustomClaimTypes.ManagerLastName);

        public string ManagerEmail => GetClaimValue(CustomClaimTypes.ManagerEmail);

        public string ADGroups => GetClaimValue(CustomClaimTypes.ADGroups);

        public string CostCenter => GetClaimValue(CustomClaimTypes.CostCenter);

        public string CostCenterDescription => GetClaimValue(CustomClaimTypes.CostCenterDescription);

        public string OICIsActive => GetClaimValue(CustomClaimTypes.OICIsActive);

        public string OICUsername => GetClaimValue(CustomClaimTypes.OICUsername);

        public string OICUserId => GetClaimValue(CustomClaimTypes.OICUserId);

        public string OICFirstName => GetClaimValue(CustomClaimTypes.OICFirstName);

        public string OICLastName => GetClaimValue(CustomClaimTypes.OICLastName);

        public string OICEmail => GetClaimValue(CustomClaimTypes.OICEmail);

        private string GetClaimValue(string claimType)
        {
            var claim = this.FindFirst(claimType);
            return claim?.Value;
        }
    }
}
