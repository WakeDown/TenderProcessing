using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectWorkCalculationModel
    {

        public static ProjectWorkCalculations Get(int id)
        {
            //using (var db = new SpeCalcEntities())
            //{
            var db = new SpeCalcEntities();
                return db.ProjectWorkCalculations.SingleOrDefault(x => x.Id == id);
            //}
        }

        public static int Create(ProjectWorkCalculations calc, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                calc.Enabled = true;
                calc.CreatorSid = user.Sid;
                calc.CreatorName = user.DisplayName;
                calc.CreateDate = DateTime.Now;
                //calc.LastChangeDate = DateTime.Now;
                //calc.LastChangerSid = user.Sid;
                //calc.LastChangerName = user.DisplayName;
                db.ProjectWorkCalculations.Add(calc);
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(calc.ProjectWorks.ProjectId, "Добавление расчета работы", new[] { calc }, user);
                return calc.Id;
            }
        }

        public static int Save(ProjectWorkCalculations calc, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var oldcalc = db.ProjectWorkCalculations.Single(x => x.Id == calc.Id);
                oldcalc.Name = calc.Name;
                oldcalc.Cost = calc.Cost;
                oldcalc.CurrencyId = calc.CurrencyId;
                oldcalc.ExecutionTimeId = calc.ExecutionTimeId;
                oldcalc.ExecutionTime = calc.ExecutionTime;
                oldcalc.ExecutorName = calc.ExecutorName;
                oldcalc.Comment = calc.Comment;
                oldcalc.LastChangeDate = DateTime.Now;
                oldcalc.LastChangerSid = user.Sid;
                oldcalc.LastChangerName = user.DisplayName;
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(calc.ProjectWorks.ProjectId, "Изменение расчета работы", new[] { oldcalc, calc }, user);
                return calc.Id;
            }
        }

        public static void Delete(int id, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var calc = db.ProjectWorkCalculations.Single(x => x.Id == id);
                calc.Enabled = false;
                calc.DeleteDate = DateTime.Now;
                calc.DeleterSid = user.Sid;
                calc.DeleterName = user.DisplayName;
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(calc.ProjectWorks.ProjectId, "Удаление расчета работы", new[] { calc }, user);
            }
        }
    }
}
