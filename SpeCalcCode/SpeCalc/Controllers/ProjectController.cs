using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using SpeCalc.Helpers;
using SpeCalc.Objects;
using SpeCalcDataAccessLayer;
using SpeCalcDataAccessLayer.Models;
using SpeCalcDataAccessLayer.ProjectModels;

namespace SpeCalc.Controllers
{
    public class ProjectController : BaseController
    {
        // GET: Project
        public ActionResult Index()
        {
                var list = ProjectModel.GetList();
                var result = new ListResult<Projects>() {List = list };
                return View(result);
        }

        [HttpGet]
        public ActionResult New()
        {
            var model = new Projects();
            model.HasBudget = true;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(Projects project)
        {
            //if (ModelState.IsValid)
            //{
                var manager = AdHelper.GetUserBySid(project.ManagerSid);
                project.ManagerName = manager.DisplayName;
                project.ManagerDepartmentName = manager.DepartmentName;
                project.ManagerChiefName = manager.ChiefName;
                project.ManagerChiefSid = manager.ChiefSid;
                int id = ProjectModel.Create(project, CurUser);
                return RedirectToAction("Card", new{ id= id});
                
            //}

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Projects project)
        {
            //if (ModelState.IsValid)
            //{
                var manager = AdHelper.GetUserBySid(project.ManagerSid);
                project.ManagerName = manager.DisplayName;
                project.ManagerDepartmentName = manager.DepartmentName;
                project.ManagerChiefName = manager.ChiefName;
                project.ManagerChiefSid = manager.ChiefSid;
                ProjectModel.Update(project, CurUser);
            //}

            return RedirectToAction("Card",new {id= project.Id});
        }

        [HttpGet]
        public ActionResult Card(int? id)
        {
            if (!id.HasValue) return RedirectToAction("New");
           var model = ProjectModel.GetFat(id.Value);
            return View(model);
        }

        public PartialViewResult GetCalculationEdit(int? id)
        {
            if (!id.HasValue) return null;
            var model = new ProjectPositionCalculations();
            if (id > 0) model = ProjectPositionCalculationModel.Get(id.Value);
            return PartialView("PositionCalculationEdit", model);
        }

        public PartialViewResult GetPositions(int? id, bool? calced)
        {
            if (!id.HasValue) return null;
            var model = ProjectPositionModel.GetListWithCalc(id.Value, calced);
            return PartialView("Positions", model);
        }

        

        public PartialViewResult GetPositionEdit(int? id)
        {
            if (!id.HasValue) return null;
            var model = new ProjectPositionModel();
            if (id.Value > 0) model = ProjectPositionModel.GetWithCalc(id.Value);
            return PartialView("PositionEdit", model);
        }

        public PartialViewResult GetPosition(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectPositionModel.GetWithCalc(id.Value);
            return PartialView("Position", model);
        }

        public PartialViewResult GetPositionNew()
        {
            return PartialView("PositionNew");
        }

        [HttpPost]
        public JsonResult CreatePosition(ProjectPositions model)
        {
            if (!String.IsNullOrEmpty(model.CalculatorSid)) model.CalculatorName = AdHelper.GetUserBySid(model.CalculatorSid).DisplayName;

            int id = ProjectPositionModel.Create(model, CurUser);
            return Json(new { id });
        }

        [HttpPost]
        public JsonResult SavePosition(ProjectPositions model)
        {
            if (!String.IsNullOrEmpty(model.CalculatorSid)) model.CalculatorName = AdHelper.GetUserBySid(model.CalculatorSid).DisplayName;

            ProjectPositionModel.Save(model, CurUser);
            return Json(new { });
        }

        public PartialViewResult GetCalculation(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectPositionCalculationModel.Get(id.Value);
            return PartialView("PositionCalculation", model);
        }

        [HttpPost]
        public JsonResult DeletePosition(int id)
        {
            ProjectPositionModel.Delete(id, CurUser);
            return Json(new { });
        }

        [HttpPost]
        public JsonResult DeletePositions(int[] ids)
        {
            ProjectPositionModel.Delete(ids, CurUser);
            return Json(new { });
        }

        [HttpPost]
        public JsonResult CreateCalculation(ProjectPositionCalculations model)
        {
            int id = ProjectPositionCalculationModel.Create(model, CurUser);
            return Json(new { id });
        }

        [HttpPost]
        public JsonResult SaveCalculation(ProjectPositionCalculations model)
        {
            int id = 0;
            if (model.Id > 0)
            {
                id = ProjectPositionCalculationModel.Save(model, CurUser);
            }
            else
            {
                id = ProjectPositionCalculationModel.Create(model, CurUser);
            }
            return Json(new { id });
        }

        [HttpPost]
        public JsonResult DeleteCalculation(int id)
        {
            ProjectPositionCalculationModel.Delete(id, CurUser);
            return Json(new { });
        }

        //Работы
        public PartialViewResult GetWorks(int? id, bool? calced)
        {
            if (!id.HasValue) return null;
            var model = ProjectWorkModel.GetListWithCalc(id.Value, calced);
            return PartialView("Works", model);
        }

        public PartialViewResult GetWorkCalculationEdit(int? id)
        {
            if (!id.HasValue) return null;
            var model = new ProjectWorkCalculations();
            if (id > 0) model = ProjectWorkCalculationModel.Get(id.Value);
            return PartialView("WorkCalculationEdit", model);
        }

        public PartialViewResult GetWorkEdit(int? id)
        {
            if (!id.HasValue) return null;
            var model = new ProjectWorkModel();
            if (id.Value > 0) model = ProjectWorkModel.GetWithCalc(id.Value);
            return PartialView("WorkEdit", model);
        }

        public PartialViewResult GetWork(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectWorkModel.GetWithCalc(id.Value);
            return PartialView("Work", model);
        }

        public PartialViewResult GetWorkNew()
        {
            return PartialView("WorkNew");
        }

        [HttpPost]
        public JsonResult CreateWork(ProjectWorks model)
        {
            if (!String.IsNullOrEmpty(model.CalculatorSid)) model.CalculatorName = AdHelper.GetUserBySid(model.CalculatorSid).DisplayName;

            int id = ProjectWorkModel.Create(model, CurUser);
            return Json(new { id });
        }

        [HttpPost]
        public JsonResult SaveWork(ProjectWorks model)
        {
            if (!String.IsNullOrEmpty(model.CalculatorSid)) model.CalculatorName = AdHelper.GetUserBySid(model.CalculatorSid).DisplayName;

            ProjectWorkModel.Save(model, CurUser);
            return Json(new { });
        }

        public PartialViewResult GetWorkCalculation(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectWorkCalculationModel.Get(id.Value);
            return PartialView("WorkCalculation", model);
        }

        [HttpPost]
        public JsonResult DeleteWork(int id)
        {
            ProjectWorkModel.Delete(id, CurUser);
            return Json(new { });
        }

        [HttpPost]
        public JsonResult DeleteWorks(int[] ids)
        {
            ProjectWorkModel.Delete(ids, CurUser);
            return Json(new { });
        }

        [HttpPost]
        public JsonResult CreateWorkCalculation(ProjectWorkCalculations model)
        {
            int id = ProjectWorkCalculationModel.Create(model, CurUser);
            return Json(new { id });
        }

        [HttpPost]
        public JsonResult SaveWorkCalculation(ProjectWorkCalculations model)
        {
            int id = 0;
            if (model.Id > 0)
            {
                id = ProjectWorkCalculationModel.Save(model, CurUser);
            }
            else
            {
                id = ProjectWorkCalculationModel.Create(model, CurUser);
            }
            return Json(new { id });
        }

        [HttpPost]
        public JsonResult DeleteWorkCalculation(int id)
        {
            ProjectWorkCalculationModel.Delete(id, CurUser);
            return Json(new { });
        }

        //

        public PartialViewResult GetEdit(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectModel.Get(id.Value);
            return PartialView("Edit", model);
        }

        public PartialViewResult GetInfo(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectModel.GetFat(id.Value);
            return PartialView("Info", model);
        }
        [HttpPost]
        public JsonResult GetSaleSubjectSelectionList(int? id = null)
        {
            var list = ProjectHelper.GetSaleSubjectSelectionList(id);
            return Json(list);
        }

        public PartialViewResult GetFolders(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectFolderModel.GetListWithFiles(id.Value);
            return PartialView("Folders", model);
        }
        
        [HttpPost]
        public JsonResult UploadFile(int pid, int fid)
        {
            var files = Request.Files;
            if (files != null & files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    byte[] data = null;
                    using (var br = new BinaryReader(file.InputStream))
                    {
                        data = br.ReadBytes(file.ContentLength);
                    }
                    ProjectFileModel.SaveFile(data, file.FileName, pid, fid, CurUser);
                }
            }
            return Json(new {});
        }

