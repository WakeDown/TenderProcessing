using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using Newtonsoft.Json;
using TenderProcessing.Helpers;
using TenderProcessingDataAccessLayer;
using TenderProcessingDataAccessLayer.Enums;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Controllers
{
    [Authorize]
    public class ClaimController : Controller
    {
        //форма заявки, если передан параметр idClaim, то загружается инфа по заявки с этим id
        public ActionResult Index(int? claimId)
        {
            //получения текущего юзера и проверка наличия у него доступа к странице
            ViewBag.Error = false.ToString().ToLower();
            var user = GetUser();
            if (user == null || !UserHelper.IsUserAccess(user))
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к приложению");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }
            ViewBag.UserName = user.Name;
            var isController = UserHelper.IsController(user);
            var isManager = UserHelper.IsManager(user);
            var isOperator = UserHelper.IsOperator(user);
            if (!isController && !isManager && !isOperator)
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к этой странице");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }
            try
            {
                //получение необходимой инфы из БД и ActiveDirectory
                var managers = UserHelper.GetManagers();
                ViewBag.Managers = managers;
                ViewBag.DateStart = DateTime.Now.ToString("dd.MM.yyyy");
                var db = new DbEngine();
                ViewBag.DealTypes = db.LoadDealTypes();
                ViewBag.ClaimStatus = db.LoadClaimStatus();
                var adProductManagers = UserHelper.GetProductManagers();
                ViewBag.ProductManagers = adProductManagers;
                ViewBag.StatusHistory = new List<ClaimStatusHistory>();
                ViewBag.Facts = db.LoadProtectFacts();
                ViewBag.HasTransmissedPosition = false.ToString().ToLower();
                TenderClaim claim = null;
                if (claimId.HasValue)
                {
                    claim = db.LoadTenderClaimById(claimId.Value);
                    if (claim != null)
                    {
                        ViewBag.HasTransmissedPosition = db.HasTenderClaimTransmissedPosition(claimId.Value).ToString().ToLower();
                        //проверка наличия доступа к данной заявке
                        if (!isController)
                        {
                            if (isManager)
                            {
                                if (claim.Manager.Id != user.Id && claim.Author != user.Id)
                                {
                                    var dict = new RouteValueDictionary();
                                    dict.Add("message", "У Вас нет доступа к этой странице");
                                    return RedirectToAction("ErrorPage", "Auth", dict);
                                }
                            }
                            else
                            {
                                if (claim.Author != user.Id)
                                {
                                    var dict = new RouteValueDictionary();
                                    dict.Add("message", "У Вас нет доступа к этой странице");
                                    return RedirectToAction("ErrorPage", "Auth", dict);
                                }
                            }
                        }
                        var managerFromAd = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
                        if (managerFromAd != null)
                        {
                            claim.Manager.Name = managerFromAd.Name;
                        }
                        //получение позиций по заявке и расчета к ним
                        claim.Positions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value);
                        if (claim.Positions != null && claim.Positions.Any())
                        {
                            var productManagers = claim.Positions.Select(x => x.ProductManager).ToList();
                            foreach (var productManager in productManagers)
                            {
                                var productManagerFromAd =
                                    adProductManagers.FirstOrDefault(x => x.Id == productManager.Id);
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
                                    if (position.State == 1) continue;
                                    position.Calculations =
                                        calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                                }
                            }
                        }
                        ViewBag.StatusHistory = db.LoadStatusHistoryForClaim(claimId.Value);
                    }
                }
                ViewBag.Claim = claim;
            }
            catch (Exception)
            {
                ViewBag.Error = true.ToString().ToLower();
            }
            return View();
        }

        //список заявок
        public ActionResult List()
        {
            //получение пользователя и через наличие у него определенных ролей, определяются настройки по 
            //функциональности на странице
            var user = GetUser();
            if (user == null || !UserHelper.IsUserAccess(user))
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к приложению");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }
            ViewBag.UserName = user.Name;
            var showCalculate = false;
            var showEdit = false;
            var changeTenderStatus = false;
            var filterProduct = string.Empty;
            var filterManager = string.Empty;
            var clickAction = string.Empty;
            var posibleAction = string.Empty;
            var userId = string.Empty;
            var author = string.Empty;
            var reportExcel = false;
            var deleteClaim = "none";
            var newClaim = "true";
            var filterClaimStatus = new List<int>();
            var isController = UserHelper.IsController(user);
            var isTenderStatus = UserHelper.IsTenderStatus(user);
            var isManager = UserHelper.IsManager(user);
            var isProduct = UserHelper.IsProductManager(user);
            var isOperator = UserHelper.IsOperator(user);
            if (isController)
            {
                showCalculate = true;
                showEdit = true;
                changeTenderStatus = true;
                clickAction = "editClaim";
                posibleAction = "all";
                reportExcel = true;
                deleteClaim = "true";
            }
            else
            {
                if (isTenderStatus)
                {
                    changeTenderStatus = true;
                    clickAction = "null";
                    posibleAction = "null";
                    newClaim = "false";
                }
                if (isManager)
                {
                    showEdit = true;
                    filterManager = user.Id;
                    clickAction = "editClaim";
                    posibleAction = "editClaim";
                    userId = user.Id;
                    filterClaimStatus.AddRange(new [] { 1,2,3,6,7 });
                    deleteClaim = "self&manager";
                }
                if (isProduct)
                {
                    showCalculate = true;
                    filterProduct = user.Id;
                    clickAction = "calculateClaim";
                    posibleAction = (isManager ? "all" : "calculateClaim");
                    userId = user.Id;
                    newClaim = "false";
                    if (!isManager) filterClaimStatus.AddRange(new[] { 2, 3, 6, 7 });
                }
                if (isOperator)
                {
                    showEdit = true;
                    clickAction = "editClaim";
                    posibleAction = (isProduct ? "all" : "editClaim");
                    author = user.Id;
                    deleteClaim = "self";
                }
            }
            ViewBag.Settings = new
            {
                showCalculate,
                showEdit,
                changeTenderStatus,
                filterProduct,
                filterManager,
                clickAction,
                posibleAction,
                userId,
                filterClaimStatus,
                author,
                reportExcel,
                deleteClaim,
                newClaim
            };
            ViewBag.Error = false.ToString().ToLower();
            ViewBag.ClaimCount = 0;
            try
            {
                //получение инфы по заявкам из БД
                var db = new DbEngine();
                var filter = new FilterTenderClaim()
                {
                    RowCount = 10,
                };
                if (!string.IsNullOrEmpty(filterManager)) filter.IdManager = filterManager;
                if (!string.IsNullOrEmpty(filterProduct)) filter.IdProductManager = filterProduct;
                if (!string.IsNullOrEmpty(author)) filter.Author = author;
                if (filterClaimStatus.Any()) filter.ClaimStatus = filterClaimStatus;
                var claims = db.FilterTenderClaims(filter);
                //снабженцы и менеджеры из ActiveDirectory
                var adProductManagers = UserHelper.GetProductManagers();
                var managers = UserHelper.GetManagers();
                if (claims != null && claims.Any())
                {
                    db.SetProductManagersForClaims(claims);
                    var claimProductManagers = claims.SelectMany(x => x.ProductManagers).ToList();
                    foreach (var claimProductManager in claimProductManagers)
                    {
                        var managerFromAD = adProductManagers.FirstOrDefault(x=>x.Id == claimProductManager.Id);
                        if (managerFromAD != null)
                        {
                            claimProductManager.Name = managerFromAD.Name;
                        }
                    }
                    foreach (var claim in claims)
                    {
                        var manager = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
                        if (manager != null)
                        {
                            claim.Manager.Name = manager.Name;
                        }
                    }
                }
                ViewBag.Claims = claims;
                ViewBag.DealTypes = db.LoadDealTypes();
                ViewBag.ClaimStatus = db.LoadClaimStatus();
                ViewBag.ProductManagers = adProductManagers;
                ViewBag.Managers = managers;
                ViewBag.ClaimCount = db.GetCountFilteredTenderClaims(filter);
                ViewBag.TenderStatus = db.LoadTenderStatus();
            }
            catch (Exception)
            {
                ViewBag.Error = true.ToString().ToLower();
            }
            return View();
        }

        //получение excel файла, для определения позиций по заявке 
        public ActionResult GetSpecificationFile()
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            try
            {
                //получение файла-шаблона
                var filePath = Path.Combine(Server.MapPath("~"), "App_Data", "Specification.xlsx");
                using (var fs = System.IO.File.OpenRead(filePath))
                {
                    var buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Count());
                    ms.Write(buffer, 0, buffer.Count());
                    ms.Seek(0, SeekOrigin.Begin);
                }
                //создание диапазона выбора снабженцев
                var productManagers = UserHelper.GetProductManagers();
                excBook = new XLWorkbook(ms);
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
                            cell.Value = GetUniqueDisplayName(manager);
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
                    excBook.SaveAs(ms);
                }
                excBook.Dispose();
                excBook = null;
                ms.Seek(0, SeekOrigin.Begin);
                
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
                    FileDownloadName = "Specification.xlsx"
                };
            }
            else
            {
                return View();
            }
        }

        //получение excel файла, содержащем только расчет по заявке
        public ActionResult GetSpecificationFileOnlyCalculation(int claimId)
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            var message = string.Empty;
            try
            {
                //получение позиций по заявке и расчетов к ним
                var db = new DbEngine();
                var positions = db.LoadSpecificationPositionsForTenderClaim(claimId);
                var facts = db.LoadProtectFacts();
                if (positions.Any())
                {
                    var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId);
                    if (calculations != null && calculations.Any())
                    {
                        foreach (var position in positions)
                        {
                            if (position.State == 1) continue;
                            position.Calculations =
                                calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                        }
                    }
                    //создание файла excel с инфой по расчетам
                    excBook = new XLWorkbook();
                    //заголовок
                    var workSheet = excBook.AddWorksheet("Расчет");
                    workSheet.Cell(1, 1).Value = "Каталожный номер*";
                    workSheet.Cell(1, 2).Value = "Наименование*";
                    workSheet.Cell(1, 3).Value = "Замена";
                    workSheet.Cell(1, 4).Value = "Цена за ед. USD";
                    workSheet.Cell(1, 5).Value = "Сумма вход USD";
                    workSheet.Cell(1, 6).Value = "Цена за ед. руб";
                    workSheet.Cell(1, 7).Value = "Сумма вход руб*";
                    workSheet.Cell(1, 9).Value = "Поставщик";
                    workSheet.Cell(1, 10).Value = "Факт получ.защиты*";
                    workSheet.Cell(1, 11).Value = "Условия защиты";
                    workSheet.Cell(1, 12).Value = "Комментарий";
                    var calcHeaderRange = workSheet.Range(workSheet.Cell(1, 1), workSheet.Cell(1, 12));
                    calcHeaderRange.Style.Font.SetBold(true);
                    calcHeaderRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 204, 255, 209);
                    calcHeaderRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    calcHeaderRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    calcHeaderRange.Style.Border.SetBottomBorderColor(XLColor.Gray);
                    calcHeaderRange.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                    calcHeaderRange.Style.Border.SetTopBorderColor(XLColor.Gray);
                    calcHeaderRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                    calcHeaderRange.Style.Border.SetRightBorderColor(XLColor.Gray);
                    calcHeaderRange.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                    calcHeaderRange.Style.Border.SetLeftBorderColor(XLColor.Gray);
                    var row = 2;
                    //строки расчета
                    foreach (var position in positions)
                    {
                        if (position.Calculations != null && position.Calculations.Any())
                        {
                            foreach (var calculation in position.Calculations)
                            {
                                workSheet.Cell(row, 1).Value = calculation.CatalogNumber;
                                workSheet.Cell(row, 2).Value = calculation.Name;
                                workSheet.Cell(row, 3).Value = calculation.Replace;
                                workSheet.Cell(row, 4).Value = !calculation.PriceUsd.Equals(0)
                                    ? calculation.PriceUsd.ToString("N2")
                                    : string.Empty;
                                workSheet.Cell(row, 5).Value = !calculation.SumUsd.Equals(0)
                                    ? calculation.SumUsd.ToString("N2")
                                    : string.Empty;
                                workSheet.Cell(row, 6).Value = !calculation.PriceRub.Equals(0)
                                    ? calculation.PriceRub.ToString("N2")
                                    : string.Empty;
                                workSheet.Cell(row, 7).Value = !calculation.SumRub.Equals(0)
                                    ? calculation.SumRub.ToString("N2")
                                    : string.Empty;
                                workSheet.Cell(row, 9).Value = calculation.Provider;
                                workSheet.Cell(row, 10).Value =
                                    facts.First(x => x.Id == calculation.ProtectFact.Id).Value;
                                workSheet.Cell(row, 11).Value = calculation.ProtectCondition;
                                workSheet.Cell(row, 12).Value = calculation.Comment;
                                row++;
                            }
                        }
                    }
                    workSheet.Columns(1, 12).AdjustToContents();
                    workSheet.Column(8).Hide();
                    excBook.SaveAs(ms);
                    excBook.Dispose();
                    ms.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    error = true;
                    message = "Нет позиций для расчета";
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
                    FileDownloadName = "Calculation_" + claimId + ".xlsx"
                };
            }
            else
            {
                ViewBag.Message = message;
                return View();
            }
        }

        //загрузка списка заявок в excel файл, с учетом фильтра - фильтр передается
        //в параметре modelJson, сериализованный в формат JSON
        public ActionResult GetListExcelFile(string modelJson)
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            var message = string.Empty;
            try
            {
                //получение объекта фильтра 
                var model = new FilterTenderClaim();
                if (!string.IsNullOrEmpty(modelJson))
                {
                    model = JsonConvert.DeserializeObject<FilterTenderClaim>(modelJson);
                }
                if (model.RowCount == 0) model.RowCount = 10;
                //получение отфильтрованной инфы по заявкам из БД
                var db = new DbEngine();
                var list = db.FilterTenderClaims(model);
                //снабженцв и менеджеры из ActiveDirectory
                var adProductManagers = UserHelper.GetProductManagers();
                var managers = UserHelper.GetManagers();
                if (list.Any())
                {
                    db.SetProductManagersForClaims(list);
                    var claimProductManagers = list.SelectMany(x => x.ProductManagers).ToList();
                    foreach (var claimProductManager in claimProductManagers)
                    {
                        var managerFromAD = adProductManagers.FirstOrDefault(x => x.Id == claimProductManager.Id);
                        if (managerFromAD != null)
                        {
                            claimProductManager.Name = managerFromAD.Name;
                        }
                    }
                    foreach (var claim in list)
                    {
                        var manager = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
                        if (manager != null)
                        {
                            claim.Manager.Name = manager.Name;
                        }
                    }
                    var dealTypes = db.LoadDealTypes();
                    var status = db.LoadClaimStatus();
                    //Создание excel файла с инфой о заявках
                    excBook = new XLWorkbook();
                    var workSheet = excBook.AddWorksheet("Заявки");
                    //заголовок
                    workSheet.Cell(1, 1).Value = "ID";
                    workSheet.Cell(1, 2).Value = "№ Конкурса";
                    workSheet.Cell(1, 3).Value = "Контрагент";
                    workSheet.Cell(1, 4).Value = "Сумма";
                    workSheet.Cell(1, 5).Value = "Менеджер";
                    workSheet.Cell(1, 6).Value = "Снабженцы";
                    workSheet.Cell(1, 7).Value = "Тип сделки";
                    workSheet.Cell(1, 8).Value = "Статус";
                    workSheet.Cell(1, 9).Value = "Создано";
                    var headRange = workSheet.Range(workSheet.Cell(1, 1), workSheet.Cell(1, 9));
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
                    //строки с инфой по заявкам
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
                        workSheet.Cell(row, 8).Value = status.First(x => x.Id == claim.ClaimStatus).Value;
                        workSheet.Cell(row, 9).Value = claim.RecordDate.ToString("dd.MM.yyyy HH:mm");
                        workSheet.Cell(row, 9).DataType = XLCellValues.DateTime;
                        row++;
                    }
                    workSheet.Columns(1, 9).AdjustToContents();
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
                var date = DateTime.Now.ToString("yyyyMMdd_HHmm");
                return new FileStreamResult(ms, "application/vnd.ms-excel")
                {
                    FileDownloadName = "Report-" + date + ".xlsx"
                };
            }
            else
            {
                ViewBag.Message = message;
                return View();
            }
        }

        //страница с функциональностью загрузки файла excel по позициям на сервер
        public ActionResult UploadFileForm()
        {
            ViewBag.FirstLoad = true;
            ViewBag.Error = "false";
            ViewBag.Message = string.Empty;
            ViewBag.IdClaim = -1;
            return View();
        }

        //получение excel файла, с инфой по позициям заявки
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
                    var productManagers = UserHelper.GetProductManagers();
                    inputStream = file.InputStream;
                    inputStream.Seek(0, SeekOrigin.Begin);
                    excBook = new XLWorkbook(inputStream);
                    //разбор файла
                    var workSheet = excBook.Worksheet("Спецификации");
                    if (workSheet != null)
                    {
                        var user = GetUser();
                        var row = 2;
                        var errorStringBuilder = new StringBuilder();
                        var repeatRowCount = 0;
                        var db = new DbEngine();
                        //проход по всем строкам
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
                                State = 1,
                                Author = user.Id
                            };
                            //получение ячеек с инфой по позициям
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
                            //наименование
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
                            //разбор инфы по Порядковый номер
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
                            //разбор инфы по Каталожный номер, Замена, Единицы
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
                            //разбор инфы по Количество
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
                            //разбор инфы по Снабженец
                            if (managerRange == null || managerRange.Value == null || string.IsNullOrEmpty(managerRange.Value.ToString()))
                            {
                                rowValid = false;
                                errorStringBuilder.Append("Строка: " + row +
                                                          ", не задано обязательное значение Снабженец<br/>");
                            }
                            else
                            {
                                var managerFromAd =
                                    productManagers.FirstOrDefault(
                                        x => GetUniqueDisplayName(x) == managerRange.Value.ToString());
                                if (managerFromAd != null) model.ProductManager = managerFromAd;
                                else
                                {
                                    rowValid = false;
                                    errorStringBuilder.Append("Строка: " + row +
                                                              ", не найден Снабженец: " + managerRange.Value + "<br/>");
                                }
                            }
                            if (commentRange != null && commentRange.Value != null)
                            {
                                model.Comment = commentRange.Value.ToString();
                            }
                            //разбор инфы по Цена и Сумма
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
                        //сохранение полученых позиций в БД
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
                            message += "<br/>Ошибки:<br/>" + errorMessage;
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

        //>>>>Уведомления
        //Сохранение заявки
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
                    var user = GetUser();
                    var db = new DbEngine();
                    model.ClaimStatus = 1;
                    model.TenderStatus = 1;
                    model.Deleted = false;
                    model.RecordDate = DateTime.Now;
                    model.Author = user.Id;
                    isComplete = db.SaveTenderClaim(model);
                    if (isComplete)
                    {
                        //История изменения статуса
                        model.ClaimDeadlineString = model.ClaimDeadline.ToString("dd.MM.yyyy");
                        model.TenderStartString = model.TenderStart.ToString("dd.MM.yyyy");
                        model.KPDeadlineString = model.KPDeadline.ToString("dd.MM.yyyy");
                        statusHistory = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdClaim = model.Id,
                            IdUser = user.Id,
                            Status = new ClaimStatus() {Id = model.ClaimStatus},
                            Comment = "Автор: " + user.Name
                        };
                        db.SaveClaimStatusHistory(statusHistory);
                        statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                        //>>>>Уведомления
                        if (model.Author != model.Manager.Id)
                        {
                            var manager = UserHelper.GetUserById(model.Manager.Id);
                            if (manager != null)
                            {
                                var host = string.Empty;
                                if (Request.Url != null) host = Request.Url.Host;
                                var message = new StringBuilder();
                                message.Append("Здравствуйте ");
                                message.Append(manager.Name);
                                message.Append(".<br/>");
                                message.Append("Пользователь ");
                                message.Append(user.Name);
                                message.Append(" создал заявку где Вы назначены менеджером.<br/>");
                                message.Append("Ссылка на заявку: ");
                                message.Append("<a href='" + host + "/Claim/Index?claimId=" + model.Id + "'>" + host +
                                               "/Claim/Index?claimId=" + model.Id + "</a>");
                                message.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(new List<UserBase>() {manager}, message.ToString(),
                                    "Новая заявка в системе СпецРасчет");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isComplete = false;
            }
            return Json(new {IsComplete = isComplete, Model = model, StatusHistory = statusHistory});
        }

        //Изменение позиции
        [HttpPost]
        public JsonResult EditClaimPosition(SpecificationPosition model)
        {
            var isComplete = false;
            try
            {
                var user = GetUser();
                model.State = 1;
                model.Author = user.Id;
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

        //удаление позиции
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

        //удаление заявки
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

        //фильтрация списка заявок
        [HttpPost]
        public JsonResult FilterClaim(FilterTenderClaim model)
        {
            var isComplete = false;
            var list = new List<TenderClaim>();
            var count = -1;
            try
            {
                var db = new DbEngine();
                if (model.RowCount == 0) model.RowCount = 10;
                list = db.FilterTenderClaims(model);
                var adProductManagers = UserHelper.GetProductManagers();
                var managers = UserHelper.GetManagers();
                if (list.Any())
                {
                    db.SetProductManagersForClaims(list);
                    var claimProductManagers = list.SelectMany(x => x.ProductManagers).ToList();
                    foreach (var claimProductManager in claimProductManagers)
                    {
                        var managerFromAD = adProductManagers.FirstOrDefault(x=>x.Id == claimProductManager.Id);
                        if (managerFromAD != null)
                        {
                            claimProductManager.Name = managerFromAD.Name;
                        }
                    }
                    foreach (var claim in list)
                    {
                        var manager = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
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

        //добавление позиции по заявке
        [HttpPost]
        public JsonResult AddClaimPosition(SpecificationPosition model)
        {
            var isComplete = false;
            try
            {
                var user = GetUser();
                model.State = 1;
                model.Author = user.Id;
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

        //>>>>Уведомления
        //передача заявки в работу
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
                        //истроия изменения статуса заявки
                        var user = GetUser();
                        var comment = string.Join("\r", productManagers.Select(x => x.Name));
                        comment += "\rАвтор: " + user.Name; 
                        model = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdClaim = id,
                            IdUser = user.Id,
                            Status = new ClaimStatus() { Id = 2 },
                            Comment = comment
                        };
                        db.SaveClaimStatusHistory(model);
                        model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                        //>>>>Уведомления
                        var claimPositions = db.LoadSpecificationPositionsForTenderClaim(id);
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var claim = db.LoadTenderClaimById(id);
                        var host = string.Empty;
                        if (Request.Url != null) host = Request.Url.Host;
                        var dealTypes = db.LoadDealTypes();
                        foreach (var productManager in productInClaim)
                        {
                            var positionCount = claimPositions.Count(x => x.ProductManager.Id == productManager.Id);
                            var messageMail = new StringBuilder();
                            messageMail.Append("Здравствуйте ");
                            messageMail.Append(productManager.Name);
                            messageMail.Append(".<br/>");
                            messageMail.Append("Пользователь ");
                            messageMail.Append(user.Name);
                            messageMail.Append(
                                " создал заявку где Вам назначены позиции для расчета. Количество назначенных позиций: " +
                                positionCount + "<br/>");
                            messageMail.Append("Заявка № " + claim.Id + ", Менеджер: " +
                                           UserHelper.GetUserById(claim.Manager.Id).ShortName +
                                           ", Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value +
                                           ", Заказчик: " + claim.Customer +
                                           ", Сумма: " + claim.Sum.ToString("N2") + ", Срок сдачи: "
                                           + claim.ClaimDeadline.ToString("dd.MM.yyyy") + ".<br/>");
                            messageMail.Append("Ссылка на заявку: ");
                            messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                           "/Calc/Index?claimId=" + claim.Id + "</a>");
                            messageMail.Append("<br/>Сообщение от системы Спец расчет");
                            Notification.SendNotification(new[] {productManager}, messageMail.ToString(),
                                "Новая заявка для расчета в системе СпецРасчет");
                        }
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

        //>>>>Уведомления
        //отмена заявки
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
                    model.IdUser = GetUser().Id;
                    model.Status = new ClaimStatus() {Id = 5};
                    db.SaveClaimStatusHistory(model);
                    model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                    //>>>>Уведомления
                    var claim = db.LoadTenderClaimById(model.IdClaim);
                    var productManagers = db.LoadProductManagersForClaim(model.IdClaim);
                    if (productManagers != null && productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        var user = GetUser();
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var host = string.Empty;
                        var dealTypes = db.LoadDealTypes();
                        if (Request.Url != null) host = Request.Url.Host;
                        var messageMail = new StringBuilder();
                        messageMail.Append("Здравствуйте");
                        messageMail.Append(".<br/>");
                        messageMail.Append("Пользователь ");
                        messageMail.Append(user.Name);
                        messageMail.Append(" отменил заявку где Вам назначены позиции для расчета.<br/>");
                        messageMail.Append("Заявка № " + claim.Id + ", Менеджер: " +
                                           UserHelper.GetUserById(claim.Manager.Id).ShortName +
                                           ", Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value +
                                           ", Заказчик: " + claim.Customer +
                                           ", Сумма: " + claim.Sum.ToString("N2") + ", Срок сдачи: "
                                           + claim.ClaimDeadline.ToString("dd.MM.yyyy") + ".<br/>");
                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                       "/Calc/Index?claimId=" + claim.Id + "</a>");
                        messageMail.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(productInClaim, messageMail.ToString(),
                            "Отмена заявки в системе СпецРасчет");
                    }

                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }

        //>>>>Уведомления
        //приостановка заявки
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
                    model.IdUser = GetUser().Id;
                    model.Status = new ClaimStatus() { Id = 4 };
                    db.SaveClaimStatusHistory(model);
                    var dealTypes = db.LoadDealTypes();
                    model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                    //>>>>Уведомления
                    var claim = db.LoadTenderClaimById(model.IdClaim);
                    var productManagers = db.LoadProductManagersForClaim(model.IdClaim);
                    if (productManagers != null && productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        var user = GetUser();
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var host = string.Empty;
                        if (Request.Url != null) host = Request.Url.Host;
                        var messageMail = new StringBuilder();
                        messageMail.Append("Здравствуйте");
                        messageMail.Append(".<br/>");
                        messageMail.Append("Пользователь ");
                        messageMail.Append(user.Name);
                        messageMail.Append(" приостановил заявку где Вам назначены позиции для расчета.<br/>");
                        messageMail.Append("Заявка № " + claim.Id + ", Менеджер: " +
                                           UserHelper.GetUserById(claim.Manager.Id).ShortName +
                                           ", Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value +
                                           ", Заказчик: " + claim.Customer +
                                           ", Сумма: " + claim.Sum.ToString("N2") + ", Срок сдачи: "
                                           + claim.ClaimDeadline.ToString("dd.MM.yyyy") + ".<br/>");
                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                       "/Calc/Index?claimId=" + claim.Id + "</a>");
                        messageMail.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(productInClaim, messageMail.ToString(),
                            "Приостановка заявки в системе СпецРасчет");
                    }
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }

        //>>>>Уведомления
        //возобновление заявки
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
                            model.IdUser = GetUser().Id;
                            model.Status = new ClaimStatus() { Id = actualStatus.Status.Id };
                            if (string.IsNullOrEmpty(model.Comment)) model.Comment = "Возобновление заявки";
                            db.SaveClaimStatusHistory(model);
                            var dealTypes = db.LoadDealTypes();
                            model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                            //>>>>Уведомления
                            var claim = db.LoadTenderClaimById(model.IdClaim);
                            var productManagers = db.LoadProductManagersForClaim(model.IdClaim);
                            if (productManagers != null && productManagers.Any())
                            {
                                var productManagersFromAd = UserHelper.GetProductManagers();
                                var user = GetUser();
                                var productInClaim =
                                    productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                                var host = string.Empty;
                                if (Request.Url != null) host = Request.Url.Host;
                                var messageMail = new StringBuilder();
                                messageMail.Append("Здравствуйте");
                                messageMail.Append(".<br/>");
                                messageMail.Append("Пользователь ");
                                messageMail.Append(user.Name);
                                messageMail.Append(" возобновил заявку для работы где Вам назначены позиции для расчета.<br/>");
                                messageMail.Append("Заявка № " + claim.Id + ", Менеджер: " +
                                           UserHelper.GetUserById(claim.Manager.Id).ShortName +
                                           ", Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value +
                                           ", Заказчик: " + claim.Customer +
                                           ", Сумма: " + claim.Sum.ToString("N2") + ", Срок сдачи: "
                                           + claim.ClaimDeadline.ToString("dd.MM.yyyy") + ".<br/>");
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                               "/Calc/Index?claimId=" + claim.Id + "</a>");
                                messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(productInClaim, messageMail.ToString(),
                                    "Возобновление заявки в системе СпецРасчет");
                            }
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

        //>>>>Уведомления
        //Отклонение позиций
        [HttpPost]
        public JsonResult SetPositonRejected(List<int> positionsId, string comment, int idClaim)
        {
            var isComplete = false;
            ClaimStatusHistory model = null;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                isComplete = db.ChangePositionsState(positionsId, 3);
                var allPositions = db.LoadSpecificationPositionsForTenderClaim(idClaim);
                var claimStatus = 3;
                var isSameCalculate = allPositions.Any(x => x.State == 2 || x.State == 4);
                if (isSameCalculate) claimStatus = 6;
                var status = db.LoadLastStatusHistoryForClaim(idClaim).Id;
                //изменение статуса заявки и истроиии изменения статусов
                if (status != claimStatus)
                {
                    var changeStatusComplete = db.ChangeTenderClaimClaimStatus(new TenderClaim() {Id = idClaim, ClaimStatus = claimStatus});
                    if (changeStatusComplete)
                    {
                        model = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdUser = user.Id,
                            IdClaim = idClaim,
                            Comment = comment,
                            Status = new ClaimStatus() {Id = claimStatus}
                        };
                        if (string.IsNullOrEmpty(model.Comment)) model.Comment = string.Empty;
                        db.SaveClaimStatusHistory(model);
                        model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                    }
                }
                if (isComplete)
                {
                    //>>>>Уведомления
                    var claim = db.LoadTenderClaimById(idClaim);
                    var dealTypes = db.LoadDealTypes();
                    var productManagers =
                        allPositions.Where(x => positionsId.Contains(x.Id)).Select(x => x.ProductManager).ToList();
                    if (productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var host = string.Empty;
                        if (Request.Url != null) host = Request.Url.Host;
                        var messageMail = new StringBuilder();
                        messageMail.Append("Здравствуйте");
                        messageMail.Append(".<br/>");
                        messageMail.Append("Пользователь ");
                        messageMail.Append(user.Name);
                        messageMail.Append(" отклонил Ваш расчет позиции по заявке № " + claim.Id + "<br/>");
                        if (!string.IsNullOrEmpty(comment)) messageMail.Append("Комментарий: " + comment + "<br/>");
                        messageMail.Append("Заявка № " + claim.Id + ", Менеджер: " +
                                           UserHelper.GetUserById(claim.Manager.Id).ShortName +
                                           ", Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value +
                                           ", Заказчик: " + claim.Customer +
                                           ", Сумма: " + claim.Sum.ToString("N2") + ", Срок сдачи: "
                                           + claim.ClaimDeadline.ToString("dd.MM.yyyy") + ".<br/>");
                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                           "/Calc/Index?claimId=" + claim.Id + "</a>");
                        messageMail.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(productInClaim, messageMail.ToString(),
                            "Отклонение расчета позиций заявки в системе СпецРасчет");
                    }
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }

        //>>>>Уведомления
        //подтверждение позиций по заявке
        public JsonResult SetClaimAllPositonConfirmed(int idClaim)
        {
            var isComplete = false;
            ClaimStatusHistory model = null;
            var message = string.Empty;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                var positions = db.LoadSpecificationPositionsForTenderClaim(idClaim);
                if (positions.Any())
                {
                    //все ли позиции имеют расчет
                    var isReady = db.IsPositionsReadyToConfirm(positions);
                    if (isReady)
                    {
                        //изменение статуса позиций, заявки и истории изменения статусов
                        isComplete = db.ChangePositionsState(positions.Select(x => x.Id).ToList(), 4);
                        if (isComplete)
                        {
                            db.ChangeTenderClaimClaimStatus(new TenderClaim() {Id = idClaim, ClaimStatus = 8});
                            model = new ClaimStatusHistory()
                            {
                                Date = DateTime.Now,
                                IdUser = user.Id,
                                IdClaim = idClaim,
                                Comment = string.Empty,
                                Status = new ClaimStatus() {Id = 8}
                            };
                            db.SaveClaimStatusHistory(model);
                            model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                            //>>>>Уведомления
                            var claim = db.LoadTenderClaimById(idClaim);
                            var productManagers = positions.Select(x => x.ProductManager).ToList();
                            if (productManagers.Any())
                            {
                                var dealTypes = db.LoadDealTypes();
                                var productManagersFromAd = UserHelper.GetProductManagers();
                                var productInClaim =
                                    productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                                var host = string.Empty;
                                if (Request.Url != null) host = Request.Url.Host;
                                var messageMail = new StringBuilder();
                                messageMail.Append("Здравствуйте");
                                messageMail.Append(".<br/>");
                                messageMail.Append("Пользователь ");
                                messageMail.Append(user.Name);
                                messageMail.Append(" подтвердил Ваш расчет позиции по заявке № " + claim.Id + "<br/>");
                                messageMail.Append("Заявка № " + claim.Id + ", Менеджер: " +
                                           UserHelper.GetUserById(claim.Manager.Id).ShortName +
                                           ", Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value +
                                           ", Заказчик: " + claim.Customer +
                                           ", Сумма: " + claim.Sum.ToString("N2") + ", Срок сдачи: "
                                           + claim.ClaimDeadline.ToString("dd.MM.yyyy") + ".<br/>");
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Calc/Index?claimId=" + claim.Id + "</a>");
                                messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(productInClaim, messageMail.ToString(),
                                    "Подтверждение расчета позиций заявки в системе СпецРасчет");
                            }
                        }

                    }
                    else
                    {
                        message = "Невозможно отправить позиции на подтверждение\rНе все позиции имеют расчет";
                    }
                }
                else
                {
                    message = "Невозможно отправить позиции на подтверждение\rВ заявке нет позиций";
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model, Message = message }, JsonRequestBehavior.AllowGet);
        }

        //Изменение статуса конкурса
        public JsonResult ChangeClaimTenderStatus(int idClaim, int tenderStatus)
        {
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                isComplete = db.ChangeTenderClaimTenderStatus(idClaim, tenderStatus);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete }, JsonRequestBehavior.AllowGet);
        }

        //Проверка позиции на уникальность по отношению к переданому массиву позиций
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

        private UserBase GetUser()
        {
            var user = UserHelper.GetUser(User.Identity);
            return user;
        }

        //создание уникального имени снабженца, для excel файла загрузки позиций
        private string GetUniqueDisplayName(UserBase user)
        {
            var result = new StringBuilder();
            var name = user.Name;
            var nameArr = name.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            if (nameArr.Count() > 2)
            {
                result.Append(nameArr[0]);
                result.Append(" ");
                result.Append(nameArr[1].Substring(0, 1));
                result.Append(".");
                result.Append(nameArr[2].Substring(0, 1));
                result.Append(".");
                result.Append(" /");
                result.Append(user.Email);
            }
            else
            {
                result.Append(name);
                result.Append("/");
                result.Append(user.Email);
            }
            return result.ToString();
        }

    }
}