using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SpeCalcDataAccessLayer.Models
{
    public class ProjectHelper
    {
        public static string GetProjectLink(int id)
        {
            string speCalcUrl = ConfigurationManager.AppSettings["AppHost"];
            string link = $"{speCalcUrl}/Project/Index?id={id}";
            return link;
        }

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
        public static IEnumerable<ProjectSaleSubjects> GetSaleSubjectList(int? idDirection=null)
        {
            //if (!idDirection.HasValue) return new List<ProjectSaleSubjects>();
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectSaleSubjects.Where(x => x.Enabled)
                    .Where(x=> !idDirection.HasValue || idDirection <= 0 || (idDirection.HasValue && idDirection > 0 && x.IdSaleDirection == idDirection))
                    .OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }

        [OutputCache(Duration = 3600)]
        public static IEnumerable<KeyValuePair<int, string>> GetSaleSubjectSelectionList(int? idDirection)
        {
            var list = new List<KeyValuePair<int, string>>();
            using (var db = new SpeCalcEntities())
            {
                var model = GetSaleSubjectList(idDirection);
                foreach (var m in model)
                {
                    var item = new KeyValuePair<int, string>(m.Id, m.Name);
                    list.Add(item);
                }
                

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

        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectStates> GetProjectStatesList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectStates.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }

        public static IEnumerable<ProjectStates> GetProjectStatesFilterList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectStates.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).Include(x=>x.Projects).ToList();
                return list;
            }
        }

        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProjectConditions> GetProjectConditionsList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectConditions.Where(x => x.Enabled).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }

        public static IEnumerable<ProjectSaleDirections> GetSaleDirection4ResponsiblesList()
        {
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectSaleDirections.Where(x => x.Enabled).Include(x=>x.ProjectSaleDirectionResponsibles).OrderBy(x => x.OrderNum).ThenBy(x => x.Name).ToList();
                return list;
            }
        }
    }
}