        [HttpGet]
        public PartialViewResult GetFolderFiles(int? fid, int? pid)
        {
            if (!fid.HasValue || !pid.HasValue) return null;

            var model = ProjectFolderModel.GetFileList(pid.Value, fid.Value);
            return PartialView("FolderFiles", model);
        }

        public ActionResult GetFileData(string guid)
        {
            var file = ProjectFileModel.Get(guid);
            string ext = file.FileName.Substring(file.FileName.LastIndexOf(".", StringComparison.Ordinal));
            string name = file.FileName.Substring(0, file.FileName.LastIndexOf(".", StringComparison.Ordinal));
            string fileName = $"{name}_v{file.VersionNumber}{ext}";
            return File(file.fileDATA, "text/plain", fileName);
        }

        public PartialViewResult GetFolderFilesHistory(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectFolderModel.GetListWithFilesHistory(id.Value);
            return PartialView("FolderFilesHistory", model);
        }

        public PartialViewResult GetFilesHistory(int? fid, int? pid)
        {
            if (!fid.HasValue || !pid.HasValue) return null;
            var model = ProjectFolderModel.GetFileListHistory(pid.Value, fid.Value);
            return PartialView("FilesHistory", model);
        }

        public PartialViewResult GetMessages(int? id)
        {
            if (!id.HasValue) return null;
            int totalCount;
            var model = ProjectMessageModel.GetList(out totalCount, id.Value, false);
            ViewBag.TotalCount = totalCount;
            ViewBag.Full = false;
            return PartialView("Messages", model);
        }

