using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core.Interfaces
{
    public interface IEmailSender : IEmailService
    {
        Task SendProjectCreatedNotificationAsync(string projectNo, string projectname, string productcode,
            string plant, string category, string createdBy, DateTime datecreated,
            IList<string> recipientEmail, IList<string> ccEmail);
        Task SendProjectCreatedAndStartedNotificationAsync(string projectNo, string projectname, string productcode,
            string plant, string category, string createdBy, string milestone, DateTime datecreated,
            IList<string> recipientEmail, IList<string> ccEmail);
        Task SendStatusChangeOnProjectNotificationAsync(string projectNo, string projectname, string productcode, string plant, string category, string fromstatus, string tostatus, string reason, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail);
        Task SendStatusChangeOnMilestoneNotificationAsync(string projectNo, string projectname, string productcode, string plant, string category, string milestone, string fromstatus, string tostatus, string reason, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail);
        Task SendProjectDetailsUpdatedNotificationAsync(string projectNo, string projectname, string createdBy, DateTime datecreated, string urlpath, IList<string> recipientEmail, IList<string> ccEmail);

        Task SendTaskCreatedNotificationAsync(string projectNo, string projectname, string productcode, string plant, string category, string milestone, string task, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail);
        //Task SendTaskUpdatedNotificationAsync(Entities.Task previewDetails, Entities.Task currentDetails, string updatedBy, DateTime updatedDate, IList<string> recipientEmail, IList<string> ccEmail);
        Task SendStatusChangeOnTaskNotificationAsync(string projectNo, string projectname, string productcode, string plant, string category, string milestone, string task, string fromstatus, string tostatus, string reason, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail);

        // Plant
        Task SendPlantMemberRegisteredNotificationAsync(string plantcode, string plantname, string membercompletename, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail);
    }
}
