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
    public class User
    {
        public static string GetEmailBySid(string sid)
        {
            if (String.IsNullOrEmpty(sid)) return String.Empty;
            SqlParameter pSid = new SqlParameter() { ParameterName = "sid", SqlValue = sid, SqlDbType = SqlDbType.VarChar };
            var dt = Db.Stuff.ExecuteQueryStoredProcedure("get_email", pSid);
            string email = String.Empty;
            if (dt.Rows.Count > 0)
            {
                email = Db.DbHelper.GetValueString(dt.Rows[0], "email");
            }
            return email;
        }
    }
}
