using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Bibliography;
using Newtonsoft.Json;
using Stuff.Objects;

namespace TenderProcessing.Models
{
    public class Question:DbModel
    {
        public int Id { get; set; }
        public Employee Manager { get; set; }
        public DateTime DateLimit { get; set; }
        public string Descr { get; set; }
        public Employee Creator { get; set; }

        public IEnumerable<QuePosition> Positions { get; set; }
        public QuePosition NewPosition { get; set; }

        public Question() { }

        public Question(int id)
        {
            Uri uri = new Uri(String.Format("{0}/Question/Get?id={1}", OdataServiceUri, id));
            string jsonString = GetJson(uri);
            var dep = JsonConvert.DeserializeObject<Question>(jsonString);
            FillSelf(dep);
        }

        public Question(int id, bool getPositions = false): this(id)
        {
            if (getPositions)Positions = GetPositions();
        }

        private void FillSelf(Question model)
        {
            Id = model.Id;
            Manager = model.Manager;
            DateLimit = model.DateLimit;
            Descr = model.Descr;
            //NewPosition = new QuePosition(){Question = new Question(){}};
        }

        public bool Save(out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/Question/Save", OdataServiceUri));
            string json = JsonConvert.SerializeObject(this);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static IEnumerable<Question> GetList()
        {
            Uri uri = new Uri(String.Format("{0}/Question/GetList", OdataServiceUri));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<Question>>(jsonString);
            return model;
        }

        public static bool Delete(int id, out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/Question/Close?id={1}", OdataServiceUri, id));
            string json = String.Empty;//String.Format("{{\"id\":{0}}}",id);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static IEnumerable<Question> GetSelectionList()
        {
            Uri uri = new Uri(String.Format("{0}/Question/GetList", OdataServiceUri));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<Question>>(jsonString);
            return model;
        }

        public IEnumerable<QuePosition> GetPositions()
        {
            Uri uri = new Uri(String.Format("{0}/QuePosition/GetList?idQuestion={1}", OdataServiceUri, Id));
            string jsonString = GetJson(uri);
            var pos = JsonConvert.DeserializeObject<IEnumerable<QuePosition>>(jsonString);
            return pos;
        }

        public static bool SetQuestion2Work(int id, out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/Question/Close?id={1}", OdataServiceUri, id));
            string json = String.Empty;//String.Format("{{\"id\":{0}}}",id);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }
    }
}