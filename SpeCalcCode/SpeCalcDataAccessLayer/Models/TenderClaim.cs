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
        public Manager Manager { get; set; }
        public int TenderStatus { get; set; }
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
        public string ManagerName { get; set; }
        public string ManagerDepartment { get; set; }
        public string ManagerChief { get; set; }

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

            //CreatorSid = Db.DbHelper.GetValueString(row, "creator_sid");
            //CreatorName = Db.DbHelper.GetValueString(row, "creator_name");
            //AuthorSid = Db.DbHelper.GetValueString(row, "author_sid");
            //AuthorName = Db.DbHelper.GetValueString(row, "author_name");
            //IdPosition = Db.DbHelper.GetValueIntOrDefault(row, "id_position");
            //PositionName = Db.DbHelper.GetValueString(row, "position_name");
            //IdDepartment = Db.DbHelper.GetValueIntOrDefault(row, "id_department");
            //DepartmentName = Db.DbHelper.GetValueString(row, "department_name");
            //ChiefSid = Db.DbHelper.GetValueString(row, "chief_sid");
            //ChiefName = Db.DbHelper.GetValueString(row, "chief_name");
            //IdCause = Db.DbHelper.GetValueIntOrDefault(row, "id_cause");
            //CauseName = Db.DbHelper.GetValueString(row, "cause_name");
            //MatcherSid = Db.DbHelper.GetValueString(row, "matcher_sid");
            //MatcherName = Db.DbHelper.GetValueString(row, "matcher_name");
            //PersonalManagerSid = Db.DbHelper.GetValueString(row, "personal_manager_sid");
            //PersonalManagerName = Db.DbHelper.GetValueString(row, "personal_manager_name");
            //DeadlineDate = Db.DbHelper.GetValueDateTimeOrNull(row, "deadline_date");
            //EndDate = Db.DbHelper.GetValueDateTimeOrNull(row, "end_date");
            //IdState = Db.DbHelper.GetValueIntOrDefault(row, "id_state");
            //StateName = Db.DbHelper.GetValueString(row, "state_name");
            //StateChangeDate = Db.DbHelper.GetValueDateTimeOrNull(row, "state_change_date");
            //StateChangerSid = Db.DbHelper.GetValueString(row, "state_changer_sid");
            //StateChangerName = Db.DbHelper.GetValueString(row, "state_changer_name");
            //DateCreate = Db.DbHelper.GetValueDateTimeOrDefault(row, "dattim1");
            //CandidateTotalCount = Db.DbHelper.GetValueIntOrDefault(row, "candidate_total_count");
            //CandidateCancelCount = Db.DbHelper.GetValueIntOrDefault(row, "candidate_cancel_count");
            ////CandidateAcceptCount = Db.DbHelper.GetValueIntOrDefault(row, "candidate_accept_count");
            //StateBackgroundColor = Db.DbHelper.GetValueString(row, "state_background_color");
            //StateIsActive = Db.DbHelper.GetValueBool(row, "state_is_active");
            //OrderEndDate = Db.DbHelper.GetValueDateTimeOrNull(row, "order_end_date");
            //ClaimEndDate = Db.DbHelper.GetValueDateTimeOrNull(row, "claim_end_date");
            //IdCity = Db.DbHelper.GetValueIntOrNull(row, "id_city");
            //CityName = Db.DbHelper.GetValueString(row, "city_name");
            //IdBranchOffice = Db.DbHelper.GetValueIntOrDefault(row, "id_branch_office");
            //BranchOfficeName = Db.DbHelper.GetValueString(row, "branch_office_name");
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
