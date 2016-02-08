using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectPositionCalculationModel
    {

        public static ProjectPositionCalculations Get(int id)
        {
            //using (var db = new SpeCalcEntities())
            //{
            var db = new SpeCalcEntities();
                return db.ProjectPositionCalculations.SingleOrDefault(x => x.Id == id);
            //}
        }

        public static int Create(ProjectPositionCalculations calc, AdUser user)
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
                db.ProjectPositionCalculations.Add(calc);
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(calc.ProjectPositions.ProjectId, "Добавление расчета оборудования", $"{calc.Name}", new[] { calc }, user);
                return calc.Id;
            }
        }

        public static int Save(ProjectPositionCalculations calc, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var oldcalc = db.ProjectPositionCalculations.Single(x => x.Id == calc.Id);
                oldcalc.CatalogNumber = calc.CatalogNumber;
                oldcalc.Name = calc.Name;
                oldcalc.Cost = calc.Cost;
                oldcalc.CurrencyId = calc.CurrencyId;
                oldcalc.Provider = calc.Provider;
                oldcalc.ProtectionFactId = calc.ProtectionFactId;
                oldcalc.ProtectionFactCondition = calc.ProtectionFactCondition;
                oldcalc.DeliveryTimeId = calc.DeliveryTimeId;
                oldcalc.Comment = calc.Comment;
                oldcalc.RecomendedPrice = calc.RecomendedPrice;
                oldcalc.LastChangeDate = DateTime.Now;
                oldcalc.LastChangerSid = user.Sid;
                oldcalc.LastChangerName = user.DisplayName;
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(calc.ProjectPositions.ProjectId, "Изменение расчета оборудования", $"{oldcalc.Name}", new[] { oldcalc, calc }, user);
                return calc.Id;
            }
        }

        public static void Delete(int id, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var calc = db.ProjectPositionCalculations.Single(x => x.Id == id);
                calc.Enabled = false;
                calc.DeleteDate = DateTime.Now;
                calc.DeleterSid = user.Sid;
                calc.DeleterName = user.DisplayName;
                
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(calc.ProjectPositions.ProjectId, "Удаление расчета оборудования",  $"{calc.Name}", new[] { calc }, user);
            }
        }
    }
}
