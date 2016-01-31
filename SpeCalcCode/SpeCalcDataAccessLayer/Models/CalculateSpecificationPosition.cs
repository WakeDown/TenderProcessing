using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProvider.Helpers;

namespace SpeCalcDataAccessLayer.Models
{
    //Класс - расчет к позиции
    public class CalculateSpecificationPosition
    {
        public int Id { get; set; }
        public int IdSpecificationPosition { get; set; }
        public int IdTenderClaim { get; set; }
        public string CatalogNumber { get; set; }
        public string Name { get; set; }
        public string Replace { get; set; }
        public double PriceCurrency { get; set; }
        public double SumCurrency { get; set; }
        public int? Currency { get; set; }
        public double PriceRub { get; set; }
        public double SumRub { get; set; }
        public string Provider { get; set; }
        public ProtectFact ProtectFact { get; set; }
        public string ProtectCondition { get; set; }
        public string Comment { get; set; }
        public string Author { get; set; }
        public double? PriceUsd { get; set; }
        public double? PriceEur { get; set; }
        public double? PriceEurRicoh { get; set; }
        public double? PriceRubl { get; set; }
        public DeliveryTime DeliveryTime { get; set; }
        public int? DeliveryTimeId { get; set; }
        public int? ProtectFactId { get; set; }
        public bool StateCanEditManager { get; set; }
        public bool StateCanEditProduct { get; set; }
        public double? b2bPrice { get; set; }

        public CalculateSpecificationPosition()
        {
            ProtectFact=new ProtectFact();
            DeliveryTime=new DeliveryTime();
        }

        public CalculateSpecificationPosition(int id):this()
        {
            SqlParameter pId = new SqlParameter() { ParameterName = "id", SqlValue = id, SqlDbType = SqlDbType.Int };
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("GetCalculationPosition", pId);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                FillSelf(row);
            }
        }

        public CalculateSpecificationPosition(DataRow row, string prefix = null)
            : this()
        {
            FillSelf(row, prefix);
        }

        private void FillSelf(DataRow row, string prefix = null)
        {
            StateCanEditManager = Db.DbHelper.GetValueBool(row, prefix + "StateCanEditManager");
            StateCanEditProduct = Db.DbHelper.GetValueBool(row, prefix + "StateCanEditProduct");
            Id = Db.DbHelper.GetValueIntOrDefault(row, prefix + "Id");
            IdSpecificationPosition = Db.DbHelper.GetValueIntOrDefault(row, prefix + "IdPosition");
            IdTenderClaim = Db.DbHelper.GetValueIntOrDefault(row, prefix + "IdClaim");
            CatalogNumber = Db.DbHelper.GetValueString(row, prefix + "CatalogNumber");
            Name = Db.DbHelper.GetValueString(row, prefix + "Name");
            Replace = Db.DbHelper.GetValueString(row, prefix + "Replace");
            //Value = Db.DbHelper.GetValueIntOrDefault(row, "Value");
            //ProductManagerId = Db.DbHelper.GetValueString(row, "ProductManager");
            //ProductManagerName = Db.DbHelper.GetValueString(row, "product_manager_display_name");
            //ProductManager = new ProductManager() { Id = Db.DbHelper.GetValueString(row, "ProductManager"), ShortName = Db.DbHelper.GetValueString(row, "product_manager_display_name") };
            Comment = Db.DbHelper.GetValueString(row, prefix + "Comment");
            PriceCurrency = (double)Db.DbHelper.GetValueDecimalOrDefault(row, prefix + "PriceCurrency");
            SumCurrency = (double)Db.DbHelper.GetValueDecimalOrDefault(row, prefix + "SumCurrency");
            PriceRub = (double)Db.DbHelper.GetValueDecimalOrDefault(row, prefix + "PriceRub");
            SumRub = (double)Db.DbHelper.GetValueDecimalOrDefault(row, prefix + "SumRub");
            PriceUsd = (double?)Db.DbHelper.GetValueDecimalOrNull(row, prefix + "PriceUsd");
            PriceEur = (double?)Db.DbHelper.GetValueDecimalOrNull(row, prefix + "PriceEur");
            PriceEurRicoh = (double?)Db.DbHelper.GetValueDecimalOrNull(row, prefix + "PriceEurRicoh");
            PriceRubl = (double?)Db.DbHelper.GetValueDecimalOrNull(row, prefix + "PriceRubl");
            ProtectFact = new ProtectFact() {Id = Db.DbHelper.GetValueIntOrDefault(row,prefix+ "ProtectFact"), Value = Db.DbHelper.GetValueString(row, prefix + "ProtectFactName") };
            DeliveryTime = new DeliveryTime() { Id = Db.DbHelper.GetValueIntOrDefault(row, prefix + "DeliveryTime"), Value = Db.DbHelper.GetValueString(row, prefix + "DeliveryTimeName") };
            ProtectCondition = Db.DbHelper.GetValueString(row, prefix + "ProtectCondition");
            Author = Db.DbHelper.GetValueString(row, prefix + "Author");
            Currency = Db.DbHelper.GetValueIntOrNull(row, prefix + "Currency");
            Provider = Db.DbHelper.GetValueString(row, prefix + "Provider");
        }

        public static IEnumerable<CalculateSpecificationPosition> GetList(int idClaim, int version, string productSid = null)
        {
            SqlParameter pId = new SqlParameter() { ParameterName = "id", SqlValue = idClaim, SqlDbType = SqlDbType.Int };
            SqlParameter pVersion = new SqlParameter() { ParameterName = "version", SqlValue = version, SqlDbType = SqlDbType.Int };
            SqlParameter pproductSid = new SqlParameter() { ParameterName = "productSid", SqlValue = productSid, SqlDbType = SqlDbType.VarChar };
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("LoadCalculateClaimPositionForClaim", pId, pVersion, pproductSid);

            var lst = new List<CalculateSpecificationPosition>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var model = new CalculateSpecificationPosition(row);
                    lst.Add(model);
                }
            }
            return lst;
        }
    }
}
