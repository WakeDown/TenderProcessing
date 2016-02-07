using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectConditionModel
    {
        public int ProjectId { get; set; }
        public ProjectConditionHistory ConditionHistory { get; set; }
        public ProjectConditions Condition { get; set; }

        public static IEnumerable<ProjectConditionModel> GetList(int id)
        {
            using (var db = new SpeCalcEntities())
            {
                var conditionsAll = db.ProjectConditions.Where(x => x.Enabled);
                var conditionsProject = db.ProjectConditionHistory.Where(x => x.ProjectId == id).Include(x=>x.ProjectConditions);
                var list = new List<ProjectConditionModel>();
                foreach (var cond in conditionsAll)
                {
                    var item = new ProjectConditionModel();
                    item.ProjectId = id;
                    item.Condition = cond;
                    item.ConditionHistory = conditionsProject.SingleOrDefault(x => x.ConditionId == cond.Id);
                    list.Add(item);
                }
                return list;
            }
        }

        public static void SetProjectCondition(Projects project, int conditionId, string comment, AdUser user, SpeCalcEntities context = null)
        {
            bool isTran = context != null;
            var db = context ?? new SpeCalcEntities();

            project.ConditionChangeDate = DateTime.Now;
            project.ConditionChangerName = user.Sid;
            project.ConditionChangerName = user.DisplayName;
            project.ConditionId = conditionId;
            db.SaveChanges();
            CreateConditionHistory(project, comment, db);

            if (!isTran)
            {
                db.Dispose();
            }

            ProjectHistoryModel.CreateHistoryItem(project.Id, "Изменение состояния", new[] { project }, user);
        }

        public static void CreateConditionHistory(Projects project, string comment, SpeCalcEntities context = null)
        {
            bool isTran = context != null;
            var db = context ?? new SpeCalcEntities();
            var condHistory = new ProjectConditionHistory();
            condHistory.CreateDate = project.ConditionChangeDate.HasValue ? project.ConditionChangeDate.Value : DateTime.Now;
            condHistory.CreatorSid = project.ChangerSid;
            condHistory.CreatorName = project.CreatorName;
            condHistory.ConditionId = project.ConditionId.HasValue ? project.ConditionId.Value : -1;
            condHistory.Comment = comment;
            condHistory.ProjectId = project.Id;
            db.ProjectConditionHistory.Add(condHistory);
            db.SaveChanges();

            if (!isTran)
            {
                db.Dispose();
            }
        }
    }
}
