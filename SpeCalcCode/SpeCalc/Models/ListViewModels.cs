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
using SpeCalcDataAccessLayer.Models;
using Stuff.Objects;

namespace SpeCalc.Models
{
    public class ListViewModels : DbModel
    {
        public List<TenderClaim> Claims { get; }
        public List<ProductManager> ProductManagers { get; }
        public List<Manager> Managers { get; }
        public int TotalClaimsCount { get; }
        public ListViewModels(FilterTenderClaim filter)
        {
            var db = new DbEngine();
            Claims = db.FilterTenderClaims(filter);
            db.SetProductManagersForClaims(Claims);
            db.SetStatisticsForClaims(Claims);
            ProductManagers = UserHelper.GetProductManagers();
            foreach (var claim in Claims)
            {
                foreach (var product in claim.ProductManagers)
                {
                    product.ShortName = ProductManagers.Find(p => p.Id == product.Id).ShortName;
                }
            }
            Uri uri = new Uri($"{OdataServiceUri}/Ad/GetUserListByAdGroup?group={AdGroup.SpeCalcManager}");
            string jsonString = GetJson(uri);
            var sidNamePairs = JsonConvert.DeserializeObject<List<KeyValuePair <string, string>>>(jsonString);
            Managers = new List<Manager>();
            foreach (var pair in sidNamePairs)
            {
                Managers.Add(new Manager() {Id = pair.Key,ShortName = pair.Value});
            }
            TotalClaimsCount = db.GetCountFilteredTenderClaims(filter);
        }

        public static string GetClaimDealType(int claimDealTypeId)
        {
            var db = new DbEngine();
            return db.LoadDealTypes().Find(dt => dt.Id == claimDealTypeId).Value;
        }

        public static string GetClaimStatus(int claimStatusId)
        {
            var db = new DbEngine();
            return db.LoadClaimStatus().Find(cs => cs.Id == claimStatusId).Value;
        }
    }
}