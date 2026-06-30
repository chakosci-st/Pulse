using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
    public class SmtpEmailSenderService : BaseEmailSender, IEmailSender
    {
        public SmtpEmailSenderService(string host, int port, string fromemail, string displayname) : base(host, port, fromemail, displayname)
        {
        }

        public async Task SendProjectCreatedNotificationAsync(string projectNo, string projectname, string productcode,
            string plant, string category, string createdBy, DateTime datecreated,
            IList<string> recipientEmail, IList<string> ccEmail)
        {
            //get all members to be notified
            //email body
            var body = $@"
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9f9f9; padding: 20px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <!-- Header -->
          <tr>
            <td style='padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
              <h1 style='margin: 0; font-size: 24px;'>Notification from PULSE</h1>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;'>
              <p>Dear User,</p>
              <p>A new project was initialized by <b>{createdBy}</b> Please find the details below:</p>
              <ul>
                <li><strong>Project No:</strong> {projectNo}</li>
                <li><strong>Project Name:</strong> {projectname}</li>
                <li><strong>Product Code(s):</strong> {productcode}</li>
                <li><strong>Plant:</strong> {plant}</li>
                <li><strong>Category:</strong> {category}</li>
                <li><strong>Date Created:</strong> {datecreated.ToString()}</li>
              </ul>
              <p>Please visit the Pulse to view and update the task(s) that are assigned to you.</p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;'>
              <p style='margin: 0;'>This is a system-generated email. Please do not reply to this message.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>

";
            await this.SendEmailAsync(string.Join(";", recipientEmail), string.Join(";", ccEmail), $"PULSE: New Project Created - {projectNo}", body, true);
        }

        public async Task SendProjectCreatedAndStartedNotificationAsync(string projectNo, string projectname, string productcode,
            string plant, string category, string createdBy, string milestone, DateTime datecreated,
            IList<string> recipientEmail, IList<string> ccEmail)
        {
            //get all members to be notified
            //email body
            var body = $@"
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9f9f9; padding: 20px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <!-- Header -->
          <tr>
            <td style='padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
              <h1 style='margin: 0; font-size: 24px;'>Notification from PULSE</h1>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;'>
              <p>Dear User,</p>
              <p>A new project was initialized and started by <b>{createdBy}</b> Please find the details below:</p>
              <ul>
                <li><strong>Project No:</strong> {projectNo}</li>
                <li><strong>Project Name:</strong> {projectname}</li>
                <li><strong>Product Code(s):</strong> {productcode}</li>
                <li><strong>Plant:</strong> {plant}</li>
                <li><strong>Category:</strong> {category}</li>
                <li><strong>Milestone:</strong> {milestone}</li>
                <li><strong>Date Created:</strong> {datecreated.ToString()}</li>
              </ul>
              <p>Please visit the Pulse to view and update the task(s) that are assigned to you.</p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;'>
              <p style='margin: 0;'>This is a system-generated email. Please do not reply to this message.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>

";
            await this.SendEmailAsync(string.Join(";", recipientEmail), string.Join(";", ccEmail), $"PULSE: New Project Created - {projectNo}", body, true);
        }


        public async Task SendProjectDetailsUpdatedNotificationAsync(string projectNo, string projectname, string createdBy, DateTime datecreated, string urlpath, IList<string> recipientEmail, IList<string> ccEmail)
        {
            //get all members to be notified
            //email body
            var body = $@"
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9f9f9; padding: 20px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <!-- Header -->
          <tr>
            <td style='padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
              <h1 style='margin: 0; font-size: 24px;'>Notification from PULSE</h1>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;'>
              <p>Dear User,</p>
              <p>Project details was recently updated by <b>{createdBy}</b> Please find the details below:</p>
              <ul>
                <li><strong>Project No:</strong> {projectNo}</li>
                <li><strong>Project Name:</strong> {projectname}</li> 
                <li><strong>Date Created:</strong> {datecreated.ToString()}</li>
              </ul>
              <p>Please visit <a target='_blank' href='{urlpath}/projects/{projectNo}'>Pulse</a> to view and updates.</p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;'>
              <p style='margin: 0;'>This is a system-generated email. Please do not reply to this message.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
";
            await this.SendEmailAsync(string.Join(";", recipientEmail), string.Join(";", ccEmail), $"PULSE: Project Details Updated - {projectNo}", body, true);
        }


        public async Task SendStatusChangeOnProjectNotificationAsync(string projectNo, string projectname, string productcode, string plant, string category, string fromstatus, string tostatus, string reason, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail)
        {
            //get all members to be notified
            //email body
            var body = $@"
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9f9f9; padding: 20px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <!-- Header -->
          <tr>
            <td style='padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
              <h1 style='margin: 0; font-size: 24px;'>Notification from P.U.L.S.E.</h1>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;'>
              <p>Dear User,</p>
              <p>The status of <b>{projectname}</b> was changed by <b>{createdBy}</b>, from <b>{fromstatus}</b> to <b>{tostatus}</b>. Please find the details below:</p>
              <ul>
                <li><strong>Project No:</strong> {projectNo}</li>
                <li><strong>Project Name:</strong> {projectname}</li>
                <li><strong>Product Code(s):</strong> {productcode}</li>
                <li><strong>Plant:</strong> {plant}</li>
                <li><strong>Category:</strong> {category}</li> 
                <li><strong>New Status:</strong> {tostatus}</li>
                <li><strong>Date Changed:</strong> {datecreated.ToString()}</li>
                <li><strong>Reason:</strong> {reason}</li>
                <li><strong>Previous Status:</strong> {fromstatus}</li>
              </ul>
              <p>Please visit the Pulse to view the complete details.</p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;'>
              <p style='margin: 0;'>This is a system-generated email. Please do not reply to this message.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>

";
            await this.SendEmailAsync(string.Join(";", recipientEmail), string.Join(";", ccEmail), $"PULSE: Status Change - {projectNo}", body, true);
        }

        public async Task SendStatusChangeOnMilestoneNotificationAsync(string projectNo, string projectname, string productcode, string plant, string category, string milestone, string fromstatus, string tostatus, string reason, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail)
        {
            var body = $@"
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9f9f9; padding: 20px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <tr>
            <td style='padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
              <h1 style='margin: 0; font-size: 24px;'>Notification from P.U.L.S.E.</h1>
            </td>
          </tr>
          <tr>
            <td style='padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;'>
              <p>Dear User,</p>
              <p>The status of Milestone: <b>{milestone}</b> was changed by <b>{createdBy}</b>, from <b>{fromstatus}</b> to <b>{tostatus}</b>. Please find the details below:</p>
              <ul>
                <li><strong>Milestone:</strong> {milestone}</li>
                <li><strong>Project No:</strong> {projectNo}</li>
                <li><strong>Project Name:</strong> {projectname}</li>
                <li><strong>Product Code(s):</strong> {productcode}</li>
                <li><strong>Plant:</strong> {plant}</li>
                <li><strong>Category:</strong> {category}</li>
                <li><strong>New Status:</strong> {tostatus}</li>
                <li><strong>Date Changed:</strong> {datecreated}</li>
                <li><strong>Reason:</strong> {reason}</li>
                <li><strong>Previous Status:</strong> {fromstatus}</li>
              </ul>
              <p>Please visit the Pulse to view the complete details.</p>
            </td>
          </tr>
          <tr>
            <td style='padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;'>
              <p style='margin: 0;'>This is a system-generated email. Please do not reply to this message.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>

";
            await this.SendEmailAsync(string.Join(";", recipientEmail), string.Join(";", ccEmail), $"PULSE: Milestone Status Change - {projectNo}", body, true);
        }





        public async Task SendStatusChangeOnTaskNotificationAsync(string projectNo, string projectname, string productcode, string plant, string category, string milestone, string task, string fromstatus, string tostatus, string reason, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail)
        {
            //get all members to be notified
            //email body
            var body = $@"
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9f9f9; padding: 20px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <!-- Header -->
          <tr>
            <td style='padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
              <h1 style='margin: 0; font-size: 24px;'>Notification from P.U.L.S.E.</h1>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;'>
              <p>Dear User,</p>
              <p>The status of Task: <b>{task}</b> was changed by <b>{createdBy}</b>, from <b>{fromstatus}</b> to <b>{tostatus}</b>. Please find the details below:</p>
              <ul>
                <li><strong>Task:</strong> {task}</li>
                <li><strong>Project No:</strong> {projectNo}</li>
                <li><strong>Project Name:</strong> {projectname}</li>
                <li><strong>Product Code(s):</strong> {productcode}</li>
                <li><strong>Plant:</strong> {plant}</li>
                <li><strong>Category:</strong> {category}</li>
                <li><strong>Milestone:</strong> {milestone}</li>
                <li><strong>New Status:</strong> {tostatus}</li>
                <li><strong>Date Changed:</strong> {datecreated.ToString()}</li>
                <li><strong>Reason:</strong> {reason}</li>
                <li><strong>Previous Status:</strong> {fromstatus}</li>
              </ul>
              <p>Please visit the Pulse to view the complete details.</p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;'>
              <p style='margin: 0;'>This is a system-generated email. Please do not reply to this message.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>

";
            await this.SendEmailAsync(string.Join(";", recipientEmail), string.Join(";", ccEmail), $"PULSE: Status Change - {projectNo}", body, true);
        }

        public async Task SendTaskCreatedNotificationAsync(string projectNo, string projectname, string productcode, string plant, string category, string milestone, string task, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail)
        {
            //get all members to be notified
            //email body
            var body = $@"
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9f9f9; padding: 20px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <!-- Header -->
          <tr>
            <td style='padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
              <h1 style='margin: 0; font-size: 24px;'>Notification from P.U.L.S.E.</h1>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;'>
              <p>Dear User,</p>
              <p>New Task: <b>{task}</b> was created by <b>{createdBy}</b>. Please find the details below:</p>
              <ul>
                <li><strong>Task:</strong> {task}</li>
                <li><strong>Project No:</strong> {projectNo}</li>
                <li><strong>Project Name:</strong> {projectname}</li>
                <li><strong>Product Code(s):</strong> {productcode}</li>
                <li><strong>Plant:</strong> {plant}</li>
                <li><strong>Category:</strong> {category}</li>
                <li><strong>Milestone:</strong> {milestone}</li>
                <li><strong>Date Created:</strong> {datecreated.ToString()}</li> 
              </ul>
              <p>Please visit the Pulse to view the complete details.</p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;'>
              <p style='margin: 0;'>This is a system-generated email. Please do not reply to this message.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>

";
            await this.SendEmailAsync(string.Join(";", recipientEmail), string.Join(";", ccEmail), $"PULSE: Task Created - {projectNo}", body, true);
        }

        public async Task SendPlantMemberRegisteredNotificationAsync(string plantcode, string plantname, string membercompletename, string createdBy, DateTime datecreated, IList<string> recipientEmail, IList<string> ccEmail)
        {
            //get all members to be notified
            //email body
            var body = $@"
 <!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <title>Registration Successful</title>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f9f9f9; padding: 20px 0;'>
    <tr>
      <td align='center'>
        <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <!-- Header -->
          <tr>
            <td style='padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;'>
              <h1 style='margin: 0; font-size: 24px;'>Notification from P.U.L.S.E.</h1>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style='padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;'>
              <p>Dear {membercompletename},</p>
              <p>
                You have been registered to <span style='color: #0078D7; font-weight: bold;'>PULSE - {plantcode}:{plantname}</span>
                by <span style='color: #0078D7; font-weight: bold;'>{createdBy}</span>.
              </p>
              <p style='font-family: Arial, sans-serif; color: #555; font-size: 15px; margin: 0 0 24px 0;'>
                You can now log in and start using our services.
              </p>
            </td>
          </tr>
          <tr>
            <td style='padding: 0px 0px 24px 0px; text-align: center;'>
              <a href='https://www.pulse.cal.st.com/auth/login' 
                 style='display: inline-block; padding: 12px 28px; background: #0078D7; color: #fff; text-decoration: none; border-radius: 4px; font-family: Arial, sans-serif; font-size: 16px;'>
                Go to Login
              </a>
            </td>
          </tr>		  
          <tr>
            <td style='padding: 24px 24px 24px 24px; text-align: center;'>
              <p style='font-family: Arial, sans-serif; color: #aaa; font-size: 13px; margin: 0;'>
                If you did not expect this registration, please contact our support team.
              </p>
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td style='padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;'>
              <p style='margin: 0;'>This is a system-generated email. Please do not reply to this message.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
";
            await this.SendEmailAsync(string.Join(";", recipientEmail), string.Join(";", ccEmail), $"PULSE: Registration Successful", body, true);
        }
    }
}
