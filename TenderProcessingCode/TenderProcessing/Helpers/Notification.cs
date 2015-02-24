using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Net.Mail;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Helpers
{
    public static class Notification
    {
        public static void SendNotification(IEnumerable<UserBase> users, string message, string header)
        {
            try
            {
                var userList = users.ToList();
                if (userList.Any() && !string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(header))
                {
                    Task.Run(() =>
                    {
                        using (var mail = new MailMessage())
                        {
                            var host = ConfigurationManager.AppSettings["SmtpHost"];
                            var port = ConfigurationManager.AppSettings["SmtpPort"];
                            var ssl = ConfigurationManager.AppSettings["SmtpSSL"];
                            var login = ConfigurationManager.AppSettings["SmtpLogin"];
                            var password = ConfigurationManager.AppSettings["SmtpPassword"];
                            var from = ConfigurationManager.AppSettings["MailFrom"];
                            mail.From = new MailAddress(from);
                            //foreach (var user in userList)
                            //{
                            //    mail.To.Add(new MailAddress(user.Email));   
                            //}
                            mail.To.Add(new MailAddress("Anton.Rehov@unitgroup.ru"));
                            foreach (var user in userList)
                            {
                                message += "<br/>" + user.Email;
                            }
                            mail.Subject = header;
                            mail.Body = message;
                            mail.IsBodyHtml = true;
                            var client = new SmtpClient();
                            client.Host = host;
                            client.Port = int.Parse(port);
                            client.EnableSsl = ssl.ToLower() == "true";
                            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                            {
                                client.Credentials = new NetworkCredential(login, password);
                            }
                            client.DeliveryMethod = SmtpDeliveryMethod.Network;
                            client.Send(mail);
                        }
                    });
                    
                }
            }
            catch (Exception)
            {
            }
        }
    }
}