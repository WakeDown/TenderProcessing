using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using Newtonsoft.Json;
using TenderProcessing.Models;
using TenderProcessingDataAccessLayer;
using TenderProcessingDataAccessLayer.Enums;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Controllers
{
    public class ClaimController : Controller
    {
        public ActionResult Index(int? claimId)
        {
            ViewBag.Managers = UserHelper.GetManagers();
            ViewBag.DateStart = DateTime.Now.ToString("dd.MM.yyyy");
            var db = new DbEngine();
            ViewBag.DealTypes = db.LoadDealTypes();
            ViewBag.ClaimStatus = db.LoadClaimStatus();
            ViewBag.ProductManagers = UserHelper.GetProductManagers();
            ViewBag.StatusHistory = new List<ClaimStatusHistory>();
            ViewBag.Facts = db.LoadProtectFacts();
            TenderClaim claim = null;
            if (claimId.HasValue)
            {
                claim = db.LoadTenderClaimById(claimId.Value);
                if (claim != null)
                {
                    var managerFromAd = UserHelper.GetManagerFromActiveDirectoryById(claim.Manager.Id);
                    if (managerFromAd != null)
                    {
                        claim.Manager.Name = managerFromAd.Name;
                    }
                    claim.Positions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value);
                    if (claim.Positions != null && claim.Positions.Any())
                    {
                        var productManagers = claim.Positions.Select(x => x.ProductManager).ToList();
                        foreach (var productManager in productManagers)
                        {
                            var productManagerFromAd = UserHelper.GetProductManagerFromActiveDirectoryById(productManager.Id);
                            if (productManagerFromAd != null)
                            {
                                productManager.Name = productManagerFromAd.Name;
                            }
                        }
                        var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId.Value);
                        if (calculations != null && calculations.Any())
                        {
                            foreach (var position in claim.Positions)
                            {
                                position.Calculations =
                                    calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                            }
                        }
                    }
                    ViewBag.StatusHistory = db.LoadStatusHistoryForClaim(claimId.Value);
                }
            }
            ViewBag.Claim = claim;
            return View();
        }

        public ActionResult List()
        {
            var db = new DbEngine();
            var claims = db.LoadTenderClaims(10);
            db.SetProductManagersForClaims(claims);
            var claimProductManagers = claims.SelectMany(x => x.ProductManagers).ToList();
            foreach (var claimProductManager in claimProductManagers)
            {
                var managerFromAD = UserHelper.GetProductManagerFromActiveDirectoryById(claimProductManager.Id.Trim());
                if (managerFromAD != null)
                {
                    claimProductManager.Name = managerFromAD.Name;
                }
            }
            foreach (var claim in claims)
            {
                var manager = UserHelper.GetManagerFromActiveDirectoryById(claim.Manager.Id);
                if (manager != null)
                {
                    claim.Manager.Name = manager.Name;
                }
            }
            ViewBag.Claims = claims;
            ViewBag.DealTypes = db.LoadDealTypes();
            ViewBag.ClaimStatus = db.LoadClaimStatus();
            ViewBag.ProductManagers = UserHelper.GetProductManagers();
            ViewBag.Managers = UserHelper.GetManagers();
            ViewBag.ClaimCount = db.GetTenderClaimCount();
            return View();
        }

        public ActionResult GetSpecificationFile()
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            try
            {
                var filePath = Path.Combine(Server.MapPath("~"), "App_Data", "Спецификация конкурса.xlsx");
                var newFilePath = Path.Combine(Server.MapPath("~"), "App_Data", Guid.NewGuid() + ".xlsx");
                System.IO.File.Copy(filePath, newFilePath);
                var productManagers = UserHelper.GetProductManagers();
                excBook = new XLWorkbook(newFilePath);
                var workSheet = excBook.Worksheet("Спецификации");
                var userRangeSheet = excBook.Worksheet(2);
                if (workSheet != null && userRangeSheet != null)
                {
                    for (var i = 0; i < productManagers.Count(); i++)
                    {
                        var manager = productManagers[i];
                        var cell = userRangeSheet.Cell(i + 1, 2);
                        if (cell != null)
                        {
                            cell.Value = manager.Name;
                        }
                    }
                    var namedRange = userRangeSheet.Range(userRangeSheet.Cell(1, 2), userRangeSheet.Cell(productManagers.Count(), 2));
                    var workRange = workSheet.Cell(2, 7);
                    if (workRange != null)
                    {
                        var validation = workRange.SetDataValidation();
                        validation.AllowedValues = XLAllowedValues.List;
                        validation.InCellDropdown = true;
                        validation.Operator = XLOperator.Between;
                        validation.List(namedRange);
                    }
                    userRangeSheet.Visibility = XLWorksheetVisibility.Hidden;
                    workSheet.Select();
                    excBook.Save();
                }
                excBook.Dispose();
                excBook = null;
                using (var fs = System.IO.File.OpenRead(newFilePath))
                {
                    var buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, buffer.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                }
                System.IO.File.Delete(newFilePath);
                
            }
            catch (Exception)
            {
                error = true;
            }
            finally
            {
                if (excBook != null)
                {
                    excBook.Dispose();
                }
            }
            if (!error)
            {
                return new FileStreamResult(ms, "application/vnd.ms-excel")
                {
                    FileDownloadName = "Спецификация конкурса.xlsx"
                };
            }
            else
            {
                return View();
            }
        }

        public ActionResult GetListExcelFile(string modelJson)
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            var message = string.Empty;
            try
            {
                var model = new FilterTenderClaim();
                if (!string.IsNullOrEmpty(modelJson))
                {
                    model = JsonConvert.DeserializeObject<FilterTenderClaim>(modelJson);
                }
                var db = new DbEngine();
                var list = db.FilterTenderClaims(model);
                if (list.Any())
                {
                    db.SetProductManagersForClaims(list);
                    var claimProductManagers = list.SelectMany(x => x.ProductManagers).ToList();
                    foreach (var claimProductManager in claimProductManagers)
                    {
                        var managerFromAD = UserHelper.GetProductManagerFromActiveDirectoryById(claimProductManager.Id.Trim());
                        if (managerFromAD != null)
                        {
                            claimProductManager.Name = managerFromAD.Name;
                        }
                    }
                    foreach (var claim in list)
                    {
                        var manager = UserHelper.GetManagerFromActiveDirectoryById(claim.Manager.Id);
                        if (manager != null)
                        {
                            claim.Manager.Name = manager.Name;
                        }
                    }
                    var dealTypes = db.LoadDealTypes();
                    var status = db.LoadClaimStatus();
                    excBook = new XLWorkbook();
                    var workSheet = excBook.AddWorksheet("Заявки");
                    workSheet.Cell(1, 1).Value = "ID";
                    workSheet.Cell(1, 2).Value = "№ Конкурса";
                    workSheet.Cell(1, 3).Value = "Контрагент";
                    workSheet.Cell(1, 4).Value = "Сумма";
                    workSheet.Cell(1, 5).Value = "Менеджер";
                    workSheet.Cell(1, 6).Value = "Снабженцы";
                    workSheet.Cell(1, 7).Value = "Тип сделки";
                    workSheet.Cell(1, 8).Value = "Статус";
                    var headRange = workSheet.Range(workSheet.Cell(1, 1), workSheet.Cell(1, 8));
                    headRange.Style.Font.SetBold(true);
                    headRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    headRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    headRange.Style.Border.SetBottomBorderColor(XLColor.Gray);
                    headRange.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                    headRange.Style.Border.SetTopBorderColor(XLColor.Gray);
                    headRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                    headRange.Style.Border.SetRightBorderColor(XLColor.Gray);
                    headRange.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                    headRange.Style.Border.SetLeftBorderColor(XLColor.Gray);
                    headRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 204, 233, 255);
                    var row = 2;
                    foreach (var claim in list)
                    {
                        workSheet.Cell(row, 1).Value = claim.Id.ToString("G");
                        workSheet.Cell(row, 2).Value = claim.TenderNumber;
                        workSheet.Cell(row, 3).Value = claim.Customer;
                        workSheet.Cell(row, 4).Value = claim.Sum.ToString("N2");
                        workSheet.Cell(row, 5).Value = claim.Manager.Name;
                        workSheet.Cell(row, 6).Value = claim.ProductManagers != null
                            ? string.Join(",", claim.ProductManagers.Select(x => x.Name))
                            : string.Empty;
                        workSheet.Cell(row, 7).Value = dealTypes.First(x => x.Id == claim.DealType).Value;
                        workSheet.Cell(row, 8).Value = status.First(x => x.Id == claim.ClaimStatus).Value; ;
                        row++;
                    }
                    workSheet.Columns(1, 8).AdjustToContents();
                    excBook.SaveAs(ms);
                    excBook.Dispose();
                    ms.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    error = true;
                    message = "Пустой набор";
                }
            }
            catch (Exception)
            {
                error = true;
                message = "Ошибка сервера";
            }
            finally
            {
                if (excBook != null)
                {
                    excBook.Dispose();
                }
            }
            if (!error)
            {
                return new FileStreamResult(ms, "application/vnd.ms-excel")
                {
                    FileDownloadName = "Заявки.xlsx"
                };
            }
            else
            {
                ViewBag.Message = message;
                return View();
            }
        }

        public ActionResult UploadFileForm()
        {
            ViewBag.FirstLoad = true;
            ViewBag.Error = "false";
            ViewBag.Message = string.Empty;
            ViewBag.IdClaim = -1;
            return View();
        }

        [HttpPost]
        public ActionResult UploadFileForm(HttpPostedFileBase file, int claimId)
        {
            var error = false;
            var message = string.Empty;
            XLWorkbook excBook = null;
            Stream inputStream = null;
            var positions = new List<SpecificationPosition>();
            try
            {
                if (file == null || !file.FileName.EndsWith(".xlsx"))
                {
                    error = true;
                    message = "Файл не предоставлен или имеет неверный формат";
                }
                else
                {
                    inputStream = file.InputStream;
                    inputStream.Seek(0, SeekOrigin.Begin);
                    excBook = new XLWorkbook(inputStream);
                    var workSheet = excBook.Worksheet("Спецификации");
                    if (workSheet != null)
                    {
                        var row = 2;
                        var errorStringBuilder = new StringBuilder();
                        var repeatRowCount = 0;
                        var db = new DbEngine();
                        while (true)
                        {
                            var rowValid = true;
                            var model = new SpecificationPosition()
                            {
                                CatalogNumber = string.Empty,
                                Comment = string.Empty,
                                Name = string.Empty,
                                ProductManager = new ProductManager(){Id = string.Empty, Name = string.Empty},
                                Replace = string.Empty,
                                IdClaim = claimId, 
                                State = 1
                            };
                            var numberRange = workSheet.Cell(row, 1);
                            var catalogNumberRange = workSheet.Cell(row, 2);
                            var nameRange = workSheet.Cell(row, 3);
                            var replaceRange = workSheet.Cell(row, 4);
                            var unitRange = workSheet.Cell(row, 5);
                            var valueRange = workSheet.Cell(row, 6);
                            var managerRange = workSheet.Cell(row, 7);
                            var commentRange = workSheet.Cell(row, 8);
                            var priceRange = workSheet.Cell(row, 9);
                            var sumRange = workSheet.Cell(row, 10);
                            if (nameRange != null && nameRange.Value != null)
                            {
                                string nameValue = nameRange.Value.ToString();
                                if (string.IsNullOrEmpty(nameValue))
                                {
                                    break;
                                }
                                model.Name = nameValue;
                            }
                            else
                            {
                                break;
                            }
                            if (numberRange != null && numberRange.Value != null)
                            {
                                string numberValue = numberRange.Value.ToString();
                                if (!string.IsNullOrEmpty(numberValue))
                                {
                                    int intValue;
                                    var isValidInt = int.TryParse(numberValue, out intValue);
                                    if (!isValidInt)
                                    {
                                        rowValid = false;
                                        errorStringBuilder.Append("Строка: " + row +
                                                                  ", значение '" + numberValue + "' в поле Порядковый номер не является целым числом<br/>");
                                    }
                                    else
                                    {
                                        model.RowNumber = intValue;
                                    }
                                }
                            }
                            if (catalogNumberRange != null && catalogNumberRange.Value != null)
                            {
                                model.CatalogNumber = catalogNumberRange.Value.ToString();
                            }
                            if (replaceRange != null && replaceRange.Value != null)
                            {
                                model.Replace = replaceRange.Value.ToString();
                            }
                            if (unitRange != null && unitRange.Value != null)
                            {
                                var value = unitRange.Value.ToString();
                                switch (value)
                                {
                                    case "Шт":
                                        model.Unit = PositionUnit.Thing;
                                        break;
                                    case "Упак":
                                        model.Unit = PositionUnit.Package;
                                        break;
                                    default:
                                        model.Unit = PositionUnit.Thing;
                                        break;
                                }
                            }
                            else
                            {
                                model.Unit = PositionUnit.Thing;
                            }
                            if (valueRange != null)
                            {
                                if (valueRange.Value == null || string.IsNullOrEmpty(valueRange.Value.ToString()))
                                {
                                    rowValid = false;
                                    errorStringBuilder.Append("Строка: " + row +
                                                              ", не задано обязательное значение Количество<br/>");
                                }
                                else
                                {
                                    string valueValue = valueRange.Value.ToString();
                                    int intValue;
                                    var isValidInt = int.TryParse(valueValue, out intValue);
                                    if (!isValidInt)
                                    {
                                        rowValid = false;
                                        errorStringBuilder.Append("Строка: " + row +
                                                                  ", значение '" + valueValue + "' в поле Количество не является целым числом<br/>");
                                    }
                                    else
                                    {
                                        model.Value = intValue;
                                    }
                                }
                            }
                            if (managerRange == null || managerRange.Value == null || string.IsNullOrEmpty(managerRange.Value.ToString()))
                            {
                                rowValid = false;
                                errorStringBuilder.Append("Строка: " + row +
                                                          ", не задано обязательное значение Снабженец<br/>");
                            }
                            else
                            {
                                var managerFromAd =
                                    UserHelper.GetProductManagerFromActiveDirectoryByName(managerRange.Value.ToString());
                                if (managerFromAd != null) model.ProductManager = managerFromAd;
                            }
                            if (commentRange != null && commentRange.Value != null)
                            {
                                model.Comment = commentRange.Value.ToString();
                            }
                            if (priceRange != null && priceRange.Value != null)
                            {
                                string priceValue = priceRange.Value.ToString();
                                if (!string.IsNullOrEmpty(priceValue))
                                {
                                    double doubleValue;
                                    var isValidDouble = double.TryParse(priceValue, out doubleValue);
                                    if (!isValidDouble)
                                    {
                                        rowValid = false;
                                        errorStringBuilder.Append("Строка: " + row +
                                                                  ", значение '" + priceValue + "' в поле Цена за единицу не является числом<br/>");
                                    }
                                    else
                                    {
                                        model.Price = doubleValue;
                                    }
                                }
                            }
                            if (sumRange != null && sumRange.Value != null)
                            {
                                string sumValue = sumRange.Value.ToString();
                                if (!string.IsNullOrEmpty(sumValue))
                                {
                                    double doubleValue;
                                    var isValidDouble = double.TryParse(sumValue, out doubleValue);
                                    if (!isValidDouble)
                                    {
                                        rowValid = false;
                                        errorStringBuilder.Append("Строка: " + row +
                                                                  ", значение '" + sumValue + "' в поле Сумма не является числом<br/>");
                                    }
                                    else
                                    {
                                        model.Sum = doubleValue;
                                    }
                                }
                            }
                            if (rowValid)
                            {
                                var isUnique = IsPositionUnique(model, positions);
                                if (isUnique)
                                {
                                    isUnique = db.ExistsSpecificationPosition(model);
                                }
                                if (isUnique)
                                {
                                    positions.Add(model);
                                }
                                else
                                {
                                    repeatRowCount++;
                                }
                            }
                            row++;
                        }
                        foreach (var position in positions)
                        {
                            db.SaveSpecificationPosition(position);
                        }
                        message = "Получено строк: " + (row - 2);
                        if (repeatRowCount > 0)
                        {
                            message += "<br/>Из них повторных: " + repeatRowCount;
                        }
                        if (positions.Any())
                        {
                            message += "<br/>Сохранено строк: " + positions.Count();
                        }
                        var errorMessage = errorStringBuilder.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            message += "<br/>Ошибки:<br />" + errorMessage;
                        }
                    }
                    else
                    {
                        error = true;
                        message = "Не найден рабочий лист со спецификациями";
                    }
                    excBook.Dispose();
                    excBook = null;
                }
            }
            catch (Exception)
            {
                error = true;
                message = "Ошибка сервера";
            }
            finally
            {
                if (inputStream != null)
                {
                    inputStream.Dispose();
                }
                if (excBook != null)
                {
                    excBook.Dispose();
                }
            }
            ViewBag.FirstLoad = false;
            ViewBag.Error = error.ToString().ToLowerInvariant();
            ViewBag.Message = message;
            ViewBag.Positions = positions;
            ViewBag.IdClaim = claimId;
            return View();
        }

        [HttpPost]
        public JsonResult SaveClaim(TenderClaim model)
        {
            var isComplete = false;
            ClaimStatusHistory statusHistory = null;
            try
            {
                model.KPDeadline = DateTime.ParseExact(model.KPDeadlineString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                model.ClaimDeadline = DateTime.ParseExact(model.ClaimDeadlineString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                model.TenderStart = DateTime.ParseExact(model.TenderStartString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                var modelValid = true;
                if (string.IsNullOrEmpty(model.Customer) || string.IsNullOrEmpty(model.CustomerInn)) modelValid = false;
                if (modelValid)
                {
                    var db = new DbEngine();
                    model.ClaimStatus = 1;
                    model.TenderStatus = 1;
                    model.Deleted = false;
                    model.RecordDate = DateTime.Now;
                    isComplete = db.SaveTenderClaim(model);
                    if (isComplete)
                    {
                        model.ClaimDeadlineString = model.ClaimDeadline.ToString("dd.MM.yyyy");
                        model.TenderStartString = model.TenderStart.ToString("dd.MM.yyyy");
                        model.KPDeadlineString = model.KPDeadline.ToString("dd.MM.yyyy");
                        statusHistory = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdClaim = model.Id,
                            IdUser = string.Empty,
                            Status = new ClaimStatus() {Id = model.ClaimStatus},
                            Comment = string.Empty
                        };
                        db.SaveClaimStatusHistory(statusHistory);
                        statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                    }
                }
            }
            catch (Exception ex)
            {
                isComplete = false;
                Log(ex.Message);
            }
            return Json(new {IsComplete = isComplete, Model = model, StatusHistory = statusHistory});
        }

        [HttpPost]
        public JsonResult EditClaimPosition(SpecificationPosition model)
        {
            var isComplete = false;
            try
            {
                model.State = 1;
                var modelValid = true;
                if (string.IsNullOrEmpty(model.Name)) modelValid = false;
                if (modelValid)
                {
                    var db = new DbEngine();
                    isComplete = db.UpdateSpecificationPosition(model);
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete });
        }

        public JsonResult DeleteClaimPosition(int id)
        {
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                isComplete = db.DeleteSpecificationPosition(id);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteClaim(int id)
        {
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                isComplete = db.DeleteTenderClaim(id);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FilterClaim(FilterTenderClaim model)
        {
            var isComplete = false;
            var list = new List<TenderClaim>();
            var count = -1;
            try
            {
                var db = new DbEngine();
                list = db.FilterTenderClaims(model);
                if (list.Any())
                {
                    db.SetProductManagersForClaims(list);
                    var claimProductManagers = list.SelectMany(x => x.ProductManagers).ToList();
                    foreach (var claimProductManager in claimProductManagers)
                    {
                        var managerFromAD = UserHelper.GetProductManagerFromActiveDirectoryById(claimProductManager.Id.Trim());
                        if (managerFromAD != null)
                        {
                            claimProductManager.Name = managerFromAD.Name;
                        }
                    }
                    foreach (var claim in list)
                    {
                        var manager = UserHelper.GetManagerFromActiveDirectoryById(claim.Manager.Id);
                        if (manager != null)
                        {
                            claim.Manager.Name = manager.Name;
                        }
                    }
                }
                count = db.GetCountFilteredTenderClaims(model);
                isComplete = true;
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Claims = list, Count = count });
        }

        [HttpPost]
        public JsonResult AddClaimPosition(SpecificationPosition model)
        {
            var isComplete = false;
            try
            {
                model.State = 1;
                var modelValid = true;
                if (string.IsNullOrEmpty(model.Name)) modelValid = false;
                if (modelValid)
                {
                    var db = new DbEngine();
                    isComplete = db.SaveSpecificationPosition(model);
                    if (string.IsNullOrEmpty(model.CatalogNumber)) model.CatalogNumber = string.Empty;
                    if (string.IsNullOrEmpty(model.Replace)) model.Replace = string.Empty;
                    if (string.IsNullOrEmpty(model.Comment)) model.Comment = string.Empty;
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }

        public JsonResult SetClaimOnWork(int id)
        {
            var isComplete = false;
            var message = string.Empty;
            ClaimStatusHistory model = null;
            try
            {
                var db = new DbEngine();
                var hasPosition = db.HasClaimPosition(id);
                if (hasPosition)
                {
                    isComplete = db.ChangeTenderClaimClaimStatus(new TenderClaim() {Id = id, ClaimStatus = 2});
                    var productManagers = db.LoadProductManagersForClaim(id);
                    if (productManagers != null && productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        foreach (var productManager in productManagers)
                        {
                            var productManagerFromAd =
                                productManagersFromAd.FirstOrDefault(x => x.Id == productManager.Id);
                            if (productManagerFromAd != null)
                            {
                                productManager.Name = productManagerFromAd.Name;
                            }
                        }
                        var comment = string.Join(",", productManagers.Select(x => x.Name));
                        model = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdClaim = id,
                            IdUser = string.Empty,
                            Status = new ClaimStatus() { Id = 2 },
                            Comment = comment
                        };
                        db.SaveClaimStatusHistory(model);
                        model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                    }
                }
                else
                {
                    message = "Невозможно передать заявку в работу без позиций спецификаций";
                }

            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Message = message, Model = model }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetClaimCancelled(ClaimStatusHistory model)
        {
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                isComplete = db.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = model.IdClaim, ClaimStatus = 5 });
                if (isComplete)
                {
                    model.Date = DateTime.Now;
                    model.IdUser = string.Empty;
                    model.Status = new ClaimStatus() {Id = 5};
                    db.SaveClaimStatusHistory(model);
                    model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }

        [HttpPost]
        public JsonResult SetClaimStopped(ClaimStatusHistory model)
        {
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                isComplete = db.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = model.IdClaim, ClaimStatus = 4 });
                if (isComplete)
                {
                    model.Date = DateTime.Now;
                    model.IdUser = string.Empty;
                    model.Status = new ClaimStatus() { Id = 4 };
                    db.SaveClaimStatusHistory(model);
                    model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }

        [HttpPost]
        public JsonResult SetClaimContinued(ClaimStatusHistory model)
        {
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                var statusHistory = db.LoadStatusHistoryForClaim(model.IdClaim);
                if (statusHistory.Count() > 1)
                {
                    var lastValueValid = statusHistory.Last().Status.Id == 4;
                    if (lastValueValid)
                    {
                        var actualStatus = statusHistory[statusHistory.Count() - 2];
                        isComplete = db.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = model.IdClaim, ClaimStatus = actualStatus.Status.Id });
                        if (isComplete)
                        {
                            model.Date = DateTime.Now;
                            model.IdUser = string.Empty;
                            model.Status = new ClaimStatus() { Id = actualStatus.Status.Id };
                            if (string.IsNullOrEmpty(model.Comment)) model.Comment = "Возобновление заявки";
                            db.SaveClaimStatusHistory(model);
                            model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                        }
                    }
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }

        private bool IsPositionUnique(SpecificationPosition model, List<SpecificationPosition> list)
        {
            var isUnique = true;
            foreach (var position in list)
            {
                if (model.CatalogNumber == position.CatalogNumber &&
                    model.Comment == position.Comment &&
                    model.Name == position.Name &&
                    model.Price.Equals(position.Price) &&
                    model.ProductManager == position.ProductManager &&
                    model.Replace == position.Replace &&
                    model.RowNumber == position.RowNumber &&
                    model.Sum.Equals(position.Sum) &&
                    model.Unit == position.Unit &&
                    model.Value == position.Value)
                {
                    isUnique = false;
                    break;
                }
            }
            return isUnique;
        }

        private void Log(string message)
        {
            var filePath = Path.Combine(Server.MapPath("~"), "App_Data", "log.txt");
            var fileMode = FileMode.Append;
            if (!System.IO.File.Exists(filePath))
            {
                fileMode = FileMode.CreateNew;
            }
            using (var stream = System.IO.File.Open(filePath, fileMode, FileAccess.Write))
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
                writer.Flush();
                stream.Flush();
            }
        }
    }
}