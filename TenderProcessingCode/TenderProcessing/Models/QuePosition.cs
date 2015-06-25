using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Stuff.Objects;

namespace TenderProcessing.Models
{
    public class QuePosition:DbModel
    {

        public int Id { get; set; }
        public Question Question { get; set; }
        public Employee User { get; set; }
        public string Descr { get; set; }
        public Employee Creator { get; set; }

        public QuePosition() { }

        public QuePosition(int id)
        {
            Uri uri = new Uri(String.Format("{0}/QuePosition/Get?id={1}", OdataServiceUri, id));
            string jsonString = GetJson(uri);
            var dep = JsonConvert.DeserializeObject<QuePosition>(jsonString);
            FillSelf(dep);
        }

        private void FillSelf(QuePosition model)
        {
            Id = model.Id;
            Question = model.Question;
            User = model.User;
            Descr = model.Descr;
        }

        public bool Save(out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/QuePosition/Save", OdataServiceUri));
            string json = JsonConvert.SerializeObject(this);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static IEnumerable<QuePosition> GetList()
        {
            Uri uri = new Uri(String.Format("{0}/QuePosition/GetList", OdataServiceUri));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<QuePosition>>(jsonString);
            return model;
        }

        public static bool Delete(int id, out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/QuePosition/Close?id={1}", OdataServiceUri, id));
            string json = String.Empty;//String.Format("{{\"id\":{0}}}",id);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static IEnumerable<QuePosition> GetSelectionList()
        {
            Uri uri = new Uri(String.Format("{0}/QuePosition/GetList", OdataServiceUri));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<QuePosition>>(jsonString);
            return model;
        }
    }
}