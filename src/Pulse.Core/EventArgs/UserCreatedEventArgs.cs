using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseEventArgs = System.EventArgs;
namespace Pulse.Core.EventArgs
{
    public class UserCreatedEventArgs : BaseEventArgs
    {
        public string UserCompleteName;
        public string Email;
        public string CreatedBy;
        public DateTime CreatedDate; 
        public UserCreatedEventArgs(string userCompleteName, string email, string createdBy, DateTime createdDate)
        {

            UserCompleteName = userCompleteName;
            Email = email;
            CreatedBy = createdBy;
            CreatedDate = createdDate;

        }
    }
}
