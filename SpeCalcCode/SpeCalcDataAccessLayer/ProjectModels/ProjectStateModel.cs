using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceClaim.Helpers;
using SpeCalcDataAccessLayer.Models;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectStateModel
    {
        public static IEnumerable<ProjectStateHistory> GetList(out int totalCount, int projectId, bool full = true)
        {
            //using (var db = new SpeCalcEntities())
            //{
            var db = new SpeCalcEntities();
            var query = db.ProjectStateHistory.Where(x => x.ProjectId == projectId);
            var page = query.OrderByDescending(p => p.Id).Take(1);
            if (full)
            {
                page = query.OrderByDescending(p => p.Id);
            }
            totalCount = query.Count();
            var list = page.Include(x=>x.ProjectStates).Select(p => p);
            return list;
            //}
        }

        public static ProjectStates GetState(string sysName)
        {
            using(var db = new SpeCalcEntities())
            {
                return db.ProjectStates.Single(x => x.Enabled && x.SysName == sysName);
            }
        }

        public static void CreateStateHistory(Projects project, string comment, SpeCalcEntities context = null)
        {
            bool isTran = context != null;
            var db = context ?? new SpeCalcEntities();
            var stateHistory = new ProjectStateHistory();
            stateHistory.CreateDate = project.StateChangeDate.HasValue ? project.StateChangeDate.Value : DateTime.Now;
            stateHistory.CreatorSid = project.ChangerSid;
            stateHistory.CreatorName = project.CreatorName;
            stateHistory.StateId = project.StateId.HasValue ? project.StateId.Value : -1;
            stateHistory.Comment = comment;
            stateHistory.ProjectId = project.Id;
            db.ProjectStateHistory.Add(stateHistory);
            db.SaveChanges();

            if (!isTran)
            {
                db.Dispose();
            }
        }

        public static void SetProjectState(Projects project, int stateId, string comment, AdUser user, SpeCalcEntities context = null)
        {
            bool isTran = context != null;
            var db = context ?? new SpeCalcEntities();

            string prevStateName = project.StateId.HasValue ? project.ProjectStates.Name : null;
            project.StateChangeDate = DateTime.Now;
            project.ChangerSid = user.Sid;
            project.ChangerName = user.DisplayName;
            project.StateId = stateId;
            //db.ProjectStates.Add(project);
            db.SaveChanges();
            CreateStateHistory(project, comment, db);
            using (var db2 = new SpeCalcEntities())
            {
                project = db2.Projects.Single(x => x.Id == project.Id);
                ProjectHistoryModel.CreateHistoryItem(project.Id, "Изменение статуса", $"C {prevStateName} на {project.ProjectStates.Name}." + (!String.IsNullOrEmpty(comment) ? $"\rКомментарий: {comment}" : null),
                    new[] {project}, user);

                //Чтобы не слатьпо два уведомления при создании проекта
                if (db2.ProjectStates.Single(x => x.Id == stateId).SysName != "NEW")
                {
                    string msg =
                        $"В проекте №{project.Id} изменился статус с {prevStateName} на {project.ProjectStates.Name} .<br />Комментарий:<br />{(!String.IsNullOrEmpty(comment) ? comment : "отсутствует")}<br /><br />Краткая информация о проекте:<br />{ProjectHelper.GetProjectShortInfo(project.Id)}<br /><br />Ссылка: {ProjectHelper.GetProjectLink(project.Id)}";
                    var emails = ProjectTeamModel.GetEmailList(project.Id);
                    MessageHelper.SendMailSmtpAsync($"[Проект №{project.Id}] {project.ProjectStates.Name}", msg, true, null,
                        emails.ToArray());
                }
            }
            if (!isTran)
            {
                db.Dispose();
            }
            
        }
    }
}
