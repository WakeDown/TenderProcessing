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
    //Класс - статус заявки
    public class ClaimStatus : ServerDirectBase
    {
        public ClaimStatus()
        {
            
        }
        //public ClaimStatus(int id)
        //{
        //    SqlParameter pId = new SqlParameter() { ParameterName = "id", SqlValue = id, SqlDbType = SqlDbType.Int };
        //    var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("", pId);
        //    if (dt.Rows.Count > 0)
        //    {
        //        var row = dt.Rows[0];
        //        FillSelf(row);
        //    }
        //}

        public ClaimStatus(string sysName)
        {
            SqlParameter psysName = new SqlParameter() { ParameterName = "sys_name", SqlValue = sysName, SqlDbType = SqlDbType.NVarChar };
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("GetClaimState", psysName);
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                FillSelf(row);
            }
        }

        public void FillSelf(DataRow row)
        {
            Id = Db.DbHelper.GetValueIntOrDefault(row, "Id");
            Value = Db.DbHelper.GetValueString(row, "Value");
        }
    }
}
