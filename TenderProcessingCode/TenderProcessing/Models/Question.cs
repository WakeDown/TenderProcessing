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
        public QueState State { get; set; }
        public DateTime DateCreate { get; set; }

        public IEnumerable<QuePosition> Positions { get; set; }

        public Question() { }

        public Question(int id)
        {
            Uri uri = new Uri(String.Format("{0}/Question/Get?id={1}", OdataServiceUri, id));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<Question>(jsonString);
            FillSelf(model);
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
            State = model.State;
            DateCreate = model.DateCreate;
            //NewPosition = new QuePosition(){Question = new Question(){}};
        }

        public bool Save(out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/Question/Save", OdataServiceUri));
            string json = JsonConvert.SerializeObject(this);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static IEnumerable<Question> GetList(QuestionFilter filter= null)
        {
            string filterStr = null;
            if (filter != null && !(!filter.Id.HasValue && filter.Manager == null && filter.Product == null && !filter.States.Any(s => s.Checked) && !filter.Top.HasValue))
            {
                filterStr = String.Format("?id={0}&managerSid={1}&queStates={2}&top={3}&prodSid={4}",
                    filter.Id.HasValue ? filter.Id.Value.ToString() : null, filter.Manager != null && !String.IsNullOrEmpty(filter.Manager.AdSid) ? filter.Manager.AdSid : null, String.Join(",", filter.States.Where(s => s.Checked).Select(s => s.Id)), filter.Top.HasValue ? filter.Top.Value.ToString() : null, filter.Product != null && !String.IsNullOrEmpty(filter.Product.AdSid) ? filter.Product.AdSid : null);
            }

            Uri uri = new Uri(String.Format("{0}/Question/GetList{1}", OdataServiceUri, filterStr));
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

        public IEnumerable<HistoryQueState> GetStateHistory()
        {
            Uri uri = new Uri(String.Format("{0}/Question/GetStateHistory?idQuestion={1}", OdataServiceUri, Id));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<HistoryQueState>>(jsonString);
            return model;
        }

        public static bool SetQuestionSent(int id, out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/Question/SetQuestionSent?id={1}", OdataServiceUri, id));
            string json = String.Empty;//String.Format("{{\"id\":{0}}}",id);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static bool SetQuestionAnswered(int id, out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/Question/SetQuestionAnswered?id={1}", OdataServiceUri, id));
            string json = String.Empty;//String.Format("{{\"id\":{0}}}",id);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }

        public static bool SetQuestionAproved(int id, out ResponseMessage responseMessage)
        {
            Uri uri = new Uri(String.Format("{0}/Question/SetQuestionAproved?id={1}", OdataServiceUri, id));
            string json = String.Empty;//String.Format("{{\"id\":{0}}}",id);
            bool result = PostJson(uri, json, out responseMessage);
            return result;
        }
        public static QueState GetQuestionCurrState(int idQuestion)
        {
            Uri uri = new Uri(String.Format("{0}/Question/GetQuestionCurrState?idQuestion={1}", OdataServiceUri, idQuestion));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<QueState>(jsonString);
            return model;
        }
    }
}