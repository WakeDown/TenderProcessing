using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Net.Mail;
using SpeCalcDataAccessLayer.Models;

namespace SpeCalc.Helpers
{
    public static class Notification
    {
        //отправка почтового сообщения
        public static void SendNotification(IEnumerable<UserBase> users, string message, string header)
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
                        //var from = ConfigurationManager.AppSettings["MailFrom"];
                        mail.From = new MailAddress(login);
                        var fakeMail = ConfigurationManager.AppSettings["FakeMailTo"].Trim();
                        if (string.IsNullOrEmpty(fakeMail))
                        {
                            foreach (var user in userList)
                            {
                                mail.To.Add(new MailAddress(user.Email));
                            }
                        }
                        else
                        {
                            mail.To.Add(new MailAddress(fakeMail));
                            foreach (var user in userList)
                            {
                                message += "<br/>" + user.Email;
                            }
                        }
                        mail.Subject = header;
                        mail.Body = message;
                        mail.IsBodyHtml = true;
                        var client = new SmtpClient();
                        client.Host = host;
                        client.Port = int.Parse(port);
                        client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
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

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the message we sent
            MailMessage msg = (MailMessage)e.UserState;

            if (e.Cancelled)
            {
                // prompt user with "send cancelled" message 
            }
            if (e.Error != null)
            {
                // prompt user with error message 
            }
            else
            {
                // prompt user with message sent!
                // as we have the message object we can also display who the message
                // was sent to etc 
            }

            // finally dispose of the message
            if (msg != null)
                msg.Dispose();
        }
    }
}