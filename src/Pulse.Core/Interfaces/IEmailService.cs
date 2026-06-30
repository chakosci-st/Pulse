using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Interface for sending emails with optional file attachments.
/// </summary>
namespace Pulse.Core.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="cc">CC email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body content.</param>
        /// <param name="isHtml">Indicates if the email body is in HTML format.</param>
        /// <param name="attachments">List of file paths to attach to the email.</param>
        void SendEmail(string to, string cc, string subject, string body, bool isHtml = true, List<string> attachments = null);
        /// <summary>
        /// Sends an email asynchronous.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="cc">CC email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body content.</param>
        /// <param name="isHtml">Indicates if the email body is in HTML format.</param>
        Task SendEmailAsync(string to, string cc, string subject, string body, bool isHtml = true, List<string> attachments = null);
    }
}
