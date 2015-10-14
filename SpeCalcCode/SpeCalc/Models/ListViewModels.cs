using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using SpeCalc.Helpers;
using SpeCalc.Objects;
using SpeCalcDataAccessLayer;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Models;
using Stuff.Objects;

namespace SpeCalc.Models
{
    public class ListViewModels : DbModel
    {
        public List<TenderClaim> Claims { get; }
        public List<ProductManager> ProductManagers { get; }
        public List<Manager> Managers { get; set; }
        public FilterTenderClaim Filter { get; set; }
        public int TotalClaimsCount { get; }

        public ListViewModels(FilterTenderClaim filter, List<KeyValuePair<string, string>> subordinates, Role mainRole)
        {
            var db = new DbEngine();
            var authorsSidNamePairs = Employee.GetUserListByAdGroup(AdGroup.SpeCalcManager);
            var managersSidNamePairs = mainRole == Role.Manager
                ? subordinates
                : authorsSidNamePairs;
            authorsSidNamePairs.AddRange(Employee.GetUserListByAdGroup(AdGroup.SpeCalcOperator));
            var productsSidNamePairs = mainRole == Role.ProductManager
                ? subordinates
                : Employee.GetUserListByAdGroup(AdGroup.SpeCalcProduct);
            Managers = new List<Manager>();
            foreach (var pair in managersSidNamePairs)
            {
                Managers.Add(new Manager() { Id = pair.Key, ShortName = pair.Value });
            }
            
            

            ProductManagers = new List<ProductManager>();
            foreach (var pair in productsSidNamePairs)
            {
                ProductManagers.Add(new ProductManager() { Id = pair.Key, ShortName = pair.Value });
            }

            Claims = db.FilterTenderClaims(filter);
            db.SetProductManagersForClaims(Claims);
            db.SetStatisticsForClaims(Claims);
            foreach (var claim in Claims)
            {
                claim.Manager.ShortName = authorsSidNamePairs.Find(m => m.Key == claim.Manager.Id).Value;
                claim.Author.ShortName = authorsSidNamePairs.Find(m => m.Key == claim.Author.Id).Value;
            }
            foreach (var claim in Claims)
            {
                foreach (var product in claim.ProductManagers)
                {
                    product.ShortName = ProductManagers.Find(p => p.Id == product.Id)?.ShortName;
                }
            }
            
            TotalClaimsCount = db.GetCountFilteredTenderClaims(filter);

            Filter = filter;
        }

        /// <summary>
        /// Получает описание типа сделки по его id.
        /// </summary>
        /// <param name="claimDealTypeId"></param>
        /// <returns></returns>
        public static string GetClaimDealType(int claimDealTypeId)
        {
            return new DbEngine().LoadDealTypes().Find(dt => dt.Id == claimDealTypeId).Value;
        }

        /// <summary>
        /// Получает название статуса по его id.
        /// </summary>
        /// <param name="claimStatusId"></param>
        /// <returns></returns>
        public static string GetClaimStatus(int claimStatusId)
        {
            return new DbEngine().LoadClaimStatus().Find(cs => cs.Id == claimStatusId).Value;
        }
    }
}