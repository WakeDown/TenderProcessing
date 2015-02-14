using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessingDataAccessLayer
{

    public class DbEngine
    {
        private readonly string _connectionString;

        public DbEngine()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["TenderProccessing"].ConnectionString;
        }

        #region TenderClaim

        public bool SaveTenderClaim(TenderClaim model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SaveTenderClaim";
                if (!string.IsNullOrEmpty(model.TenderNumber))
                    cmd.Parameters.AddWithValue("@tenderNumber", model.TenderNumber);
                cmd.Parameters.AddWithValue("@tenderStart", model.TenderStart);
                cmd.Parameters.AddWithValue("@claimDeadline", model.ClaimDeadline);
                cmd.Parameters.AddWithValue("@kPDeadline", model.KPDeadline);
                if (!string.IsNullOrEmpty(model.Comment))
                    cmd.Parameters.AddWithValue("@comment", model.Comment);
                cmd.Parameters.AddWithValue("@customer", model.Customer);
                cmd.Parameters.AddWithValue("@customerInn", model.CustomerInn);
                if (!model.Sum.Equals(0)) cmd.Parameters.AddWithValue("@totalSum", model.Sum);
                cmd.Parameters.AddWithValue("@dealType", model.DealType);
                if (!string.IsNullOrEmpty(model.TenderUrl))
                    cmd.Parameters.AddWithValue("@tenderUrl", model.TenderUrl);
                cmd.Parameters.AddWithValue("@tenderStatus", model.TenderStatus);
                cmd.Parameters.AddWithValue("@manager", model.Manager.Name);
                cmd.Parameters.AddWithValue("@managerSubDivision", model.Manager.SubDivision);
                cmd.Parameters.AddWithValue("@claimStatus", model.ClaimStatus);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    var id = rd.GetInt32(0);
                    if (id != -1)
                    {
                        result = true;
                        model.Id = id;
                    }
                }
                rd.Dispose();
            }
            return result;
        }

        public bool ChangeTenderClaimClaimStatus(TenderClaim model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ChangeTenderClaimClaimStatus";
                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@claimStatus", model.ClaimStatus);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        #endregion

        #region SpecificationPosition

        public bool SaveSpecificationPosition(SpecificationPosition model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SaveClaimPosition";
                if (!string.IsNullOrEmpty(model.Replace))
                    cmd.Parameters.AddWithValue("@replaceValue", model.Replace);
                if (!string.IsNullOrEmpty(model.CatalogNumber))
                    cmd.Parameters.AddWithValue("@catalogNumber", model.CatalogNumber);
                if (!model.Price.Equals(0)) cmd.Parameters.AddWithValue("@price", model.Price);
                if (!model.Sum.Equals(0)) cmd.Parameters.AddWithValue("@sumMax", model.Sum);
                if (model.RowNumber != 0) cmd.Parameters.AddWithValue("@rowNumber", model.RowNumber);
                cmd.Parameters.AddWithValue("@idClaim", model.IdClaim);
                cmd.Parameters.AddWithValue("@name", model.Name);
                cmd.Parameters.AddWithValue("@unit", model.Unit);
                cmd.Parameters.AddWithValue("@value", model.Value);
                cmd.Parameters.AddWithValue("@productManager", model.ProductManager);
                cmd.Parameters.AddWithValue("@comment", model.Comment);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    var id = rd.GetInt32(0);
                    if (id != -1)
                    {
                        result = true;
                        model.Id = id;
                    }
                }
                rd.Dispose();
            }
            return result;
        }

        public bool UpdateSpecificationPosition(SpecificationPosition model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UpdateClaimPosition";
                if (!string.IsNullOrEmpty(model.Replace))
                    cmd.Parameters.AddWithValue("@replaceValue", model.Replace);
                if (!string.IsNullOrEmpty(model.CatalogNumber))
                    cmd.Parameters.AddWithValue("@catalogNumber", model.CatalogNumber);
                if (!model.Price.Equals(0)) cmd.Parameters.AddWithValue("@price", model.Price);
                if (!model.Sum.Equals(0)) cmd.Parameters.AddWithValue("@sumMax", model.Sum);
                if (model.RowNumber != 0) cmd.Parameters.AddWithValue("@rowNumber", model.RowNumber);
                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@name", model.Name);
                cmd.Parameters.AddWithValue("@unit", model.Unit);
                cmd.Parameters.AddWithValue("@value", model.Value);
                cmd.Parameters.AddWithValue("@productManager", model.ProductManager);
                cmd.Parameters.AddWithValue("@comment", model.Comment);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public bool ExistsSpecificationPosition(SpecificationPosition model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ExistsClaimPosition";
                if (!string.IsNullOrEmpty(model.Replace))
                    cmd.Parameters.AddWithValue("@replaceValue", model.Replace);
                if (!string.IsNullOrEmpty(model.CatalogNumber))
                    cmd.Parameters.AddWithValue("@catalogNumber", model.CatalogNumber);
                if (!model.Price.Equals(0)) cmd.Parameters.AddWithValue("@price", model.Price);
                if (!model.Sum.Equals(0)) cmd.Parameters.AddWithValue("@sumMax", model.Sum);
                if (model.RowNumber != 0) cmd.Parameters.AddWithValue("@rowNumber", model.RowNumber);
                cmd.Parameters.AddWithValue("@idClaim", model.IdClaim);
                cmd.Parameters.AddWithValue("@name", model.Name);
                cmd.Parameters.AddWithValue("@unit", model.Unit);
                cmd.Parameters.AddWithValue("@value", model.Value);
                cmd.Parameters.AddWithValue("@productManager", model.ProductManager);
                cmd.Parameters.AddWithValue("@comment", model.Comment);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    result = rd.GetInt32(0) == 0;
                }
                rd.Dispose();
            }
            return result;
        }

        public bool DeleteSpecificationPosition(int id)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DeleteClaimPosition";
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public bool HasClaimPosition(int id)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "HasClaimPosition";
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                result = (int) cmd.ExecuteScalar() > 0;
            }
            return result;
        }

        #endregion

        #region Справочники

        public List<DealType> LoadDealTypes()
        {
            var list = new List<DealType>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadDealTypes";
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new DealType()
                        {
                            Id = rd.GetInt32(0),
                            Value = rd.GetString(1)
                        };
                        list.Add(model);
                    }
                }
                rd.Dispose();
            }
            return list;
        }

        public List<ClaimStatus> LoadClaimStatus()
        {
            var list = new List<ClaimStatus>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadClaimStatus";
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new ClaimStatus()
                        {
                            Id = rd.GetInt32(0),
                            Value = rd.GetString(1)
                        };
                        list.Add(model);
                    }
                }
                rd.Dispose();
            }
            return list;
        }

        #endregion

    }
}
