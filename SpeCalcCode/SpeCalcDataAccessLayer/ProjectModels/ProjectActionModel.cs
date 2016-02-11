using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ServiceClaim.Helpers;
using SpeCalcDataAccessLayer.Models;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectActionModel
    {
        public static IEnumerable<ProjectActions> GetActive(int projectId)
        {
            using (var db = new SpeCalcEntities())
            {
                //var db = new SpeCalcEntities();
                var list = db.ProjectActions.Where(x => x.ProjectId == projectId && !x.Done).OrderByDescending(x=>x.Id).ToList();
                return list;
            }
        }

        public static IEnumerable<ProjectActions> GetList(out int totalCount, int projectId, bool full = true)
        {
            using (var db = new SpeCalcEntities())
            {
                //var db = new SpeCalcEntities();
                var query = db.ProjectActions.Where(x => x.ProjectId == projectId);
                var page = query.OrderByDescending(p => p.Id).Take(1);
                if (full)
                {
                    page = query.OrderByDescending(p => p.Id);
                }
                else
                {
                    page = query.OrderBy(p => p.Done).ThenByDescending(p=>p.Id).Take(1);
                }
                totalCount = query.Count();
                var list = page.Select(p => p).ToList();
                return list;
            }
        }

        public static void SetDone(int aid, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var action = db.ProjectActions.Single(x => x.Id==aid);
                action.Done = true;
                action.DoneDate = DateTime.Now;
                action.DoneSetterSid = user.Sid;
                action.DoneSetterName = user.DisplayName;
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(action.ProjectId, "Выполнение действия", $"Дата действия: {action.NoticeDate:dd.MM.YYYY}\rОтветственный: {action.ResponsibleName}\rОписание: {action.Descr}",
                        new[] { action }, user);
            }
        }

        public static void Delete(int aid, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var action = db.ProjectActions.Single(x => x.Id == aid);
                if (!action.Done)
                {
                    action.DeleteDate = DateTime.Now;
                    action.DeleterSid = user.Sid;
                    action.DeleterName = user.DisplayName;
                    db.SaveChanges();
                    ProjectHistoryModel.CreateHistoryItem(action.ProjectId, "Удаление действия",
                        $"Дата действия: {action.NoticeDate:dd.MM.YYYY}\rОтветственный: {action.ResponsibleName}\rОписание: {action.Descr}",
                        new[] {action}, user);
                }
            }
        }

        public static void Create(int projectId, string descr, string respSid, string respName, DateTime noticeDate, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var action = new ProjectActions();
                action.Enabled = true;
                action.CreateDate = DateTime.Now;
                action.CreatorSid = user.Sid;
                action.CreatorName = user.DisplayName;
                action.ProjectId = projectId;
                action.Descr = descr;
                action.ResponsibleSid = respSid;
                action.ResponsibleName = respName;
                action.NoticeDate = noticeDate;
                
                db.ProjectActions.Add(action);
                db.SaveChanges();

                using (var db2 = new SpeCalcEntities())
                {
                    action = db2.ProjectActions.Single(x => x.Id == action.Id);
                    ProjectHistoryModel.CreateHistoryItem(projectId, "Добавление действия", $"Дата действия: {noticeDate:dd.MM.YYYY}\rОтветственный: {respName}\rОписание: {descr}",
                        new[] { action }, user);

                    //Если создатель не ответственный то шлем уведомление
                    if (respSid != user.Sid)
                    {
                        string msg =
                            $"В проекте №{projectId} пользователем {user.DisplayName} добавлено действие где вы указаны как ответственный.<br />Дата действия:{noticeDate:dd.MM.YYYY}<br />Описание:<br />{(!String.IsNullOrEmpty(descr) ? descr : "отсутствует")}<br /><br />Краткая информация о проекте:<br />{ProjectHelper.GetProjectShortInfo(projectId)}<br /><br />Ссылка: {ProjectHelper.GetProjectLink(projectId)}";
                        var email = new MailAddress(User.GetEmailBySid(respSid));
                        MessageHelper.SendMailSmtpAsync($"[Проект №{projectId}] Новое действие", msg, true, null,
                            new [] { email });
                    }
                }
            }
        }
    }
}
