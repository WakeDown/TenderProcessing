using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProvider.Helpers;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Objects;
using Stuff.Models;

namespace SpeCalcDataAccessLayer.Models
{
    //класс - заявка
    public class TenderClaim
    {
        public int Id { get; set; }
        public string TenderNumber { get; set; }
        public DateTime TenderStart { get; set; }
        public DateTime ClaimDeadline { get; set; }
        public DateTime KPDeadline { get; set; }
        public string TenderStartString => TenderStart.ToString("dd.MM.yyyy");
        public string ClaimDeadlineString => ClaimDeadline.ToString("dd.MM.yyyy");
        public string KPDeadlineString => KPDeadline.ToString("dd.MM.yyyy");
        public string Comment { get; set; }
        public string Customer { get; set; }
        public string CustomerInn { get; set; }
        public double Sum { get; set; }
        public int SumCurrency { get; set; }
        public int DealType { get; set; }
        public string DealTypeName { get; set; }
        public string TenderUrl { get; set; }
        public int ClaimStatus { get; set; }
        public string ClaimStatusName { get; set; }
        public Manager Manager { get; set; }
        public int TenderStatus { get; set; }
        public string TenderStatusName { get; set; }
        public DateTime RecordDate { get; set; }
        public string RecordDateString => RecordDate.ToString("dd.MM.yyyy HH:mm");
        public AdUser Author { get; set; }
        public bool Deleted { get; set; }
        public double CurrencyUsd { get; set; }
        public double CurrencyEur { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? DeliveryDateEnd { get; set; }
        public string DeliveryPlace { get; set; }
        public DateTime? AuctionDate { get; set; }
        public string DeliveryDateString => DeliveryDate.HasValue
                        ? DeliveryDate.Value.ToString("dd.MM.yyyy")
                        : string.Empty;
        public string DeliveryDateEndString => DeliveryDateEnd.HasValue
                        ? DeliveryDateEnd.Value.ToString("dd.MM.yyyy")
                        : string.Empty;
        public string AuctionDateString => AuctionDate.HasValue
                        ? AuctionDate.Value.ToString("dd.MM.yyyy")
                        : string.Empty;
        public int PositionsCount { get; set; }
        public int CalculatesCount { get; set; }
        public int CalculatePositionsCount { get; set; }
        public List<ProductManager> ProductManagers { get; set; }
        public List<SpecificationPosition> Positions { get; set; }
        //public string ManagerName { get; set; }
        //public string ManagerDepartment { get; set; }
        //public string ManagerChief { get; set; }
        public int IdClaimType { get; set; }
        public string ClaimTypeSysName { get; set; }
        public string ClaimTypeName { get; set; }
        public string StrSum
        {
            get
            {
                var str = String.Format("{0:### ### ### ### ###.##}", Sum);
                switch (SumCurrency)
                {
                    case 1:
                        str += " руб.";
                        break;
                    case 2:
                        str += " USD";
                        break;
                    case 3:
                        str += " EUR";
                        break;
                }
                return str;
            }
        }

        public List<ClaimCert> Certs { get; set; }
        public List<TenderClaimFile> Files { get; set; }
        public bool StateCanAddPositions { get; set; }
        public bool StateIsEnd { get; set; }
        public bool StateIsActive { get; set; }
        public bool StateManagerPositionWork { get; set; }
        public string StateSysName { get; set; }
        public bool StateCanStop { get; set; }
        public bool StateCanPause { get; set; }
        public bool StateCanStart { get; set; }
        public bool StateCanConfirmPositions { get; set; }
        public bool StateCanDiscartPositions { get; set; }
        public bool StateCanChangeProduct { get; set; }
        public bool StateCanCallRejectPositions { get; set; }

        public TenderClaim()
        {

        }

        public TenderClaim(int id)
        {
            SqlParameter pId = new SqlParameter() { ParameterName = "id", SqlValue = id, SqlDbType = SqlDbType.Int };
            
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("LoadTenderClaimById", pId);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                FillSelf(row);
            }
        }

        public TenderClaim(DataRow row)
            : this()
        {
            FillSelf(row);
        }

