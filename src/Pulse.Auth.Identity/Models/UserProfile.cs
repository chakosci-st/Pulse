using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Auth.Identity.Models
{
  public  class UserProfile
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeId { get; set; }
        public string STEduid { get; set; }
        public string STJobFunctionDescription { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        public object Photo { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterDescription { get; set; }
        public string PhoneNo { get; set; }
        public string STRegion { get; set; }
        public string STRegionDescription { get; set; }
        public string STCountry { get; set; }
        public string STLocation { get; set; }
        public string Company { get; set; }
        public string ManagerUsername { get; set; }
        public string OICIsActive { get; set; }
        public string OICUserId { get; set; }
        public UserProfile Manager { get; set; }
        public UserProfile OIC { get; set; }
    }
}
