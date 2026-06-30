/// <summary>
/// Service for sending emails with optional file attachments using SMTP.
/// </summary>
using ProjectPulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProjectPulse.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;

        /// <summary>
        /// Initializes the EmailService with SMTP configuration.
        /// </summary>
        public EmailService()
        {
            // Load SMTP settings from configuration (appsettings.json or Web.config)
            _smtpServer = System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
            _smtpPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"]);
            _smtpUser = System.Configuration.ConfigurationManager.AppSettings["SmtpUser"];
            _smtpPassword = System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"];
            _fromEmail = System.Configuration.ConfigurationManager.AppSettings["FromEmail"];
        }

        /// <summary>
        /// Sends an email using the configured SMTP server with optional file attachments.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="cc">cc email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body content.</param>
        /// <param name="isHtml">Indicates if the email body is in HTML format.</param>
        /// <param name="attachments">List of file paths to attach to the email.</param>
        public void SendEmail(string to, string cc, string subject, string body, bool isHtml = true, List<string> attachments = null)
        {
            try
            {
                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    //smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPassword);
                    //smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };

                    if (!string.IsNullOrEmpty(to))
                        mailMessage.To.Add(to);

                    if(!string.IsNullOrEmpty(cc))
                        mailMessage.CC.Add(cc);

                    // Add attachments if provided
                    if (attachments != null)
                    {
                        foreach (var filePath in attachments)
                        {
                            if (File.Exists(filePath))
                            {
                                mailMessage.Attachments.Add(new Attachment(filePath));
                            }
                            else
                            {
                                Console.WriteLine($"Attachment not found: {filePath}");
                            }
                        }
                    }

                    smtpClient.Send(mailMessage);

                    Console.WriteLine($"Email sent to {to} successfully.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (replace with a real logging framework)
                Console.WriteLine($"Error sending email to {to}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends an asynchronous email using the configured SMTP server with optional file attachments.
        /// </summary>
        /// <param name="to">Recipient email address.</param>
        /// <param name="cc">cc email address.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body content.</param>
        /// <param name="isHtml">Indicates if the email body is in HTML format.</param>
        /// <param name="attachments">List of file paths to attach to the email.</param>
        public async Task SendEmailAsync(string to, string cc, string subject, string body, bool isHtml = true, List<string> attachments = null)
        {
            try
            {
                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    //smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPassword);
                    //smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };

                    if (!string.IsNullOrEmpty(to))
                        mailMessage.To.Add(to);

                    if (!string.IsNullOrEmpty(cc))
                        mailMessage.CC.Add(cc);

                    // Add attachments if provided
                    if (attachments != null)
                    {
                        foreach (var filePath in attachments)
                        {
                            if (File.Exists(filePath))
                            {
                                mailMessage.Attachments.Add(new Attachment(filePath));
                            }
                            else
                            {
                                Console.WriteLine($"Attachment not found: {filePath}");
                            }
                        }
                    }

                    await smtpClient.SendMailAsync(mailMessage);

                    Console.WriteLine($"Email sent to {to} successfully.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (replace with a real logging framework)
                Console.WriteLine($"Error sending email to {to}: {ex.Message}");
            }
        }
    }
}
