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
                db.ProjectPositionCalculations.Add(calc);
                db.SaveChanges();
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
                db.SaveChanges();
                return calc.Id;
            }
        }

        public static void Delete(int id, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var pos = db.ProjectPositionCalculations.Single(x => x.Id == id);
                pos.Enabled = false;
                pos.DeleteDate = DateTime.Now;
                pos.DeleterSid = user.Sid;
                pos.DeleterName = user.DisplayName;
                db.SaveChanges();
            }
        }
    }
}
