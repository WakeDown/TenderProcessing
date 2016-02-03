using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DataProvider.Helpers;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Models;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer
{

    public class DbEngine
    {
        private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["SpeCalcConnectionString"].ConnectionString;

        public DbEngine()
        {
        }

        public static DataTable GetCalcPositionsChanges(int idCalcPosition)
        {
            DataTable dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetCalcPositionsChanges";
                cmd.Parameters.AddWithValue("@idCalcPosition", idCalcPosition);
                conn.Open();
                dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }

        public static int CopyPositionsForNewVersion(int idClaim, int calcVersion, string creatorSid, int[] selIds)
        {
            int result = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CopyPositionsForNewVersion";
                cmd.Parameters.AddWithValue("@idClaim", idClaim);
                cmd.Parameters.AddWithValue("@calcVersion", calcVersion);
                cmd.Parameters.AddWithValue("@creatorSid", creatorSid);
                cmd.Parameters.AddWithValue("@ids", String.Join(",", selIds));
                conn.Open();
                var rd = cmd.ExecuteReader();

                if (rd.HasRows)
                {
                    rd.Read();
                    result = rd.GetInt32(0);
                }
            }
            return result;
        }
        public static int[] GetCalcVersionList(int idClaim)
        {
            var result = new List<int>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetCalcVersionList";
                cmd.Parameters.AddWithValue("@idClaim", idClaim);
                conn.Open();
                var rd = cmd.ExecuteReader();

                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        result.Add(rd.GetInt32(0));
                    }
                }
                rd.Dispose();
            }

            return result.ToArray();
        }

        #region TenderClaim
        public void UpdateClaimDeadline(int idClaim, DateTime claimDeadline)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UpdateClaimDeadline";
                cmd.Parameters.AddWithValue("@IdClaim", idClaim);
                cmd.Parameters.AddWithValue("@ClaimDeadline", claimDeadline);
                conn.Open();
                var rd = cmd.ExecuteReader();
                rd.Dispose();
            }
        }
        public ClaimCert GetCertFile(string guid)
        {
            byte[] file = null;
            string name = String.Empty;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetCertFile";
                cmd.Parameters.AddWithValue("@guid", guid);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    file = (byte[]) rd["fileDATA"];
                    name = rd["fileName"].ToString();
                }
                rd.Dispose();
            }
            return new ClaimCert() { File = file, FileName = name };
        }
        public TenderClaimFile GetTenderClaimFile(string guid)
        {
            byte[] file = null;
            string name = String.Empty;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetTenderClaimFile";
                cmd.Parameters.AddWithValue("@guid", guid);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    file = (byte[])rd["fileDATA"];
                    name = rd["fileName"].ToString();
                }
                rd.Dispose();
            }
            return new TenderClaimFile() { File = file, FileName = name };
        }
        public List<ClaimCert> LoadClaimCerts(int idClaim)
        {
            var list = new List<ClaimCert>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadClaimCertList";
                cmd.Parameters.AddWithValue("@IdClaim", idClaim);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        list.Add(new ClaimCert()
                        {
                            Id = rd.GetInt32(0),
                            FileUrl = rd.GetString(1),
                            FileName = rd.GetString(2)
                            ,
                            FileGuid = rd["fileGUID"].ToString()
                        });
                    }

                }
                rd.Dispose();
            }
            return list;
        }
        public List<TenderClaimFile> LoadTenderClaimFiles(int idClaim)
        {
            var list = new List<TenderClaimFile>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadTenderClaimFiles";
                cmd.Parameters.AddWithValue("@IdClaim", idClaim);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        list.Add(new TenderClaimFile()
                        {
                            Id = rd.GetInt32(0),
                            FileUrl = rd.GetString(1),
                            FileName = rd.GetString(2),
                            FileGuid = rd["fileGUID"].ToString()
                        });
                    }

                }
                rd.Dispose();
            }
            return list;
        }
        public bool SaveTenderClaimFile(ref TenderClaimFile model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SaveTenderClaimFile";
                cmd.Parameters.AddWithValue("@IdClaim", model.IdClaim);
                cmd.Parameters.AddWithValue("@file", model.File);
                cmd.Parameters.AddWithValue("@fileName", model.FileName);
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
        public bool SaveClaimCertFile(ref ClaimCert model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SaveClaimCertFile";
                cmd.Parameters.AddWithValue("@IdClaim", model.IdClaim);
                cmd.Parameters.AddWithValue("@file", model.File);
                cmd.Parameters.AddWithValue("@fileName", model.FileName);
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

        public bool SaveTenderClaim(ref TenderClaim model)
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
                if (!string.IsNullOrEmpty(model.CustomerInn))
                cmd.Parameters.AddWithValue("@customerInn", model.CustomerInn);
                if (model.Sum >= 0) cmd.Parameters.AddWithValue("@totalSum", model.Sum);
                cmd.Parameters.AddWithValue("@dealType", model.DealType);
                if (!string.IsNullOrEmpty(model.TenderUrl))
                    cmd.Parameters.AddWithValue("@tenderUrl", model.TenderUrl);
                if (!model.CurrencyUsd.Equals(0)) cmd.Parameters.AddWithValue("@currencyUsd", model.CurrencyUsd);
                if (!model.CurrencyEur.Equals(0)) cmd.Parameters.AddWithValue("@currencyEur", model.CurrencyEur);
                if (!string.IsNullOrEmpty(model.DeliveryPlace))
                    cmd.Parameters.AddWithValue("@deliveryPlace", model.DeliveryPlace);
                if (model.DeliveryDate.HasValue) cmd.Parameters.AddWithValue("@deliveryDate", model.DeliveryDate.Value);
                if (model.DeliveryDateEnd.HasValue) cmd.Parameters.AddWithValue("@deliveryDateEnd", model.DeliveryDateEnd.Value);
                if (model.AuctionDate.HasValue) cmd.Parameters.AddWithValue("@auctionDate", model.AuctionDate.Value);
                cmd.Parameters.AddWithValue("@tenderStatus", model.TenderStatus);
                cmd.Parameters.AddWithValue("@manager", model.Manager.Id);
                cmd.Parameters.AddWithValue("@managerSubDivision", model.Manager.SubDivision);
                cmd.Parameters.AddWithValue("@claimStatus", model.ClaimStatus);
                cmd.Parameters.AddWithValue("@recordDate", model.RecordDate);
                cmd.Parameters.AddWithValue("@deleted", model.Deleted);
                cmd.Parameters.AddWithValue("@author", model.Author.Sid);
                if (model.SumCurrency > 0)
                cmd.Parameters.AddWithValue("@idSumCurrency", model.SumCurrency);
                cmd.Parameters.AddWithValue("@ClaimTypeId", model.IdClaimType);
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

        public static bool ChangeTenderClaimClaimStatus(TenderClaim model)
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

        public static bool ChangeTenderClaimTenderStatus(int idClaim, int status)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ChangeTenderClaimTenderStatus";
                cmd.Parameters.AddWithValue("@id", idClaim);
                cmd.Parameters.AddWithValue("@status", status);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public TenderClaim LoadTenderClaimById(int id)
        {
            TenderClaim model = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadTenderClaimById";
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    model = new TenderClaim()
                    {
                        Id = rd.GetInt32(0),
                        TenderNumber = rd.GetString(1),
                        TenderStart = rd.GetDateTime(2),
                        ClaimDeadline = rd.GetDateTime(3),
                        KPDeadline = rd.GetDateTime(4),
                        Comment = rd.GetString(5),
                        Customer = rd.GetString(6),
                        CustomerInn = rd[7] == DBNull.Value ? String.Empty : rd.GetString(7),
                        Sum = (double) rd.GetDecimal(8),
                        DealType = rd.GetInt32(9),
                        TenderUrl = rd.GetString(10),
                        TenderStatus = rd.GetInt32(11),
                        Manager = new Manager()
                        {
                            Id = rd.GetString(12),
                            SubDivision = rd.GetString(13)
                        },
                        ClaimStatus = rd.GetInt32(14),
                        RecordDate = rd.GetDateTime(15),
                        Author = new AdUser() {Sid = rd.GetString(17)},
                        CurrencyUsd = (double) rd.GetDecimal(20),
                        CurrencyEur = (double) rd.GetDecimal(21),
                        DeliveryDate = rd[22] == DBNull.Value ? null : (DateTime?) rd.GetDateTime(22),
                        DeliveryPlace = rd.GetString(23),
                        AuctionDate = rd[24] == DBNull.Value ? null : (DateTime?) rd.GetDateTime(24),
                        ProductManagers = new List<ProductManager>()
                        ,
                        SumCurrency = rd[25] == DBNull.Value ? 1 : rd.GetInt32(25),
                        DeliveryDateEnd = rd[26] == DBNull.Value ? null : (DateTime?)rd.GetDateTime(26)
                    };
                    if (model.Sum.Equals(-1)) model.Sum = 0;
                    if (model.CurrencyUsd.Equals(-1)) model.CurrencyUsd = 0;
                    if (model.CurrencyEur.Equals(-1)) model.CurrencyEur = 0;
                    //model.KPDeadlineString = model.KPDeadline.ToString("dd.MM.yyyy");
                    //model.TenderStartString = model.TenderStart.ToString("dd.MM.yyyy");
                    //model.ClaimDeadlineString = model.ClaimDeadline.ToString("dd.MM.yyyy");
                    //model.RecordDateString = model.RecordDate.ToString("dd.MM.yyyy HH:mm");
                    //model.DeliveryDateString = model.DeliveryDate.HasValue
                    //    ? model.DeliveryDate.Value.ToString("dd.MM.yyyy")
                    //    : string.Empty;
                    //model.DeliveryDateEndString = model.DeliveryDateEnd.HasValue
                    //    ? model.DeliveryDateEnd.Value.ToString("dd.MM.yyyy")
                    //    : string.Empty;
                    //model.AuctionDateString = model.AuctionDate.HasValue
                    //    ? model.AuctionDate.Value.ToString("dd.MM.yyyy")
                    //    : string.Empty;
                    

                    //model.StrSum = String.Format("{0:### ### ### ### ###.##}", model.Sum);

                    //switch (model.SumCurrency)
                    //{
                    //    case 1:
                    //        model.StrSum += " руб.";
                    //        break;
                    //    case 2:
                    //        model.StrSum += " USD";
                    //        break;
                    //    case 3:
                    //        model.StrSum += " EUR";
                    //        break;
                    //}
                }
                rd.Dispose();
            }
            return model;
        }

        public bool HasTenderClaimTransmissedPosition(int id, int version)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "HasClaimTranmissedPosition";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@version", version);
                conn.Open();
                result = (int)cmd.ExecuteScalar() > 0;
            }
            return result;
        }

        public bool DeleteTenderClaim(int id, AdUser user)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DeleteTenderClaims";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@deletedUser", user.Sid);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public List<TenderClaim> FilterTenderClaims(FilterTenderClaim filter)
        {
            var list = new List<TenderClaim>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FilterTenderClaims";
                cmd.Parameters.AddWithValue("@rowCount", filter.RowCount);
                if (filter.IdClaim != 0) cmd.Parameters.AddWithValue("@idClaim", filter.IdClaim);
                if (!string.IsNullOrEmpty(filter.TenderNumber)) cmd.Parameters.AddWithValue("@tenderNumber", filter.TenderNumber);
                if (!string.IsNullOrEmpty(filter.Customer)) cmd.Parameters.AddWithValue("@customer", filter.Customer);
                if (filter.ClaimStatus != null && filter.ClaimStatus.Any()) cmd.Parameters.AddWithValue("@claimStatusIds", string.Join(",", filter.ClaimStatus));
                if (filter.DealTypeId != 0) cmd.Parameters.AddWithValue("@dealTypeId", filter.DealTypeId);
                if (!string.IsNullOrEmpty(filter.IdManager)) cmd.Parameters.AddWithValue("@manager", filter.IdManager);
                if (!string.IsNullOrEmpty(filter.ManagerSubDivision)) cmd.Parameters.AddWithValue("@managerSubDivision", filter.ManagerSubDivision);
                if (filter.Overdie.HasValue) cmd.Parameters.AddWithValue("@overdie", filter.Overdie);
                if (!string.IsNullOrEmpty(filter.IdProductManager)) cmd.Parameters.AddWithValue("@idProductManager", filter.IdProductManager);
                if (!string.IsNullOrEmpty(filter.Author)) cmd.Parameters.AddWithValue("@author", filter.Author);
                if (!string.IsNullOrEmpty(filter.TenderStartFrom)) cmd.Parameters.AddWithValue("@tenderStartFrom", DateTime.ParseExact(filter.TenderStartFrom, "dd.MM.yyyy", CultureInfo.CurrentCulture));
                if (!string.IsNullOrEmpty(filter.TenderStartTo)) cmd.Parameters.AddWithValue("@tenderStartTo", DateTime.ParseExact(filter.TenderStartTo, "dd.MM.yyyy", CultureInfo.CurrentCulture));
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new TenderClaim()
                        {
                            Id = rd.GetInt32(0),
                            TenderNumber = rd.GetString(1),
                            TenderStart = rd.GetDateTime(2),
                            ClaimDeadline = rd.GetDateTime(3),
                            KPDeadline = rd.GetDateTime(4),
                            Comment = rd.GetString(5),
                            Customer = rd.GetString(6),
                            
                            Sum = (double) rd.GetDecimal(8),
                            DealType = rd.GetInt32(9),
                            TenderUrl = rd.GetString(10),
                            TenderStatus = rd.GetInt32(11),
                            Manager = new Manager()
                            {
                                Id = rd.GetString(12),
                                SubDivision = rd.GetString(13)
                            },
                            ClaimStatus = rd.GetInt32(14),
                            RecordDate = rd.GetDateTime(15),
                            Author = new AdUser() {Sid = rd.GetString(17)},
                            CurrencyUsd = (double)rd.GetDecimal(20),
                            CurrencyEur = (double)rd.GetDecimal(21),
                            DeliveryDate = rd[22] == DBNull.Value ? null : (DateTime?)rd.GetDateTime(22),
                            DeliveryPlace = rd.GetString(23),
                            AuctionDate = rd[24] == DBNull.Value ? null : (DateTime?)rd.GetDateTime(24),
                            ProductManagers = new List<ProductManager>(),
                            //StrSum = String.Format("{0:### ### ### ### ###.##}", (double)rd.GetDecimal(8))
                        };
                        if (!rd.IsDBNull(7))
                        {
                            model.CustomerInn = rd.GetString(7);
                        }
                        else
                        {
                            model.CustomerInn = String.Empty;}
                        if (model.Sum.Equals(-1)) model.Sum = 0;
                        if (model.CurrencyUsd.Equals(-1)) model.CurrencyUsd = 0;
                        if (model.CurrencyEur.Equals(-1)) model.CurrencyEur = 0;
                        //model.KPDeadlineString = model.KPDeadline.ToString("dd.MM.yyyy");
                        //model.TenderStartString = model.TenderStart.ToString("dd.MM.yyyy");
                        //model.ClaimDeadlineString = model.ClaimDeadline.ToString("dd.MM.yyyy");
                        //model.RecordDateString = model.RecordDate.ToString("dd.MM.yyyy HH:mm");
                        //model.DeliveryDateString = model.DeliveryDate.HasValue
                        //? model.DeliveryDate.Value.ToString("dd.MM.yyyy")
                        //: string.Empty;
                        //model.AuctionDateString = model.AuctionDate.HasValue
                        //    ? model.AuctionDate.Value.ToString("dd.MM.yyyy")
                        //    : string.Empty;
                        list.Add(model);
                    }
                }
                rd.Dispose();
            }
            return list;
        }

        public int GetCountFilteredTenderClaims(FilterTenderClaim filter)
        {
            var count = -1;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FilterTenderClaimsCount";
                if (filter.IdClaim != 0) cmd.Parameters.AddWithValue("@idClaim", filter.IdClaim);
                if (!string.IsNullOrEmpty(filter.TenderNumber)) cmd.Parameters.AddWithValue("@tenderNumber", filter.TenderNumber);
                if (!string.IsNullOrEmpty(filter.Customer)) cmd.Parameters.AddWithValue("@customer", filter.Customer);
                if (filter.ClaimStatus != null && filter.ClaimStatus.Any()) cmd.Parameters.AddWithValue("@claimStatusIds", string.Join(",", filter.ClaimStatus));
                if (filter.DealTypeId != 0) cmd.Parameters.AddWithValue("@dealTypeId", filter.DealTypeId);
                if (!string.IsNullOrEmpty(filter.IdManager)) cmd.Parameters.AddWithValue("@manager", filter.IdManager);
                if (!string.IsNullOrEmpty(filter.ManagerSubDivision)) cmd.Parameters.AddWithValue("@managerSubDivision", filter.ManagerSubDivision);
                if (filter.Overdie.HasValue) cmd.Parameters.AddWithValue("@overdie", filter.Overdie);
                if (!string.IsNullOrEmpty(filter.IdProductManager)) cmd.Parameters.AddWithValue("@idProductManager", filter.IdProductManager);
                if (!string.IsNullOrEmpty(filter.Author)) cmd.Parameters.AddWithValue("@author", filter.Author);
                if (!string.IsNullOrEmpty(filter.TenderStartFrom)) cmd.Parameters.AddWithValue("@tenderStartFrom", DateTime.ParseExact(filter.TenderStartFrom, "dd.MM.yyyy", CultureInfo.CurrentCulture));
                if (!string.IsNullOrEmpty(filter.TenderStartTo)) cmd.Parameters.AddWithValue("@tenderStartTo", DateTime.ParseExact(filter.TenderStartTo, "dd.MM.yyyy", CultureInfo.CurrentCulture));
                
                conn.Open();
                count = (int)cmd.ExecuteScalar();
            }
            return count;
        }

        public void SetProductManagersForClaims(List<TenderClaim> claims)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetProductsForClaims";
                cmd.Parameters.AddWithValue("@ids", string.Join(",", claims.Select(x => x.Id)));
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var idClaim = rd.GetInt32(0);
                        var model = new ProductManager() {Id = rd.GetString(1)};
                        claims.First(x => x.Id == idClaim).ProductManagers.Add(model);
                    }
                }
                rd.Dispose();
            }
        }

        public void SetStatisticsForClaims(List<TenderClaim> claims)
        {
            claims.SelectMany(x=>x.ProductManagers).ToList().ForEach(x =>
            {
                x.CalculatePositionsCount = 0;
                x.CalculatesCount = 0;
                x.PositionsCount = 0;
            });
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetClaimsPositionsStatistics";
                cmd.Parameters.AddWithValue("@ids", string.Join(",", claims.Select(x => x.Id)));
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var idClaim = rd.GetInt32(0);
                        var product = rd.GetString(1);
                        var count = rd.GetInt32(2);
                        var claim = claims.FirstOrDefault(x => x.Id == idClaim);
                        if (claim != null)
                        {
                            var productManager = claim.ProductManagers.FirstOrDefault(x => x.Id == product);
                            if (productManager != null)
                            {
                                productManager.PositionsCount = count;
                            }
                        }
                    }
                }
                rd.Dispose();
                cmd.CommandText = "GetClaimsCalculatePositionsStatistics";
                rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var idClaim = rd.GetInt32(0);
                        var product = rd.GetString(1);
                        var count = rd.GetInt32(3);
                        var claim = claims.FirstOrDefault(x => x.Id == idClaim);
                        if (claim != null)
                        {
                            var productManager = claim.ProductManagers.FirstOrDefault(x => x.Id == product);
                            if (productManager != null)
                            {
                                productManager.CalculatesCount += count;
                                productManager.CalculatePositionsCount++;
                            }
                        }
                    }
                }
                rd.Dispose();
                claims.ForEach(x =>
                {
                    if (x.ProductManagers.Any())
                    {
                        x.PositionsCount = x.ProductManagers.Sum(y => y.PositionsCount);
                        x.CalculatesCount = x.ProductManagers.Sum(y => y.CalculatesCount);
                        x.CalculatePositionsCount = x.ProductManagers.Sum(y => y.CalculatePositionsCount);
                    }
                });
            }   
        }

        public bool UpdateClaimCurrency(TenderClaim model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UpdateTenderClaimCurrency";
                cmd.Parameters.AddWithValue("@id", model.Id);
                if (!model.CurrencyUsd.Equals(0)) cmd.Parameters.AddWithValue("@currencyUsd", model.CurrencyUsd);
                if (!model.CurrencyEur.Equals(0)) cmd.Parameters.AddWithValue("@currencyEur", model.CurrencyEur);
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
                if (!model.PriceTzr.Equals(0)) cmd.Parameters.AddWithValue("@priceTzr", model.PriceTzr);
                if (!model.SumTzr.Equals(0)) cmd.Parameters.AddWithValue("@sumTzr", model.SumTzr);
                if (!model.PriceNds.Equals(0)) cmd.Parameters.AddWithValue("@priceNds", model.PriceNds);
                if (!model.SumNds.Equals(0)) cmd.Parameters.AddWithValue("@sumNds", model.SumNds);
                if (model.RowNumber != 0) cmd.Parameters.AddWithValue("@rowNumber", model.RowNumber);
                cmd.Parameters.AddWithValue("@idClaim", model.IdClaim);
                cmd.Parameters.AddWithValue("@name", model.Name);
                cmd.Parameters.AddWithValue("@unit", model.Unit);
                cmd.Parameters.AddWithValue("@value", model.Value);
                cmd.Parameters.AddWithValue("@productManager", model.ProductManagerId);
                cmd.Parameters.AddWithValue("@comment", model.Comment);
                cmd.Parameters.AddWithValue("@positionState", model.State);
                cmd.Parameters.AddWithValue("@author", model.Author);
                cmd.Parameters.AddWithValue("@currency", model.Currency);
                cmd.Parameters.AddWithValue("@version", model.Version);
                cmd.Parameters.AddWithValue("@ContractDeliveryTime", model.ContractDeliveryTime);
                cmd.Parameters.AddWithValue("@Brand", model.Brand);
                cmd.Parameters.AddWithValue("@RecipientDetails", model.RecipientDetails);
                cmd.Parameters.AddWithValue("@QuestionnaireNum", model.QuestionnaireNum);
                cmd.Parameters.AddWithValue("@MaxPrice", model.MaxPrice);
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
                if (!model.PriceTzr.Equals(0)) cmd.Parameters.AddWithValue("@priceTzr", model.PriceTzr);
                if (!model.SumTzr.Equals(0)) cmd.Parameters.AddWithValue("@sumTzr", model.SumTzr);
                if (!model.PriceNds.Equals(0)) cmd.Parameters.AddWithValue("@priceNds", model.PriceNds);
                if (!model.SumNds.Equals(0)) cmd.Parameters.AddWithValue("@sumNds", model.SumNds);
                if (model.RowNumber != 0) cmd.Parameters.AddWithValue("@rowNumber", model.RowNumber);
                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@name", model.Name);
                cmd.Parameters.AddWithValue("@unit", model.Unit);
                cmd.Parameters.AddWithValue("@value", model.Value);
                cmd.Parameters.AddWithValue("@productManager", model.ProductManagerId);
                cmd.Parameters.AddWithValue("@comment", model.Comment);
                cmd.Parameters.AddWithValue("@positionState", model.State);
                cmd.Parameters.AddWithValue("@author", model.Author);
                cmd.Parameters.AddWithValue("@currency", model.Currency);
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
                if (!model.PriceTzr.Equals(0)) cmd.Parameters.AddWithValue("@priceTzr", model.PriceTzr);
                if (!model.SumTzr.Equals(0)) cmd.Parameters.AddWithValue("@sumTzr", model.SumTzr);
                if (!model.PriceNds.Equals(0)) cmd.Parameters.AddWithValue("@priceNds", model.PriceNds);
                if (!model.SumNds.Equals(0)) cmd.Parameters.AddWithValue("@sumNds", model.SumNds);
                if (model.RowNumber != 0) cmd.Parameters.AddWithValue("@rowNumber", model.RowNumber);
                cmd.Parameters.AddWithValue("@idClaim", model.IdClaim);
                cmd.Parameters.AddWithValue("@name", model.Name);
                cmd.Parameters.AddWithValue("@unit", model.Unit);
                cmd.Parameters.AddWithValue("@value", model.Value);
                cmd.Parameters.AddWithValue("@productManager", model.ProductManager.Id);
                cmd.Parameters.AddWithValue("@comment", model.Comment);
                cmd.Parameters.AddWithValue("@positionState", model.State);
                cmd.Parameters.AddWithValue("@currency", model.Currency);
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

        public bool DeleteSpecificationPosition(int id, AdUser user)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DeleteClaimPosition";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@deletedUser", user.Sid);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public List<SpecificationPosition> LoadSpecificationPositionsForTenderClaim(int claimId, int version)
        {
            return SpecificationPosition.GetList(idClaim: claimId, version: version).ToList();

            var list = new List<SpecificationPosition>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadClaimPositionForTenderClaim";
                cmd.Parameters.AddWithValue("@id", claimId);
                cmd.Parameters.AddWithValue("@calcVersion", version); 
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new SpecificationPosition()
                        {
                            Id = rd.GetInt32(0),
                            RowNumber = rd.GetInt32(2),
                            CatalogNumber = rd.GetString(3),
                            Name = rd.GetString(4),
                            Replace = rd.GetString(5),
                            //TODO: исправить Unit
                            //Unit = (PositionUnit)Enum.Parse(typeof(PositionUnit), rd.GetString(6)),

                            Value = rd.GetInt32(7),
                            ProductManager = new ProductManager() {Id = rd.GetString(8)},
                            Comment = rd.GetString(9),
                            Price = (double)rd.GetDecimal(10),
                            Sum = (double)rd.GetDecimal(11),
                            State = rd.GetInt32(12),
                            Author = rd.GetString(13),
                            Currency = rd.GetInt32(17),
                            PriceTzr = (double)rd.GetDecimal(18),
                            SumTzr = (double)rd.GetDecimal(19),
                            PriceNds = (double)rd.GetDecimal(20),
                            SumNds = (double)rd.GetDecimal(21),
                        };
                        if (model.Sum.Equals(-1)) model.Sum = 0;
                        if (model.Price.Equals(-1)) model.Price = 0;
                        if (model.PriceTzr.Equals(-1)) model.PriceTzr = 0;
                        if (model.SumTzr.Equals(-1)) model.SumTzr = 0;
                        if (model.PriceNds.Equals(-1)) model.PriceNds = 0;
                        if (model.SumNds.Equals(-1)) model.SumNds = 0;
                        if (model.RowNumber == -1) model.RowNumber = 0;
                        list.Add(model);
                    }
                }
                rd.Dispose();
            }
            return list;
        }

        public List<SpecificationPosition> LoadSpecificationPositionsForTenderClaimForProduct(int claimId, string product, int version)
        {
            var list = new List<SpecificationPosition>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadClaimPositionForTenderClaimForProduct";
                cmd.Parameters.AddWithValue("@id", claimId);
                cmd.Parameters.AddWithValue("@product", product);
                cmd.Parameters.AddWithValue("@calcVersion", version);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new SpecificationPosition()
                        {
                            Id = rd.GetInt32(0),
                            RowNumber = rd.GetInt32(2),
                            CatalogNumber = rd.GetString(3),
                            Name = rd.GetString(4),
                            Replace = rd.GetString(5),
                            //TODO: Исправить Unit
                            //Unit = (PositionUnit)Enum.Parse(typeof(PositionUnit), rd.GetString(6)),

                            Value = rd.GetInt32(7),
                            ProductManager = new ProductManager() {Id = rd.GetString(8)},
                            Comment = rd.GetString(9),
                            Price = (double)rd.GetDecimal(10),
                            Sum = (double)rd.GetDecimal(11),
                            State = rd.GetInt32(12),
                            Author = rd.GetString(13),
                            Currency = rd.GetInt32(17),
                            PriceTzr = (double)rd.GetDecimal(18),
                            SumTzr = (double)rd.GetDecimal(19),
                            PriceNds = (double)rd.GetDecimal(20),
                            SumNds = (double)rd.GetDecimal(21),
                        };
                        if (model.Sum.Equals(-1)) model.Sum = 0;
                        if (model.Price.Equals(-1)) model.Price = 0;
                        if (model.PriceTzr.Equals(-1)) model.PriceTzr = 0;
                        if (model.SumTzr.Equals(-1)) model.SumTzr = 0;
                        if (model.PriceNds.Equals(-1)) model.PriceNds = 0;
                        if (model.SumNds.Equals(-1)) model.SumNds = 0;
                        if (model.RowNumber == -1) model.RowNumber = 0;
                        list.Add(model);
                    }
                }
                rd.Dispose();
            }
            return list;
        }

        public bool ChangePositionsState(List<int> ids, int state)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ChangePositionsState";
                cmd.Parameters.AddWithValue("@ids", string.Join(",", ids));
                cmd.Parameters.AddWithValue("@state", state);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public bool ChangePositionsProduct(List<int> ids, string productId)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ChangePositionsProduct";
                cmd.Parameters.AddWithValue("@ids", string.Join(",", ids));
                cmd.Parameters.AddWithValue("@product", productId);
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

        public bool IsPositionsReadyToConfirm(List<SpecificationPosition> positions)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "IsPositionsReadyToConfirm";
                cmd.Parameters.AddWithValue("@ids", string.Join(",", positions.Select(x => x.Id)));
                conn.Open();
                var list = new List<PositionCalculateCount>();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new PositionCalculateCount()
                        {
                            Id = rd.GetInt32(0),
                            Count = rd.GetInt32(1)
                        };
                        if (model.Count == 0)
                        {
                            break;
                        }
                        list.Add(model);
                    }
                    if (list.Count() == positions.Count())
                    {
                        var isNullCount = list.Any(x => x.Count == 0);
                        if (!isNullCount)
                        {
                            result = true;
                        }
                    }
                }
                rd.Dispose();
            }
            return result;
        }

        public bool SetPositionsToConfirm(List<SpecificationPosition> positions)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SetPositionsToConfirm";
                cmd.Parameters.AddWithValue("@ids", string.Join(",", positions.Select(x => x.Id)));
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public List<ProductManager> LoadProductManagersForClaim(int claimId, int version, int[] selIds = null, bool? getActualize = null)
        {
            if (version <= 0)version = 1;
            var list = new List<ProductManager>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadProductManagersForClaim";
                cmd.Parameters.AddWithValue("@idClaim", claimId);
                cmd.Parameters.AddWithValue("@version", version);
                cmd.Parameters.AddWithValue("@selIds", selIds != null ? String.Join(",",selIds) : null);
                cmd.Parameters.AddWithValue("@getActualize", getActualize);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new ProductManager()
                        {
                            Id = rd.GetString(0)
                        };
                        list.Add(model);
                    }
                }
                rd.Dispose();
            }
            return list;
        }

        #endregion

        #region CalculateSpecificationPosition

        public bool SaveCalculateSpecificationPosition(CalculateSpecificationPosition model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SaveCalculateClaimPosition";
                if (!string.IsNullOrEmpty(model.Replace))
                    cmd.Parameters.AddWithValue("@replaceValue", model.Replace);
                if (!string.IsNullOrEmpty(model.Comment))
                    cmd.Parameters.AddWithValue("@comment", model.Comment);
                if (!string.IsNullOrEmpty(model.ProtectCondition))
                    cmd.Parameters.AddWithValue("@protectCondition", model.ProtectCondition);
                if (!string.IsNullOrEmpty(model.Provider))
                    cmd.Parameters.AddWithValue("@provider", model.Provider);
                if (!model.PriceCurrency.Equals(0)) cmd.Parameters.AddWithValue("@priceCurrency", model.PriceCurrency);
                if (!model.PriceRub.Equals(0)) cmd.Parameters.AddWithValue("@priceRub", model.PriceRub);
                if (!model.SumCurrency.Equals(0)) cmd.Parameters.AddWithValue("@sumCurrency", model.SumCurrency);
                cmd.Parameters.AddWithValue("@idClaim", model.IdTenderClaim);
                cmd.Parameters.AddWithValue("@idPosition", model.IdSpecificationPosition);
                cmd.Parameters.AddWithValue("@name", model.Name);
                cmd.Parameters.AddWithValue("@catalogNumber", model.CatalogNumber);
                if (model.ProtectFact != null)cmd.Parameters.AddWithValue("@protectFact", model.ProtectFactId);
                cmd.Parameters.AddWithValue("@sumRub", model.SumRub);
                cmd.Parameters.AddWithValue("@author", model.Author);
                //cmd.Parameters.AddWithValue("@currency", model.Currency);

                cmd.Parameters.AddWithValue("@priceUsd", model.PriceUsd);
                cmd.Parameters.AddWithValue("@priceEur", model.PriceEur);
                cmd.Parameters.AddWithValue("@priceEurRicoh", model.PriceEurRicoh);
                cmd.Parameters.AddWithValue("@priceRubl", model.PriceRubl);
                cmd.Parameters.AddWithValue("@deliveryTime", model.DeliveryTimeId);
                cmd.Parameters.AddWithValue("@b2bPrice", model.b2bPrice);
                cmd.Parameters.AddWithValue("@Brand", model.Brand);

                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    var id = rd.GetInt32(0);
                    if (id > 0)
                    {
                        result = true;
                        model.Id = id;
                    }
                }
                rd.Dispose();
            }
            return result;
        }

        public bool UpdateCalculateSpecificationPosition(CalculateSpecificationPosition model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UpdateCalculateClaimPosition";
                if (!string.IsNullOrEmpty(model.Replace))
                    cmd.Parameters.AddWithValue("@replaceValue", model.Replace);
                if (!string.IsNullOrEmpty(model.Comment))
                    cmd.Parameters.AddWithValue("@comment", model.Comment);
                if (!string.IsNullOrEmpty(model.ProtectCondition))
                    cmd.Parameters.AddWithValue("@protectCondition", model.ProtectCondition);
                if (!string.IsNullOrEmpty(model.Provider))
                    cmd.Parameters.AddWithValue("@provider", model.Provider);
                if (!model.PriceCurrency.Equals(0)) cmd.Parameters.AddWithValue("@priceCurrency", model.PriceCurrency);
                if (!model.PriceRub.Equals(0)) cmd.Parameters.AddWithValue("@priceRub", model.PriceRub);
                if (!model.SumCurrency.Equals(0)) cmd.Parameters.AddWithValue("@sumCurrency", model.SumCurrency);
                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.Parameters.AddWithValue("@name", model.Name);
                cmd.Parameters.AddWithValue("@catalogNumber", model.CatalogNumber);
                if (model.ProtectFact != null) cmd.Parameters.AddWithValue("@protectFact", model.ProtectFactId);
                cmd.Parameters.AddWithValue("@sumRub", model.SumRub);
                cmd.Parameters.AddWithValue("@author", model.Author);
                //cmd.Parameters.AddWithValue("@currency", model.Currency);

                cmd.Parameters.AddWithValue("@priceUsd", model.PriceUsd);
                cmd.Parameters.AddWithValue("@priceEur", model.PriceEur);
                cmd.Parameters.AddWithValue("@priceEurRicoh", model.PriceEurRicoh);
                cmd.Parameters.AddWithValue("@priceRubl", model.PriceRubl);
                cmd.Parameters.AddWithValue("@deliveryTime", model.DeliveryTimeId);
                cmd.Parameters.AddWithValue("@b2bPrice", model.b2bPrice);
                cmd.Parameters.AddWithValue("@Brand", model.Brand);

                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public bool DeleteCalculateSpecificationPosition(int id, AdUser user)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DeleteCalculateClaimPosition";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@deletedUser", user.Sid);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public List<CalculateSpecificationPosition> LoadCalculateSpecificationPositionsForTenderClaim(int claimId, int version)
        {
            var list = new List<CalculateSpecificationPosition>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadCalculateClaimPositionForClaim";
                cmd.Parameters.AddWithValue("@id", claimId);
                cmd.Parameters.AddWithValue("@version", version);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new CalculateSpecificationPosition()
                        {
                            Id = rd.GetInt32(0),
                            IdSpecificationPosition = rd.GetInt32(1),
                            IdTenderClaim = rd.GetInt32(2),
                            CatalogNumber = rd.GetString(3),
                            Name = rd.GetString(4),
                            Replace = rd.GetString(5),
                            PriceCurrency = (double)rd.GetDecimal(6),
                            SumCurrency = (double)rd.GetDecimal(7),
                            PriceRub = (double)rd.GetDecimal(8),
                            SumRub = (double)rd.GetDecimal(9),
                            Provider = rd.GetString(10),
                            ProtectCondition = rd.GetString(12),
                            Comment = rd.GetString(13),
                            Author = rd.GetString(14)
                        };

                        if (!rd.IsDBNull(11))
                        {
                            model.ProtectFact = new ProtectFact() {Id = rd.GetInt32(11)};
                        }

                        if (!rd.IsDBNull(18))
                        {
                            model.Currency = rd.GetInt32(18);
                        }

                        if (!rd.IsDBNull(19))
                        {
                            model.PriceUsd = (double)rd.GetDecimal(19);
                        }

                        if (!rd.IsDBNull(20))
                        {
                            model.PriceEur = (double)rd.GetDecimal(20);
                        }

                        if (!rd.IsDBNull(21))
                        {
                            model.PriceEurRicoh = (double)rd.GetDecimal(21);
                        }

                        if (!rd.IsDBNull(22))
                        {
                            model.PriceRubl = (double)rd.GetDecimal(22);
                        }

                        if (!rd.IsDBNull(23))
                        {
                            model.DeliveryTime = new DeliveryTime() { Id = rd.GetInt32(23) };
                        }

                        if (model.PriceCurrency.Equals(-1)) model.PriceCurrency = 0;
                        if (model.SumCurrency.Equals(-1)) model.SumCurrency = 0;
                        if (model.PriceRub.Equals(-1)) model.PriceRub = 0;
                        list.Add(model);
                    }
                }
                rd.Dispose();
            }
            return list;
        }

        public void DeleteCalculateForPositions(int idClaim, List<SpecificationPosition> positions)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DeleteCalculateForPositions";
                cmd.Parameters.AddWithValue("@idClaim", idClaim);
                cmd.Parameters.AddWithValue("@ids", string.Join(",", positions.Select(x => x.Id)));
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region ClaimStatusHistory

        public bool SaveClaimStatusHistory(ClaimStatusHistory model)
        {
            var result = false;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SaveClaimStatusHistory";
                if (!string.IsNullOrEmpty(model.Comment))
                    cmd.Parameters.AddWithValue("@comment", model.Comment);
                cmd.Parameters.AddWithValue("@idClaim", model.IdClaim);
                cmd.Parameters.AddWithValue("@recordDate", model.Date);
                cmd.Parameters.AddWithValue("@idUser", model.IdUser);
                cmd.Parameters.AddWithValue("@idStatus", model.Status.Id);
                conn.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public List<ClaimStatusHistory> LoadStatusHistoryForClaim(int claimId)
        {
            var list = new List<ClaimStatusHistory>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadStatusHistoryForClaim";
                cmd.Parameters.AddWithValue("@idClaim", claimId);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new ClaimStatusHistory()
                        {
                            Id = rd.GetInt32(0),
                            Date = rd.GetDateTime(1),
                            IdClaim = rd.GetInt32(2),
                            Comment = rd.GetString(4),
                            IdUser = rd.GetString(5),
                            Status = new ClaimStatus()
                            {
                                Id = rd.GetInt32(3),
                                Value = rd.GetString(6)
                            }
                        };
                        model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                        list.Add(model);
                    }
                }
                rd.Dispose();
            }
            return list;
        }

        public ClaimStatusHistory LoadLastStatusHistoryForClaim(int claimId)
        {
            ClaimStatusHistory model = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadLastStatusHistoryForClaim";
                cmd.Parameters.AddWithValue("@idClaim", claimId);
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    model = new ClaimStatusHistory()
                    {
                        Id = rd.GetInt32(0),
                        Date = rd.GetDateTime(1),
                        IdClaim = rd.GetInt32(2),
                        Comment = rd.GetString(4),
                        IdUser = rd.GetString(5),
                        Status = new ClaimStatus()
                        {
                            Id = rd.GetInt32(3),
                            Value = rd.GetString(6)
                        }
                    };
                }
                rd.Dispose();
            }
            return model;
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

        public List<TenderStatus> LoadTenderStatus()
        {
            var list = new List<TenderStatus>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadTenderStatus";
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new TenderStatus()
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

        public List<ProtectFact> LoadProtectFacts()
        {
            var list = new List<ProtectFact>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadProtectFacts";
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new ProtectFact()
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

        public List<DeliveryTime> LoadDeliveryTimes()
        {
            var list = new List<DeliveryTime>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadDeliveryTimes";
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new DeliveryTime()
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

        public List<Currency> LoadCurrencies()
        {
            var list = new List<Currency>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadCurrencies";
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new Currency()
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

        public List<UserRole> LoadRoles()
        {
            var list = new List<UserRole>();
            using (var conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoadRoles";
                conn.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        var model = new UserRole()
                        {
                            Role = (Role)rd.GetInt32(0),
                            Sid = rd.GetString(1),
                            Name = rd.GetString(2)
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