        public PartialViewResult GetMessagesHistory(int? id)
        {
            if (!id.HasValue) return null;
            int totalCount;
            var model = ProjectMessageModel.GetList(out totalCount, id.Value, true);
            ViewBag.TotalCount = totalCount;
            ViewBag.Full = true;
            return PartialView("Messages", model);
        }

        [HttpPost]
        public JsonResult SendMessage(int id, string message)
        {
            ProjectMessageModel.Send(id, message, CurUser);

            return Json(new {});
        }

        public PartialViewResult GetHistory(int? id)
        {
            if (!id.HasValue) return null;
            int totalCount;
            var model = ProjectStateModel.GetList(out totalCount, id.Value, false);
            ViewBag.TotalCount = totalCount;
            ViewBag.Full = false;
            return PartialView("History", model);
        }

        public PartialViewResult GetAllHistory(int? id)
        {
            if (!id.HasValue) return null;
            int totalCount;
            var model = ProjectStateModel.GetList(out totalCount, id.Value, true);
            ViewBag.TotalCount = totalCount;
            ViewBag.Full = true;
            return PartialView("History", model);
        }

        [HttpPost]
        public JsonResult Play(int id, string comment)
        {
            ProjectModel.SetPlayState(id, comment, CurUser);
            return Json(new {});
        }

        [HttpPost]
        public JsonResult Pause(int id, string comment)
        {
            ProjectModel.SetPauseState(id, comment, CurUser);
            return Json(new { });
        }

