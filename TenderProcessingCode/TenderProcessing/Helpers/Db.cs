using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TenderProcessing.Helpers
{
    public class Db
    {
        #region const
        
        private static SqlConnection unitConn { get { return new SqlConnection(ConfigurationManager.ConnectionStrings["unitConnectionString"].ConnectionString); } }
        public const string spRates = "hp_exchange_rate";
        public const string spUsers = "ui_users";
        public static DataTable ExecuteQueryStoredProcedure(string spName, string action, params SqlParameter[] sqlParams)
        {
            DataTable dt = new DataTable();

            using (var conn = unitConn)
            using (var cmd = new SqlCommand(spName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 1000
            })
            {
                if (!string.IsNullOrEmpty(action) && !string.IsNullOrWhiteSpace(action))
                {
                    SqlParameter pAction = new SqlParameter() { ParameterName = "action", Value = action, DbType = DbType.AnsiString };
                    cmd.Parameters.Add(pAction);
                }

                cmd.Parameters.AddRange(sqlParams);
                conn.Open();
                dt.Load(cmd.ExecuteReader());
            }

            return dt;
        }
        #endregion

        public static DataTable GetExchangeRatesOnDate(DateTime dateRate)
        {
            SqlParameter pDateRate = new SqlParameter() { ParameterName = "date_rate", Value = dateRate, DbType = DbType.DateTime };

            DataTable dt = new DataTable();

            dt = ExecuteQueryStoredProcedure(spRates, "getExchangeRatesOnDate", pDateRate);
            return dt;
        }

        public static DataTable CheckManagerIsMoscou(string managerSid)
        {
            SqlParameter pUserSid = new SqlParameter() { ParameterName = "user_sid", Value = managerSid, DbType = DbType.AnsiString };

            DataTable dt = new DataTable();

            dt = ExecuteQueryStoredProcedure(spUsers, "checkUserIsMoscou", pUserSid);
            return dt;
        }
    }
}