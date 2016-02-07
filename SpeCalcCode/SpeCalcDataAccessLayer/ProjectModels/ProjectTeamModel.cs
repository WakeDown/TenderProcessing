﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectTeamModel
    {
        public int ProjectId { get; set; }
        public IEnumerable<ProjectTeams> Team { get; set; }
        public ProjectRoles Role { get; set; }

        public static IEnumerable<ProjectTeams> GetRoleList(int projectId, int? roleId)
        {
            using (var db = new SpeCalcEntities())
            {
                return db.ProjectTeams.Where(x => x.Enabled && x.ProjectId == projectId && x.RoleId == roleId)
                    .Include(x => x.ProjectRoles).ToList();
            }
        }

        public static IEnumerable<ProjectTeamModel> GetList(int projectId)
        {
            using (var db = new SpeCalcEntities())
            {
                var rolesAll = db.ProjectRoles.OrderBy(x=>x.OrderNum).ToList();
                var team = db.ProjectTeams.Where(x => x.Enabled && x.ProjectId == projectId)
                    .Include(x => x.ProjectRoles);
                var list = new List<ProjectTeamModel>();
                foreach (var role in rolesAll)
                {
                    var item = new ProjectTeamModel();
                    item.ProjectId = projectId;
                    item.Role = role;
                    item.Team = team.Where(x => x.RoleId == role.Id).ToList();
                    list.Add(item);
                }
                return list;
            }
        }

        public static void Create(int projectId, int roleId, string userSid, string userName, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                if (!db.ProjectTeams.Any(x => x.Enabled && x.ProjectId == projectId && x.RoleId == roleId && x.UserSid == userSid))
                {
                    var item = new ProjectTeams();
                    item.Enabled = true;
                    item.CreateDate = DateTime.Now;
                    item.CreatorSid = user.Sid;
                    item.CreatorName = user.DisplayName;
                    item.ProjectId = projectId;
                    item.RoleId = roleId;
                    item.UserSid = userSid;
                    item.UserName = userName;
                    db.ProjectTeams.Add(item);
                    db.SaveChanges();
                    ProjectHistoryModel.CreateHistoryItem(projectId, "Добавление участника в команду", new[] { item }, user);
                }
            }
        }

        public static void Delete(int memberId, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var member = db.ProjectTeams.Single(x => x.Id == memberId);

                member.Enabled = false;
                member.DeleteDate = DateTime.Now;
                member.DeleterSid = user.Sid;
                member.DeleterName = user.DisplayName;
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(member.ProjectId, "Удаление участника из команды", new[] { member }, user);
            }
        }
    }
}
