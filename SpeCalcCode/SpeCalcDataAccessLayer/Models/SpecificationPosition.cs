using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProvider.Helpers;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.Models
{
    //класс - позиция заявки
    public class SpecificationPosition
    {
        public int Id { get; set; }
        public int IdClaim { get; set; }
        public int RowNumber { get; set; }
        public string CatalogNumber { get; set; }
        public string Name { get; set; }
        public string Replace { get; set; }
        public int Unit { get; set; }
        public int Value { get; set; }
        public string ProductManagerId { get; set; }
        public ProductManager ProductManager { get; set; }
        public string Comment { get; set; }
        public double Price { get; set; }
        public double Sum { get; set; }
        public double PriceTzr { get; set; }
        public double SumTzr { get; set; }
        public double PriceNds { get; set; }
        public double SumNds { get; set; }
        public int State { get; set; }
        public string Author { get; set; }
        public int? Currency { get; set; }
        public List<CalculateSpecificationPosition> Calculations { get; set; } 
        public int Version { get; set; }
        public string PositionStateName { get; set; }
        public string UnitName { get; set; }
        public string ProductManagerName { get; set; }

        public SpecificationPosition()
        {

        }

        public SpecificationPosition(int id)
        {
            SqlParameter pId = new SqlParameter() { ParameterName = "id", SqlValue = id, SqlDbType = SqlDbType.Int };
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("GetClaimPosition", pId);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                FillSelf(row);
            }
        }

        public SpecificationPosition(DataRow row)
            : this()
        {
            FillSelf(row);
        }

        private void FillSelf(DataRow row)
        {
            Id = Db.DbHelper.GetValueIntOrDefault(row, "id");
            IdClaim = Db.DbHelper.GetValueIntOrDefault(row, "IdClaim");
            RowNumber = Db.DbHelper.GetValueIntOrDefault(row, "RowNumber");
            CatalogNumber = Db.DbHelper.GetValueString(row, "CatalogNumber");
            Name = Db.DbHelper.GetValueString(row, "Name");
            Replace = Db.DbHelper.GetValueString(row, "Replace");
            Value = Db.DbHelper.GetValueIntOrDefault(row, "Value");
            ProductManagerId = Db.DbHelper.GetValueString(row, "ProductManager");
            ProductManagerName = Db.DbHelper.GetValueString(row, "product_manager_display_name");
            ProductManager = new ProductManager() { Id = Db.DbHelper.GetValueString(row, "ProductManager"), ShortName = Db.DbHelper.GetValueString(row, "product_manager_display_name") };
            Comment = Db.DbHelper.GetValueString(row, "Comment");
            Price = (double)Db.DbHelper.GetValueDecimalOrDefault(row, "Price");
            Sum = (double)Db.DbHelper.GetValueDecimalOrDefault(row, "TotalSum");
            PriceTzr= (double)Db.DbHelper.GetValueDecimalOrDefault(row, "PriceTzr");
            SumTzr = (double)Db.DbHelper.GetValueDecimalOrDefault(row, "SumTzr");
            PriceNds = (double)Db.DbHelper.GetValueDecimalOrDefault(row, "PriceNds");
            SumNds = (double)Db.DbHelper.GetValueDecimalOrDefault(row, "SumNds");
            State = Db.DbHelper.GetValueIntOrDefault(row, "State");
            Author = Db.DbHelper.GetValueString(row, "Author");
            Currency = Db.DbHelper.GetValueIntOrNull(row, "Currency");
            Version = Db.DbHelper.GetValueIntOrDefault(row, "Version");
            PositionStateName = Db.DbHelper.GetValueString(row, "PositionStateName");
            UnitName = Db.DbHelper.GetValueString(row, "UnitName");
            Unit = Db.DbHelper.GetValueIntOrDefault(row, "Unit");
        }

        public static IEnumerable<SpecificationPosition> GetList(int idClaim, int version)
        {
            SqlParameter pId = new SqlParameter() { ParameterName = "id", SqlValue = idClaim, SqlDbType = SqlDbType.Int };
            SqlParameter pVersion = new SqlParameter() { ParameterName = "calcVersion", SqlValue = version, SqlDbType = SqlDbType.Int };
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("LoadClaimPositionForTenderClaim", pId, pVersion);

            var lst = new List<SpecificationPosition>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var model = new SpecificationPosition(row);
                    lst.Add(model);
                }
            }
            return lst;
        }
    }
}
