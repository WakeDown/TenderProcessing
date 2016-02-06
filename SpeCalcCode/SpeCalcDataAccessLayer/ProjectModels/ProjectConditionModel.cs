using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectConditionModel
    {
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
                    item.Condition = cond;
                    item.ConditionHistory = conditionsProject.SingleOrDefault(x => x.ConditionId == cond.Id);
                    list.Add(item);
                }
                return list;
            }
        }
    }
}
