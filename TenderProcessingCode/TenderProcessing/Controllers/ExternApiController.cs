using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Management;
using TenderProcessingDataAccessLayer;

namespace TenderProcessing.Controllers
{
    public class ExternApiController : ApiController
    {
        //изменение статуса конкурса, параметр sig - хеш по определенному алгоритму, контекация строк параметров и секретного ключа
        //: параметр1имя=Параметр1значениеПараметр2имя=Параметр2значениеСекретныйКлюч
        //md5.Hash(idClaim=5status=3secretkey)
        [HttpGet]
        public ApiRequestResult ChangeClaimTenderStatus(int idClaim, int status, string sig)
        {
            var model = new ApiRequestResult() {IsComplete = false, Message = string.Empty, ErrorCode = 0};
            try
            {
                //проверка sig
                var md5 = MD5.Create();
                var key = ConfigurationManager.AppSettings["AppKey"];
                var paramArr = new[]
                {
                    "idClaim=" + idClaim,
                    "status=" + status
                };
                var validSig = GetMd5Hash(md5, string.Join(string.Empty, paramArr) + key);
                var isSigValid = sig == validSig;
                if (isSigValid)
                {
                    //изменение статуса
                    var db = new DbEngine();
                    var tenderStatus = db.LoadTenderStatus();
                    if (tenderStatus.FirstOrDefault(x => x.Id == status) == null)
                    {
                        model.Message = "Status is not valid";
                        model.ErrorCode = 3;
                    }
                    else
                    {
                        var claim = db.LoadTenderClaimById(idClaim);
                        if (claim == null)
                        {
                            model.Message = "Claim with Id = " + idClaim + " is not exists";
                            model.ErrorCode = 4;
                        }
                        else
                        {
                            model.IsComplete =  db.ChangeTenderClaimTenderStatus(idClaim, status);       
                        }
                    }
                }
                else
                {
                    model.Message = "Sig is not valid";
                    model.ErrorCode = 2;
                }
            }
            catch (Exception)
            {
                model.IsComplete = false;
                model.Message = "Server error";
                model.ErrorCode = 1;
            }
            return model;
        }

        private string GetMd5Hash(MD5 md5Hash, string input)
        {
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public class ApiRequestResult
        {
            public bool IsComplete { get; set; }

            public int ErrorCode { get; set; }

            public string Message { get; set; }
        }
    }
}
