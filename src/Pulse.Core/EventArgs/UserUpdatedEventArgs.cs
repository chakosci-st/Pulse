using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseEventArgs = System.EventArgs;
namespace Pulse.Core.EventArgs
{
    public class UserUpdatedEventArgs : BaseEventArgs
    {
        public string UserCompleteName;
        public string Email;
        public string ModifiedBy;
        public DateTime ModifiedDate; 
        public UserUpdatedEventArgs(string userCompleteName, string email, string modifiedBy, DateTime modifiedDate)
        {

            UserCompleteName = userCompleteName;
            Email = email;
            ModifiedBy = modifiedBy;
            ModifiedDate = modifiedDate;

        }
    }
}
