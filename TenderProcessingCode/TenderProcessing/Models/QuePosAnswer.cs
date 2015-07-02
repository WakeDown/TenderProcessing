using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Stuff.Objects;

namespace TenderProcessing.Models
{
    public class QuePosAnswer:DbModel
    {
        public int Id { get; set; }
        public QuePosition QuePosition { get; set; }
        public Employee Answerer { get; set; }
        public string Descr { get; set; }
        public Employee Creator { get; set; }

        public QuePosAnswer() { }

        public QuePosAnswer(int id)
        {
            Uri uri = new Uri(String.Format("{0}/QuePosAnswer/Get?id={1}", OdataServiceUri, id));
            string jsonString = GetJson(uri);
            var dep = JsonConvert.DeserializeObject<QuePosAnswer>(jsonString);
            FillSelf(dep);
        }

        private void FillSelf(QuePosAnswer model)
        {
            Id = model.Id;
            QuePosition = model.QuePosition;
            Answerer = model.Answerer;
            Descr = model.Descr;
        }

        public bool Save(out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/QuePosAnswer/Save", OdataServiceUri));
            string json = JsonConvert.SerializeObject(this);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static IEnumerable<QuePosAnswer> GetList()
        {
            Uri uri = new Uri(String.Format("{0}/QuePosAnswer/GetList", OdataServiceUri));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<QuePosAnswer>>(jsonString);
            return model;
        }

        public static bool Delete(int id, out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/QuePosAnswer/Close?id={1}", OdataServiceUri, id));
            string json = String.Empty;//String.Format("{{\"id\":{0}}}",id);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static IEnumerable<QuePosAnswer> GetSelectionList()
        {
            Uri uri = new Uri(String.Format("{0}/QuePosAnswer/GetList", OdataServiceUri));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<QuePosAnswer>>(jsonString);
            return model;
        }
    }
}