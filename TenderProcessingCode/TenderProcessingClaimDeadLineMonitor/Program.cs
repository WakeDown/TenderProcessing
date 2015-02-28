using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using TenderProcessingDataAccessLayer;
using TenderProcessingDataAccessLayer.Enums;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessingClaimDeadLineMonitor
{
    class Program
    {
        private const string ApproachingTenderClaimsFileName = "ApproachingTenderClaims.dt";
        private const string OverdieTenderClaimsFileName = "OverdieTenderClaims.dt";
        //Консольное приложение для отправки уведомлений по просроченным заявкам или у которых срок сдачи меньше чем 24 часа
        static void Main(string[] args)
        {
            Console.WriteLine("Обработка...");
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["TenderProccessing"].ConnectionString;
                var db = new DbEngine();
                using (var conn = new SqlConnection(connectionString))
                {
                    var approachingClaimsList = LoadProcessedApproachingClaimsId();
                    //Обращение к БД за инфой о заявкам до срока сдачи которых осталось 24 часа и меньше
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "LoadApproachingTenderClaim";
                    conn.Open();
                    var claimsId = new List<int>();
                    var rd = cmd.ExecuteReader();
                    if (rd.HasRows)
                    {
                        while (rd.Read())
                        {
                            claimsId.Add(rd.GetInt32(0));
                        }
                    }
                    rd.Dispose();
                    if (claimsId.Any())
                    {
                        //Получение снабженцев по заявке, у которых есть не подтвержденные позиции
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "GetProductsForClaim";
                        foreach (var id in claimsId)
                        {
                            if (approachingClaimsList.Contains(id)) continue;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@id", id);
                            var products = new List<string>();
                            rd = cmd.ExecuteReader();
                            if (rd.HasRows)
                            {
                                while (rd.Read())
                                {
                                    products.Add(rd.GetString(0));
                                }
                            }
                            rd.Dispose();
                            if (products.Any())
                            {
                                approachingClaimsList.Add(id);
                                //Отправка писем полученым снабженцам
                                var claim = db.LoadTenderClaimById(id);
                                var users = new List<UserBase>();
                                foreach (var product in products)
                                {
                                    users.Add(GetUserById(product));
                                }
                                
                                var host = ConfigurationManager.AppSettings["AppHost"];
                                var messageMail = new StringBuilder();
                                messageMail.Append("Здравствуйте");
                                messageMail.Append(".<br/>");
                                messageMail.Append("Приближается срок сдачи по Заявке №" + claim.Id + ", у которой есть не подтверженные Вами позиции расчета<br/>");
                                messageMail.Append(GetClaimInfo(claim));
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Calc/Index?claimId=" + claim.Id + "</a>");
                                messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                SendNotification(users, messageMail.ToString(),
                                    "Приближение срока сдачи расчета в системе СпецРасчет");
                            }
                        }
                        SaveProcessedApproachingClaimsId(approachingClaimsList);
                    }
                    //Обращение к БД за инфой о просроченных заявках
                    var overdieClaimsList = LoadProcessedOverdieClaimsId();
                    var controllers = GetControllers();
                    var expiredNoteUsers = GetExpiredNoteUsers();
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "LoadOverdieTenderClaim";
                    claimsId.Clear();
                    rd = cmd.ExecuteReader();
                    if (rd.HasRows)
                    {
                        while (rd.Read())
                        {
                            claimsId.Add(rd.GetInt32(0));
                        }
                    }
                    rd.Dispose();
                    if (claimsId.Any())
                    {
                        //Получение снабженцев по заявке, у которых есть не подтвержденные позиции
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "GetProductsForClaim";
                        foreach (var id in claimsId)
                        {
                            if (overdieClaimsList.Contains(id)) continue;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@id", id);
                            var products = new List<string>();
                            rd = cmd.ExecuteReader();
                            if (rd.HasRows)
                            {
                                while (rd.Read())
                                {
                                    products.Add(rd.GetString(0));
                                }
                            }
                            rd.Dispose();
                            if (products.Any())
                            {
                                overdieClaimsList.Add(id);
                                //Отправка писем полученным снабженцам
                                var claim = db.LoadTenderClaimById(id);
                                var users = new List<UserBase>();
                                foreach (var product in products)
                                {
                                    users.Add(GetUserById(product));
                                }
                                var host = ConfigurationManager.AppSettings["AppHost"];
                                var messageMail = new StringBuilder();
                                messageMail.Append("Здравствуйте");
                                messageMail.Append(".<br/>");
                                messageMail.Append("Cрок сдачи по Заявке №" + claim.Id + " истек, у Вас есть не подтверженные Вами позиции расчета в этой заявке<br/>");
                                messageMail.Append(GetClaimInfo(claim));
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Calc/Index?claimId=" + claim.Id + "</a>");
                                messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                SendNotification(users, messageMail.ToString(),
                                    "Срока сдачи расчета по заявке истек. Система СпецРасчет");
                                var userList = new List<UserBase>();
                                userList.AddRange(controllers);
                                userList.AddRange(expiredNoteUsers);
                                //Отправка писем контроллерам
                                messageMail = new StringBuilder();
                                messageMail.Append("Здравствуйте");
                                messageMail.Append(".<br/>");
                                messageMail.Append("Cрок сдачи по Заявке №" + claim.Id + " истек, у этой заявки есть неподтвержденные позиции расчета<br/>");
                                messageMail.Append(GetClaimInfo(claim));
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Calc/Index?claimId=" + claim.Id + "</a>");
                                messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                SendNotification(userList, messageMail.ToString(),
                                    "Срока сдачи расчета по заявке истек. Система СпецРасчет");
                            }
                        }
                        SaveProcessedOverdieClaimsId(overdieClaimsList);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка");
            }
            Console.WriteLine("Обработка завершена...");
        }

        //Получени инфы о юзере из ActiveDirectory
        private static UserBase GetUserById(string id)
        {
            UserBase user = null;
            var domain = new PrincipalContext(ContextType.Domain);
            var userPrincipal = UserPrincipal.FindByIdentity(domain, IdentityType.Sid, id);
            if (userPrincipal != null)
            {
                var email = userPrincipal.EmailAddress;
                var name = userPrincipal.DisplayName;
                var sid = userPrincipal.Sid.Value;
                var shortName = GetShortName(name);
                user = new UserBase()
                {
                    Id = sid,
                    Name = name,
                    ShortName = shortName,
                    Email = email,
                    Roles = new List<Role>() { Role.Enter }
                };
            }
            return user;
        }

        private static string GetShortName(string name)
        {
            var shortName = new StringBuilder();
            var partNames = name.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (partNames.Count() > 2)
            {
                shortName.Append(partNames[0]);
                shortName.Append(" ");
                shortName.Append(partNames[1].Substring(0, 1));
                shortName.Append(".");
                shortName.Append(partNames[2].Substring(0, 1));
                shortName.Append(".");
            }
            return shortName.ToString();
        }

        //Отправка почты
        private static void SendNotification(IEnumerable<UserBase> users, string message, string header)
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

        //Получение контроллеров из ActiveDirectory
        private static List<UserBase> GetControllers()
        {
            var db = new DbEngine();
            var roles = db.LoadRoles();
            var list = new List<UserBase>();
            var domain = new PrincipalContext(ContextType.Domain);
            var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Name, roles.First(x=>x.Role == Role.Controller).Name);
            if (group != null)
            {
                var members = group.GetMembers(true);
                foreach (var principal in members)
                {
                    var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.Name);
                    if (userPrincipal != null)
                    {
                        var email = userPrincipal.EmailAddress;
                        var name = userPrincipal.DisplayName;
                        var sid = userPrincipal.Sid.Value;
                        var shortName = GetShortName(name);
                        var user = new UserBase()
                        {
                            Id = sid,
                            Name = name,
                            ShortName = shortName,
                            Email = email
                        };
                        list.Add(user);
                    }
                }
            }
            return list;
        }

        //Получение получателей извещений о просроченных заявках из ActiveDirectory
        private static List<UserBase> GetExpiredNoteUsers()
        {
            var db = new DbEngine();
            var roles = db.LoadRoles();
            var list = new List<UserBase>();
            var domain = new PrincipalContext(ContextType.Domain);
            var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Name, roles.First(x => x.Role == Role.ExpiredNote).Name);
            if (group != null)
            {
                var members = group.GetMembers(true);
                foreach (var principal in members)
                {
                    var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.Name);
                    if (userPrincipal != null)
                    {
                        var email = userPrincipal.EmailAddress;
                        var name = userPrincipal.DisplayName;
                        var sid = userPrincipal.Sid.Value;
                        var shortName = GetShortName(name);
                        var user = new UserBase()
                        {
                            Id = sid,
                            Name = name,
                            Email = email,
                            ShortName = shortName,
                            Roles = new List<Role>() { Role.ExpiredNote }
                        };
                        list.Add(user);
                    }
                }
            }
            return list;
        }

        private static string GetClaimInfo(TenderClaim claim)
        {
            var db = new DbEngine();
            var dealTypes = db.LoadDealTypes();
            return "Заявка № " + claim.Id + ", Автор: " + GetUserById(claim.Author.Id).ShortName +
                   ", Номер конкурса: " + claim.TenderNumber + ", Дата начала" +
                   claim.TenderStart.ToString("dd.MM.yyyy") + ", Срок сдачи: "
                   + claim.ClaimDeadline.ToString("dd.MM.yyyy") + ", Менеджер: " +
                   GetUserById(claim.Manager.Id).ShortName + ", Подразделение менеджера: " +
                   claim.Manager.SubDivision +
                   ", Заказчик: " + claim.Customer + " ИНН заказчика: " + claim.CustomerInn +
                   ", Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value + (claim.Sum > 0
                       ? ", Сумма: " + claim.Sum.ToString("N2")
                       : string.Empty) + (!string.IsNullOrEmpty(claim.TenderUrl)
                           ? ", Сcылка на конкурс: <a href='" + claim.TenderUrl + "'>[Ссылка]</a>]"
                           : string.Empty) + ".<br/>";
        }

        private static List<int> LoadProcessedApproachingClaimsId()
        {
            var list = new List<int>();
            var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (assemblyLocation != null)
            {
                var filePath = Path.Combine(assemblyLocation, ApproachingTenderClaimsFileName);
                if (File.Exists(filePath))
                {
                    var formatter = new BinaryFormatter();
                    using (var stream = File.OpenRead(filePath))
                    {
                        list = (List<int>)formatter.Deserialize(stream);
                    }
                }
            }
            return list;
        }

        private static void SaveProcessedApproachingClaimsId(List<int> list)
        {
            var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (assemblyLocation != null)
            {
                var filePath = Path.Combine(assemblyLocation, ApproachingTenderClaimsFileName);
                var fileMode = FileMode.Truncate;
                if (!File.Exists(filePath))
                {
                    fileMode = FileMode.CreateNew;
                }
                var formatter = new BinaryFormatter();
                using (var stream = File.Open(filePath, fileMode, FileAccess.Write))
                {
                    formatter.Serialize(stream, list);
                    stream.Flush();
                }
            }
        }

        private static List<int> LoadProcessedOverdieClaimsId()
        {
            var list = new List<int>();
            var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (assemblyLocation != null)
            {
                var filePath = Path.Combine(assemblyLocation, OverdieTenderClaimsFileName);
                if (File.Exists(filePath))
                {
                    var formatter = new BinaryFormatter();
                    using (var stream = File.OpenRead(filePath))
                    {
                        list = (List<int>)formatter.Deserialize(stream);
                    }
                }
            }
            return list;
        }

        private static void SaveProcessedOverdieClaimsId(List<int> list)
        {
            var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (assemblyLocation != null)
            {
                var filePath = Path.Combine(assemblyLocation, OverdieTenderClaimsFileName);
                var fileMode = FileMode.Truncate;
                if (!File.Exists(filePath))
                {
                    fileMode = FileMode.CreateNew;
                }
                var formatter = new BinaryFormatter();
                using (var stream = File.Open(filePath, fileMode, FileAccess.Write))
                {
                    formatter.Serialize(stream, list);
                    stream.Flush();
                }
            }
        }
    }
}