        [HttpPost]
        public JsonResult Stop(int id, string comment)
        {
            ProjectModel.SetStopState(id, comment, CurUser);
            return Json(new { });
        }

        public PartialViewResult GetIndicatorBig(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectConditionModel.GetList(id.Value);
            return PartialView("IndicatorBig", model);
        }
        [HttpPost]
        public JsonResult ChangeCondition(int pid, int cid, string comment)
        {
            ProjectModel.SetCondition(pid, cid, comment, CurUser);
            return Json(new {});
        }
        [HttpPost]
        public JsonResult DeleteFile(string guid)
        {
            ProjectFileModel.Delete(guid, CurUser);
            return Json(new { });
        }

        [HttpPost]
        public JsonResult GetPositionCounts(int id)
        {
            var list = ProjectModel.GetPositionCounts(id);
            return Json(new
            {
                total = list.SingleOrDefault(x => x.Key == "total").Value,
                calced = list.SingleOrDefault(x => x.Key == "calced").Value,
                notCalced = list.SingleOrDefault(x => x.Key == "notCalced").Value
            });
        }

        [HttpPost]
        public JsonResult GetWorkCounts(int id)
        {
            var list = ProjectModel.GetWorkCounts(id);
            return Json(new
            {
                total = list.SingleOrDefault(x => x.Key == "total").Value,
                calced = list.SingleOrDefault(x => x.Key == "calced").Value,
                notCalced = list.SingleOrDefault(x => x.Key == "notCalced").Value
            });
        }

        public ActionResult GetCalculationExcel(int? id)
        {
            if (!id.HasValue) return null;

            var project = ProjectModel.GetFat(id.Value);

            var ms = new MemoryStream();
            var filePath = Path.Combine(Server.MapPath("~"), "App_Data", "ProjectCalculation.xlsx");
            using (var fs = System.IO.File.OpenRead(filePath))
            {
                var buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Count());
                ms.Write(buffer, 0, buffer.Count());
                ms.Seek(0, SeekOrigin.Begin);
            }

            XLWorkbook wb = new XLWorkbook(ms);
            var ws = wb.Worksheet(1);

            int r = 1;
            int c = 4;
            ws.Cell(r, c).Value = project.ClientName;
            ws.Cell(++r, c).Value = project.HasBudget ? $"{project.Budget:F2}" + (project.CurrencyId.HasValue ? project.ProjectCurrencies.Name : null) : "неизвествен";
            ws.Cell(++r, c).Value = project.SaleDirectionId.HasValue ?  project.ProjectSaleDirections.Name : null;
            ws.Cell(++r, c).Value = project.SaleSubjectId.HasValue ? project.ProjectSaleSubjects.Name : null;
            ws.Cell(++r, c).Value = project.ManagerName;
            ws.Cell(++r, c).Value = project.Comment;


            var positions = ProjectPositionModel.GetListWithCalc(id.Value);
            
            int i = 0;
            r++;
            
            foreach (var pos in positions)
            {
                r++;
                i++;
                c = 0;
                ws.Cell(r, ++c).Value = i;
                ws.Cell(r, ++c).Value = pos.Position.CatalogNumber;
                ws.Cell(r, ++c).Value = pos.Position.Vendor;
                ws.Cell(r, ++c).Value = pos.Position.Name;
                ws.Cell(r, ++c).Value = pos.Position.Quantity;
                ws.Cell(r, ++c).Value = pos.Position.ProjectPositionQuantityUnits.Name;
                ws.Cell(r, ++c).Value = pos.Position.CalculatorName;
                var calcCol = c;

                if (pos.HasCalculations)
                {
                    foreach (var calc in pos.Calculations)
                    {
                        c = calcCol;
                        ws.Cell(r, ++c).Value = calc.CatalogNumber;
                        ws.Cell(r, ++c).Value = calc.Name;
                        ws.Cell(r, ++c).Value = calc.DeliveryTimeId.HasValue
                            ? calc.ProjectPositionDeliveryTimes.Name
                            : null;
                        ws.Cell(r, ++c).Value = calc.Cost.HasValue ? calc.Cost.Value.ToString("F2") : null;
                        ws.Cell(r, ++c).Value = calc.CurrencyId.HasValue ? calc.ProjectCurrencies.Name : null;
                        ws.Cell(r, ++c).Value = calc.RecomendedPrice.HasValue
                            ? calc.RecomendedPrice.Value.ToString("F2")
                            : null;
                        ws.Cell(r, ++c).Value = calc.Provider;
                        ws.Cell(r, ++c).Value = calc.ProtectionFactId.HasValue ? calc.ProjectProtectionFacts.Name : null;
                        ws.Cell(r, ++c).Value = calc.ProtectionFactCondition;
                        ws.Cell(r, ++c).Value = calc.Comment;
                        r++;
                    }
                    r--;
                }
            }

