using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using Microsoft.Vbe.Interop;
using TenderProcessing.Helpers;
using TenderProcessingDataAccessLayer;
using TenderProcessingDataAccessLayer.Enums;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Controllers
{
    [Authorize]
    public class CalcController : Controller
    {
        //>>>>Уведомления
        public ActionResult Index(int? claimId)
        {
            var user = GetUser();
            if (user == null || !UserHelper.IsUserAccess(user))
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к приложению");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }
            var isController = UserHelper.IsController(user);
            var isProduct = UserHelper.IsProductManager(user);
            if (!isController && !isProduct)
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к этой странице");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }
            ViewBag.Error = false.ToString().ToLower();
            ViewBag.DealType = string.Empty;
            ViewBag.Status = string.Empty;
            ViewBag.StatusHistory = new List<ClaimStatusHistory>();
            try
            {
                var db = new DbEngine();
                TenderClaim claim = null;
                var dealTypeString = string.Empty;
                var tenderStatus = string.Empty;
                ViewBag.ClaimStatus = db.LoadClaimStatus();
                if (claimId.HasValue)
                {
                    claim = db.LoadTenderClaimById(claimId.Value);
                    if (claim != null)
                    {
                        if (claim.ClaimStatus == 1 || claim.ClaimStatus == 4 || claim.ClaimStatus == 5 || claim.ClaimStatus == 8)
                        {
                            var dict = new RouteValueDictionary();
                            dict.Add("message", "Статус заявки не позволяет производить расчет позиций");
                            return RedirectToAction("ErrorPage", "Auth", dict);
                        }
                        if (!isController)
                        {
                            claim.Positions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId.Value,
                                user.Id);
                        }
                        else
                        {
                            claim.Positions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value);
                        }
                        if (claim.Positions != null && claim.Positions.Any())
                        {
                            if (claim.ClaimStatus == 2)
                            {
                                claim.ClaimStatus = 3;
                                db.ChangeTenderClaimClaimStatus(claim);
                                var statusHistory = new ClaimStatusHistory()
                                {
                                    IdClaim = claim.Id,
                                    Date = DateTime.Now,
                                    Comment = string.Empty,
                                    Status = new ClaimStatus() { Id = claim.ClaimStatus },
                                    IdUser = user.Id
                                };
                                db.SaveClaimStatusHistory(statusHistory);
                                //>>>>Уведомления
                                var manager = UserHelper.GetUserById(claim.Manager.Id);
                                if (manager != null)
                                {
                                    var host = ConfigurationManager.AppSettings["AppHost"];
                                    var message = new StringBuilder();
                                    message.Append("Здравствуйте ");
                                    message.Append(manager.Name);
                                    message.Append(".<br/>");
                                    message.Append("Пользователь ");
                                    message.Append(user.Name);
                                    message.Append(" приступил к работе на заявкой №" + claim.Id + ".<br/>");
                                    message.Append("Ссылка на заявку: ");
                                    message.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Claim/Index?claimId=" + claim.Id + "</a>");
                                    message.Append("<br/>Сообщение от системы Спец расчет");
                                    Notification.SendNotification(new List<UserBase>() {manager}, message.ToString(),
                                        "Заявка №" + claim.Id + " принята работу в системе СпецРасчет");
                                }
                            }
                            var managers = UserHelper.GetManagers();
                            var managerFromAd = managers.FirstOrDefault(x => x.Id == claim.Manager.Id); 
                            if (managerFromAd != null)
                            {
                                claim.Manager.Name = managerFromAd.Name;
                                claim.Manager.Chief = managerFromAd.Chief;
                            }
                            var adProductsManager = UserHelper.GetProductManagers();
                            var productManagers = claim.Positions.Select(x => x.ProductManager).ToList();
                            foreach (var productManager in productManagers)
                            {
                                var productManagerFromAd = adProductsManager.First(x => x.Id == productManager.Id);
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
                        else
                        {
                            if (isController)
                            {
                                var dict = new RouteValueDictionary();
                                dict.Add("message", "У заявки нет позиций");
                                return RedirectToAction("ErrorPage", "Auth", dict); 
                            }
                            else
                            {
                                var dict = new RouteValueDictionary();
                                dict.Add("message", "У Вас нет доступа к этой заявке, Вам не назначены позиции для расчета");
                                return RedirectToAction("ErrorPage", "Auth", dict);   
                            }
                        }
                        var dealTypes = db.LoadDealTypes();
                        var dealType = dealTypes.FirstOrDefault(x => x.Id == claim.DealType);
                        if (dealType != null)
                        {
                            dealTypeString = dealType.Value;
                        }
                        var tenderStatusList = db.LoadTenderStatus();
                        var status = tenderStatusList.FirstOrDefault(x => x.Id == claim.TenderStatus);
                        if (status != null)
                        {
                            tenderStatus = status.Value;
                        }
                        ViewBag.StatusHistory = db.LoadStatusHistoryForClaim(claimId.Value);
                    }
                }
                ViewBag.Claim = claim;
                ViewBag.DealType = dealTypeString;
                ViewBag.Status = tenderStatus;
                ViewBag.ProtectFacts = db.LoadProtectFacts();
            }
            catch (Exception)
            {
                ViewBag.Error = true.ToString().ToLower();
            }
            return View();
        }

        public ActionResult GetSpecificationFile(int claimId, bool forManager)
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            var message = string.Empty;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                var positions = new List<SpecificationPosition>();
                if (UserHelper.IsController(user) || UserHelper.IsManager(user))
                {
                    positions = db.LoadSpecificationPositionsForTenderClaim(claimId);
                }
                else
                {
                    if (UserHelper.IsProductManager(user))
                    {
                        positions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId, user.Id);
                    }
                }
                if (positions.Any())
                {
                    positions = positions.Where(x => x.State == 1 || x.State == 3).ToList();
                    if (positions.Any())
                    {
                        var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId);
                        if (calculations != null && calculations.Any())
                        {
                            foreach (var position in positions)
                            {
                                if (UserHelper.IsManager(user) && position.State == 1) continue;
                                position.Calculations =
                                    calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                            }
                        }
                        excBook = new XLWorkbook();
                        var workSheet = excBook.AddWorksheet("Расчет");
                        var directRangeSheet = excBook.AddWorksheet("Справочники");
                        var facts = db.LoadProtectFacts();
                        var protectFactList = facts.Select(x => x.Value).ToList();
                        for (var i = 0; i < protectFactList.Count(); i++)
                        {
                            var protectFact = protectFactList[i];
                            var cell = directRangeSheet.Cell(i + 1, 1);
                            if (cell != null)
                            {
                                cell.Value = protectFact;
                            }
                        }
                        var protectFactRange = directRangeSheet.Range(directRangeSheet.Cell(1, 1),
                            directRangeSheet.Cell(protectFactList.Count(), 1));
                        directRangeSheet.Visibility = XLWorksheetVisibility.Hidden;
                        var row = 1;
                        foreach (var position in positions)
                        {
                            workSheet.Cell(row, 1).Value = "Каталожный номер";
                            workSheet.Cell(row, 2).Value = "Наименование";
                            workSheet.Cell(row, 3).Value = "Замена";
                            workSheet.Cell(row, 4).Value = "Единица";
                            workSheet.Cell(row, 5).Value = "Количество";
                            workSheet.Cell(row, 6).Value = "Комментарий";
                            workSheet.Cell(row, 7).Value = "Сумма, максимум";
                            workSheet.Cell(row, 8).Value = "Id";
                            workSheet.Range(workSheet.Cell(row, 1), workSheet.Cell(row, 8)).Style.Font.SetBold(true);
                            row++;
                            workSheet.Cell(row, 1).Value = position.CatalogNumber;
                            workSheet.Cell(row, 2).Value = position.Name;
                            workSheet.Cell(row, 3).Value = position.Replace;
                            workSheet.Cell(row, 4).Value = GetUnitString(position.Unit);
                            workSheet.Cell(row, 5).Value = position.Value;
                            workSheet.Cell(row, 6).Value = position.Comment;
                            workSheet.Cell(row, 7).Value = !position.Sum.Equals(0)
                                ? position.Sum.ToString("N2")
                                : string.Empty;
                            workSheet.Cell(row, 8).Value = position.Id;
                            var positionRange = workSheet.Range(workSheet.Cell(row - 1, 1), workSheet.Cell(row, 8));
                            positionRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            positionRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                            positionRange.Style.Border.SetBottomBorderColor(XLColor.Gray);
                            positionRange.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                            positionRange.Style.Border.SetTopBorderColor(XLColor.Gray);
                            positionRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                            positionRange.Style.Border.SetRightBorderColor(XLColor.Gray);
                            positionRange.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                            positionRange.Style.Border.SetLeftBorderColor(XLColor.Gray);
                            positionRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 204, 233, 255);
                            row++;
                            workSheet.Cell(row, 1).Value = "Каталожный номер*";
                            workSheet.Cell(row, 2).Value = "Наименование*";
                            workSheet.Cell(row, 3).Value = "Замена";
                            workSheet.Cell(row, 4).Value = "Цена за ед. USD";
                            workSheet.Cell(row, 5).Value = "Сумма вход USD";
                            workSheet.Cell(row, 6).Value = "Цена за ед. руб";
                            workSheet.Cell(row, 7).Value = "Сумма вход руб*";
                            workSheet.Cell(row, 8).Value = "callHd";
                            workSheet.Cell(row, 9).Value = "Поставщик";
                            workSheet.Cell(row, 10).Value = "Факт получ.защиты*";
                            workSheet.Cell(row, 11).Value = "Условия защиты";
                            workSheet.Cell(row, 12).Value = "Комментарий";
                            var calcHeaderRange = workSheet.Range(workSheet.Cell(row, 1), workSheet.Cell(row, 12));
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
                            if (position.Calculations != null && position.Calculations.Any())
                            {
                                foreach (var calculation in position.Calculations)
                                {
                                    row++;
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
                                    var validation = workSheet.Cell(row, 10).SetDataValidation();
                                    validation.AllowedValues = XLAllowedValues.List;
                                    validation.InCellDropdown = true;
                                    validation.Operator = XLOperator.Between;
                                    validation.List(protectFactRange);
                                    workSheet.Cell(row, 10).Value =
                                        facts.First(x => x.Id == calculation.ProtectFact.Id).Value;
                                    workSheet.Cell(row, 11).Value = calculation.ProtectCondition;
                                    workSheet.Cell(row, 12).Value = calculation.Comment;
                                }
                            }
                            else
                            {
                                row++;
                                var validation = workSheet.Cell(row, 10).SetDataValidation();
                                validation.AllowedValues = XLAllowedValues.List;
                                validation.InCellDropdown = true;
                                validation.Operator = XLOperator.Between;
                                validation.List(protectFactRange);
                            }
                            row++;
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
                    FileDownloadName = "Specification_" + claimId + ".xlsx"
                };
            }
            else
            {
                ViewBag.Message = message;
                return View();
            }
        }

        public ActionResult UploadFileForm(int claimId)
        {
            ViewBag.FirstLoad = true;
            ViewBag.Error = "false";
            ViewBag.Message = string.Empty;
            ViewBag.ClaimId = claimId;
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
                    var workSheet = excBook.Worksheet("Расчет");
                    if (workSheet != null)
                    {
                        var user = GetUser();
                        var row = 0;
                        var errorStringBuilder = new StringBuilder();
                        var db = new DbEngine();
                        var emptyRowCount = 0;
                        var readyToParse = false;
                        SpecificationPosition model = null;
                        CalculateSpecificationPosition calculate = null;
                        var protectFacts = db.LoadProtectFacts();
                        var adProductManagers = UserHelper.GetProductManagers();
                        while (true)
                        {
                            row++;
                            var rowValid = true;
                            var controlCell = workSheet.Cell(row, 8);
                            var rowType = RowType.CalculateHeader;
                            var controlValue = controlCell.Value;
                            if (controlValue != null)
                            {
                                var controlValueString = controlValue.ToString();
                                if (string.IsNullOrEmpty(controlValueString))
                                {
                                    rowType = RowType.CalculateRow;
                                }
                                else
                                {
                                    if (controlValueString == "Id")
                                    {
                                        emptyRowCount = 0;
                                        rowType = RowType.PositionHeader;
                                        if (model != null)
                                        {
                                            positions.Add(model);
                                        }
                                        model = new SpecificationPosition() { Calculations = new List<CalculateSpecificationPosition>(), Author = user.Id};
                                    }
                                    else if (controlValueString == "callHd")
                                    {
                                        emptyRowCount = 0;
                                        rowType = RowType.CalculateHeader;
                                    }
                                    else
                                    {
                                        emptyRowCount = 0;
                                        int id;
                                        var converting = int.TryParse(controlValueString, out id);
                                        if (converting)
                                        {
                                            if (model == null)
                                            {
                                                errorStringBuilder.Append("Нарушена последовательность следования строк, в строке: " + row + "<br/>");
                                                break;
                                            }
                                            model.Id = id;
                                            rowType = RowType.PositionRecord;
                                        }
                                        else
                                        {
                                            errorStringBuilder.Append("Ошибка разбора Id позиции в строке: " + row + "<br/>");
                                            rowType = RowType.PositionRecord;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                rowType = RowType.CalculateRow;
                            }
                            if (rowType == RowType.CalculateHeader)
                            {
                                readyToParse = true;
                            }
                            if (rowType == RowType.PositionHeader || rowType == RowType.PositionRecord)
                            {
                                readyToParse = false;
                            }
                            if (rowType != RowType.CalculateRow)
                            {
                                continue;
                            }
                            if (!readyToParse)
                            {
                                errorStringBuilder.Append("Нарушена последовательность следования строк, в строке: " + row + "<br/>");
                                break;
                            }
                            else
                            {
                                var nameValue = workSheet.Cell(row, 2).Value;
                                if (nameValue != null)
                                {
                                    var nameValueString = nameValue.ToString().Trim();
                                    if (string.IsNullOrEmpty(nameValueString))
                                    {
                                        emptyRowCount++;
                                        if (emptyRowCount > 1)
                                        {
                                            if (model != null)
                                            {
                                                positions.Add(model);
                                            }
                                            break;
                                        }
                                        continue;
                                    }
                                    if (model == null)
                                    {
                                        errorStringBuilder.Append("Нарушена последовательность следования строк, в строке: " + row + "<br/>");
                                        break;
                                    }
                                    calculate = new CalculateSpecificationPosition()
                                    {
                                        IdSpecificationPosition = model.Id,
                                        IdTenderClaim = claimId,
                                        Name = nameValueString,
                                        Author = user.Id
                                    };
                                    var catalogValue = workSheet.Cell(row, 1).Value;
                                    var replaceValue = workSheet.Cell(row, 3).Value;
                                    var priceUsdValue = workSheet.Cell(row, 4).Value;
                                    var sumUsdValue = workSheet.Cell(row, 5).Value;
                                    var priceRubValue = workSheet.Cell(row, 6).Value;
                                    var sumRubValue = workSheet.Cell(row, 7).Value;
                                    var providerValue = workSheet.Cell(row, 9).Value;
                                    var protectFactValue = workSheet.Cell(row, 10).Value;
                                    var protectConditionValue = workSheet.Cell(row, 11).Value;
                                    var commentValue = workSheet.Cell(row, 12).Value;
                                    if (catalogValue == null || string.IsNullOrEmpty(catalogValue.ToString().Trim()))
                                    {
                                        rowValid = false;
                                        errorStringBuilder.Append("Строка: " + row +
                                                              ", не задано обязательное значение Каталожный номер<br/>");
                                    }
                                    else
                                    {
                                        calculate.CatalogNumber = catalogValue.ToString().Trim();
                                    }
                                    if (protectFactValue == null || string.IsNullOrEmpty(protectFactValue.ToString().Trim()))
                                    {
                                        rowValid = false;
                                        errorStringBuilder.Append("Строка: " + row +
                                                              ", не задано обязательное значение Факт получ.защиты<br/>");
                                    }
                                    else
                                    {
                                        var protectFactValueString = protectFactValue.ToString().Trim();
                                        var possibleValues = protectFacts.Select(x => x.Value);
                                        if (!possibleValues.Contains(protectFactValueString))
                                        {
                                            rowValid = false;
                                            errorStringBuilder.Append("Строка: " + row +
                                                                  ", Значение '" + protectFactValueString + "' не является допустимым для Факт получ.защиты<br/>");
                                        }
                                        else
                                        {
                                            var fact = protectFacts.First(x => x.Value == protectFactValueString);
                                            calculate.ProtectFact = fact;
                                            if (protectFactValueString != "Не предоставляется")
                                            {
                                                if (protectConditionValue == null ||
                                                    string.IsNullOrEmpty(protectConditionValue.ToString().Trim()))
                                                {
                                                    rowValid = false;
                                                    errorStringBuilder.Append("Строка: " + row +
                                                                              ", не задано обязательное значение Условия защиты<br/>");
                                                }
                                                else
                                                {
                                                    calculate.ProtectCondition = protectConditionValue.ToString().Trim();
                                                }
                                            }
                                            else
                                            {
                                                if (protectConditionValue != null &&
                                                    !string.IsNullOrEmpty(protectConditionValue.ToString().Trim()))
                                                {
                                                    calculate.ProtectCondition = protectConditionValue.ToString().Trim();
                                                }
                                            }
                                        }
                                    }
                                    if (sumRubValue == null || string.IsNullOrEmpty(sumRubValue.ToString().Trim()))
                                    {
                                        rowValid = false;
                                        errorStringBuilder.Append("Строка: " + row +
                                                                  ", не задано обязательное значение Сумма вход руб<br/>");
                                    }
                                    else
                                    {
                                        double doubleValue;
                                        var isValidDouble = double.TryParse(sumRubValue.ToString().Trim(), out doubleValue);
                                        if (!isValidDouble)
                                        {
                                            rowValid = false;
                                            errorStringBuilder.Append("Строка: " + row +
                                                                      ", значение '" + sumRubValue.ToString().Trim() + "' в поле Сумма вход руб не является числом<br/>");
                                        }
                                        else
                                        {
                                            calculate.SumRub = doubleValue;
                                        }
                                    }
                                    if (priceUsdValue != null && !string.IsNullOrEmpty(priceUsdValue.ToString().Trim()))
                                    {
                                        double doubleValue;
                                        var isValidDouble = double.TryParse(priceUsdValue.ToString().Trim(), out doubleValue);
                                        if (!isValidDouble)
                                        {
                                            rowValid = false;
                                            errorStringBuilder.Append("Строка: " + row +
                                                                      ", значение '" + priceUsdValue.ToString().Trim() + "' в поле Цена за ед. USD не является числом<br/>");
                                        }
                                        else
                                        {
                                            calculate.PriceUsd = doubleValue;
                                        }
                                    }
                                    if (sumUsdValue != null && !string.IsNullOrEmpty(sumUsdValue.ToString().Trim()))
                                    {
                                        double doubleValue;
                                        var isValidDouble = double.TryParse(sumUsdValue.ToString().Trim(), out doubleValue);
                                        if (!isValidDouble)
                                        {
                                            rowValid = false;
                                            errorStringBuilder.Append("Строка: " + row +
                                                                      ", значение '" + sumUsdValue.ToString().Trim() + "' в поле Сумма вход USD не является числом<br/>");
                                        }
                                        else
                                        {
                                            calculate.SumUsd = doubleValue;
                                        }
                                    }
                                    if (priceRubValue != null && !string.IsNullOrEmpty(priceRubValue.ToString().Trim()))
                                    {
                                        double doubleValue;
                                        var isValidDouble = double.TryParse(priceRubValue.ToString().Trim(), out doubleValue);
                                        if (!isValidDouble)
                                        {
                                            rowValid = false;
                                            errorStringBuilder.Append("Строка: " + row +
                                                                      ", значение '" + priceRubValue.ToString().Trim() + "' в поле Цена за ед. руб не является числом<br/>");
                                        }
                                        else
                                        {
                                            calculate.PriceRub = doubleValue;
                                        }
                                    }
                                    if (replaceValue != null && !string.IsNullOrEmpty(replaceValue.ToString().Trim()))
                                    {
                                        calculate.Replace = replaceValue.ToString().Trim();
                                    }
                                    if (providerValue != null && !string.IsNullOrEmpty(providerValue.ToString().Trim()))
                                    {
                                        calculate.Provider = providerValue.ToString().Trim();
                                    }
                                    if (commentValue != null && !string.IsNullOrEmpty(commentValue.ToString().Trim()))
                                    {
                                        calculate.Comment = commentValue.ToString().Trim();
                                    }
                                }
                                else
                                {
                                    emptyRowCount++;
                                    if (emptyRowCount > 1)
                                    {
                                        if (model != null)
                                        {
                                            positions.Add(model);
                                        }
                                        break;
                                    }
                                    continue;
                                }
                            }
                            if (rowValid)
                            {
                                if (calculate != null && model != null)
                                {
                                    model.Calculations.Add(calculate);
                                }
                            }
                        }
                        var userPositions = new List<SpecificationPosition>();
                        if (UserHelper.IsController(user))
                        {
                            userPositions = db.LoadSpecificationPositionsForTenderClaim(claimId);
                        }
                        else if (UserHelper.IsProductManager(user))
                        {
                            userPositions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId, user.Id);
                        }
                        var possibleEditPosition = userPositions.Where(x => x.State == 1 || x.State == 3).ToList();
                        if (possibleEditPosition.Any())
                        {
                            db.DeleteCalculateForPositions(claimId, possibleEditPosition);
                            var userPositionsId = possibleEditPosition.Select(x => x.Id).ToList();
                            var positionCalculate = 0;
                            var calculateCount = 0;
                            if (positions != null && positions.Any())
                            {
                                foreach (var position in positions)
                                {
                                    if (!userPositionsId.Contains(position.Id)) continue;
                                    if (position.Calculations.Any()) positionCalculate++;
                                    foreach (var calculatePosition in position.Calculations)
                                    {
                                        calculateCount++;
                                        calculatePosition.IdSpecificationPosition = position.Id;
                                        calculatePosition.IdTenderClaim = claimId;
                                        db.SaveCalculateSpecificationPosition(calculatePosition);
                                    }
                                }
                            }
                            message = "Позиций расчитано: " + positionCalculate + "<br/>Строк расчета: " +
                                      calculateCount + "<br/>Ошибки:<br/>" + errorStringBuilder;
                        }
                        else
                        {
                            message = "нет позиций для расчета<br/>Ошибки:<br/>" + errorStringBuilder;
                        }
                        var isController = UserHelper.IsController(user);
                        if (!isController)
                        {
                            positions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId,
                                user.Id);
                        }
                        else
                        {
                            positions = db.LoadSpecificationPositionsForTenderClaim(claimId);
                        }
                        var productManagers = positions.Select(x => x.ProductManager).ToList();
                        foreach (var productManager in productManagers)
                        {
                            var productManagerFromAd = adProductManagers.FirstOrDefault(x => x.Id == productManager.Id);
                            if (productManagerFromAd != null)
                            {
                                productManager.Name = productManagerFromAd.Name;
                            }
                        }
                        var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId);
                        if (calculations != null && calculations.Any())
                        {
                            foreach (var position in positions)
                            {
                                position.Calculations =
                                    calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                            }
                        }
                    }
                    else
                    {
                        error = true;
                        message = "Не найден рабочий лист с расчетом спецификаций";
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
            ViewBag.ClaimId = claimId;
            return View();
        }

        [HttpPost]
        public JsonResult Save(CalculateSpecificationPosition model)
        {
            var isComplete = false;
            var id = -1;
            try
            {
                model.Author = GetUser().Id;
                var db = new DbEngine();
                isComplete = db.SaveCalculateSpecificationPosition(model);
                id = model.Id;
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new {IsComplete = isComplete, Id = id});
        }

        [HttpPost]
        public JsonResult Edit(CalculateSpecificationPosition model)
        {
            var isComplete = false;
            try
            {
                model.Author = GetUser().Id;
                var db = new DbEngine();
                isComplete = db.UpdateCalculateSpecificationPosition(model);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete });
        }

        public JsonResult Delete(int id)
        {
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                isComplete = db.DeleteCalculateSpecificationPosition(id);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetPositionToConfirm(int idClaim)
        {
            var isComplete = false;
            var message = string.Empty;
            ClaimStatusHistory model = null;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                var positions = new List<SpecificationPosition>();
                if (UserHelper.IsController(user))
                {
                    positions = db.LoadSpecificationPositionsForTenderClaim(idClaim);
                }
                else
                {
                    if (UserHelper.IsProductManager(user))
                    {
                        positions = db.LoadSpecificationPositionsForTenderClaimForProduct(idClaim, user.Id);
                    }
                }
                if (positions.Any())
                {
                    var isReady = db.IsPositionsReadyToConfirm(positions);
                    if (isReady)
                    {
                        isComplete = db.SetPositionsToConfirm(positions);
                        if (!isComplete) message = "Позиции не отправлены";
                        else
                        {
                            var allPositions = db.LoadSpecificationPositionsForTenderClaim(idClaim);
                            var isAllCalculate = allPositions.Count() ==
                                                 allPositions.Count(x => x.State == 2 || x.State == 4);
                            var claimStatus = isAllCalculate ? 7 : 6;
                            var status = db.LoadLastStatusHistoryForClaim(idClaim).Id;
                            if (status != claimStatus)
                            {
                                db.ChangeTenderClaimClaimStatus(new TenderClaim()
                                {
                                    Id = idClaim,
                                    ClaimStatus = claimStatus
                                });
                                var statusHistory = new ClaimStatusHistory()
                                {
                                    Date = DateTime.Now,
                                    Comment = "",
                                    IdClaim = idClaim,
                                    IdUser = user.Id,
                                    Status = new ClaimStatus() {Id = claimStatus}
                                };
                                db.SaveClaimStatusHistory(statusHistory);
                                statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                                model = statusHistory;
                            }
                            var claim = db.LoadTenderClaimById(idClaim);
                            var host = ConfigurationManager.AppSettings["AppHost"];
                            var productManagersFromAd = UserHelper.GetProductManagers();
                            var productManagers = db.LoadProductManagersForClaim(claim.Id);
                            var productInClaim =
                                productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                            var manager = UserHelper.GetUserById(claim.Manager.Id);
                            var author = UserHelper.GetUserById(claim.Author);
                            var to = new List<UserBase>();
                            to.Add(manager);
                            if (author.Id != manager.Id)
                            {
                                to.Add(author);
                            }
                            //>>>>Уведомления
                            if (claimStatus == 7)
                            {
                                var messageMail = new StringBuilder();
                                messageMail.Append("Здравствуйте");
                                messageMail.Append(".<br/>");
                                messageMail.Append("Заявка №" + claim.Id + " по которой вы назначены менеджером или являетесь автором полностью расчитана всеми снабженцами.<br/>");
                                messageMail.Append("Заявка № " + claim.Id + ", Заказчик: " + claim.Customer +
                                                   ", Срок сдачи: " + claim.ClaimDeadline.ToString("dd.MM.yyyy") +
                                                   ".<br/>");
                                messageMail.Append("Снабженцы: <br/>");
                                foreach (var productManager in productInClaim)
                                {
                                    messageMail.Append(productManager.Name + "<br/>");
                                }
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Claim/Index?claimId=" + claim.Id + "</a>");
                                messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(to, messageMail.ToString(),
                                    "Полный расчет заявки в системе СпецРасчет");
                            }
                            //>>>>Уведомления
                            if (claimStatus == 6)
                            {
                                var noneCalculatePositionManagers =
                                    allPositions.Where(x => x.State == 1 || x.State == 3)
                                        .Select(x => x.ProductManager)
                                        .ToList();
                                if (noneCalculatePositionManagers.Any())
                                {
                                    var products =
                                        productManagersFromAd.Where(x => noneCalculatePositionManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                                    var messageMail = new StringBuilder();
                                    messageMail.Append("Здравствуйте");
                                    messageMail.Append(".<br/>");
                                    messageMail.Append("Заявка №" + claim.Id + " по которой вы назначены менеджером или являетесь автором частично расчитана<br/>");
                                    messageMail.Append("Заявка № " + claim.Id + ", Заказчик: " + claim.Customer +
                                                       ", Срок сдачи: " + claim.ClaimDeadline.ToString("dd.MM.yyyy") +
                                                       ".<br/>");
                                    messageMail.Append("Снабженцы, не приславшие расчет: <br/>");
                                    foreach (var productManager in products)
                                    {
                                        messageMail.Append(productManager.Name + "<br/>");
                                    }
                                    messageMail.Append("Ссылка на заявку: ");
                                    messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" + host +
                                                       "/Claim/Index?claimId=" + claim.Id + "</a>");
                                    messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                    Notification.SendNotification(to, messageMail.ToString(),
                                        "Частичный расчет заявки в системе СпецРасчет");   
                                }
                            }
                        }
                    }
                    else
                    {
                        message = "Невозможно отправить позиции на подтверждение\rНе все позиции имеют расчет";
                    }
                }
            }
            catch (Exception)
            {
                isComplete = false;
                message = "Ошибка сервера";
            }
            return Json(new { IsComplete = isComplete, Message = message, Model = model }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddComment(int idClaim, string comment)
        {
            var isComplete = false;
            ClaimStatusHistory model = null;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                var lastHistory = db.LoadLastStatusHistoryForClaim(idClaim);
                if (lastHistory != null)
                {
                    lastHistory.Date = DateTime.Now;
                    lastHistory.Comment = comment;
                    lastHistory.IdUser = user.Id;
                    isComplete = db.SaveClaimStatusHistory(lastHistory);
                    lastHistory.DateString = lastHistory.Date.ToString("dd.MM.yyyy HH:mm");
                    model = lastHistory;
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model }, JsonRequestBehavior.AllowGet);
        }

        private string GetUnitString(PositionUnit unit)
        {
            var result = string.Empty;
            switch (unit)
            {
                case PositionUnit.Package:
                    result = "Упак.";
                    break;
                case PositionUnit.Thing:
                    result = "Шт.";
                    break;
            }
            return result;
        }

        private UserBase GetUser()
        {
            var user = UserHelper.GetUser(User.Identity);
            return user;
        }

        private enum RowType
        {
            PositionHeader = 1,
            PositionRecord = 2,
            CalculateHeader = 3,
            CalculateRow = 4
        }
    }
}