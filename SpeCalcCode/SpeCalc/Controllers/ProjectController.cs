using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult New(Projects project)
        {
            if (ModelState.IsValid)
            {
                var manager = AdHelper.GetUserBySid(project.ManagerSid);
                project.ManagerName = manager.DisplayName;
                project.ManagerDepartmentName = manager.DepartmentName;
                project.ManagerChiefName = manager.ChiefName;
                project.ManagerChiefSid = manager.ChiefSid;
                ProjectModel.Create(project, CurUser);
                return RedirectToAction("Card", project.Id);
                
            }

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

        public PartialViewResult GetPositions(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectPositionModel.GetListWithCalc(id.Value);
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
        public PartialViewResult GetWorks(int? id)
        {
            if (!id.HasValue) return null;
            var model = ProjectWorkModel.GetListWithCalc(id.Value);
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
        public JsonResult GetSaleSubjectSelectionList(int id)
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
    }
}