using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using TenderProcessing.Models;
using TenderProcessingDataAccessLayer;
using TenderProcessingDataAccessLayer.Enums;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Controllers
{
    public class CalculationSpecificationController : Controller
    {
        public ActionResult Index(int? claimId)
        {
            var db = new DbEngine();
            TenderClaim claim = null;
            var dealTypeString = string.Empty;
            var tenderStatus = string.Empty;
            if (claimId.HasValue)
            {
                claim = db.LoadTenderClaimById(claimId.Value);
                if (claim != null)
                {
                    var managerFromAd = UserHelper.GetManagerFromActiveDirectoryById(claim.Manager.Id);
                    if (managerFromAd != null)
                    {
                        claim.Manager.Name = managerFromAd.Name;
                        claim.Manager.Chief = managerFromAd.Chief;
                    }
                    claim.Positions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value);
                    if (claim.Positions != null && claim.Positions.Any())
                    {
                        var productManagers = claim.Positions.Select(x => x.ProductManager).ToList();
                        foreach (var productManager in productManagers)
                        {
                            var productManagerFromAd =
                                UserHelper.GetProductManagerFromActiveDirectoryById(productManager.Id);
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
                }
            }
            ViewBag.Claim = claim;
            ViewBag.DealType = dealTypeString;
            ViewBag.Status = tenderStatus;
            return View();
        }

        public ActionResult GetSpecificationFile(int claimId)
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            try
            {
                var db = new DbEngine();
                var positions = db.LoadSpecificationPositionsForTenderClaim(claimId);
                if (positions != null && positions.Any())
                {
                    var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId);
                    if (calculations != null && calculations.Any())
                    {
                        foreach (var position in positions)
                        {
                            position.Calculations =
                                calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                        }
                    }
                    excBook = new XLWorkbook();
                    var workSheet = excBook.AddWorksheet("Расчет Позиций");
                    var directRangeSheet = excBook.AddWorksheet("Справочники");
                    var protectFactList = new[] { "Получена нами", "Получена конкурентом", "Не предоставляется" };
                    for (var i = 0; i < protectFactList.Count(); i++)
                    {
                        var protectFact = protectFactList[i];
                        var cell = directRangeSheet.Cell(i + 1, 1);
                        if (cell != null)
                        {
                            cell.Value = protectFact;
                        }
                    }
                    var protectFactRange = directRangeSheet.Range(directRangeSheet.Cell(1, 1), directRangeSheet.Cell(protectFactList.Count(), 1));
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
                        workSheet.Cell(row, 7).Value = !position.Sum.Equals(0) ? position.Sum.ToString("N2") : string.Empty;
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
                        positionRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 251, 172, 159);
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
                        calcHeaderRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 41, 158, 185);
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
                                workSheet.Cell(row, 10).Value = calculation.ProtectFact;
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
                    FileDownloadName = "Позиции заявки id " + claimId + ".xlsx"
                };
            }
            else
            {
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
                    var workSheet = excBook.Worksheet("Расчет Позиций");
                    if (workSheet != null)
                    {
                        var row = 0;
                        var errorStringBuilder = new StringBuilder();
                        var parseError = false;
                        var db = new DbEngine();
                        var emptyRowCount = 0;
                        var readyToParse = false;
                        SpecificationPosition model = null;
                        CalculateSpecificationPosition calculate = null;
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
                                        model = new SpecificationPosition() { Calculations = new List<CalculateSpecificationPosition>() };
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
                                                parseError = true;
                                                rowValid = false;
                                                errorStringBuilder.Append("Нарушена последовательность следования строк, в строке: " + row + "<br/>");
                                                break;
                                            }
                                            model.Id = id;
                                            rowType = RowType.PositionRecord;
                                        }
                                        else
                                        {
                                            parseError = true;
                                            rowValid = false;
                                            errorStringBuilder.Append("Ошибка разбора Id позиции в строке: " + row +"<br/>");
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
                                parseError = true;
                                rowValid = false;
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
                                        parseError = true;
                                        rowValid = false;
                                        errorStringBuilder.Append("Нарушена последовательность следования строк, в строке: " + row + "<br/>");
                                        break;
                                    }
                                    calculate = new CalculateSpecificationPosition()
                                    {
                                        IdSpecificationPosition = model.Id,
                                        IdTenderClaim = claimId,
                                        Name = nameValueString
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
                                        parseError = true;
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
                                        parseError = true;
                                        rowValid = false;
                                        errorStringBuilder.Append("Строка: " + row +
                                                              ", не задано обязательное значение Факт получ.защиты<br/>");
                                    }
                                    else
                                    {
                                        var protectFactValueString = protectFactValue.ToString().Trim();
                                        var possibleValues = new[]
                                        {"Получена нами", "Получена конкурентом", "Не предоставляется"};
                                        if (!possibleValues.Contains(protectFactValueString))
                                        {
                                            parseError = true;
                                            rowValid = false;
                                            errorStringBuilder.Append("Строка: " + row +
                                                                  ", Значение '" + protectFactValueString + "' не является допустимым для Факт получ.защиты<br/>");
                                        }
                                        else
                                        {
                                            calculate.ProtectFact = protectFactValueString;
                                            if (protectFactValueString != "Не предоставляется")
                                            {
                                                if (protectConditionValue == null ||
                                                    string.IsNullOrEmpty(protectConditionValue.ToString().Trim()))
                                                {
                                                    parseError = true;
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
                                        parseError = true;
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
                                            parseError = true;
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
                                            parseError = true;
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
                                            parseError = true;
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
                                            parseError = true;
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
                        if (parseError)
                        {
                            error = true;
                            message = errorStringBuilder.ToString();
                        }
                        else
                        {
                            db.DeleteCalculateSpecificationPositionForClaim(claimId);
                            if (positions != null && positions.Any())
                            {
                                foreach (var position in positions)
                                {
                                    foreach (var calculatePosition in position.Calculations)
                                    {
                                        calculatePosition.IdSpecificationPosition = position.Id;
                                        calculatePosition.IdTenderClaim = claimId;
                                        db.SaveCalculateSpecificationPosition(calculatePosition);
                                    }
                                }
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
            try
            {
                var db = new DbEngine();
                var positions = db.LoadSpecificationPositionsForTenderClaim(idClaim);
                var isReady = db.IsPositionsReadyToConfirm(positions);
                if (isReady)
                {
                    isComplete = db.SetPositionsToConfirm(positions);
                    if (!isComplete) message = "Позиции не отправлены";
                }
                else
                {
                    message = "Невозможно отправить позиции на подтверждение<br/>Не все позиции имеют расчет";
                }
            }
            catch (Exception)
            {
                isComplete = false;
                message = "Ошибка сервера";
            }
            return Json(new { IsComplete = isComplete, Message = message }, JsonRequestBehavior.AllowGet);
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

        private enum RowType
        {
            PositionHeader = 1,
            PositionRecord = 2,
            CalculateHeader = 3,
            CalculateRow = 4
        }
    }
}