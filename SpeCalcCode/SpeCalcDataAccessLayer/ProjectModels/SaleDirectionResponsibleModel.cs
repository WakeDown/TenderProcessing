using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class SaleDirectionResponsibleModel
    {
        public static IEnumerable<ProjectSaleDirectionResponsibles> GetResponsiblesList(int directionId)
        {
            using (var db = new SpeCalcEntities())
            {
                return db.ProjectSaleDirectionResponsibles.Where(x => x.Enabled && x.SaleDirectionId == directionId).ToList();
            }
        }

        public static void Create(int directionId, string userSid, string userName, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                if (!db.ProjectSaleDirectionResponsibles.Any(x => x.Enabled && x.SaleDirectionId == directionId && x.UserSid == userSid))
                {
                    var member = new ProjectSaleDirectionResponsibles();
                    member.Enabled = true;
                    member.CreateDate = DateTime.Now;
                    member.CreatorSid = user.Sid;
                    member.CreatorName = user.DisplayName;
                    member.SaleDirectionId = directionId;
                    member.UserSid = userSid;
                    member.UserName = userName;
                    db.ProjectSaleDirectionResponsibles.Add(member);

                    db.SaveChanges();
                }
            }
        }

        public static void Delete(int memberId, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var member = db.ProjectSaleDirectionResponsibles.Single(x => x.Id == memberId);

                member.Enabled = false;
                member.DeleteDate = DateTime.Now;
                member.DeleterSid = user.Sid;
                member.DeleterName = user.DisplayName;
                db.SaveChanges();
            }
        }
    }
}
