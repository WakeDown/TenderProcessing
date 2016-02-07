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
        public static IEnumerable<ProjectHistory> GetList(int projectId)
        {
            using (var db = new SpeCalcEntities())
            {
                return db.ProjectHistory.Where(x => x.ProjectId == projectId);
            }
        }

        public static void CreateHistoryItem(int projectId, string comment, object[] objects, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var item = new ProjectHistory();
                item.CreateDate = DateTime.Now;
                item.CreatorSid = user.Sid;
                item.CreatorName = user.DisplayName;
                item.Comment = comment;
                item.ProjectId = projectId;

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
