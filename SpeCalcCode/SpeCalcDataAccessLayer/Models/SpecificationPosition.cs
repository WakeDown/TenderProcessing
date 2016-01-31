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
        public bool StateCanEditManager { get; set; }
        public bool StateCanEditProduct { get; set; }
        public bool StateIsEnd { get; set; }
        public string StateBackgroundColor { get; set; }
        public string StateSysName { get; set; }
        public string StateImageClass { get; set; }
        public string StateImageColorClass { get; set; }

        public SpecificationPosition()
        {
            Calculations = new List<CalculateSpecificationPosition>();
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
            StateImageColorClass = Db.DbHelper.GetValueString(row, "StateImageColorClass");
            StateImageClass = Db.DbHelper.GetValueString(row, "StateImageClass");
            StateBackgroundColor = Db.DbHelper.GetValueString(row, "StateBackgroundColor");
            StateSysName = Db.DbHelper.GetValueString(row, "StateSysName");
            StateCanEditManager = Db.DbHelper.GetValueBool(row, "StateCanEditManager");
            StateCanEditProduct = Db.DbHelper.GetValueBool(row, "StateCanEditProduct");
            StateIsEnd = Db.DbHelper.GetValueBool(row, "StateIsEnd");
            Id = Db.DbHelper.GetValueIntOrDefault(row, "Id");
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

        public static IEnumerable<SpecificationPosition> GetList(int idClaim, int version, string productSid = null, int? stateId = null)
        {
            SqlParameter pId = new SqlParameter() { ParameterName = "id", SqlValue = idClaim, SqlDbType = SqlDbType.Int };
            SqlParameter pVersion = new SqlParameter() { ParameterName = "calcVersion", SqlValue = version, SqlDbType = SqlDbType.Int };
            SqlParameter pproductSid = new SqlParameter() { ParameterName = "productSid", SqlValue = productSid, SqlDbType = SqlDbType.VarChar };
            SqlParameter pstateId = new SqlParameter() { ParameterName = "stateId", SqlValue = stateId, SqlDbType = SqlDbType.Int };
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("LoadClaimPositionForTenderClaim", pId, pVersion, pproductSid, pstateId);

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

        public static IEnumerable<SpecificationPosition> GetListWithCalc(int idClaim, int version, string productSid = null)
        {
            var posList = GetList(idClaim, version, productSid);
            var calcList = CalculateSpecificationPosition.GetList(idClaim, version, productSid);

            foreach (SpecificationPosition position in posList)
            {
                var calcs = calcList.Where(x => x.IdSpecificationPosition == position.Id);
                position.Calculations.AddRange(calcs);
            }

            
            return posList;
        }
    }
}
