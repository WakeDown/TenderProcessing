using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectMessageModel
    {
        public static void Send(int projectId, string message, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var msg = new ProjectMessages();
                msg.Enabled = true;
                msg.ProjectId = projectId;
                msg.CreateDate = DateTime.Now;
                msg.CreatorSid = user.Sid;
                msg.CreatorName = user.DisplayName;
                msg.Message = message;
                db.ProjectMessages.Add(msg);
                db.SaveChanges();
            }
        }

        public static IEnumerable<ProjectMessages> GetList(out int totalCount, int projectId, bool full = true)
        {
            //using (var db = new SpeCalcEntities())
            //{
            var db = new SpeCalcEntities();
            var query = db.ProjectMessages.Where(x => x.ProjectId == projectId && x.Enabled);
            var page = query.OrderByDescending(p => p.Id).Take(3);
            if (full)
            {
                page = query.OrderByDescending(p => p.Id);
            }
            totalCount = query.Count();
            var list = page.Select(p => p);
            return list;
            //}
        }
    }
}
