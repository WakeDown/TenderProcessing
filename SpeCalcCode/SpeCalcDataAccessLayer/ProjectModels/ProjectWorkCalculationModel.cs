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
                db.ProjectWorkCalculations.Add(calc);
                db.SaveChanges();
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
                db.SaveChanges();
                return calc.Id;
            }
        }

        public static void Delete(int id, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var pos = db.ProjectWorkCalculations.Single(x => x.Id == id);
                pos.Enabled = false;
                pos.DeleteDate = DateTime.Now;
                pos.DeleterSid = user.Sid;
                pos.DeleterName = user.DisplayName;
                db.SaveChanges();
            }
        }
    }
}
