using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Vbe.Interop;
using SpeCalc.Helpers;
using SpeCalc.Models;
using SpeCalc.Objects;
using SpeCalcDataAccessLayer;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Models;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalc.Controllers
{
    [Authorize]
    public class CalcController : BaseController
    {
        [HttpPost]
        public JsonResult GetMinPrice(string partNum)
        {
            string result = CatalogProduct.PriceRequest(partNum);
            return Json(new { priceStr = result });
        }

        public ActionResult GetCertFile(string guid)
        {
            var cert = new DbEngine().GetCertFile(guid);

            //var cd = new System.Net.Mime.ContentDisposition
            //{
            //    // for example foo.bak
            //    FileName = guid,

            //    // always prompt the user for downloading, set to true if you want 
            //    // the browser to try to show the file inline
            //    Inline = false,
            //};
            //Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(cert.File, "text/plain", cert.FileName);
        }
        public ActionResult GetTenderClaimFile(string guid)
        {
            var claimFile = new DbEngine().GetTenderClaimFile(guid);

            //var cd = new System.Net.Mime.ContentDisposition
            //{
            //    // for example foo.bak
            //    FileName = guid,

            //    // always prompt the user for downloading, set to true if you want 
            //    // the browser to try to show the file inline
            //    Inline = false,
            //};
            //Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(claimFile.File, "text/plain", claimFile.FileName);
        }

        public ActionResult IndexProduct(int? claimId, int? cv)
        {
            if (claimId.HasValue && !cv.HasValue)
            {
                int lastVersion = DbEngine.GetCalcVersionList(claimId.Value).Last();
                return RedirectToAction("IndexProduct", new { claimId = claimId, cv = lastVersion });
            }
            if (!CurUser.HasAccess(AdGroup.SpeCalcKontroler, AdGroup.SpeCalcProduct))
                return RedirectToAction("Index", "Claim", new { claimId = claimId, cv = cv });
            var user = GetUser();
            ViewBag.UserName = user.FullName;
            var isController = user.Is(AdGroup.SpeCalcKontroler);//UserHelper.IsController(user);
            var isProduct = user.Is(AdGroup.SpeCalcProduct);//UserHelper.IsProductManager(user);
            if (!isController && !isProduct)
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к этой странице");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }
            //ViewBag.Error = false.ToString().ToLower();
            //ViewBag.DealType = string.Empty;
            //ViewBag.Status = string.Empty;
            //ViewBag.StatusHistory = new List<ClaimStatusHistory>();
            var newClaim = true;
            if (!isController) newClaim = false;
            ViewBag.NewClaim = newClaim.ToString().ToLower();
            TenderClaim claim = null;
            try
            {
                //получение инфы по заявке и сопутствующих справочников
                var db = new DbEngine();
                
                var dealTypeString = string.Empty;
                var tenderStatus = string.Empty;
                //ViewBag.ClaimStatus = db.LoadClaimStatus();
                //ViewBag.Currencies = db.LoadCurrencies();
                //ViewBag.DeliveryTimes = db.LoadDeliveryTimes();
                if (claimId.HasValue)
                {
                    claim = new TenderClaim(claimId.Value); //db.LoadTenderClaimById(claimId.Value);
                    claim.Certs = db.LoadClaimCerts(claimId.Value);
                    claim.Files = db.LoadTenderClaimFiles(claimId.Value);
                    var adProductsManager = UserHelper.GetProductManagers();
                    if (claim != null)
                    {
                        //if (claim.ClaimStatus == 1)
                        //{
                        //    var dict = new RouteValueDictionary();
                        //    dict.Add("message", "Статус заявки не позволяет производить расчет позиций");
                        //    return RedirectToAction("ErrorPage", "Auth", dict);
                        //}
                        //позиции заявки, в зависимости от роли юзера
                        if (!isController)
                        {
                            claim.Positions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId.Value,
                                user.Sid, cv.Value);
                        }
                        else
                        {
                            claim.Positions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                        }
                        if (claim.Positions != null && claim.Positions.Any())
                        {
                            //изменение статуса  в Работе если это первая загрузка для расчета по данной заявке
                            if (claim.ClaimStatus == 2)
                            {
                                claim.ClaimStatus = 3;
                                DbEngine.ChangeTenderClaimClaimStatus(claim);
                                var statusHistory = new ClaimStatusHistory()
                                {
                                    IdClaim = claim.Id,
                                    Date = DateTime.Now,
                                    Comment = string.Empty,
                                    Status = new ClaimStatus() { Id = claim.ClaimStatus },
                                    IdUser = user.Sid
                                };
                                db.SaveClaimStatusHistory(statusHistory);
                            }
                            //менеджеры и снабженцы из ActiveDirectory
                            //var managerFromAd = UserHelper.GetUserById(claim.Manager.Id);
                            //claim.Manager.Name = managerFromAd.Name;
                            //claim.Manager.ShortName = managerFromAd.ShortName;
                            //claim.Manager.ChiefShortName = managerFromAd.ManagerName;
                            //var managers = UserHelper.GetManagers();
                            //var managerFromAd = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
                            //if (managerFromAd != null)
                            //{
                            //    claim.Manager.Name = managerFromAd.Name;
                            //    claim.Manager.ShortName = managerFromAd.ShortName;
                            //    claim.Manager.ChiefShortName = managerFromAd.ChiefShortName;
                            //}

                            var subordinateList = Employee.GetSubordinates(user.Sid);


                            var productManagers = claim.Positions.Select(x => x.ProductManager).ToList();
                            var prodManSelList = UserHelper.GetProductManagersSelectionList();
                            bool hasAccess = isController || claim.Positions.Any(x => x.ProductManager.Id == user.Sid);

                            foreach (var productManager in productManagers)
                            {
                                hasAccess = hasAccess || (subordinateList.Any() && Employee.UserIsSubordinate(subordinateList, productManager.Id));// subordinateList.ToList().Contains(productManager.Id);
                                productManager.ShortName = prodManSelList.FirstOrDefault(x => x.Id == productManager.Id)?.ShortName;
                                //var productUser = UserHelper.GetUserById(productManager.Id);
                                //if (productUser != null)
                                //{
                                //    productManager.Name = productUser.Name;
                                //    productManager.ShortName = productUser.ShortName;
                                //}
                                //var productManagerFromAd = adProductsManager.First(x => x.Id == productManager.Id);
                                //if (productManagerFromAd != null)
                                //{
                                //    productManager.Name = productManagerFromAd.Name;
                                //    productManager.ShortName = productManagerFromAd.ShortName;
                                //}
                            }
                            if (!hasAccess)
                            {
                                var dict = new RouteValueDictionary();
                                dict.Add("message", "У Вас нет доступа к этой заявке, Вам не назначены позиции для расчета");
                                return RedirectToAction("ErrorPage", "Auth", dict);
                            }
                            //Расчет по позициям
                            var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                            if (calculations != null && calculations.Any())
                            {
                                foreach (var position in claim.Positions)
                                {
                                    position.Calculations =
                                        calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                                    position.Calculations.Reverse();
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
                        ViewBag.ProductManagers = adProductsManager;
                    }
                }
                //ViewBag.Claim = claim;
                //ViewBag.DealType = dealTypeString;
                //ViewBag.Status = tenderStatus;
                //ViewBag.ProtectFacts = db.LoadProtectFacts();
            }
            catch (Exception ex)
            {
                ViewBag.Error = true.ToString().ToLower();
            }
            return View(claim);
        }

        //Страница расчета позиций по заявке
        public ActionResult Index(int? claimId, int? cv)
        {
            var user = GetUser();
            //if (!UserHelper.IsController(user) && (UserHelper.IsManager(user) || UserHelper.IsOperator(user)))
            if (!CurUser.HasAccess(AdGroup.SpeCalcKontroler, AdGroup.SpeCalcProduct))
                return RedirectToAction("Index", "Claim", new { claimId = claimId, cv = cv });

            //проверка наличия доступа к странице
            if (user == null || !CurUser.HasAccess(AdGroup.SpeCalcKontroler, AdGroup.SpeCalcProduct))
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к приложению");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }

            if (claimId.HasValue && !cv.HasValue)
            {
                int lastVersion = DbEngine.GetCalcVersionList(claimId.Value).Last();
                return RedirectToAction("Index", new { claimId = claimId, cv = lastVersion });
            }

            return RedirectToAction("IndexProduct", new { claimId, cv });

            ViewBag.UserName = user.FullName;
            var isController = user.Is(AdGroup.SpeCalcKontroler);//UserHelper.IsController(user);
            var isProduct = user.Is(AdGroup.SpeCalcProduct);//UserHelper.IsProductManager(user);
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
            var newClaim = true;
            if (!isController) newClaim = false;
            ViewBag.NewClaim = newClaim.ToString().ToLower();
            try
            {
                //получение инфы по заявке и сопутствующих справочников
                var db = new DbEngine();
                TenderClaim claim = null;
                var dealTypeString = string.Empty;
                var tenderStatus = string.Empty;
                ViewBag.ClaimStatus = db.LoadClaimStatus();
                ViewBag.Currencies = db.LoadCurrencies();
                ViewBag.DeliveryTimes = db.LoadDeliveryTimes();
                if (claimId.HasValue)
                {
                    claim = db.LoadTenderClaimById(claimId.Value);
                    claim.Certs = db.LoadClaimCerts(claimId.Value);
                    claim.Files = db.LoadTenderClaimFiles(claimId.Value);
                    var adProductsManager = UserHelper.GetProductManagers();
                    if (claim != null)
                    {
                        //if (claim.ClaimStatus == 1)
                        //{
                        //    var dict = new RouteValueDictionary();
                        //    dict.Add("message", "Статус заявки не позволяет производить расчет позиций");
                        //    return RedirectToAction("ErrorPage", "Auth", dict);
                        //}
                        //позиции заявки, в зависимости от роли юзера
                        if (!isController)
                        {
                            claim.Positions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId.Value,
                                user.Sid, cv.Value);
                        }
                        else
                        {
                            claim.Positions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                        }
                        if (claim.Positions != null && claim.Positions.Any())
                        {
                            //изменение статуса  в Работе если это первая загрузка для расчета по данной заявке
                            if (claim.ClaimStatus == 2)
                            {
                                claim.ClaimStatus = 3;
                                DbEngine.ChangeTenderClaimClaimStatus(claim);
                                var statusHistory = new ClaimStatusHistory()
                                {
                                    IdClaim = claim.Id,
                                    Date = DateTime.Now,
                                    Comment = string.Empty,
                                    Status = new ClaimStatus() { Id = claim.ClaimStatus },
                                    IdUser = user.Sid
                                };
                                db.SaveClaimStatusHistory(statusHistory);
                            }
                            //менеджеры и снабженцы из ActiveDirectory
                            //var managerFromAd = UserHelper.GetUserById(claim.Manager.Id);
                            //claim.Manager.Name = managerFromAd.Name;
                            //claim.Manager.ShortName = managerFromAd.ShortName;
                            //claim.Manager.ChiefShortName = managerFromAd.ManagerName;
                            //var managers = UserHelper.GetManagers();
                            //var managerFromAd = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
                            //if (managerFromAd != null)
                            //{
                            //    claim.Manager.Name = managerFromAd.Name;
                            //    claim.Manager.ShortName = managerFromAd.ShortName;
                            //    claim.Manager.ChiefShortName = managerFromAd.ChiefShortName;
                            //}

                            var subordinateList = Employee.GetSubordinates(user.Sid);


                            var productManagers = claim.Positions.Select(x => x.ProductManager).ToList();
                            var prodManSelList = UserHelper.GetProductManagersSelectionList();
                            bool hasAccess = isController || claim.Positions.Any(x => x.ProductManager.Id == user.Sid);

                            foreach (var productManager in productManagers)
                            {
                                hasAccess = hasAccess || (subordinateList.Any() && Employee.UserIsSubordinate(subordinateList, productManager.Id));// subordinateList.ToList().Contains(productManager.Id);
                                productManager.ShortName = prodManSelList.FirstOrDefault(x => x.Id == productManager.Id)?.ShortName;
                                //var productUser = UserHelper.GetUserById(productManager.Id);
                                //if (productUser != null)
                                //{
                                //    productManager.Name = productUser.Name;
                                //    productManager.ShortName = productUser.ShortName;
                                //}
                                //var productManagerFromAd = adProductsManager.First(x => x.Id == productManager.Id);
                                //if (productManagerFromAd != null)
                                //{
                                //    productManager.Name = productManagerFromAd.Name;
                                //    productManager.ShortName = productManagerFromAd.ShortName;
                                //}
                            }
                            if (!hasAccess)
                            {
                                var dict = new RouteValueDictionary();
                                dict.Add("message", "У Вас нет доступа к этой заявке, Вам не назначены позиции для расчета");
                                return RedirectToAction("ErrorPage", "Auth", dict);
                            }
                            //Расчет по позициям
                            var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                            if (calculations != null && calculations.Any())
                            {
                                foreach (var position in claim.Positions)
                                {
                                    position.Calculations =
                                        calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                                    position.Calculations.Reverse();
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
                        ViewBag.ProductManagers = adProductsManager;
                    }
                }
                ViewBag.Claim = claim;
                ViewBag.DealType = dealTypeString;
                ViewBag.Status = tenderStatus;
                ViewBag.ProtectFacts = db.LoadProtectFacts();
            }
            catch (Exception ex)
            {
                ViewBag.Error = true.ToString().ToLower();
            }
            return View();
        }

        //Excel
        //получение excel файла с инфой по позициям и расчетам к ним
        public ActionResult GetSpecificationFile(int claimId, bool forManager, int cv)
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            var message = string.Empty;
            //try
            //{
                var user = GetUser();
                var db = new DbEngine();
                var positions = new List<SpecificationPosition>();
                //получение позиций исходя из роли юзера
                //if (UserHelper.IsController(user) || UserHelper.IsManager(user))
                if (CurUser.HasAccess(AdGroup.SpeCalcManager, AdGroup.SpeCalcOperator))
                {
                    positions = db.LoadSpecificationPositionsForTenderClaim(claimId, cv);
                }
                else
                {
                    //if (UserHelper.IsProductManager(user))
                    if (CurUser.HasAccess(AdGroup.SpeCalcProduct))
                    {
                        positions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId, user.Sid, cv);
                    }
                }
                if (positions.Any())
                {
                    //if (forManager) positions = positions.Where(x => x.State == 2 || x.State == 4).ToList();
                    //else positions = positions.Where(x => x.State == 1 || x.State == 3).ToList();
                    if (positions.Any())
                    {
                        //расчет к позициям
                        var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId, cv);
                        if (calculations != null && calculations.Any())
                        {
                            foreach (var position in positions)
                            {
                                //if (UserHelper.IsManager(user) && position.State == 1 && !UserHelper.IsController(user) && !UserHelper.IsProductManager(user)) continue;
                                if (CurUser.HasAccess(AdGroup.SpeCalcManager, AdGroup.SpeCalcProduct) && position.State == 1) continue;
                                position.Calculations =
                                    calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                            }
                        }
                        var filePath = Path.Combine(Server.MapPath("~"), "App_Data", "Template.xlsx");
                        using (var fs = System.IO.File.OpenRead(filePath))
                        {
                            var buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, buffer.Count());
                            ms.Write(buffer, 0, buffer.Count());
                            ms.Seek(0, SeekOrigin.Begin);
                        }
                        //создание excel файла
                        excBook = new XLWorkbook(ms);
                        var workSheet = excBook.Worksheet("WorkSheet");
                        workSheet.Name = "Расчет";
                        var claim = db.LoadTenderClaimById(claimId);
                        //>>>>>>>>Шапка - Заполнение инфы о заявке<<<<<<
                        var dealTypes = db.LoadDealTypes();
                        var manager = UserHelper.GetUserById(claim.Manager.Id);
                        //workSheet.Cell(1, 3).Value = !claim.CurrencyUsd.Equals(0)
                        //    ? claim.CurrencyUsd.ToString("N2")
                        //    : string.Empty;
                        //workSheet.Cell(2, 3).Value = !claim.CurrencyEur.Equals(0)
                        //    ? claim.CurrencyEur.ToString("N2")
                        //    : string.Empty;
                        //workSheet.Cell(1, 3).DataType = XLCellValues.Number;
                        //workSheet.Cell(2, 3).DataType = XLCellValues.Number;
                        workSheet.Cell(1, 4).Value = claim.TenderNumber;
                        //workSheet.Cell(4, 3).Value = claim.TenderStartString;
                        //workSheet.Cell(4, 3).DataType = XLCellValues.DateTime;
                        //workSheet.Cell(5, 3).Value = claim.ClaimDeadlineString;
                        //workSheet.Cell(5, 3).DataType = XLCellValues.DateTime;
                        //workSheet.Cell(6, 3).Value = claim.KPDeadlineString;
                        //workSheet.Cell(6, 3).DataType = XLCellValues.DateTime;
                        workSheet.Cell(2, 4).Value = claim.Customer;
                        //workSheet.Cell(8, 3).Value = claim.CustomerInn;
                        //workSheet.Cell(9, 3).Value = !claim.Sum.Equals(0) ? claim.Sum.ToString("N2") : string.Empty;
                        //workSheet.Cell(10, 3).Value = dealTypes.First(x => x.Id == claim.DealType).Value;
                        //workSheet.Cell(11, 3).Value = claim.TenderUrl;
                        workSheet.Cell(3, 4).Value = manager != null ? manager.ShortName : string.Empty;
                        //workSheet.Cell(13, 3).Value = claim.Manager.SubDivision;
                        //workSheet.Cell(14, 3).Value = claim.DeliveryDateString;
                        //workSheet.Cell(14, 3).DataType = XLCellValues.DateTime;
                        //workSheet.Cell(15, 3).Value = claim.DeliveryPlace;
                        //workSheet.Cell(16, 3).Value = claim.AuctionDateString;
                        //workSheet.Cell(16, 3).DataType = XLCellValues.DateTime;
                        //workSheet.Cell(17, 3).Value = claim.Comment;
                        var directRangeSheet = excBook.AddWorksheet("Справочники");
                        //создание дипазона выбора значений Факт получения защиты 
                        var facts = db.LoadProtectFacts();
                        //var currencies = db.LoadCurrencies();
                        var deliveryTimes = db.LoadDeliveryTimes();

                        var deliveryTimesList = deliveryTimes.Select(x => x.Value).ToList();
                        for (var i = 0; i < deliveryTimesList.Count(); i++)
                        {
                            var time = deliveryTimesList[i];
                            var cell = directRangeSheet.Cell(i + 1, 3);
                            if (cell != null)
                            {
                                cell.Value = time;
                            }
                        }

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



                        //var availableCurrencies = currencies.Where(x => x.Value.ToLowerInvariant() != "руб").ToList();
                        //for (var i = 0; i < currencies.Count(); i++)
                        //{
                        //    var currency = currencies[i];
                        //    var cell = directRangeSheet.Cell(i + 1, 2);
                        //    if (cell != null)
                        //    {
                        //        cell.Value = currency.Value;
                        //    }
                        //}




                        var protectFactRange = directRangeSheet.Range(directRangeSheet.Cell(1, 1),
                            directRangeSheet.Cell(protectFactList.Count(), 1));
                        //var currenciesRange = directRangeSheet.Range(directRangeSheet.Cell(1, 2),
                        //    directRangeSheet.Cell(currencies.Count(), 2));
                        var deliveryTimeRange = directRangeSheet.Range(directRangeSheet.Cell(1, 3),
                            directRangeSheet.Cell(deliveryTimes.Count(), 3));

                        directRangeSheet.Visibility = XLWorksheetVisibility.Hidden;
                        //>>>>>>>номер строки начало вывода инфы<<<<<<
                        var row = 4;

                        //В первой колонке храним id шники
                        workSheet.Column(1).Hide();

                        //вывод инфы по позициям
                        //workSheet.Cell(row, 2).Value = "Запрос";

                        //заголовок для строк расчета
                        //workSheet.Cell(row, 3).Value = "Каталожный номер*";
                        //workSheet.Cell(row, 4).Value = "Наименование*";
                        //workSheet.Cell(row, 5).Value = "Замена";
                        //workSheet.Cell(row, 6).Value = "Цена USD";
                        //workSheet.Cell(row, 7).Value = "Цена EUR";
                        //workSheet.Cell(row, 8).Value = "Цена EUR Ricoh";
                        //workSheet.Cell(row, 9).Value = "Цена руб";
                        //workSheet.Cell(row, 10).Value = "Поставщик";
                        //workSheet.Cell(row, 11).Value = "Срок поставки";
                        //workSheet.Cell(row, 12).Value = "Факт защиты*";
                        //workSheet.Cell(row, 13).Value = "Условия защиты";
                        //workSheet.Cell(row, 14).Value = "Комментарий";
                        //workSheet.Range(workSheet.Cell(row, 2), workSheet.Cell(row, 14)).Style.Font.SetBold(true);
                        var posCounter = 0;

                        foreach (var position in positions)
                        {
                            //заголовок и данные по позиции

                            //workSheet.Cell(row, 1).Value = "Каталожный номер";
                            //workSheet.Cell(row, 2).Value = "Наименование";
                            //workSheet.Cell(row, 3).Value = "Замена";
                            //workSheet.Cell(row, 3).Value = "Единица";
                            //workSheet.Cell(row, 4).Value = "Количество";
                            //workSheet.Cell(row, 5).Value = "Комментарий";
                            //workSheet.Cell(row, 7).Value = "Сумма, максимум";
                            //workSheet.Cell(row, 8).Value = "Id";
                            //workSheet.Cell(row, 9).Value = "Сумма с ТЗР";
                            //workSheet.Cell(row, 10).Value = "Сумма с НДС";

                            row++;

                            var idCell = workSheet.Cell(row, 1);
                            idCell.Value = position.Id;
                            workSheet.Cell(row, 2).Value = ++posCounter;
                            workSheet.Cell(row, 2).Style.Font.SetBold();
                            workSheet.Cell(row, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, 2).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                            workSheet.Cell(row, 3).Value = position.CatalogNumber;
                            var posCell = workSheet.Cell(row, 4);
                            posCell.Value = String.Format("{2}\r\n{5}", position.Id, position.CatalogNumber, position.Name, position.UnitName, position.Value, position.Comment);
                            posCell.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                            posCell.Style.Alignment.SetWrapText();
                            workSheet.Row(row).AdjustToContents();

                            workSheet.Cell(row, 5).Value = String.Format("{1} {0}", position.UnitName, position.Value);

                            //Объединяем две ячейки чтобы удобнее было добавлять строки пользователям руками
                            workSheet.Range(workSheet.Cell(row, 1), workSheet.Cell(row + 1, 1)).Merge();
                            workSheet.Range(workSheet.Cell(row, 2), workSheet.Cell(row + 1, 2)).Merge();
                            workSheet.Range(workSheet.Cell(row, 3), workSheet.Cell(row + 1, 3)).Merge();
                            workSheet.Range(workSheet.Cell(row, 4), workSheet.Cell(row + 1, 4)).Merge();
                            workSheet.Range(workSheet.Cell(row, 5), workSheet.Cell(row + 1, 5)).Merge();
                            //workSheet.Rows(row, row+1).AdjustToContents();


                            //workSheet.Cell(row, 1).Value = position.CatalogNumber;
                            //workSheet.Cell(row, 2).Value = position.Name;
                            ////workSheet.Cell(row, 3).Value = position.Replace;
                            //workSheet.Cell(row, 3).Value = GetUnitString(position.Unit);
                            //workSheet.Cell(row, 4).Value = position.Value;
                            //workSheet.Cell(row, 5).Value = position.Comment;
                            //var currency = currencies.First(x => x.Id == position.Currency);
                            //workSheet.Cell(row, 7).Value = !position.Sum.Equals(0)
                            //    ? position.Sum.ToString("N2") + " " + currency.Value
                            //    : string.Empty;
                            //workSheet.Cell(row, 9).Value = !position.Sum.Equals(0)
                            //    ? position.SumTzr.ToString("N2") + " " + currency.Value
                            //    : string.Empty;
                            //workSheet.Cell(row, 10).Value = !position.Sum.Equals(0)
                            //    ? position.SumNds.ToString("N2") + " " + currency.Value
                            //    : string.Empty;
                            //workSheet.Cell(row, 8).Value = position.Id;
                            //var positionRange = workSheet.Range(workSheet.Cell(row - 1, 1), workSheet.Cell(row, 5));
                            //positionRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            //positionRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                            //positionRange.Style.Border.SetBottomBorderColor(XLColor.Gray);
                            //positionRange.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                            //positionRange.Style.Border.SetTopBorderColor(XLColor.Gray);
                            //positionRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                            //positionRange.Style.Border.SetRightBorderColor(XLColor.Gray);
                            //positionRange.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                            //positionRange.Style.Border.SetLeftBorderColor(XLColor.Gray);
                            //positionRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 204, 233, 255);
                            //row++;
                            ////заголовок для строк расчета
                            //workSheet.Cell(row, 1).Value = "Каталожный номер*";
                            //workSheet.Cell(row, 2).Value = "Наименование*";
                            //workSheet.Cell(row, 3).Value = "Замена";
                            //workSheet.Cell(row, 4).Value = "Цена за ед.";
                            //workSheet.Cell(row, 5).Value = "Сумма вход";
                            //workSheet.Cell(row, 6).Value = "Валюта";
                            ////workSheet.Cell(row, 7).Value = "Цена за ед. руб";
                            ////workSheet.Cell(row, 9).Value = "Сумма вход руб*";
                            //workSheet.Cell(row, 7).Value = "Поставщик";
                            //workSheet.Cell(row, 8).Value = "callHd";
                            //workSheet.Cell(row, 9).Value = "Факт получ.защиты*";
                            //workSheet.Cell(row, 10).Value = "Условия защиты";
                            //workSheet.Cell(row, 11).Value = "Комментарий";
                            //var calcHeaderRange = workSheet.Range(workSheet.Cell(row, 1), workSheet.Cell(row, 11));
                            //calcHeaderRange.Style.Font.SetBold(true);
                            //calcHeaderRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 204, 255, 209);
                            //calcHeaderRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            //calcHeaderRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                            //calcHeaderRange.Style.Border.SetBottomBorderColor(XLColor.Gray);
                            //calcHeaderRange.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                            //calcHeaderRange.Style.Border.SetTopBorderColor(XLColor.Gray);
                            //calcHeaderRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                            //calcHeaderRange.Style.Border.SetRightBorderColor(XLColor.Gray);
                            //calcHeaderRange.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                            //calcHeaderRange.Style.Border.SetLeftBorderColor(XLColor.Gray);


                            var firstPosRow = row;
                            //вывод инфы по расчету к позиции
                            if (position.Calculations != null && position.Calculations.Any())
                            {
                                var calcCount = 1;

                                foreach (var calculation in position.Calculations)
                                {
                                    ExcelDataFormatCalcRow(ref workSheet, row, deliveryTimeRange, protectFactRange);

                                    if (calcCount > 1)
                                    {
                                        row++;

                                        ExcelDataFormatCalcRow(ref workSheet, row, deliveryTimeRange, protectFactRange);

                                        workSheet.Range(workSheet.Cell(firstPosRow, 1), workSheet.Cell(row, 1)).Merge();
                                        workSheet.Range(workSheet.Cell(firstPosRow, 2), workSheet.Cell(row, 2)).Merge();
                                        workSheet.Range(workSheet.Cell(firstPosRow, 3), workSheet.Cell(row, 3)).Merge();
                                        workSheet.Range(workSheet.Cell(firstPosRow, 4), workSheet.Cell(row , 4)).Merge();
                                        workSheet.Range(workSheet.Cell(firstPosRow, 5), workSheet.Cell(row, 5)).Merge();
                                    }

                                    workSheet.Cell(row, 6).Value = calculation.CatalogNumber;
                                    workSheet.Cell(row, 7).Value = calculation.Name;
                                    workSheet.Cell(row, 7).Style.Alignment.SetWrapText();
                                    workSheet.Cell(row, 8).Value = calculation.Replace;
                                    workSheet.Cell(row, 8).Style.Alignment.SetWrapText();

                                    workSheet.Cell(row, 9).Value = calculation.PriceUsd;
                                    workSheet.Cell(row, 10).Value = calculation.PriceEur;
                                    workSheet.Cell(row, 11).Value = calculation.PriceEurRicoh;
                                    workSheet.Cell(row, 12).Value = calculation.PriceRubl;
                                    workSheet.Cell(row, 13).Value = calculation.Provider;
                                    workSheet.Cell(row, 13).Style.Alignment.SetWrapText();

                                    if (calculation.DeliveryTime != null) workSheet.Cell(row, 14).Value = deliveryTimes.First(x => x.Id == calculation.DeliveryTime.Id).Value;
                                    workSheet.Cell(row, 14).Style.Alignment.SetWrapText();

                                    if (calculation.ProtectFact != null)
                                        workSheet.Cell(row, 15).Value = calculation.ProtectFact.Value;
                                        //facts.First(x => x.Id == calculation.ProtectFact.Id).Value;

                                    workSheet.Cell(row, 16).Value = calculation.ProtectCondition;
                                    workSheet.Cell(row, 16).Style.Alignment.SetWrapText();
                                    workSheet.Cell(row, 17).Value = calculation.Comment;
                                    workSheet.Cell(row, 17).Style.Alignment.SetWrapText();

                                    calcCount++;
                                }

                                //foreach (var calculation in position.Calculations)
                                //{
                                //    row++;
                                //    workSheet.Cell(row, 1).Value = calculation.CatalogNumber;
                                //    workSheet.Cell(row, 2).Value = calculation.Name;
                                //    workSheet.Cell(row, 3).Value = calculation.Replace;
                                //    workSheet.Cell(row, 4).Value = !calculation.PriceCurrency.Equals(0)
                                //        ? calculation.PriceCurrency.ToString("N2")
                                //        : string.Empty;
                                //    workSheet.Cell(row, 5).Value = !calculation.SumCurrency.Equals(0)
                                //        ? calculation.SumCurrency.ToString("N2")
                                //        : string.Empty;
                                //    var validation = workSheet.Cell(row, 6).SetDataValidation();
                                //    validation.AllowedValues = XLAllowedValues.List;
                                //    validation.InCellDropdown = true;
                                //    validation.Operator = XLOperator.Between;
                                //    validation.List(currenciesRange);
                                //    workSheet.Cell(row, 6).Value =
                                //        currencies.First(x => x.Id == calculation.Currency).Value;
                                //    //workSheet.Cell(row, 7).Value = !calculation.PriceRub.Equals(0)
                                //    //    ? calculation.PriceRub.ToString("N2")
                                //    //    : string.Empty;
                                //    //workSheet.Cell(row, 9).Value = !calculation.SumRub.Equals(0)
                                //    //    ? calculation.SumRub.ToString("N2")
                                //    //    : string.Empty;
                                //    workSheet.Cell(row, 7).Value = calculation.Provider;
                                //    validation = workSheet.Cell(row, 9).SetDataValidation();
                                //    validation.AllowedValues = XLAllowedValues.List;
                                //    validation.InCellDropdown = true;
                                //    validation.Operator = XLOperator.Between;
                                //    validation.List(protectFactRange);
                                //    workSheet.Cell(row, 9).Value =
                                //        facts.First(x => x.Id == calculation.ProtectFact.Id).Value;
                                //    workSheet.Cell(row, 10).Value = calculation.ProtectCondition;
                                //    workSheet.Cell(row, 11).Value = calculation.Comment;
                                //}
                            }
                            else
                            {


                                //var validation = workSheet.Cell(row, 12).SetDataValidation();
                                //validation.AllowedValues = XLAllowedValues.List;
                                //validation.InCellDropdown = true;
                                //validation.Operator = XLOperator.Between;
                                //validation.List(deliveryTimeRange);

                                //validation = workSheet.Cell(row, 13).SetDataValidation();
                                //validation.AllowedValues = XLAllowedValues.List;
                                //validation.InCellDropdown = true;
                                //validation.Operator = XLOperator.Between;
                                //validation.List(protectFactRange);


                                ExcelDataFormatCalcRow(ref workSheet, row, deliveryTimeRange, protectFactRange);
                                //Специально добавляем строчу так как мы делаем специально две на позицию чтобы ыбло удобнее добавлять руками в Экселе
                                row++;
                                ExcelDataFormatCalcRow(ref workSheet, row, deliveryTimeRange, protectFactRange);

                                //row++;
                                //var validation = workSheet.Cell(row, 6).SetDataValidation();
                                //validation.AllowedValues = XLAllowedValues.List;
                                //validation.InCellDropdown = true;
                                //validation.Operator = XLOperator.Between;
                                //validation.List(currenciesRange);
                                //validation = workSheet.Cell(row, 9).SetDataValidation();
                                //validation.AllowedValues = XLAllowedValues.List;
                                //validation.InCellDropdown = true;
                                //validation.Operator = XLOperator.Between;
                                //validation.List(protectFactRange);
                            }
                            //row++;
                        }

                        var list = workSheet.Range(workSheet.Cell(4, 1), workSheet.Cell(row, 17));

                        list.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                        list.Style.Border.SetBottomBorderColor(XLColor.Gray);
                        list.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                        list.Style.Border.SetTopBorderColor(XLColor.Gray);
                        list.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                        list.Style.Border.SetRightBorderColor(XLColor.Gray);
                        list.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                        list.Style.Border.SetLeftBorderColor(XLColor.Gray);

                        //workSheet.Columns(1, 11).AdjustToContents();
                        //workSheet.Column(8).Hide();
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
            //}
            //catch (Exception)
            //{
            //    error = true;
            //    message = "Ошибка сервера";
            //}
            //finally
            //{
            //    if (excBook != null)
            //    {
            //        excBook.Dispose();
            //    }
            //}
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

        public void ExcelDataFormatCalcRow(ref IXLWorksheet workSheet, int row, IXLRange deliveryTimeRange, IXLRange protectFactRange)
        {
            workSheet.Cell(row, 9).DataType = XLCellValues.Number;
            workSheet.Cell(row, 9).DataValidation.Decimal.GreaterThan(0);
            workSheet.Cell(row, 9).DataValidation.ErrorTitle = "Введите число";
            workSheet.Cell(row, 9).DataValidation.ErrorMessage = "Введите число";

            workSheet.Cell(row, 10).DataType = XLCellValues.Number;
            workSheet.Cell(row, 10).DataValidation.Decimal.GreaterThan(0);
            workSheet.Cell(row, 10).DataValidation.ErrorTitle = "Введите число";
            workSheet.Cell(row, 10).DataValidation.ErrorMessage = "Введите число";

            workSheet.Cell(row, 11).DataType = XLCellValues.Number;
            workSheet.Cell(row, 11).DataValidation.Decimal.GreaterThan(0);
            workSheet.Cell(row, 11).DataValidation.ErrorTitle = "Введите число";
            workSheet.Cell(row, 11).DataValidation.ErrorMessage = "Введите число";
            workSheet.Cell(row, 12).DataType = XLCellValues.Number;
            workSheet.Cell(row, 12).DataValidation.Decimal.GreaterThan(0);
            workSheet.Cell(row, 12).DataValidation.ErrorTitle = "Введите число";
            workSheet.Cell(row, 12).DataValidation.ErrorMessage = "Введите число";

            var validation = workSheet.Cell(row, 14).SetDataValidation();
            validation.AllowedValues = XLAllowedValues.List;
            validation.InCellDropdown = true;
            validation.Operator = XLOperator.Between;
            validation.List(deliveryTimeRange);
            validation.ErrorMessage = "Выберите из списка";

            validation = workSheet.Cell(row, 15).SetDataValidation();
            validation.AllowedValues = XLAllowedValues.List;
            validation.InCellDropdown = true;
            validation.Operator = XLOperator.Between;
            validation.List(protectFactRange);
            validation.ErrorMessage = "Выберите из списка";
        }

        //страница с функциональностью загрузки на сервер excel файла с расчетом по позициям
        public ActionResult UploadFileForm(int claimId)
        {
            ViewBag.FirstLoad = true;
            ViewBag.Error = "false";
            ViewBag.Message = string.Empty;
            ViewBag.ClaimId = claimId;
            return View();
        }

        //Excel
        //обработка загруженного файла excel, с расчетом по позициям
        [HttpPost]
        public ActionResult UploadFileForm(HttpPostedFileBase file, int claimId, int cv)
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
                    //разбор полученного файла
                    if (workSheet != null)
                    {
                        var user = GetUser();
                        //<<<<<<<Номер строки - начало разбора инфы>>>>>>
                        var row = 5;
                        var errorStringBuilder = new StringBuilder();
                        var db = new DbEngine();
                        var emptyRowCount = 0;
                        SpecificationPosition model = null;
                        CalculateSpecificationPosition calculate = null;
                        var protectFacts = db.LoadProtectFacts();
                        var deliveryTimes = db.LoadDeliveryTimes();
                        var currencies = db.LoadCurrencies();
                        var adProductManagers = UserHelper.GetProductManagers();
                        int? idPos = null;
                        //проход по всем строкам
                        while (true)
                        {
                            var rowValid = true;
                            var controlCell = workSheet.Cell(row, 1);

                            //определение типа строки
                            var controlValue = controlCell.Value;
                            bool isCalcRow = false;
                            if (controlValue != null && String.IsNullOrEmpty(controlValue.ToString()) && controlCell.IsMerged() && idPos.HasValue)
                            {
                                controlValue = idPos.Value;
                                isCalcRow = true;
                            }

                            if (controlValue != null || isCalcRow)
                            {
                                if (!isCalcRow)
                                {
                                    var controlValueString = controlValue.ToString();
                                    if (string.IsNullOrEmpty(controlValueString))
                                    {
                                        //Если строка запроса пустая то Конец
                                        if (!workSheet.Cell(row, 3).IsMerged() &&
                                            String.IsNullOrEmpty(workSheet.Cell(row, 3).Value.ToString().Trim()))
                                        {
                                            break;
                                        }

                                        //строка расчета
                                        errorStringBuilder.Append("Не найден идентификатор позиции в строке: " + row + "<br/>");
                                        break;
                                    }
                                    else
                                    {

                                        int id;
                                        var converting = int.TryParse(controlValueString, out id);
                                        if (converting)
                                        {
                                            model = new SpecificationPosition()
                                            {
                                                Calculations = new List<CalculateSpecificationPosition>(),
                                                Author = user.Sid
                                            };
                                            model.Id = id;
                                            idPos = id;
                                            positions.Add(model);
                                        }
                                        else
                                        {
                                            errorStringBuilder.Append("Ошибка разбора Id позиции в строке: " + row +
                                                                      "<br/>");
                                            break;
                                        }
                                    }
                                }
                            }

                            //разбор инфы по расчету к позиции
                            
                            //Если строка расчета не пустая, то парсим ее
                            bool flag4Parse = false;
                            for (int i = 4; i <= 15; i++)
                            {
                                if (!String.IsNullOrEmpty(workSheet.Cell(row, i).Value.ToString().Trim()))
                                {
                                    flag4Parse = true;
                                    break;
                                }
                            }

                            if (flag4Parse)
                            {
                                calculate = new CalculateSpecificationPosition()
                                {
                                    IdSpecificationPosition = model.Id,
                                    IdTenderClaim = claimId,
                                    Author = user.Sid
                                };

                                //получение значений расчета из ячеек
                                var catalogValue = workSheet.Cell(row, 6).Value;
                                var nameValue = workSheet.Cell(row, 7).Value;
                                var replaceValue = workSheet.Cell(row, 8).Value;
                                var priceUsd = workSheet.Cell(row, 9).Value;
                                var priceEur = workSheet.Cell(row, 10).Value;
                                var priceEurRicoh = workSheet.Cell(row, 11).Value;
                                var priceRubl = workSheet.Cell(row, 12).Value;
                                var providerValue = workSheet.Cell(row, 13).Value;
                                var deliveryTimeValue = workSheet.Cell(row, 14).Value;
                                var protectFactValue = workSheet.Cell(row, 15).Value;
                                var protectConditionValue = workSheet.Cell(row, 16).Value;
                                var commentValue = workSheet.Cell(row, 17).Value;

                                //Проверка
                                if (deliveryTimeValue != null && string.IsNullOrEmpty(deliveryTimeValue.ToString().Trim()))
                                {
                                    rowValid = false;
                                    errorStringBuilder.Append("Строка: " + row +
                                                          ", не задано обязательное значение Срок поставки<br/>");
                                }
                                if ((priceUsd != null && string.IsNullOrEmpty(priceUsd.ToString().Trim()))
                                    && (priceEur != null && string.IsNullOrEmpty(priceEur.ToString().Trim()))
                                    &&
                                    (priceEurRicoh != null && string.IsNullOrEmpty(priceEurRicoh.ToString().Trim()))
                                    && (priceRubl != null && string.IsNullOrEmpty(priceRubl.ToString().Trim())))
                                {
                                    rowValid = false;
                                    errorStringBuilder.Append("Строка: " + row +
                                                          ", не указано ни одной цены<br/>");
                                }

                                //Заполняем
                                calculate.CatalogNumber = catalogValue.ToString();
                                calculate.Name = nameValue.ToString();
                                calculate.Replace = replaceValue.ToString();

                                double prUsd;
                                if (!String.IsNullOrEmpty(priceUsd.ToString().Trim()) && double.TryParse(priceUsd.ToString().Trim(), out prUsd))
                                {
                                    calculate.PriceUsd = prUsd;
                                }

                                double prEur;
                                if (!String.IsNullOrEmpty(priceEur.ToString().Trim()) && double.TryParse(priceEur.ToString().Trim(), out prEur))
                                {
                                    calculate.PriceEur = prEur;
                                }

                                double prEurRicoh;
                                if (!String.IsNullOrEmpty(priceEurRicoh.ToString().Trim()) && double.TryParse(priceEurRicoh.ToString().Trim(), out prEurRicoh))
                                {
                                    calculate.PriceEurRicoh = prEurRicoh;
                                }

                                double prRubl;
                                if (!String.IsNullOrEmpty(priceRubl.ToString().Trim()) && double.TryParse(priceRubl.ToString().Trim(), out prRubl))
                                {
                                    calculate.PriceRubl = prRubl;
                                }

                                calculate.Provider = providerValue.ToString();

                                var delivertTimeValueString = deliveryTimeValue.ToString().Trim();
                                var possibleDelTimValues = deliveryTimes.Select(x => x.Value);
                                if (!possibleDelTimValues.Contains(delivertTimeValueString))
                                {
                                    rowValid = false;
                                    errorStringBuilder.Append("Строка: " + row +
                                                          ", Значение '" + delivertTimeValueString + "' не является допустимым для Срок поставки<br/>");
                                }
                                else
                                {
                                    var delTime = deliveryTimes.First(x => x.Value == delivertTimeValueString);
                                    calculate.DeliveryTime = delTime;
                                }

                                var protectFactValueString = protectFactValue.ToString().Trim();
                                var possibleValues = protectFacts.Select(x => x.Value);
                                if (!possibleValues.Contains(protectFactValueString))
                                {
                                    //rowValid = false;
                                    //errorStringBuilder.Append("Строка: " + row +
                                    //                      ", Значение '" + protectFactValueString + "' не является допустимым для Факт получ.защиты<br/>");
                                    calculate.ProtectFact = null;
                                }
                                else
                                {
                                    var fact = protectFacts.First(x => x.Value == protectFactValueString);
                                    calculate.ProtectFact = fact;
                                }

                                calculate.ProtectCondition = protectConditionValue.ToString();
                                calculate.Comment = commentValue.ToString();

                                //Если есть ошибки то не добавляем
                                if (rowValid)model.Calculations.Add(calculate);
                            }

                            row++;
                        }


                        //получение позиций для текущего юзера
                        var userPositions = new List<SpecificationPosition>();
                        //if (UserHelper.IsController(user))
                        if (CurUser.HasAccess(AdGroup.SpeCalcKontroler))
                        {
                            userPositions = db.LoadSpecificationPositionsForTenderClaim(claimId, cv);
                        }
                        //else if (UserHelper.IsProductManager(user))
                        else if (CurUser.Is(AdGroup.SpeCalcProduct))
                        {
                            userPositions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId, user.Sid, cv);
                        }
                        //позиции доступные для изменения
                        var possibleEditPosition = userPositions.Where(x => x.State == 1 || x.State == 3).ToList();
                        if (possibleEditPosition.Any())
                        {
                            //сохранение позиций и расчета к ним в БД
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
                            var errorPart = errorStringBuilder.ToString().Trim();
                            if (string.IsNullOrEmpty(errorPart)) errorPart = "нет";
                            else errorPart = "<br/>" + errorPart;
                            message = "Позиций расчитано: " + positionCalculate + "<br/>Строк расчета: " +
                                      calculateCount + "<br/>Ошибки: " + errorPart;
                        }
                        else
                        {
                            var errorPart = errorStringBuilder.ToString().Trim();
                            if (string.IsNullOrEmpty(errorPart)) errorPart = "нет";
                            else errorPart = "<br/>" + errorPart;
                            message = "нет позиций для расчета<br/>Ошибки: " + errorPart;
                        }
                        //получение позиций и расчетов к ним для текущего юзера для передачи в ответ
                        var isController = user.Is(AdGroup.SpeCalcKontroler);//UserHelper.IsController(user);
                        if (!isController)
                        {
                            positions = db.LoadSpecificationPositionsForTenderClaimForProduct(claimId,
                                user.Sid, cv);
                        }
                        else
                        {
                            positions = db.LoadSpecificationPositionsForTenderClaim(claimId, cv);
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
                        var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId, cv);
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

        //сохранение расчета
        [HttpPost]
        public JsonResult Save(CalculateSpecificationPosition model)
        {
            var isComplete = false;
            var id = -1;
            //try
            //{
            var db = new DbEngine();
            model.Author = GetUser().Sid;
            string priceOnline = CatalogProduct.PriceRequest(model.CatalogNumber);
            double price;
            double.TryParse(priceOnline, out price);
            if (price > 0) model.b2bPrice = price;
            if (model.Id <= 0)
            {
                
                
                isComplete = db.SaveCalculateSpecificationPosition(model);
                id = model.Id;
            }
            else
            {
                db.UpdateCalculateSpecificationPosition(model);
                id = model.Id;
            }
            //}
            //catch (Exception ex)
            //{
            //    isComplete = false;
            //}
            return Json(new { IsComplete = isComplete, Id = id });
        }

        //изменение расчета
        [HttpPost]
        public JsonResult Edit(CalculateSpecificationPosition model)
        {
            var isComplete = false;
            try
            {
                model.Author = GetUser().Sid;
                var db = new DbEngine();
                isComplete = db.UpdateCalculateSpecificationPosition(model);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete });
        }

        //удаление расчета
        public JsonResult Delete(int id)
        {
            var isComplete = false;
            //try
            //{
                var user = GetUser();
                var db = new DbEngine();
                isComplete = db.DeleteCalculateSpecificationPosition(id, user);
            //}
            //catch (Exception)
            //{
            //    isComplete = false;
            //}
            return Json(new { IsComplete = isComplete }, JsonRequestBehavior.AllowGet);
        }

        //>>>>Уведомления
        //отправка позиций на подтверждение - изменение статуса позиции
        [HttpPost]
        public JsonResult SetPositionToConfirm(List<int> posIds, int idClaim, string comment, int cv)
        {
            var isComplete = false;
            var message = string.Empty;
            ClaimStatusHistory model = null;
            //try
            //{
                var user = GetUser();
                var db = new DbEngine();
                //получение позиций для текущего юзера
                var positions = new List<SpecificationPosition>();
                //if (UserHelper.IsController(user))
                if (CurUser.HasAccess(AdGroup.SpeCalcKontroler))
                {
                    positions = db.LoadSpecificationPositionsForTenderClaim(idClaim, cv);
                }
                else
                {
                    //if (UserHelper.IsProductManager(user))
                    if (CurUser.Is(AdGroup.SpeCalcProduct))
                    {
                        positions = db.LoadSpecificationPositionsForTenderClaimForProduct(idClaim, user.Sid, cv);
                    }
                }
                //if (positions.Any())
                if (posIds.Any())
                {
                    //Переделано для частичной передачи расчета
                    positions = new List<SpecificationPosition>();
                    foreach (int p in posIds)
                    {
                        positions.Add(new SpecificationPosition(){Id=p});
                    }
                    // /> частичная передача

                    //проверка наличия у позиций строк расчета
                    var isReady = db.IsPositionsReadyToConfirm(positions);
                    if (isReady)
                    {
                        //изменения статуса позиций на - отправлено
                        isComplete = db.SetPositionsToConfirm(positions);
                        if (!isComplete) message = "Позиции не отправлены";
                        else
                        {
                            var allPositions = db.LoadSpecificationPositionsForTenderClaim(idClaim, cv);
                            var isAllCalculate = allPositions.Count() ==
                                                 allPositions.Count(x => x.State == 2 || x.State == 4);
                            var claimStatus = isAllCalculate ? 7 : 6;
                            //Изменение статуса заявки и истроии изменения статусов
                            var status = db.LoadLastStatusHistoryForClaim(idClaim).Status.Id;
                            if (status != claimStatus)
                            {
                                DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim()
                                {
                                    Id = idClaim,
                                    ClaimStatus = claimStatus
                                });
                                var statusHistory = new ClaimStatusHistory()
                                {
                                    Date = DateTime.Now,
                                    Comment = comment,
                                    IdClaim = idClaim,
                                    IdUser = user.Sid,
                                    Status = new ClaimStatus() {Id = claimStatus}
                                };
                                db.SaveClaimStatusHistory(statusHistory);
                                statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                                model = statusHistory;
                            }
                            else
                            {
                                var statusHistory = new ClaimStatusHistory()
                                {
                                    Date = DateTime.Now,
                                    Comment = comment,
                                    IdClaim = idClaim,
                                    IdUser = user.Sid,
                                    Status = new ClaimStatus() {Id = status}
                                };
                                db.SaveClaimStatusHistory(statusHistory);
                                statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                                model = statusHistory;
                            }
                            //инфа для уведомления
                            var claim = db.LoadTenderClaimById(idClaim);
                            var host = ConfigurationManager.AppSettings["AppHost"];
                            var productManagersFromAd = UserHelper.GetProductManagers();
                            var productManagers = db.LoadProductManagersForClaim(claim.Id, cv);
                            var productInClaim =
                                productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id))
                                    .ToList();
                            var manager = UserHelper.GetUserById(claim.Manager.Id);
                            var author = UserHelper.GetUserById(claim.Author.Sid);
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
                                messageMail.Append("Добрый день!");
                                messageMail.Append("<br/>");
                                messageMail.Append("Заявка №" + claim.Id + " - версия " + cv + " - полностью расчитана.");
                                //messageMail.Append("<br/><br />");
                                //messageMail.Append(GetClaimInfo(claim));
                                messageMail.Append("<br/>");
                                //messageMail.Append("Продакты/Снабженцы: <br/>");
                                //foreach (var productManager in productInClaim)
                                //{
                                //    messageMail.Append(productManager.Name + "<br/>");
                                //}
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Claim/Index?claimId=" + claim.Id + "</a>");
                                //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(to, messageMail.ToString(),
                                    String.Format("{0} - версия {2} - {1} - Полный расчет заявки СпецРасчет", claim.TenderNumber,
                                        claim.Customer, cv));
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
                                        productManagersFromAd.Where(
                                            x => noneCalculatePositionManagers.Select(y => y.Id).Contains(x.Id))
                                            .ToList();
                                    var messageMail = new StringBuilder();
                                    messageMail.Append("Добрый день!");
                                    messageMail.Append("<br/>");
                                    messageMail.Append("Заявка №" + claim.Id + " частично расчитана.");
                                    messageMail.Append("Продакты/Снабженцы, у которых расчет еще в работе: <br/>");
                                    foreach (var productManager in products)
                                    {
                                        messageMail.Append(productManager.Name + "<br/>");
                                    }
                                    //messageMail.Append("<br/>");
                                    //messageMail.Append(GetClaimInfo(claim));
                                    //messageMail.Append("<br/>");

                                    messageMail.Append("Ссылка на заявку: ");
                                    messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" +
                                                       host +
                                                       "/Claim/Index?claimId=" + claim.Id + "</a>");
                                    //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                    Notification.SendNotification(to, messageMail.ToString(),
                                        String.Format("{0} - {1} - Частичный расчет заявки СпецРасчет",
                                            claim.TenderNumber, claim.Customer));
                                }
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
                    message = "Выберите хотябы одну позицию!";
                }
            //}
            //catch (Exception)
            //{
            //    isComplete = false;
            //    message = "Ошибка сервера";
            //}
            return Json(new { IsComplete = isComplete, Message = message, Model = model }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SetPositionRejected(List<int> posIds, int idClaim, string comment, int cv)
        {
            var isComplete = false;
            var message = string.Empty;
            ClaimStatusHistory model = null;
           /* try
            {*/
                var user = GetUser();
                var db = new DbEngine();
                //получение позиций для текущего юзера
                var positions = new List<SpecificationPosition>();
                //if (UserHelper.IsController(user))
                if (CurUser.HasAccess(AdGroup.SpeCalcKontroler))
                {
                    positions = db.LoadSpecificationPositionsForTenderClaim(idClaim, cv);
                }
                else
                {
                    //if (UserHelper.IsProductManager(user))
                    if (CurUser.Is(AdGroup.SpeCalcProduct))
                    {
                        positions = db.LoadSpecificationPositionsForTenderClaimForProduct(idClaim, user.Sid, cv);
                    }
                }
                var positionIds = new List<int>();
                //if (positions.Any())
                if (posIds.Any())
                {
                    //Переделано для частичной передачи расчета
                    positionIds = posIds;
                }
                //изменения статуса позиций на - отправлено
                else
                {
                    foreach (var position in positions)
                    {
                        positionIds.Add(position.Id);
                    }
                }       
                
                isComplete = db.ChangePositionsState(positionIds,5);
                if (!isComplete) message = "Позиции не отклонены";
                else
                        {
                            var allPositions = db.LoadSpecificationPositionsForTenderClaim(idClaim, cv);
                            var isAllRejected = allPositions.Count() ==
                                                 allPositions.Count(x => x.State == 5);
                            var lastClaimStatus = db.LoadLastStatusHistoryForClaim(idClaim).Status.Id;
                    var claimStatus = isAllRejected ? 9 : lastClaimStatus;
                            //Изменение статуса заявки и истроии изменения статусов
                            if (lastClaimStatus != claimStatus)
                            {
                    DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim()
                                {
                                    Id = idClaim,
                                    ClaimStatus = claimStatus
                                });
                                var statusHistory = new ClaimStatusHistory()
                                {
                                    Date = DateTime.Now,
                                    Comment = String.Format("Пользователь {0} отклонил {2} из {3} позиций.<br/>Комментарий: {1} ",user.DisplayName,comment,positionIds.Count,allPositions.Count),
                                    IdClaim = idClaim,
                                    IdUser = user.Sid,
                                    Status = new ClaimStatus() { Id = claimStatus }
                                };
                                db.SaveClaimStatusHistory(statusHistory);
                                statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                                model = statusHistory;
                            }
                            else
                            {
                                var statusHistory = new ClaimStatusHistory()
                                {
                                    Date = DateTime.Now,
                                    Comment = String.Format("Пользователь {0} отклонил {2} из {3} позиций.<br/>Комментарий: {1} ", user.DisplayName, comment, positionIds.Count, allPositions.Count),
                                    IdClaim = idClaim,
                                    IdUser = user.Sid,
                                    Status = new ClaimStatus() { Id = lastClaimStatus }
                                };
                                db.SaveClaimStatusHistory(statusHistory);
                                statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                                model = statusHistory;
                            }
                            //инфа для уведомления
                            var claim = db.LoadTenderClaimById(idClaim);
                            var host = ConfigurationManager.AppSettings["AppHost"];
                            var productManagersFromAd = UserHelper.GetProductManagers();
                            var productManagers = db.LoadProductManagersForClaim(claim.Id, cv);
                            var productInClaim =
                                productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id))
                                    .ToList();
                            var manager = UserHelper.GetUserById(claim.Manager.Id);
                            var author = UserHelper.GetUserById(claim.Author.Sid);
                            var to = new List<UserBase>();
                            to.Add(manager);
                            if (author.Id != manager.Id)
                            {
                                to.Add(author);
                            }
                            //>>>>Уведомления
                            if (claimStatus == 9)
                            {
                                var messageMail = new StringBuilder();
                                messageMail.Append("Добрый день!<br/>");
                                messageMail.Append("Позиции в заявке № " + claim.Id + " отклонены пользователем " + user.FullName + ".<br/>");
                                messageMail.Append("Отклонены все позиции.<br/>");
                                messageMail.Append("Комментарий:<br/>");
                                messageMail.Append(comment+"<br/>");
                                //messageMail.Append("Продакты/Снабженцы: <br/>");
                                //foreach (var productManager in productInClaim)
                                //{
                                //    messageMail.Append(productManager.Name + "<br/>");
                                //}
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Claim/Index?claimId=" + claim.Id + "</a>");
                                //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(to, messageMail.ToString(),
                                    String.Format("{0} - {1} - Полное отклонение заявки СпецРасчет", claim.TenderNumber,
                                        claim.Customer));
                            }
                            //>>>>Уведомления
                            if (claimStatus == lastClaimStatus)
                            {
                                var noneRejectedPositionManagers =
                                    allPositions.Where(x => x.State == 1 || x.State == 3)
                                        .Select(x => x.ProductManager)
                                        .ToList();
                                if (noneRejectedPositionManagers.Any())
                                {
                                    var products =
                                        productManagersFromAd.Where(
                                            x => noneRejectedPositionManagers.Select(y => y.Id).Contains(x.Id))
                                            .ToList();
                                    var messageMail = new StringBuilder();
                                    messageMail.Append("Добрый день!<br/>");
                                    messageMail.Append("Позиции в заявке №" + claim.Id + " отклонены пльзователем "+             user.FullName+".<br/>");
                                    messageMail.Append("Отклонено позиций "+allPositions.Count(x => x.State==5)+" из "+allPositions.Count+".<br/>");
                                    
                                    //messageMail.Append("<br/>");
                                    //messageMail.Append(GetClaimInfo(claim));
                                    //messageMail.Append("<br/>");

                                    messageMail.Append("Ссылка на заявку: ");
                                    messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" +
                                                       host +
                                                       "/Claim/Index?claimId=" + claim.Id + "</a>");
                                    //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                    Notification.SendNotification(to, messageMail.ToString(),
                                        String.Format("{0} - {1} - Частичное отклонение заявки СпецРасчет",
                                            claim.TenderNumber, claim.Customer));
                                }
                            }
                        }
                  /*   }
                    else
                    {
                        message = "Невозможно отправить позиции на подтверждение\rНе все позиции имеют расчет";
                   }
              }
                else
                {
                    message = "Выберите хотябы одну позицию!";
                }*/
      /*      }
            catch (Exception)
            {
                isComplete = false;
                message = "Ошибка сервера";
            }*/
            return Json(new { IsComplete = isComplete, Message = message, Model = model }, JsonRequestBehavior.AllowGet);
        }
        //>>>>Уведомления
        //переназначение позиций другому снабженцу
        [HttpPost]
        public JsonResult ChangePositionsProduct(List<int> ids, string productId, int idClaim)
        {
            var isComplete = false;
            ClaimStatusHistory model = null;
            var deleted = true;
            try
            {
                var user = GetUser();
                //if (UserHelper.IsController(user)) deleted = false;
                if (CurUser.HasAccess(AdGroup.SpeCalcKontroler)) deleted = false;
                var newProduct = UserHelper.GetUserById(productId);
                var db = new DbEngine();
                isComplete = db.ChangePositionsProduct(ids, productId);
                if (isComplete)
                {
                    var comment = "Пользователь " + user.DisplayName + " переназначил позиции (" + ids.Count() +
                                  " шт.) пользователю " + newProduct.ShortName;
                    var status = db.LoadLastStatusHistoryForClaim(idClaim).Status.Id;
                    var statusHistory = new ClaimStatusHistory()
                    {
                        Date = DateTime.Now,
                        Comment = comment,
                        IdClaim = idClaim,
                        IdUser = user.Sid,
                        Status = new ClaimStatus() { Id = status }
                    };
                    db.SaveClaimStatusHistory(statusHistory);
                    statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                    model = statusHistory;
                    //>>>>Уведомления
                    var claim = db.LoadTenderClaimById(idClaim);
                    var host = ConfigurationManager.AppSettings["AppHost"];
                    var manager = UserHelper.GetUserById(claim.Manager.Id);

                    //Сообщение менеджеру и автору
                    var messageMail = new StringBuilder();
                    messageMail.Append("Добрый день!");
                    //messageMail.Append(manager.Name);
                    messageMail.Append("<br/>");
                    messageMail.Append("В заявке №" + claim.Id + " произошли изменения.");
                    messageMail.Append("<br/>");
                    messageMail.Append(comment);
                    messageMail.Append("<br/>");
                    //messageMail.Append(GetClaimInfo(claim));
                    messageMail.Append("Ссылка на заявку: ");
                    messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" + host +
                                       "/Claim/Index?claimId=" + claim.Id + "</a>");
                    //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                    Notification.SendNotification(new[] { manager }, messageMail.ToString(),
                        String.Format("{0} - {1} - Переназначение позиций в заявке СпецРасчет", claim.TenderNumber, claim.Customer));

                    //Сообщение продакту
                    messageMail = new StringBuilder();
                    messageMail.Append("Добрый день!");
                    //messageMail.Append(newProduct.Name);
                    messageMail.Append("<br/>");
                    messageMail.Append("В заявке №" + claim.Id + " Вам переназначены позиции от пользователя " + user.DisplayName + "<br/>");
                    messageMail.Append("Кол-во позиций: " + ids.Count());
                    messageMail.Append("<br/>");
                    messageMail.Append(GetClaimInfo(claim));
                    messageMail.Append("<br/><br />");
                    messageMail.Append("Ссылка на заявку: ");
                    messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                                   "/Calc/Index?claimId=" + claim.Id + "</a>");
                    //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                    Notification.SendNotification(new[] { newProduct }, messageMail.ToString(),
                        String.Format("{0} - {1} - Переназначение позиций в заявке СпецРасчет", claim.TenderNumber, claim.Customer));
                }

            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model, Deleted = deleted });
        }

        [HttpGet]
        public PartialViewResult GetPositions(int? claimId, int? cv)
        {
            if (!claimId.HasValue) return null;
            if (!cv.HasValue) cv = 1;
            string productSid = null;
            if (CurUser.Is(AdGroup.SpeCalcProduct))
                productSid = CurUser.Sid;
            var list = SpecificationPosition.GetListWithCalc(claimId.Value, cv.Value, productSid);// db.LoadSpecificationPositionsForTenderClaim(); ;
            return PartialView("Positions", list);
        }

        [HttpPost]
        public PartialViewResult GetCalculation(int? id)
        {
            var model = new CalculateSpecificationPosition();
            if (id.HasValue) { model = new CalculateSpecificationPosition(id.Value); }
            return PartialView("Calculation", model);
        }

        [HttpPost]
        public PartialViewResult GetCalculationEdit(int? id)
        {
            var model = new CalculateSpecificationPosition();
            if (id.HasValue && id.Value > 0) { model = new CalculateSpecificationPosition(id.Value);}
            return PartialView("CalculationEdit", model);
        }

        [HttpPost]
        public PartialViewResult GetCalculationEmpty()
        {
            return PartialView("CalculationEmpty");
        }

        [HttpPost]
        //добавление комментария
        public JsonResult AddComment(int idClaim, string comment, int cv)
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
                    lastHistory.Comment = user.DisplayName + ": " + comment;
                    lastHistory.IdUser = user.Sid;
                    isComplete = db.SaveClaimStatusHistory(lastHistory);

                    if (isComplete)
                    {

                        //var productManagers = db.LoadProductManagersForClaim(idClaim);
                        //var productManagersFromAd = UserHelper.GetProductManagers();
                        //var productInClaim =
                        //        productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var claim = db.LoadTenderClaimById(idClaim);
                        var host = ConfigurationManager.AppSettings["AppHost"];
                        var messageMail = new StringBuilder();
                        messageMail.Append("Добрый день!");
                        messageMail.Append("<br/>");
                        messageMail.Append("В заявке № " + idClaim + " пользователь ");
                        messageMail.Append(GetUser().DisplayName);
                        messageMail.Append(" создал комментарий: " + comment);
                        messageMail.Append("<br/>");
                        //messageMail.Append(GetClaimInfo(claim));
                        //messageMail.Append("<br/><br/>");
                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" + host +
                                       "/Claim/Index?claimId=" + claim.Id + "</a>");
                        //messageMail.Append("<br/>Сообщение от системы Спец расчет");

                        var author = UserHelper.GetUserById(claim.Author.Sid);
                        var manager = UserHelper.GetUserById(claim.Manager.Id); ;
                        var to = new List<UserBase>();
                        to.Add(manager);
                        if (author.Id != manager.Id)
                        {
                            to.Add(author);
                        }

                        Notification.SendNotification(to, messageMail.ToString(),
                            String.Format("{0} - {1} - Комментарий к заявке СпецРасчет", claim.TenderNumber, claim.Customer));
                    }

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

        //private string GetUnitString(PositionUnit unit)
        //{
        //    var result = string.Empty;
        //    switch (unit)
        //    {
        //        case PositionUnit.Package:
        //            result = "упак";
        //            break;
        //        case PositionUnit.Thing:
        //            result = "шт";
        //            break;
        //        case PositionUnit.Metr:
        //            result = "м";
        //            break;
        //    }
        //    return result;
        //}

        private AdUser GetUser()
        {
            return CurUser;
            //if(Session["CurUser"] != null)
            //{
            //    return (UserBase)Session["CurUser"];
            //}

            //var user = UserHelper.GetUser(User.Identity);

            //Session["CurUser"] = user;

            //return user;
        }

        private string GetClaimInfo(TenderClaim claim)
        {
            var db = new DbEngine();
            var dealTypes = db.LoadDealTypes();
            return "Заявка № " + claim.Id + "<br /><br />Автор: " + UserHelper.GetUserById(claim.Author.Sid).ShortName +
                   "<br /><br />Номер конкурса: " + claim.TenderNumber + "<br /><br />Заказчик: " + claim.Customer + "<br /><br />ИНН заказчика: " + claim.CustomerInn + "<br /><br />Дата начала: " +
                   claim.TenderStart.ToString("dd.MM.yyyy") + "<br /><br />Срок сдачи: "
                   + claim.ClaimDeadline.ToString("dd.MM.yyyy") + "<br /><br />Менеджер: " +
                   UserHelper.GetUserById(claim.Manager.Id).ShortName + "<br /><br />Подразделение менеджера: " +
                   claim.Manager.SubDivision +
                   "<br /><br />Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value + (claim.Sum > 0
                       ? "<br /><br />Бюджет: " + claim.Sum.ToString("N2")
                       : string.Empty) + (!string.IsNullOrEmpty(claim.TenderUrl)
                           ? "<br /><br />Сcылка на конкурс: <a href='" + claim.TenderUrl + "'>[Ссылка]</a>]"
                           : "не указана");
        }

        private enum RowType
        {
            PositionHeader = 1,
            PositionRecord = 2,
            CalculateHeader = 3,
            CalculateRow = 4
        }

        [HttpPost]
        public ActionResult SaveFile(string claimId)
        {
            if (Request.Files.Count > 0)
            {
                int? idClaim = null;

                try
                {
                    idClaim = Convert.ToInt32(Request.QueryString["claimId"]);
                    //idClaim = Convert.ToInt32(RouteData.Values["claimId"]);


                }
                catch (Exception ex)
                {
                    idClaim = null;
                }


                if (idClaim != null && idClaim > 0)
                {
                    //foreach (HttpPostedFileWrapper file in Request.Files)
                    //{
                    for(int i=0; i<Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        byte[] fileData = null;
                        using (var br = new BinaryReader(file.InputStream))
                        {
                            fileData = br.ReadBytes(file.ContentLength);
                        }
                        var db = new DbEngine();
                        var cert = new ClaimCert() {IdClaim = idClaim.Value, File = fileData, FileName = file.FileName};
                        db.SaveClaimCertFile(ref cert);
                    }
                    //}
                }
            }

            return RedirectToAction("Index", "Calc", new { claimId = Request.QueryString["claimId"] });
        }
    }
}