            var works = ProjectWorkModel.GetListWithCalc(id.Value);

            foreach (var work in works)
            {
                r++;
                i++;
                c = 0;
                ws.Cell(r, ++c).Value = i;
                c++;//ws.Cell(r, ++c).Value = pos.Position.CatalogNumber;
                c++;//ws.Cell(r, ++c).Value = pos.Position.Vendor;
                ws.Cell(r, ++c).Value = work.Work.Name;
                ws.Cell(r, ++c).Value = work.Work.Quantity;
                ws.Cell(r, ++c).Value = work.Work.ProjectWorkQuantityUnits.Name;
                ws.Cell(r, ++c).Value = work.Work.CalculatorName;
                var calcCol = c;
                if (work.HasCalculations)
                {
                    foreach (var calc in work.Calculations)
                    {
                        c = calcCol;
                        c++; //ws.Cell(r, ++c).Value = calc.ExecutorName;
                        ws.Cell(r, ++c).Value = calc.Name;
                        ws.Cell(r, ++c).Value = calc.ExecutionTimeId.HasValue
                            ? calc.ProjectWorkExecutinsTimes.Name
                            : null;
                        ws.Cell(r, ++c).Value = calc.Cost.HasValue ? calc.Cost.Value.ToString("F2") : null;
                        ws.Cell(r, ++c).Value = calc.CurrencyId.HasValue ? calc.ProjectCurrencies.Name : null;
                        c++;
                            //ws.Cell(r, ++c).Value = calc.RecomendedPrice.HasValue ? calc.RecomendedPrice.Value.ToString("F2") : null;
                        ws.Cell(r, ++c).Value = calc.ExecutorName;
                        c++;
                            //ws.Cell(r, ++c).Value = calc.ProtectionFactId.HasValue ? calc.ProjectProtectionFacts.Name : null;
                        c++; //ws.Cell(r, ++c).Value = calc.ProtectionFactCondition;
                        ws.Cell(r, ++c).Value = calc.Comment;
                        r++;
                    }
                    r--;
                }
            }


            wb.SaveAs(ms);
            wb.Dispose();
            ms.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(ms, "application/vnd.ms-excel")
            {
                FileDownloadName = $"ProjectCalculation_{project.Id}.xlsx"
            };
        }

        public PartialViewResult GetTeam(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectTeamModel.GetList(id.Value);
            return PartialView("Team", model);
        }
        [HttpPost]
        public JsonResult Add2Team(int pid, int rid, string userSid)
        {
            string userName = AdHelper.GetUserBySid(userSid).DisplayName;
            ProjectTeamModel.Create(pid, rid, userSid, userName, CurUser);
            return Json(new {});
        }

        public PartialViewResult GetRoleTeam(int? id, int? rid)
        {
            if (!id.HasValue || !rid.HasValue) return null;
            var model = ProjectTeamModel.GetRoleList(id.Value, rid.Value);
            return PartialView("RoleTeamList", model);
        }

        [HttpPost]
        public JsonResult DeleteFromRoleTeam(int mid)
        {
            ProjectTeamModel.Delete(mid, CurUser);
            return Json(new { });
        }
    }
}