        private void FillSelf(DataRow row)
        {
            StateCanCallRejectPositions = Db.DbHelper.GetValueBool(row, "StateCanCallRejectPositions");
            StateManagerPositionWork = Db.DbHelper.GetValueBool(row, "StateManagerPositionWork");
            StateCanAddPositions = Db.DbHelper.GetValueBool(row, "StateCanAddPositions");
            StateIsEnd = Db.DbHelper.GetValueBool(row, "StateIsEnd");
            StateIsActive = Db.DbHelper.GetValueBool(row, "StateIsActive");
            Id = Db.DbHelper.GetValueIntOrDefault(row, "id");
            TenderNumber= Db.DbHelper.GetValueString(row, "TenderNumber");
            TenderStart = Db.DbHelper.GetValueDateTimeOrDefault(row, "TenderStart");
            ClaimDeadline = Db.DbHelper.GetValueDateTimeOrDefault(row, "ClaimDeadline");
            KPDeadline = Db.DbHelper.GetValueDateTimeOrDefault(row, "KPDeadline");
            Comment = Db.DbHelper.GetValueString(row, "Comment");
            Customer = Db.DbHelper.GetValueString(row, "Customer");
            CustomerInn = Db.DbHelper.GetValueString(row, "CustomerInn");
            Sum = (double)Db.DbHelper.GetValueDecimalOrDefault(row, "TotalSum");
            SumCurrency = Db.DbHelper.GetValueIntOrDefault(row, "IdSumCurrency");
            DealType = Db.DbHelper.GetValueIntOrDefault(row, "DealType");
            DealTypeName = Db.DbHelper.GetValueString(row, "DealTypeName");
            TenderUrl = Db.DbHelper.GetValueString(row, "TenderUrl");
            ClaimStatus = Db.DbHelper.GetValueIntOrDefault(row, "ClaimStatus");
            ClaimStatusName = Db.DbHelper.GetValueString(row, "ClaimStatusName");
            Manager = new Manager() {Id = Db.DbHelper.GetValueString(row, "Manager"), ShortName = Db.DbHelper.GetValueString(row, "manager_display_name"), ChiefShortName = Db.DbHelper.GetValueString(row, "chief_display_name"), SubDivision = Db.DbHelper.GetValueString(row, "manager_department_name") };
            TenderStatus = Db.DbHelper.GetValueIntOrDefault(row, "TenderStatus");
            TenderStatusName = Db.DbHelper.GetValueString(row, "TenderStatusName");
            RecordDate = Db.DbHelper.GetValueDateTimeOrDefault(row, "RecordDate");
            Author= new AdUser() {DisplayName = Db.DbHelper.GetValueString(row, "author_display_name"), Sid = Db.DbHelper.GetValueString(row, "Author") };
            DeliveryDate = Db.DbHelper.GetValueDateTimeOrNull(row, "DeliveryDate");
            DeliveryDateEnd = Db.DbHelper.GetValueDateTimeOrNull(row, "DeliveryDateEnd");
            AuctionDate = Db.DbHelper.GetValueDateTimeOrNull(row, "DeliveryDateEnd");
            DeliveryPlace = Db.DbHelper.GetValueString(row, "DeliveryPlace");
            PositionsCount = Db.DbHelper.GetValueIntOrDefault(row, "PositionsCount");
            CalculatesCount = Db.DbHelper.GetValueIntOrDefault(row, "CalculatesCount");
            CalculatePositionsCount = Db.DbHelper.GetValueIntOrDefault(row, "CalculatePositionsCount");
            IdClaimType = Db.DbHelper.GetValueIntOrDefault(row, "IdClaimType");
            ClaimTypeSysName = Db.DbHelper.GetValueString(row, "ClaimTypeSysName");
            ClaimTypeName = Db.DbHelper.GetValueString(row, "ClaimTypeName");
            StateSysName = Db.DbHelper.GetValueString(row, "StateSysName");
            StateCanStop = Db.DbHelper.GetValueBool(row, "StateCanStop");
            StateCanPause = Db.DbHelper.GetValueBool(row, "StateCanPause");
            StateCanStart = Db.DbHelper.GetValueBool(row, "StateCanStart");
            StateCanConfirmPositions = Db.DbHelper.GetValueBool(row, "StateCanConfirmPositions");
            StateCanDiscartPositions = Db.DbHelper.GetValueBool(row, "StateCanDiscartPositions");
            StateCanChangeProduct = Db.DbHelper.GetValueBool(row, "StateCanChangeProduct");
        }

        public static IEnumerable<SpecificationPosition> GetRejectedPositions(out int totalCount, int id, int version)
        {
            totalCount = 0;
            return SpecificationPosition.GetList(id, version);
        }

        public static IEnumerable<HistoryItem> GetHistory(out int totalCount, int id, bool fullList = false)
        {
            SqlParameter pId = new SqlParameter() { ParameterName = "id", SqlValue = id, SqlDbType = SqlDbType.Int };
            SqlParameter pFullList = new SqlParameter() { ParameterName = "full_list", SqlValue = fullList, SqlDbType = SqlDbType.Bit };
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("claim_get_history", pId, pFullList);

            totalCount = 0;
            var lst = new List<HistoryItem>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var model = new HistoryItem(row);
                    lst.Add(model);
                }
                totalCount = Db.DbHelper.GetValueIntOrDefault(dt.Rows[0], "total_count");
            }
            return lst;
        }
    }
}
