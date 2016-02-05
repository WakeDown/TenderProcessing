using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SpeCalcDataAccessLayer.Models
{
    public class ProjectHelper
    {
        
        [OutputCache(Duration = 3600)]
        public static IEnumerable<string> GetDeadlineDateList()
        {
            var list = new List<string>();

            int i = 0;
            int quarters = (12 - (DateTime.Now.Month))/3;//Вычисляем сколько кварталов осталось
            i = 4 - quarters;
            while (true)
            {
                i++;
                string item = $"{i} квартал {DateTime.Now.Year}";
                list.Add(item);

                if (i >= 4) break;
            }
            i = 0;
            while (true)
            {
                i++;
                string item = $"{i} квартал {DateTime.Now.Year + 1}";
                list.Add(item);
                
                if (i>= 4)break;
            }

            return list;
        }
        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectPositionQuantityUnits> GetPositionQuantityUnitList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectPositionQuantityUnits.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }

        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectWorkQuantityUnits> GetWorkQuantityUnitList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectWorkQuantityUnits.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }

        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectProtectionFacts> GetProtectFactList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectProtectionFacts.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }

        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectPositionDeliveryTimes> GetDeliveryTimesList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectPositionDeliveryTimes.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }

        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectWorkExecutinsTimes> GetExecutionTimesList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectWorkExecutinsTimes.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }

        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectBusinessTargets> GetBusinessTargetList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectBusinessTargets.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x=>x.Name).ToList();
                return list;
            }
        }
        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectCurrencies> GetCurrencyList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectCurrencies.ToList();
                return list;
            }
        }
        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectSaleDirections> GetSaleDirectionList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectSaleDirections.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }
        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectSaleSubjects> GetSaleSubjectList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectSaleSubjects.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }
        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectClientRelationships> GetProjectClientRelationshipsList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectClientRelationships.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }
    }
}
