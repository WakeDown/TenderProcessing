using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using ServiceClaim.Helpers;
using SpeCalcDataAccessLayer.HelpModels;
using SpeCalcDataAccessLayer.Models;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalcDataAccessLayer.ProjectModels
{
    public class ProjectModel
    {
        public static Projects Get(int id)
        {
            using (var db = new SpeCalcEntities())
            {
                return db.Projects.SingleOrDefault(x => x.Id == id);
            }
        }

        public static Projects GetFat(int id)
        {
            //using (var db = new SpeCalcEntities())
            //{
                var db = new SpeCalcEntities();
                return db.Projects.Include(x => x.ProjectCurrencies).Include(x => x.ProjectSaleDirections).Include(x => x.ProjectSaleSubjects).Include(x => x.ProjectStates).Include(x => x.ProjectBusinessTargets).Include(x => x.ProjectCurrencies).Include(x => x.ProjectClientRelationships).Include(x=>x.ProjectTeams).SingleOrDefault(x => x.Id == id);
            //}
        }

        public static List<Projects> GetList(out int totalCount, int? topRows, int pageNum = 1, int? id = null, string aucnum = null, string client = null, string budget = null, int? directId = null, int? subjectId = null, string team = null, string deadline = null, string probab = null, int[] stateIds = null, int? conditionId = null, DateTime? createStart = null, DateTime? createEnd = null, string curUserSid = null)
        {
            //using (var db = new SpeCalcEntities())
            //{
            decimal budgetDecimal;
            if (budget != null && !String.IsNullOrEmpty(budget))
            {
                string budgCheck = budget.Replace(">", "").Replace("<", "").Replace("=", "").Trim();
                decimal.TryParse(budgCheck, out budgetDecimal);
            }
            else
            {
                budgetDecimal = 0;
            }

            int probabInt;
            if (!String.IsNullOrEmpty(probab))
            {
                string probabCheck = probab.Replace(">", "").Replace("<", "").Replace("=", "").Trim();
                int.TryParse(probabCheck, out probabInt);
            }
            else
            {
                probabInt = 0;
            }


            if (stateIds == null)
                stateIds = new int[0];

            
            var db = new SpeCalcEntities();
            var list = db.Projects.Where(x => x.Enabled);
            totalCount = list.Count();
            var result = new List<Projects>();
            if (topRows.HasValue)
            {
                result = list
                //FILTER
                .Where(x => !id.HasValue || id <= 0 || (id.HasValue && id > 0 && x.Id == id))
                .Where(x => String.IsNullOrEmpty(aucnum) || (!String.IsNullOrEmpty(aucnum) && x.AuctionNumber.Contains(aucnum)))
                .Where(x => String.IsNullOrEmpty(client) || (!String.IsNullOrEmpty(client) && (x.ClientName.Contains(client) || x.ClientInn.Contains(client))))
                .Where(x => budget == null || String.IsNullOrEmpty(budget) || (budget != null && !String.IsNullOrEmpty(budget)
                        && ((budget.StartsWith(">") && x.Budget > budgetDecimal) || (budget.StartsWith("<") && x.Budget < budgetDecimal) || x.Budget == budgetDecimal)
                        ))
                .Where(x => !directId.HasValue || directId <= 0 || (directId.HasValue && directId > 0 && x.SaleDirectionId == directId))
                .Where(x => !subjectId.HasValue || subjectId <= 0 || (subjectId.HasValue && subjectId > 0 && x.SaleSubjectId == subjectId))
                .Where(x => team == null || String.IsNullOrEmpty(team) || (team != null && !String.IsNullOrEmpty(team) && x.ProjectTeams.Any(y => y.UserName.Contains(team))))
                .Where(x => deadline == null || String.IsNullOrEmpty(deadline) || (deadline != null && !String.IsNullOrEmpty(deadline) && x.DeadlineDate.Contains(deadline)))
                .Where(x => String.IsNullOrEmpty(probab) || (!String.IsNullOrEmpty(probab)
                        && ((probab.StartsWith(">") && x.Probability > probabInt) || (budget.StartsWith("<") && x.Probability < probabInt) || x.Probability == probabInt)
                        ))
                .Where(x => !stateIds.Any() || (stateIds.Any() &&  x.StateId.HasValue && stateIds.Contains(x.StateId.Value)))
                .Where(x => !conditionId.HasValue || conditionId <= 0 || (conditionId.HasValue && conditionId > 0 && x.ConditionId == conditionId))
                .Where(x => !createStart.HasValue || (createStart.HasValue && x.CreateDate >= createStart))
                .Where(x => !createEnd.HasValue || (createEnd.HasValue && x.CreateDate <= createEnd))
                //</FILTER
                //Access

                .Where(x =>

                (String.IsNullOrEmpty(curUserSid)
                ||
                (!String.IsNullOrEmpty(curUserSid) &&
                    (
                        //Где указан в команде
                        x.ProjectTeams.Any(y => y.Enabled && y.UserSid == curUserSid)
                        ||
                        //Его направления
                        (x.SaleDirectionId.HasValue &&
                        db.ProjectSaleDirectionResponsibles.Where(y => y.Enabled && y.UserSid == curUserSid).Select(y => y.SaleDirectionId).Contains(x.SaleDirectionId.Value)
                        )
                        ||
                              //Продакт
                              (
                              db.ProjectPositions.Any(y => y.Enabled && y.CalculatorSid == curUserSid) ||
                              db.ProjectWorks.Any(y => y.Enabled && y.CalculatorSid == curUserSid)
                              )
                        ||
                        //Менеджер
                        x.ManagerSid == curUserSid
                        ||
                        //Автор
                        x.CreatorSid == curUserSid
                        )
                )
                )
                )

                //</Access
                .OrderByDescending(x => x.Id)
                .Skip((pageNum - 1) * topRows.Value).Take(topRows.Value).Include(x => x.ProjectSaleDirections).Include(x => x.ProjectSaleSubjects).Include(x => x.ProjectStates).Include(x => x.ProjectBusinessTargets).Include(x => x.ProjectCurrencies).Include(x => x.ProjectTeams)

                .ToList();
            }
            else
            {
                result = list
                //FILTER
                .Where(x => !id.HasValue || id <= 0 || (id.HasValue && id > 0 && x.Id == id))
                .Where(x => String.IsNullOrEmpty(aucnum) || (!String.IsNullOrEmpty(aucnum) && x.AuctionNumber.Contains(aucnum)))
                .Where(x => String.IsNullOrEmpty(client) || (!String.IsNullOrEmpty(client) && (x.ClientName.Contains(client) || x.ClientInn.Contains(client))))
                .Where(x => budget == null || String.IsNullOrEmpty(budget) || (budget != null && !String.IsNullOrEmpty(budget)
                        && ((budget.StartsWith(">") && x.Budget > budgetDecimal) || (budget.StartsWith("<") && x.Budget < budgetDecimal) || x.Budget == budgetDecimal)
                        ))
                .Where(x => !directId.HasValue || directId <= 0 || (directId.HasValue && directId > 0 && x.SaleDirectionId == directId))
                .Where(x => !subjectId.HasValue || subjectId <= 0 || (subjectId.HasValue && subjectId > 0 && x.SaleSubjectId == subjectId))
                .Where(x => team == null || String.IsNullOrEmpty(team) || (team != null && !String.IsNullOrEmpty(team) && x.ProjectTeams.Any(y => y.UserName.Contains(team))))
                .Where(x => deadline == null || String.IsNullOrEmpty(deadline) || (deadline != null && !String.IsNullOrEmpty(deadline) && x.DeadlineDate.Contains(deadline)))
                .Where(x => probab == null || String.IsNullOrEmpty(probab) || (probab != null && !String.IsNullOrEmpty(probab)
                        && ((probab.StartsWith(">") && x.Probability > probabInt) || (budget.StartsWith("<") && x.Probability < probabInt) || x.Probability == probabInt)
                        ))
                .Where(x => !stateIds.Any() || (stateIds.Any() && x.StateId.HasValue && stateIds.Contains(x.StateId.Value)))
                .Where(x => !conditionId.HasValue || conditionId <= 0 || (conditionId.HasValue && conditionId > 0 && x.ConditionId == conditionId))
                .Where(x => !createStart.HasValue || (createStart.HasValue && x.CreateDate >= createStart))
                .Where(x => !createEnd.HasValue || (createEnd.HasValue && x.CreateDate <= createEnd))
                //</FILTER
                //Access

                .Where(x =>
                
                (String.IsNullOrEmpty(curUserSid) 
                || 
                (!String.IsNullOrEmpty(curUserSid) && 
                    (
                        //Где указан в команде
                        x.ProjectTeams.Any(y => y.Enabled && y.UserSid == curUserSid) 
                        ||
                        //Его направления
                        (x.SaleDirectionId.HasValue &&
                        db.ProjectSaleDirectionResponsibles.Where(y => y.Enabled && y.UserSid == curUserSid).Select(y => y.SaleDirectionId).Contains(x.SaleDirectionId.Value)
                        )
                         ||
                              //Продакт
                              (
                              db.ProjectPositions.Any(y => y.Enabled && y.CalculatorSid == curUserSid) ||
                              db.ProjectWorks.Any(y => y.Enabled && y.CalculatorSid == curUserSid)
                              )
                        ||
                        //Менеджер
                        x.ManagerSid == curUserSid
                        ||
                        //Автор
                        x.CreatorSid == curUserSid

                    )
                )
                )
                )

                //</Access
                .OrderByDescending(x => x.Id).Include(x => x.ProjectSaleDirections).Include(x => x.ProjectSaleSubjects).Include(x => x.ProjectStates).Include(x => x.ProjectBusinessTargets).Include(x => x.ProjectCurrencies).Include(x => x.ProjectTeams).ToList();
            }
            return result;
            //}
        }

        private static int CalculateProbability(string deadlineDate, bool hasBudget, int? clientRelationshipsId)
        {
            int result = 0;

            if (deadlineDate.Contains(DateTime.Now.Year.ToString())) result += 30;
            if (hasBudget) result += 40;
            if (clientRelationshipsId.HasValue)
            {
                using (var db = new SpeCalcEntities())
                {
                    var rel = db.ProjectClientRelationships.Single(x => x.Id == clientRelationshipsId);
                    result += rel.ProbabilityValue;
                }
            }

            return result;
        }

        public static int Create(Projects project, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                //using (var tran = db.Database.BeginTransaction())
                //{
                //try
                //{
                project.Enabled = true;
                project.CreateDate = DateTime.Now;
                project.CreatorSid = user.Sid;
                project.CreatorName = user.DisplayName;
                project.HasBudget = project.Budget.HasValue && project.Budget > 0;
                project.Probability = CalculateProbability(project.DeadlineDate, project.HasBudget,
                project.ClientRelationshipId);
                //project.LastInfoChangeDate = DateTime.Now;
                //project.LastInfoChangerSid = user.Sid;
                //project.LastInfoChangerName = user.DisplayName;
                db.Projects.Add(project);
                db.SaveChanges();
                ProjectHistoryModel.CreateHistoryItem(project.Id, "Создание", null, new[] { project }, user);
                var stateId = ProjectStateModel.GetState("NEW").Id;
                ProjectStateModel.SetProjectState(project, stateId, "", user, db);

                string msg = $"В систему СпецРасчет добавлен проект №{project.Id}.<br />Необходимо назначить команду и запустить проект в работу.<br /><br />Краткая информация о проекте:<br />{ProjectHelper.GetProjectShortInfo(project.Id)}<br /><br />Ссылка: {ProjectHelper.GetProjectLink(project.Id)}";
                var emails = SaleDirectionResponsibleModel.GetResponsiblesEmailList(project.SaleDirectionId.Value);
                MessageHelper.SendMailSmtpAsync($"[Проект №{project.Id}] Новый проект", msg, true, null, emails.ToArray());


                return project.Id;
                //    tran.Commit();

                //}
                //catch (DbEntityValidationException ex)
                //{
                //    tran.Rollback();
                //    foreach (var ve in ex.EntityValidationErrors)
                //    {
                //        Debug.WriteLine(ve.ValidationErrors);
                //    }
                //    throw;
                //}
                //catch (Exception ex)
                //{
                //    tran.Rollback();
                //    throw;
                //}
                //}
            }
        }

        public static void Update(Projects project, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                

                var old = db.Projects.SingleOrDefault(x => x.Id == project.Id);
                bool directionIsCahnged = old.SaleDirectionId != project.SaleDirectionId;
                old.IsAuction = project.IsAuction;
                old.AuctionLink = project.AuctionLink;
                old.AuctionNumber = project.AuctionNumber;
                old.AuctionDeadlineDate = project.AuctionDeadlineDate;
                old.TkpDeadlineDate = project.TkpDeadlineDate;
                old.ClientRelationshipId = project.ClientRelationshipId;
                old.DeadlineDate = project.DeadlineDate;
                old.ClientInn = project.ClientInn;
                old.ClientName = project.ClientName;
                old.BusinessTargetId = project.BusinessTargetId;
                old.BusinessTargetName = project.BusinessTargetName;
                old.SaleDirectionId = project.SaleDirectionId;
                old.SaleSubjectId = project.SaleSubjectId;
                old.Budget = project.Budget;
                old.CurrencyId = project.CurrencyId;
                old.HasBudget = project.Budget.HasValue && project.Budget > 0;
                old.Probability = CalculateProbability(project.DeadlineDate, project.HasBudget,
                        project.ClientRelationshipId);
                old.ManagerSid = project.ManagerSid;
                old.ManagerName = project.ManagerName;
                old.ManagerDepartmentName = project.ManagerDepartmentName;
                old.ManagerChiefName = project.ManagerChiefName;
                old.ManagerChiefSid = project.ManagerChiefSid;
                old.Comment = project.Comment;
                old.LastInfoChangeDate = DateTime.Now;
                old.LastInfoChangerSid = user.Sid;
                old.LastInfoChangerName = user.DisplayName;
                ProjectHistoryModel.CreateHistoryItem(project.Id, "Обновление", null, new[] { old, project }, user);
                db.SaveChanges();

                if (directionIsCahnged)
                {
                    string msg = $"В системе СпецРасчет изменено направление проекта №{project.Id}.<br />Необходимо назначить команду и запустить проект в работу.<br /><br />Краткая информация о проекте:<br />{ProjectHelper.GetProjectShortInfo(project.Id)}<br /><br />Ссылка: {ProjectHelper.GetProjectLink(project.Id)}";

                    var emails = SaleDirectionResponsibleModel.GetResponsiblesEmailList(project.SaleDirectionId.Value);
                    MessageHelper.SendMailSmtpAsync($"[Проект №{project.Id}] Новый проект", msg, true,null, emails.ToArray());
                }
            }
        }

        public static void SetDoneState(int id, string comment, AdUser user)
        {
            var stateId = ProjectStateModel.GetState("DONE").Id;
            SetState(id, stateId, comment, user);
        }
        public static void SetPlayState(int id, string comment, AdUser user)
        {
            var stateId = ProjectStateModel.GetState("PLAY").Id;
            SetState(id, stateId, comment, user);
        }

        public static void SetPauseState(int id, string comment, AdUser user)
        {
            var stateId = ProjectStateModel.GetState("PAUSE").Id;
            SetState(id, stateId, comment, user);
        }

        public static void SetStopState(int id, string comment, AdUser user)
        {
            var stateId = ProjectStateModel.GetState("STOP").Id;
            SetState(id, stateId, comment, user);
        }

        public static void SetState(int id, int stateId, string comment, AdUser user, SpeCalcEntities context = null)
        {
            bool isTran = context != null;
            var db = context ?? new SpeCalcEntities();
            var project = db.Projects.Single(x => x.Id == id);
            ProjectStateModel.SetProjectState(project, stateId, comment, user, db);
            if (!isTran)
            {
                db.Dispose();
            }
        }

        public static void SetCondition(int projectId, int conditionId, string comment, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var project = db.Projects.Single(x => x.Id == projectId);
                if (project.ProjectConditionHistory.All(x => x.ConditionId != conditionId))
                {
                    ProjectConditionModel.SetProjectCondition(project, conditionId, comment, user, db);
                }
            }
        }

        public static IEnumerable<KeyValuePair<string, int>> GetPositionCounts(int id)
        {
            var list = new List<KeyValuePair<string, int>>();
            using (var db = new SpeCalcEntities())
            {
                var projectPositions = db.Projects.Single(x => x.Id == id).ProjectPositions.Where(x => x.Enabled);
                list.Add(new KeyValuePair<string, int>("total", projectPositions.Count()));
                list.Add(new KeyValuePair<string, int>("calced", projectPositions.Count(x => x.ProjectPositionCalculations.Any(y => y.Enabled))));
                list.Add(new KeyValuePair<string, int>("notCalced", projectPositions.Count(x => !x.ProjectPositionCalculations.Any(y => y.Enabled))));
            }
            return list;
        }

        public static IEnumerable<KeyValuePair<string, int>> GetWorkCounts(int id)
        {
            var list = new List<KeyValuePair<string, int>>();
            using (var db = new SpeCalcEntities())
            {
                var projectWorks = db.Projects.Single(x => x.Id == id).ProjectWorks.Where(x => x.Enabled);
                list.Add(new KeyValuePair<string, int>("total", projectWorks.Count()));
                list.Add(new KeyValuePair<string, int>("calced", projectWorks.Count(x => x.ProjectWorkCalculations.Any(y => y.Enabled))));
                list.Add(new KeyValuePair<string, int>("notCalced", projectWorks.Count(x => !x.ProjectWorkCalculations.Any(y => y.Enabled))));
            }
            return list;
        }

        public static void SendNotice2Calculators(int id, AdUser user)
        {
            using (var db = new SpeCalcEntities())
            {
                var posCalcs =
                    db.ProjectPositions.Where(x => x.Enabled && x.ProjectId == id)
                        .GroupBy(x => x.CalculatorSid)
                        .Select(x => x.Key).ToList();
                var workCalcs =
                    db.ProjectWorks.Where(x => x.Enabled && x.ProjectId == id)
                        .GroupBy(x => x.CalculatorSid)
                        .Select(x => x.Key).ToList();
                posCalcs.AddRange(workCalcs);
                var list = posCalcs.Distinct().ToList();
                var emails = new List<MailAddress>();
                foreach (var member in list)
                {
                    var item = new MailAddress(User.GetEmailBySid(member));
                    emails.Add(item);
                }

                string msg = $"{user.DisplayName} уведомляет о необходимости расчитать проект №{id}.<br /><br />Краткая информация о проекте:<br />{ProjectHelper.GetProjectShortInfo(id)}<br /><br />Ссылка: {ProjectHelper.GetProjectLink(id)}";
                MessageHelper.SendMailSmtpAsync($"[Проект №{id}] Расчет проекта", msg, true, null, emails.ToArray());
            }
        }

        public static bool UserProjectGeneral(int id, string userSid)
        {
            bool result = false;
            using (var db = new SpeCalcEntities())
            {
                result = db.Projects.Any(x =>
                    x.Id == id &&

                     (
                      (
                          //Где указан в команде
                          x.ProjectTeams.Any(y => y.Enabled && y.UserSid == userSid && y.ProjectRoles.ProjectGeneral)
                          ||
                          //Его направления
                          (x.SaleDirectionId.HasValue &&
                           db.ProjectSaleDirectionResponsibles.Where(y => y.Enabled && y.UserSid == userSid)
                               .Select(y => y.SaleDirectionId)
                               .Contains(x.SaleDirectionId.Value)
                              )
                          )
                         )

                    );
            }
            return result;
        }

        public static bool UserCanChangeProject(int id, string userSid)
        {
            bool result = false;
            using (var db = new SpeCalcEntities())
            {
                result = db.Projects.Any(x =>
                    x.Id == id &&
                    
                     (
                      (
                          //Где указан в команде
                          x.ProjectTeams.Any(y => y.Enabled && y.UserSid == userSid)
                          ||
                          //Его направления
                          (x.SaleDirectionId.HasValue &&
                           db.ProjectSaleDirectionResponsibles.Where(y => y.Enabled && y.UserSid == userSid)
                               .Select(y => y.SaleDirectionId)
                               .Contains(x.SaleDirectionId.Value)
                              )
                          )
                         )
                        
                    );
            }
            return result;
        }

        public static bool UserCanViewProject(int id, string userSid)
        {
            bool result = false;
            using (var db = new SpeCalcEntities())
            {
                result = db.Projects.Any(x =>
                    x.Id == id &&
                    (
                        
                         (
                          //Где указан в команде
                          x.ProjectTeams.Any(y => y.Enabled && y.UserSid == userSid)
                          ||
                          //Его направления
                          (x.SaleDirectionId.HasValue &&
                           db.ProjectSaleDirectionResponsibles.Where(y => y.Enabled && y.UserSid == userSid)
                               .Select(y => y.SaleDirectionId)
                               .Contains(x.SaleDirectionId.Value)
                              )
                              ||
                              //Продакт
                              (
                              db.ProjectPositions.Any(y=> y.Enabled && y.CalculatorSid==userSid) ||
                              db.ProjectWorks.Any(y => y.Enabled && y.CalculatorSid == userSid)
                              )
                              ||
                             //Менеджер
                             x.ManagerSid == userSid
                             ||
                             //Автор
                             x.CreatorSid == userSid
                         
                         )
                         )
                    );
            }
            return result;
        }

        public static bool UserIsProjectProductonly(int id, string userSid)
        {
            bool result = false;
            using (var db = new SpeCalcEntities())
            {
                result = db.Projects.Any(x =>
                    x.Id == id &&
                    (
                         (
                          //Где указан в команде
                          !x.ProjectTeams.Any(y => y.Enabled && y.UserSid == userSid)
                          &&
                          //Его направления
                          (x.SaleDirectionId.HasValue &&
                           !db.ProjectSaleDirectionResponsibles.Where(y => y.Enabled && y.UserSid == userSid)
                               .Select(y => y.SaleDirectionId)
                               .Contains(x.SaleDirectionId.Value)
                              )
                              &&
                              //Продакт
                              (
                              db.ProjectPositions.Any(y => y.Enabled && y.CalculatorSid == userSid) ||
                              db.ProjectWorks.Any(y => y.Enabled && y.CalculatorSid == userSid)
                              )
                         )
                         
                         )
                    );
            }
            return result;
        }
        
    }
}
