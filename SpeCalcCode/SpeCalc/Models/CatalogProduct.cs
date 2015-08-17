using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Stuff.Objects;

namespace SpeCalc.Models
{
    public class CatalogProduct:DbModel
    {
        public class Some
        {
            public string partNum { get; set; }
        }

        public static string PriceRequest(string partNum)
        {
            ResponseMessage responseMessage;
            //Uri uri = new Uri(String.Format("{0}/CatalogProduct/GetMinPrice?partNum={1}", OdataServiceUri, partNum));
            Uri uri = new Uri(String.Format("{0}/CatalogProduct/GetMinPrice", OdataServiceUri));
            string json = String.Format("{{\"partNum\":\"{0}\"}}", partNum);
            bool cpmlete = PostJson(uri, json, out responseMessage);
            //var result = JsonConvert.DeserializeObject(jsonString);
            return responseMessage.Value;
        }
    }
}