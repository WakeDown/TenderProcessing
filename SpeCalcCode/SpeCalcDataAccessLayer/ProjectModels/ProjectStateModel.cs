using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var page = query.OrderByDescending(p => p.Id).Take(4);
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

            project.StateChangeDate = DateTime.Now;
            project.ChangerSid = user.Sid;
            project.ChangerName = user.DisplayName;
            project.StateId = stateId;
            //db.ProjectStates.Add(project);
            db.SaveChanges();
            CreateStateHistory(project, comment, db);

            if (!isTran)
            {
                db.Dispose();
            }
        }
    }
}
