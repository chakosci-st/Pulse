using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using Pulse.Core.Enums;

namespace Pulse.Core.Interfaces
{


    public interface IActiveDirectoryService
    {

        ICollection<ActiveDirectoryUser> SearchUsers(string searchTerm);
        ICollection<ActiveDirectoryUser> SearchUsersFull(string searchTerm);
        ActiveDirectoryUser FindUser(string key);
        ActiveDirectoryUser FindUser(string key, ActiveDirectoryKeyType type);
        Task<ActiveDirectoryUser> FindUserAsync(string searchTerm);
        List<UserPrincipal> SearchUsers(string username = "", string employeeId = "");

        ActiveDirectoryGroup SearchActiveDirectoryGroup(string adgroup);
        ActiveDirectoryUser FindUserByEDUID(string key);

        void UpdateUserProfilePicture(byte[] imageBytes);

        string SearchActiveDirectoryGroupsPerUserName(string username);


        byte[] GetProfilePhoto(string username);

        object GetAttributePerUser(string key, LDAPParameters ldapparameter);




    }
}
