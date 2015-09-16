using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using SpeCalc.Helpers;
using SpeCalcDataAccessLayer;
using Stuff.Objects;

namespace SpeCalc.Models
{
    public class CalcPosition:DbModel
    {
        public int Id { get; set; }
        public int IdParent { get; set; }

        public static CalcPositionChanges GetChanges(int id)
        {
           var dt =  DbEngine.GetCalcPositionsChanges(id);
            var result = new CalcPositionChanges();
            if (dt.Rows.Count > 0)
            {
                result = new CalcPositionChanges(dt.Rows[0]);
            }
            return result;
        }
    }

    public class CalcPositionChanges
    {
        public bool IsNewCalcPosition { get; set; }
        public bool CatalogNumberChange { get; set; }
        public bool NameChange { get; set; }
        public bool ProviderChange { get; set; }
        public bool ProtectFactChange { get; set; }
        public bool ProtectConditionChange { get; set; }
        public bool CommentChange { get; set; }
        public bool PriceUsdChange { get; set; }
        public bool PriceEurChange { get; set; }
        public bool PriceEurRicohChange { get; set; }
        public bool PriceRublChange { get; set; }
        public bool DeliveryTimeChange { get; set; }
        

        public CalcPositionChanges()
        {

        }

        public CalcPositionChanges(DataRow row)
        {
            FillSelf(row);
        }

        public void FillSelf(DataRow row)
        {
            IsNewCalcPosition = DbHelper.GetValueBool(row, "IsNewCalcPosition");
            CatalogNumberChange = DbHelper.GetValueBool(row, "CatalogNumberChange");
            NameChange = DbHelper.GetValueBool(row, "NameChange");
            ProviderChange = DbHelper.GetValueBool(row, "ProviderChange");
            ProtectFactChange = DbHelper.GetValueBool(row, "ProtectFactChange");
            ProtectConditionChange = DbHelper.GetValueBool(row, "ProtectConditionChange");
            CommentChange = DbHelper.GetValueBool(row, "CommentChange");
            PriceUsdChange = DbHelper.GetValueBool(row, "PriceUsdChange");
            PriceEurChange = DbHelper.GetValueBool(row, "PriceEurChange");
            PriceEurRicohChange = DbHelper.GetValueBool(row, "PriceEurRicohChange");
            PriceRublChange = DbHelper.GetValueBool(row, "PriceRublChange");
            DeliveryTimeChange = DbHelper.GetValueBool(row, "DeliveryTimeChange");
        }
    }
}