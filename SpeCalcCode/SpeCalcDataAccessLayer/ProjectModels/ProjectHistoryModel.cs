using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectHistoryModel
    {
        public ProjectHistory Item { get; set; }
        public static IEnumerable<ProjectHistoryModel> GetList(int projectId)
        {
            var result = new List<ProjectHistoryModel>();
            using (var db = new SpeCalcEntities())
            {
                var list = db.ProjectHistory.Where(x => x.ProjectId == projectId).OrderByDescending(x=>x.Id);
                foreach (var history in list)
                {
                    var item = new ProjectHistoryModel();
                    history.Comment = !String.IsNullOrEmpty(history.Comment) ?  history.Comment.Replace("\n", "<br />").Replace("\r", "<br />") : null;
                    item.Item = history;
                    result.Add(item);
                }
                return result;
            }
        }

        public static void CreateHistoryItem(int projectId, string title, string comment, object[] objects, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var item = new ProjectHistory();
                item.CreateDate = DateTime.Now;
                item.CreatorSid = user.Sid;
                item.CreatorName = user.DisplayName;
                item.Comment = comment;
                item.ProjectId = projectId;
                item.Title = title;

                string sysInfo = null;
                foreach (var o in objects)
                {
                    sysInfo += GetObjectProperties(o);
                }
                item.SysInfo = sysInfo;

                db.ProjectHistory.Add(item);
                db.SaveChanges();
            }
        }

        private static string GetObjectProperties(object obj)
        {
            string varName = GetMemberName(() => obj);
            string result = $"{varName}: ";
            foreach (var prop in obj.GetType().GetProperties())
            {
                result +=$"[{prop.Name} = {prop.GetValue(obj, null)}],";
            }
            return result;
        }

        public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        {
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            return expressionBody.Member.Name;
        }
    }
}
