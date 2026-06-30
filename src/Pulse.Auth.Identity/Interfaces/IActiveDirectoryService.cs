using Pulse.Auth.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Auth.Identity.Interfaces
{
    public interface IActiveDirectoryService
    {
        UserProfile GetUserProfile(string username);

        IDictionary<string, string> GetUserClaims(string key);
    }
}
