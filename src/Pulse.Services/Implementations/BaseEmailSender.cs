using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Pulse.Core.Interfaces;
using System.Text.RegularExpressions;
using System.IO;

namespace Pulse.Services.Implementations
{
    public class BaseEmailSender : IEmailService
    {
 
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;
        private readonly string _fromEmailName;
        public BaseEmailSender(string host, int port, string fromemail, string displayname)
        {
            _fromEmail = fromemail;
            _fromEmailName = displayname;
            _smtpClient = new SmtpClient
            {
                Host = host,
                Port = port
            };
        }


        public async Task SendEmailAsync(string to, string cc, string subject, string body, bool isHtml = true, List<string> attachments = null)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromEmailName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            foreach (var email in Regex.Split(to, @"[\s,;|\t]+"))
            {
                mailMessage.To.Add(email);
            }

            //ADD IF WITH CC
            if (!string.IsNullOrEmpty(cc))
                foreach (var email in Regex.Split(cc, @"[\s,;|\t]+"))
                {
                    mailMessage.CC.Add(email);
                }

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    if (File.Exists(attachment))
                    {
                        mailMessage.Attachments.Add(new Attachment(attachment));
                    }
                    else
                    {
                        Console.WriteLine($"Attachment not found: {attachment}");
                    }
                }
            }
            await _smtpClient.SendMailAsync(mailMessage);

        }



        public void SendEmail(string to, string cc, string subject, string body, bool isHtml = true, List<string> attachments = null)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromEmailName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            foreach (var email in Regex.Split(to, @"[\s,;|\t]+"))
            {
                mailMessage.To.Add(email);
            }

            //ADD IF WITH CC
            if (!string.IsNullOrEmpty(cc))
                foreach (var email in Regex.Split(cc, @"[\s,;|\t]+"))
                {
                    mailMessage.CC.Add(email);
                }

            if (attachments != null)
            {
                foreach (var attachment in attachments) {
                    if (File.Exists(attachment))
                    {
                        mailMessage.Attachments.Add(new Attachment(attachment));
                    }
                    else
                    {
                        Console.WriteLine($"Attachment not found: {attachment}");
                    }
                } 
            }

            _smtpClient.Send(mailMessage);

        }

    }
}
