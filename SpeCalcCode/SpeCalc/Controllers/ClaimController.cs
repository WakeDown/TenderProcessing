using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using SpeCalc.Helpers;
using SpeCalc.Models;
using SpeCalc.Objects;
using SpeCalcDataAccessLayer;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Models;
using SpeCalcDataAccessLayer.Objects;
using Stuff.Objects;

namespace SpeCalc.Controllers
{
    [Authorize]
    public class ClaimController : BaseController
    {
        [HttpPost]
        public JsonResult GetMinPrice(string partNum)
        {
            string result = CatalogProduct.PriceRequest(partNum);
            return Json(new { priceStr = result });
        }

        [HttpPost]
        public ActionResult GoActual(int claimId, int cv, int[] selIds)
        {
            if (claimId <= 0)throw new ArgumentException("Не указана заявка");
            if (cv<= 0) throw new ArgumentException("Не указана версия для актулизации");
            
            int newClaimState = 10;
            bool isComplete = DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = claimId, ClaimStatus = newClaimState });
            int newVersion = DbEngine.CopyPositionsForNewVersion(claimId, cv, CurUser.Sid, selIds);
            
            var db = new DbEngine();
            
            var productManagers = db.LoadProductManagersForClaim(claimId, newVersion, getActualize:true);
            if (productManagers != null && productManagers.Any())
            {
                var productManagersFromAd = UserHelper.GetProductManagers();
                foreach (var productManager in productManagers)
                {
                    var productManagerFromAd =
                        productManagersFromAd.FirstOrDefault(x => x.Id == productManager.Id);
                    if (productManagerFromAd != null)
                    {
                        productManager.ShortName = productManagerFromAd.ShortName;
                    }
                }
                //истроия изменения статуса заявки
                //var user = GetUser();
                var comment = "Продакты/снабженцы:<br />";
                comment += string.Join("<br />", productManagers.Select(x => x.ShortName));
                comment += "<br />Автор: " + CurUser.DisplayName;
                ClaimStatusHistory model = new ClaimStatusHistory()
                {
                    Date = DateTime.Now,
                    IdClaim = claimId,
                    IdUser = CurUser.Sid,
                    Status = new ClaimStatus() {Id = newClaimState },
                    Comment = comment
                };
                db.SaveClaimStatusHistory(model);
                model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                //>>>>Уведомления
                var claimPositions = db.LoadSpecificationPositionsForTenderClaim(claimId, newVersion);
                var productInClaim =
                    productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                var claim = db.LoadTenderClaimById(claimId);
                var host = ConfigurationManager.AppSettings["AppHost"];
                foreach (var productManager in productInClaim)
                {
                    var positionCount = claimPositions.Count(x => x.ProductManager.Id == productManager.Id);
                    var messageMail = new StringBuilder();
                    messageMail.Append("Добрый день!");
                    messageMail.Append(String.Format("<br/>На имя {0} назначена Актуализация расчета по заявке в системе СпецРасчет.",
                        productManager.ShortName));
                    //messageMail.Append("<br/>Пользователь ");
                    //messageMail.Append(user.Name);
                    //messageMail.Append(
                    //    " создал заявку где Вам назначены позиции для расчета. Количество назначенных позиций: " +
                    //    positionCount + "<br/>");
                    messageMail.Append("<br/><br />");
                    messageMail.Append(GetClaimInfo(claim));
                    messageMail.Append("<br />Ссылка на заявку: ");
                    messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                       "/Calc/Index?claimId=" + claim.Id + "&cv=" + newVersion + "</a>");
                    //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                    Notification.SendNotification(new[] {productManager}, messageMail.ToString(),
                        String.Format("{0} - {1} - Актуализация расчета заявки СпецРасчет", claim.TenderNumber, claim.Customer));
                }
            }

            return Json(new { claimId = claimId, newVersion = newVersion });
            //return RedirectToAction("Index", new {claimId = claimId, cv= newVersion });
        }

        [HttpPost]
        public JsonResult AskPositionReject(List<int> posIds, int idClaim, int cv)
        {
            var isComplete = false;
            var message = string.Empty;
            ClaimStatusHistory model = null;
            var user = GetUser();
            var db = new DbEngine();
            var positions = new List<SpecificationPosition>();
            
            var positionIds = new List<int>();
            if (posIds.Any())
            {
                positionIds = posIds;
            }
            var rejectState = new PositionState("CALLREJECT");
            isComplete = db.ChangePositionsState(positionIds, rejectState.Id);
            if (!isComplete) message = "Запрос не отправлен";
            else
            {


                var allPositions = SpecificationPosition.GetList(idClaim, cv).ToList(); //db.LoadSpecificationPositionsForTenderClaim(idClaim, cv);
                var isAllRejected = allPositions.Count() ==
                                     allPositions.Count(x => x.State == rejectState.Id);
                var lastClaimStatus = db.LoadLastStatusHistoryForClaim(idClaim).Status.Id;
                var claimStatus = lastClaimStatus;
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
                        Comment = String.Format("Пользователь {0} запросил отклонение {1} из {2} позиций.<br/>", user.DisplayName, positionIds.Count, allPositions.Count),
                        IdClaim = idClaim,
                        IdUser = CurUser.Sid,
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
                        Comment = String.Format("Пользователь {0} запросил отклонение {1} из {2} позиций.<br/>", user.DisplayName, positionIds.Count, allPositions.Count),
                        IdClaim = idClaim,
                        IdUser = CurUser.Sid,
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
                if (isAllRejected)
                {
                    var messageMail = new StringBuilder();
                    messageMail.Append("Добрый день!<br/>");
                    messageMail.Append("Запрос на отклонение позиций в заявке №" + claim.Id + " пльзователем " + user.FullName + ".<br/>");
                    //messageMail.Append("Комментарий:<br/>");
                    //messageMail.Append(comment + "<br/>");
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
                        String.Format("{0} - {1} - Запрос на отклонение позиций заявки СпецРасчет", claim.TenderNumber,
                            claim.Customer));
                }
                //>>>>Уведомления
                if (!isAllRejected)
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
                        messageMail.Append("Запрос на отклонение позиций в заявке №" + claim.Id + " пльзователем " + user.FullName + ".<br/>");
                        messageMail.Append("Отклонено позиций " + allPositions.Count(x => x.State == 5) + " из " + allPositions.Count + ".<br/>");

                        //messageMail.Append("<br/>");
                        //messageMail.Append(GetClaimInfo(claim));
                        //messageMail.Append("<br/>");

                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Claim/Index?claimId=" + claim.Id + "'>" +
                                           host +
                                           "/Claim/Index?claimId=" + claim.Id + "</a>");
                        //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(to, messageMail.ToString(),
                            String.Format("{0} - {1} - Запрос на отклонение позиций заявки СпецРасчет",
                                claim.TenderNumber, claim.Customer));
                    }
                }
            }
            
            return Json(new { IsComplete = isComplete, Message = message, Model = model }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetTenderClaimFile(string guid)
        //{
            
        //}

        

        public ActionResult IndexManager(int? claimId, int? cv)
        {
            //if (!cv.HasValue)return RedirectToAction("IndexManager", new {claimId, cv=1});

            var user = CurUser;

            //Проверка версии, по умолчанию показываем первую если никакой не указано
            if (claimId.HasValue && !cv.HasValue)
            {
                var verList = DbEngine.GetCalcVersionList(claimId.Value);
                if (verList.Any())
                {
                    int lastVersion = verList.Last();
                    return RedirectToAction("Index", new { claimId = claimId, cv = lastVersion });
                }
                else
                {
                    return RedirectToAction("Index", new { claimId = claimId, cv = 1 });
                }
            }

            //получения текущего юзера и проверка наличия у него доступа к странице
            ViewBag.Error = false.ToString().ToLower();
            TempData["tenderClaimFileFormats"] = WebConfigurationManager.AppSettings["FileFormat4TenderClaimFile"];

            TenderClaim claim = null;

            ViewBag.UserName = user.FullName;
            var isController = user.Is(AdGroup.SpeCalcKontroler);//UserHelper.IsController(user);
            var isManager = user.Is(AdGroup.SpeCalcManager);//UserHelper.IsManager(user);
            var isOperator = user.Is(AdGroup.SpeCalcOperator);//UserHelper.IsOperator(user);
            //if (!isController && !isManager && !isOperator)
            //{
            //    var dict = new RouteValueDictionary();
            //    dict.Add("message", "У Вас нет доступа к этой странице");
            //    return RedirectToAction("ErrorPage", "Auth", dict);
            //}
            try
            {
                //получение необходимой инфы из БД и ActiveDirectory
                //var managers = UserHelper.GetManagers();
                //ViewBag.Managers = managers;
                //ViewBag.DateStart = DateTime.Now.ToString("dd.MM.yyyy");
                var db = new DbEngine();
                //ViewBag.NextDateMin = DateTime.Now.DayOfWeek == DayOfWeek.Friday
                //    ? DateTime.Now.AddDays(4).ToShortDateString()
                //    : DateTime.Now.AddDays(2).ToShortDateString();
                //ViewBag.DealTypes = db.LoadDealTypes();
                //ViewBag.ClaimStatus = db.LoadClaimStatus();
                //var adProductManagers = UserHelper.GetProductManagers();
                //ViewBag.ProductManagers = adProductManagers;
                //ViewBag.StatusHistory = new List<ClaimStatusHistory>();
                //ViewBag.Facts = db.LoadProtectFacts();
                //ViewBag.DeliveryTimes = db.LoadDeliveryTimes();
                //ViewBag.HasTransmissedPosition = false.ToString().ToLower();
                //ViewBag.Currencies = db.LoadCurrencies();
                
                var dealTypeString = String.Empty;
                var tenderStatus = String.Empty;
                if (claimId.HasValue && cv.HasValue)
                {
                    claim = new TenderClaim(claimId.Value);
                    if (claim != null)
                    {
                        //////var allPositions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                        //////var editablePosIds = new List<int>();
                        //////foreach (var position in allPositions)
                        //////{
                        //////    if (position.State == 5) editablePosIds.Add(position.Id);
                        //////}
                        //////ViewBag.EditablePositions = editablePosIds;
                        //////ViewBag.HasTransmissedPosition = db.HasTenderClaimTransmissedPosition(claimId.Value, cv.Value).ToString().ToLower();
                        //проверка наличия доступа к данной заявке
                        if (!isController)
                        {
                            if (claim.Manager.Id == user.Sid || claim.Author.Sid == user.Sid)
                            {

                            }
                            else if (isManager)
                            {
                                var subs = Employee.GetSubordinates(user.Sid).ToList();
                                if (!Employee.UserIsSubordinate(subs, claim.Manager.Id) && !Employee.UserIsSubordinate(subs, claim.Author.Sid))
                                {
                                    var dict = new RouteValueDictionary();
                                    dict.Add("message", "У Вас нет доступа к этой странице");
                                    return RedirectToAction("ErrorPage", "Auth", dict);
                                }
                            }
                        }

                        //var managerFromAd = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
                        //if (managerFromAd != null)
                        //{
                        //    claim.Manager.Name = managerFromAd.Name;
                        //    claim.Manager.ShortName = managerFromAd.ShortName;
                        //    claim.Manager.ChiefShortName = managerFromAd.ChiefShortName;
                        //}



                        //var dealTypes = db.LoadDealTypes();
                        //var dealType = dealTypes.FirstOrDefault(x => x.Id == claim.DealType);
                        //if (dealType != null)
                        //{
                        //    dealTypeString = dealType.Value;
                        //}
                        //var tenderStatusList = db.LoadTenderStatus();
                        //var status = tenderStatusList.FirstOrDefault(x => x.Id == claim.TenderStatus);
                        //if (status != null)
                        //{
                        //    tenderStatus = status.Value;
                        //}
                        //получение позиций по заявке и расчета к ним
                        claim.Certs = db.LoadClaimCerts(claimId.Value);
                        claim.Files = db.LoadTenderClaimFiles(claimId.Value);
                        //////claim.Positions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                        //////if (claim.Positions != null && claim.Positions.Any())
                        //////{
                        //////    var productManagers = claim.Positions.Select(x => x.ProductManager).ToList();
                        //////    foreach (var productManager in productManagers)
                        //////    {
                        //////        var productManagerFromAd =
                        //////            adProductManagers.FirstOrDefault(x => x.Id == productManager.Id);
                        //////        if (productManagerFromAd != null)
                        //////        {
                        //////            productManager.Name = productManagerFromAd.Name;
                        //////        }
                        //////    }
                        //////    var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                        //////    if (calculations != null && calculations.Any())
                        //////    {
                        //////        foreach (var position in claim.Positions)
                        //////        {
                        //////            if (position.State == 1) continue;
                        //////            position.Calculations =
                        //////                calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                        //////            position.Calculations.Reverse();
                        //////        }
                        //////    }
                        //////}
                        //ViewBag.StatusHistory = db.LoadStatusHistoryForClaim(claimId.Value);
                    }
                }
                //ViewBag.Claim = claim;
                ViewBag.DealType = dealTypeString;
                ViewBag.status = tenderStatus;
            }
            catch (Exception ex)
            {
                ViewBag.Error = true.ToString().ToLower();
            }
            return View(claim);
        }


        //форма заявки, если передан параметр idClaim, то загружается инфа по заявки с этим id
        public ActionResult Index(int? claimId, int? cv)
        {


            var user = CurUser;
            //if (!UserHelper.IsController(user) && UserHelper.IsProductManager(user))
            if (CurUser.Is(AdGroup.SpeCalcProduct))
                return RedirectToAction("Index", "Calc", new { claimId = claimId, cv = cv });

            if (user == null || !CurUser.HasAccess(AdGroup.SpeCalcManager, AdGroup.SpeCalcOperator,AdGroup.SpeCalcProduct))
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к приложению");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }

            if (CurUser.HasAccess(AdGroup.SpeCalcManager, AdGroup.SpeCalcOperator))
                return RedirectToAction("IndexManager", new { claimId , cv});

            return null;
            //Проверка версии, по умолчанию показываем первую если никакой не указано
            if (claimId.HasValue && !cv.HasValue)
            {
                var verList = DbEngine.GetCalcVersionList(claimId.Value);
                if (verList.Any())
                {
                    int lastVersion = verList.Last();
                    return RedirectToAction("Index", new {claimId = claimId, cv = lastVersion});
                }
                else
                {
                    cv = 0;
                }
            }

            //получения текущего юзера и проверка наличия у него доступа к странице
            ViewBag.Error = false.ToString().ToLower();
            TempData["tenderClaimFileFormats"] = WebConfigurationManager.AppSettings["FileFormat4TenderClaimFile"];
            
            
            ViewBag.UserName = user.FullName;
            var isController = user.Is(AdGroup.SpeCalcKontroler);//UserHelper.IsController(user);
            var isManager = user.Is(AdGroup.SpeCalcManager);//UserHelper.IsManager(user);
            var isOperator = user.Is(AdGroup.SpeCalcOperator);//UserHelper.IsOperator(user);
            //if (!isController && !isManager && !isOperator)
            //{
            //    var dict = new RouteValueDictionary();
            //    dict.Add("message", "У Вас нет доступа к этой странице");
            //    return RedirectToAction("ErrorPage", "Auth", dict);
            //}
            try
            {
                //получение необходимой инфы из БД и ActiveDirectory
                var managers = UserHelper.GetManagers();
                ViewBag.Managers = managers;
                ViewBag.DateStart = DateTime.Now.ToString("dd.MM.yyyy");
                var db = new DbEngine();
                ViewBag.NextDateMin = DateTime.Now.DayOfWeek == DayOfWeek.Friday
                    ? DateTime.Now.AddDays(4).ToShortDateString()
                    : DateTime.Now.AddDays(2).ToShortDateString();
                ViewBag.DealTypes = db.LoadDealTypes();
                ViewBag.ClaimStatus = db.LoadClaimStatus();
                var adProductManagers = UserHelper.GetProductManagers();
                ViewBag.ProductManagers = adProductManagers;
                ViewBag.StatusHistory = new List<ClaimStatusHistory>();
                ViewBag.Facts = db.LoadProtectFacts();
                ViewBag.DeliveryTimes = db.LoadDeliveryTimes();
                ViewBag.HasTransmissedPosition = false.ToString().ToLower();
                ViewBag.Currencies = db.LoadCurrencies();
                TenderClaim claim = null;
                var dealTypeString = String.Empty;
                var tenderStatus = String.Empty;
                if (claimId.HasValue && cv.HasValue)
                {
                    claim = db.LoadTenderClaimById(claimId.Value);
                    if (claim != null)
                    {
                        var allPositions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                        var editablePosIds = new List<int>();
                        foreach (var position in allPositions)
                        {
                           if (position.State == 5) editablePosIds.Add(position.Id);
                        }
                        ViewBag.EditablePositions = editablePosIds;
                        ViewBag.HasTransmissedPosition = db.HasTenderClaimTransmissedPosition(claimId.Value, cv.Value).ToString().ToLower();
                        //проверка наличия доступа к данной заявке
                        if (!isController)
                        {
                            if (claim.Manager.Id == user.Sid || claim.Author.Sid == user.Sid)
                            {
                                
                            }
                            else if (isManager)
                            {
                                var subs = Employee.GetSubordinates(user.Sid).ToList();
                                if (!Employee.UserIsSubordinate(subs, claim.Manager.Id) && !Employee.UserIsSubordinate(subs, claim.Author.Sid))
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
                            claim.Manager.ShortName = managerFromAd.ShortName;
                            claim.Manager.ChiefShortName = managerFromAd.ChiefShortName;
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
                        //получение позиций по заявке и расчета к ним
                        claim.Certs = db.LoadClaimCerts(claimId.Value);
                        claim.Files = db.LoadTenderClaimFiles(claimId.Value);
                        claim.Positions = db.LoadSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
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
                            var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId.Value, cv.Value);
                            if (calculations != null && calculations.Any())
                            {
                                foreach (var position in claim.Positions)
                                {
                                    if (position.State == 1) continue;
                                    position.Calculations =
                                        calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                                    position.Calculations.Reverse();
                                }
                            }
                        }
                        ViewBag.StatusHistory = db.LoadStatusHistoryForClaim(claimId.Value);
                    }
                }
                ViewBag.Claim = claim;
                ViewBag.DealType = dealTypeString;
                ViewBag.status = tenderStatus;
            }
            catch (Exception)
            {
                ViewBag.Error = true.ToString().ToLower();
            }
            return View();
        }
        [HttpGet]
        //список заявок
        public ActionResult List(string filterString)
        {
            var filter = string.IsNullOrEmpty(filterString)
                ? null
                : JsonConvert.DeserializeObject<FilterTenderClaim>(filterString);
            /*var testRoles = new List<Role>()
            {
                Role.Enter,
                Role.Manager,
               // Role.Controller
            };
            var user = new EmployeeSm("Some Sid").GetUserBase(testRoles);*/
            var user = GetUser();
            var isManager = user.Is(AdGroup.SpeCalcManager);//UserHelper.IsManager(user);
            var isProduct = user.Is(AdGroup.SpeCalcProduct);//UserHelper.IsProductManager(user);
            var isController = user.Is(AdGroup.SpeCalcKontroler);//UserHelper.IsController(user);
            var isOperator = user.Is(AdGroup.SpeCalcOperator);//UserHelper.IsOperator(user);
            var subordinates = Employee.GetSubordinates(user.Sid).ToList();
            subordinates.Add(new KeyValuePair<string, string>(user.Sid, user.DisplayName));
            var subSids = string.Join(",", subordinates.Select(s => s.Key));
            filter = filter ?? new FilterTenderClaim() {RowCount = 30};
            if (!isController)
            {
                if (isManager && (string.IsNullOrEmpty(filter.IdManager) || !subSids.Contains(filter.IdManager)))
                filter.IdManager = subSids;
                if (isProduct && (string.IsNullOrEmpty(filter.IdProductManager) || !subSids.Contains(filter.IdProductManager)))
                filter.IdProductManager = subSids;
            }
            var mainRole = isController
                    ? Role.Controller
                    : isManager 
                        ? Role.Manager 
                        : isProduct 
                            ? Role.ProductManager 
                            :  isOperator
                            ? Role.Operator
                            : Role.Enter;
            if (mainRole == Role.Enter)
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к приложению");
                return RedirectToAction("ErrorPage", "Auth", dict);
            }
            var listViewModel = new ListViewModels(filter, subordinates, mainRole);
            ViewBag.UserName = user.FullName;
            ViewBag.CanEdit = isManager || isOperator || isController;
            ViewBag.CanCalc = isProduct || isController;
            return View(listViewModel);
        }
        [HttpGet]
        public ActionResult NewClaim(string errorMessage)
        {
            var user = new EmployeeSm(CurUser.Sid);
            ViewBag.UserName = user.FullName;
            TempData["userSid"] = user.AdSid;
            TempData["errorMessage"] = errorMessage;
            TempData["department"] = user.DepartmentName;
            return View();
        }

        [HttpPost]
        public ActionResult NewClaim(TenderClaim model, string managerSid)
        {
            var manager = new EmployeeSm(managerSid);
            model.Manager = new Manager()
            {
                Id = manager.AdSid,
                ShortName = manager.DisplayName,
                SubDivision = manager.DepartmentName
            };
            model.Author = new AdUser() { Sid= CurUser.Sid, DisplayName = CurUser.DisplayName, FullName = CurUser.FullName};
            model.RecordDate = DateTime.Now;
            model.ClaimStatus = 1;
            model.TenderStatus = 1;
            var db = new DbEngine();
            var success = db.SaveTenderClaim(ref model);
            if (success)
            {
                string message = "";
                if (Request.Files.Count > 0)
                {
                    int idClaim = model.Id;
                    if (idClaim != null && idClaim > 0)
                    {
                        for (int i = 0; i < Request.Files.Count; i++)
                        {
                            var file = Request.Files[i];
                            var fileFormats = WebConfigurationManager.AppSettings["FileFormat4TenderClaimFile"].Split(',').Select(s => s.ToLower()).ToArray();
                            byte[] fileData = null;
                            if (Array.IndexOf(fileFormats, Path.GetExtension(file.FileName).ToLower()) > -1)
                            {
                                using (var br = new BinaryReader(file.InputStream))
                                {
                                    fileData = br.ReadBytes(file.ContentLength);
                                }
                                var claimFile = new TenderClaimFile() { IdClaim = idClaim, File = fileData, FileName = file.FileName };
                                db.SaveTenderClaimFile(ref claimFile);
                            }
                            else if (file.ContentLength > 0) message += String.Format("Файл {0} имеет недопустимое расширение.", file.FileName);
                        }
                        //}
                    }
                }
                TempData["error"] = message;
                if (success)
                {
                    //История изменения статуса
                    var statusHistory = new ClaimStatusHistory()
                    {
                        Date = DateTime.Now,
                        IdClaim = model.Id,
                        IdUser = model.Author.Sid,
                        Status = new ClaimStatus() { Id = model.ClaimStatus },
                        Comment = "Автор: " + model.Author.DisplayName
                    };
                    db.SaveClaimStatusHistory(statusHistory);
                    statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                    //>>>>Уведомления
                    if (model.Author.Sid != model.Manager.Id)
                    {
                        var host = ConfigurationManager.AppSettings["AppHost"];
                        var emessage = new StringBuilder();
                        emessage.Append("Добрый день!");
                        //message.Append(manager.Name);
                        emessage.Append("<br/>");
                        emessage.Append("Пользователь ");
                        emessage.Append(model.Author.DisplayName);
                        emessage.Append(" создал заявку где Вы назначены менеджером.");
                        emessage.Append("<br/><br />");
                        emessage.Append(GetClaimInfo(model));
                        emessage.Append("<br/>");
                        emessage.Append("Ссылка на заявку: ");
                        emessage.Append("<a href='" + host + "/Claim/Index?claimId=" + model.Id + "'>" + host +
                                       "/Claim/Index?claimId=" + model.Id + "</a>");
                        //message.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(new List<UserBase>() { manager.GetUserBase(new List<Role>() { Role.Manager }) }, emessage.ToString(),
                            String.Format("{0} - {1} - Новая заявка СпецРасчет", model.TenderNumber, model.Customer));
                    }
                }
                return RedirectToAction("Index", "Claim", new { claimId = model.Id });
            }
            return RedirectToAction("NewClaim", "Claim", new { errorMessage = "при сохранении возникла ошибка" });
        }

        [HttpGet]
        public string GetDepartment(string sid)
        {
            return new EmployeeSm(sid).DepartmentName;
        }
        //////public ActionResult ListOld()
        //////{
        //////    //получение пользователя и через наличие у него определенных ролей, определяются настройки по 
        //////    //функциональности на странице
        //////    var user = GetUser();
        //////    if (user == null || !UserHelper.IsUserAccess(user))
        //////    {
        //////        var dict = new RouteValueDictionary();
        //////        dict.Add("message", "У Вас нет доступа к приложению");
        //////        return RedirectToAction("ErrorPage", "Auth", dict);
        //////    }
        //////    ViewBag.UserName = user.Name;
        //////    var showCalculate = false;
        //////    var showEdit = false;
        //////    var changeTenderStatus = false;
        //////    var filterProduct = string.Empty;
        //////    var filterManager = string.Empty;
        //////    var clickAction = string.Empty;
        //////    var posibleAction = string.Empty;
        //////    var userId = string.Empty;
        //////    var author = string.Empty;
        //////    var reportExcel = false;
        //////    var deleteClaim = "none";
        //////    var newClaim = "true";
        //////    var filterClaimStatus = new List<int>();
        //////    var isController = UserHelper.IsController(user);
        //////    var isTenderStatus = UserHelper.IsTenderStatus(user);
        //////    var isManager = UserHelper.IsManager(user);
        //////    var isProduct = UserHelper.IsProductManager(user);
        //////    var isOperator = UserHelper.IsOperator(user);
        //////    if (isController)
        //////    {
        //////        showCalculate = true;
        //////        showEdit = true;
        //////        changeTenderStatus = true;
        //////        clickAction = "editClaim";
        //////        posibleAction = "all";
        //////        reportExcel = true;
        //////        deleteClaim = "true";
        //////    }
        //////    else
        //////    {
        //////        if (isTenderStatus)
        //////        {
        //////            changeTenderStatus = true;
        //////            clickAction = "null";
        //////            posibleAction = "null";
        //////            newClaim = "false";
        //////            if (isOperator || isManager) newClaim = "true";
        //////        }
        //////        if (isManager)
        //////        {
        //////            showEdit = true;
        //////            //filterManager = user.Id;
        //////            clickAction = "editClaim";
        //////            posibleAction = "editClaim";
        //////            userId = user.Id;
        //////            filterClaimStatus.AddRange(new[] { 1, 2, 3, 6, 7 });
        //////            deleteClaim = "self&manager";
        //////        }
        //////        if (isProduct)
        //////        {
        //////            showCalculate = true;
        //////           // filterProduct = user.Id;
        //////            clickAction = "calculateClaim";
        //////            posibleAction = (isManager ? "all" : "calculateClaim");
        //////            userId = user.Id;
        //////            newClaim = "false";
        //////            if (isOperator || isManager) newClaim = "true";
        //////            if (!isManager) filterClaimStatus.AddRange(new[] { 2, 3, 6, 7 });
        //////        }
        //////        if (isOperator)
        //////        {
        //////            showEdit = true;
        //////            clickAction = "editClaim";
        //////            posibleAction = (isProduct ? "all" : "editClaim");
        //////            author = user.Id;
        //////            deleteClaim = "self";
        //////        }
        //////    }
        //////    ViewBag.Settings = new
        //////    {
        //////        showCalculate,
        //////        showEdit,
        //////        changeTenderStatus,
        //////        filterProduct,
        //////        filterManager,
        //////        clickAction,
        //////        posibleAction,
        //////        userId,
        //////        filterClaimStatus,
        //////        author,
        //////        reportExcel,
        //////        deleteClaim,
        //////        newClaim
        //////    };
        //////    ViewBag.Error = false.ToString().ToLower();
        //////    ViewBag.ClaimCount = 0;
        //////    try
        //////    {
        //////        //получение инфы по заявкам из БД
        //////        var db = new DbEngine();
        //////        var filter = new FilterTenderClaim()
        //////        {
        //////            RowCount = 30,
        //////        };
        //////        var subsProduct = new List<KeyValuePair<string, string>>();
        //////        var subsManagers = new List<KeyValuePair<string, string>>();
        //////        if (!string.IsNullOrEmpty(filterManager)) filter.IdManager = filterManager;
        //////        else
        //////        {
        //////            if (isManager && !isController)
        //////            {
        //////                filter.IdManager = user.Id;
        //////                subsManagers = Employee.GetSubordinates(user.Id).ToList();
        //////                if (subsManagers.Any())
        //////                {
        //////                    filter.IdManager = user.Id;// + ","+ String.Join(",", subsManagers);
        //////                    foreach (var sub in subsManagers)
        //////                    {
        //////                        if (sub.Key != null)
        //////                        {
        //////                            filter.IdManager += "," + sub.Key;
        //////                        }
        //////                    }
        //////                }
        //////            }
        //////        }
                
        //////        if (!string.IsNullOrEmpty(filterProduct)) filter.IdProductManager = filterProduct;
        //////        else
        //////        {
                    
        //////            if (isProduct && !isController)
        //////            {
        //////                filter.IdProductManager = user.Id;
        //////                subsProduct = Employee.GetSubordinates(user.Id).ToList();
        //////                if (subsProduct.Any())
        //////                {
        //////                    filter.IdProductManager = user.Id;// + "," + String.Join(",", subsProduct);
        //////                    foreach (var sub in subsProduct)
        //////                    {
        //////                        if (sub.Key != null)
        //////                        {
        //////                            filter.IdProductManager += "," + sub.Key;
        //////                        }
        //////                    }
        //////                }
        //////            }
        //////            //filter.IdProductManager = isProduct && !isController
        //////            //    ? String.Join(",", Employee.GetSubordinates(user.Id))
        //////            //    : String.Empty;
        //////        }
        //////        if (!string.IsNullOrEmpty(author)) filter.Author = author;
        //////        if (filterClaimStatus.Any()) filter.ClaimStatus = filterClaimStatus;
        //////        var claims = db.FilterTenderClaims(filter);
        //////        //снабженцы и менеджеры из ActiveDirectory
        //////        var prodManSelList = UserHelper.GetProductManagersSelectionList();
                
        //////        var adProductManagers = new List<ProductManager>();
                
        //////        adProductManagers = prodManSelList;
                
        //////        if (!isController && isProduct)
        //////        {
        //////            var subProds = Employee.GetSubordinateProductManagers(user.Id, subsProduct);
        //////            if (subProds.Any())
        //////            {
        //////                adProductManagers = subProds;
        //////            }
        //////            else
        //////            {
        //////                var curProd = new ProductManager() {Id = user.Id, ShortName = user.ShortName};
        //////                adProductManagers = new List<ProductManager>();
        //////                adProductManagers.Add(curProd);
        //////            }
        //////        }
        //////        var manSelList = UserHelper.GetManagersSelectionList();
                
        //////        var managers = new List<Manager>();
                
        //////            managers = manSelList;
                
        //////        if (!isController && isManager)
        //////        {
        //////            var subMans = Employee.GetSubordinateManagers(user.Id, subsManagers);
        //////            if (subMans.Any())
        //////            {
        //////                managers = subMans;
        //////            }
        //////            else
        //////            {
        //////                var curMan = new Manager() { Id = user.Id, ShortName = user.ShortName };
        //////                managers = new List<Manager>();
        //////                managers.Add(curMan);
        //////            }
        //////        }
                

        //////        //var prodManSelList = UserHelper.Get();

        //////        if (claims != null && claims.Any())
        //////        {
        //////            db.SetProductManagersForClaims(claims);
        //////            var claimProductManagers = claims.SelectMany(x => x.ProductManagers).ToList();
        //////            foreach (var claimProductManager in claimProductManagers)
        //////            {
        //////                claimProductManager.ShortName = prodManSelList.FirstOrDefault(x=>x.Id== claimProductManager.Id)?.ShortName;
        //////                //var productUser = UserHelper.GetUserById(claimProductManager.Id);
        //////                //if (productUser != null)
        //////                //{
        //////                //    claimProductManager.Name = productUser.Name;
        //////                //    claimProductManager.Name = productUser.ShortName;
        //////                //}
        //////                //var managerFromAD = adProductManagers.FirstOrDefault(x => x.Id == claimProductManager.Id);
        //////                //if (managerFromAD != null)
        //////                //{
        //////                //    claimProductManager.Name = managerFromAD.Name;
        //////                //    claimProductManager.ShortName = managerFromAD.ShortName;
        //////                //}
        //////            }
        //////            //var authorsList = UserHelper.GetAuthorsSelectionList();
        //////            foreach (var claim in claims)
        //////            {
        //////                claim.Manager.ShortName = manSelList.FirstOrDefault(x => x.Id == claim.Manager.Id)?.ShortName;
        //////                //if (manager != null)
        //////                //{
        //////                //    claim.Manager.ShortName = manager.ShortName;
        //////                //}
        //////                //var auth = authorsList.SingleOrDefault(x => x.Key == claim.Author.Id);
        //////                claim.Author = UserHelper.GetUserById(claim.Author.Id);//new UserBase() { Id= auth.Key, ShortName = auth.Value};// UserHelper.GetUserById(claim.Author.Id);

        //////            }
        //////            db.SetStatisticsForClaims(claims);
        //////        }
        //////        ViewBag.Claims = claims;
        //////        ViewBag.DealTypes = db.LoadDealTypes();
        //////        ViewBag.ClaimStatus = db.LoadClaimStatus();
        //////        ViewBag.ProductManagers = adProductManagers;
        //////        ViewBag.Managers = managers;
        //////        ViewBag.ClaimCount = db.GetCountFilteredTenderClaims(filter);
        //////        ViewBag.TenderStatus = db.LoadTenderStatus();
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        ViewBag.Error = true.ToString().ToLower();
        //////    }
        //////    return View();
        //////}

        //Excel
        //получение excel файла, для определения позиций по заявке 
        public ActionResult GetSpecificationFile(int claimId)
        {
            //if (!claimId.HasValue) return null;

            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            try
            {
                var db = new DbEngine();
                var claim = db.LoadTenderClaimById(claimId);
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
                var units = PositionUnit.GetList().ToArray();
                excBook = new XLWorkbook(ms);
                var workSheet = excBook.Worksheet("Лот");
                var userRangeSheet = excBook.Worksheet(2);
                var unitRangeSheet = excBook.Worksheet(3);

                if (workSheet != null && userRangeSheet != null)
                {
                    userRangeSheet.Visibility = XLWorksheetVisibility.Hidden;
                    unitRangeSheet.Visibility = XLWorksheetVisibility.Hidden;
                    //>>>>>>>>Шапка - Заполнение инфы о заявке<<<<<<
                    var dealTypes = db.LoadDealTypes();

                    var manager = UserHelper.GetUserById(claim.Manager.Id);
                    workSheet.Cell(1, 3).Value = claim.TenderNumber;
                    workSheet.Cell(2, 3).Value = claim.Customer;
                    workSheet.Cell(3, 3).Value = manager != null ? manager.ShortName : string.Empty;

                    for (var i = 0; i < units.Count(); i++)
                    {
                        var unit = units[i].Name;
                        var cell = unitRangeSheet.Cell(i + 1, 1);
                        if (cell != null)
                        {
                            cell.Value = unit;
                        }
                    }
                    var namedRangeUnit = unitRangeSheet.Range(unitRangeSheet.Cell(1, 1), unitRangeSheet.Cell(units.Count(), 1));

                    for (var i = 0; i < productManagers.Count(); i++)
                    {
                        var product = productManagers[i];
                        var cell = userRangeSheet.Cell(i + 1, 1);
                        if (cell != null)
                        {
                            cell.Value = GetUniqueDisplayName(product);
                        }
                    }
                    var namedRange = userRangeSheet.Range(userRangeSheet.Cell(1, 1), userRangeSheet.Cell(productManagers.Count(), 1));

                    for (int uc = 0; uc <= 1000; uc++)
                    {
                        var workRangeUnit = workSheet.Cell(uc+5, 4);
                        if (workRangeUnit != null){
                        
                        var validation = workRangeUnit.SetDataValidation();
                        validation.AllowedValues = XLAllowedValues.List;
                        validation.InCellDropdown = true;
                        validation.Operator = XLOperator.Between;
                        validation.List(namedRangeUnit);
                        }

                        var workRange = workSheet.Cell(uc + 5, 6);
                        if (workRange != null)
                        {
                            var validation = workRange.SetDataValidation();
                            validation.AllowedValues = XLAllowedValues.List;
                            validation.InCellDropdown = true;
                            validation.Operator = XLOperator.Between;
                            validation.List(namedRange);
                        }
                    }
                    
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

        /// <summary>
        /// Шаблон для Транснефти
        /// </summary>
        /// <param name="claimId"></param>
        /// <returns></returns>
        public ActionResult GetSpecificationFileTrans(int claimId)
        {
            //if (!claimId.HasValue) return null;

            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            try
            {
                var db = new DbEngine();
                var claim = db.LoadTenderClaimById(claimId);
                //получение файла-шаблона
                var filePath = Path.Combine(Server.MapPath("~"), "App_Data", "SpecificationTrans.xlsx");
                using (var fs = System.IO.File.OpenRead(filePath))
                {
                    var buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Count());
                    ms.Write(buffer, 0, buffer.Count());
                    ms.Seek(0, SeekOrigin.Begin);
                }
                //создание диапазона выбора снабженцев
                var productManagers = UserHelper.GetProductManagers();
                var units = PositionUnit.GetList().ToArray();
                excBook = new XLWorkbook(ms);
                var workSheet = excBook.Worksheet("Лот");
                var userRangeSheet = excBook.Worksheet(2);
                var unitRangeSheet = excBook.Worksheet(3);

                if (workSheet != null && userRangeSheet != null)
                {
                    userRangeSheet.Visibility = XLWorksheetVisibility.Hidden;
                    unitRangeSheet.Visibility = XLWorksheetVisibility.Hidden;
                    //>>>>>>>>Шапка - Заполнение инфы о заявке<<<<<<
                    var dealTypes = db.LoadDealTypes();

                    var manager = UserHelper.GetUserById(claim.Manager.Id);
                    workSheet.Cell(1, 3).Value = claim.TenderNumber;
                    workSheet.Cell(2, 3).Value = claim.Customer;
                    workSheet.Cell(3, 3).Value = manager != null ? manager.ShortName : string.Empty;

                    for (var i = 0; i < units.Count(); i++)
                    {
                        var unit = units[i].Name;
                        var cell = unitRangeSheet.Cell(i + 1, 1);
                        if (cell != null)
                        {
                            cell.Value = unit;
                        }
                    }
                    var namedRangeUnit = unitRangeSheet.Range(unitRangeSheet.Cell(1, 1), unitRangeSheet.Cell(units.Count(), 1));

                    for (var i = 0; i < productManagers.Count(); i++)
                    {
                        var product = productManagers[i];
                        var cell = userRangeSheet.Cell(i + 1, 1);
                        if (cell != null)
                        {
                            cell.Value = GetUniqueDisplayName(product);
                        }
                    }
                    var namedRange = userRangeSheet.Range(userRangeSheet.Cell(1, 1), userRangeSheet.Cell(productManagers.Count(), 1));

                    for (int uc = 0; uc <= 1000; uc++)
                    {
                        var workRangeUnit = workSheet.Cell(uc + 5, 9);
                        if (workRangeUnit != null)
                        {

                            var validation = workRangeUnit.SetDataValidation();
                            validation.AllowedValues = XLAllowedValues.List;
                            validation.InCellDropdown = true;
                            validation.Operator = XLOperator.Between;
                            validation.List(namedRangeUnit);
                        }

                        var workRange = workSheet.Cell(uc + 5, 11);
                        if (workRange != null)
                        {
                            var validation = workRange.SetDataValidation();
                            validation.AllowedValues = XLAllowedValues.List;
                            validation.InCellDropdown = true;
                            validation.Operator = XLOperator.Between;
                            validation.List(namedRange);
                        }
                    }

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
                    FileDownloadName = "SpecificationTrans.xlsx"
                };
            }
            else
            {
                return null;
            }
        }

        //private string GetUnitStr(PositionUnit unit)
        //{
        //    string res = String.Empty;

        //    switch (unit)
        //    {
        //        case PositionUnit.Thing:
        //            res = "шт";
        //            break;
        //        case PositionUnit.Package:
        //            res = "упак";
        //            break;
        //        case PositionUnit.Metr:
        //            res = "м";
        //            break;
        //    }

        //    return res;
        //}

        //Excel
        //получение excel файла, содержащем только расчет по заявке
        public ActionResult GetSpecificationFileOnlyCalculation(int claimId, int cv)
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            var message = string.Empty;
            try
            {
                
                //получение позиций по заявке и расчетов к ним
                var db = new DbEngine();
                var positions = SpecificationPosition.GetListWithCalc(claimId, cv);//db.LoadSpecificationPositionsForTenderClaim(claimId, cv);
                //var facts = db.LoadProtectFacts();
                if (positions.Any())
                {
                    //var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId, cv);
                    //if (calculations != null && calculations.Any())
                    //{
                    //    foreach (var position in positions)
                    //    {
                    //        if (position.State == 1) continue;
                    //        position.Calculations =
                    //            calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                    //    }
                    //}
                    var filePath = Path.Combine(Server.MapPath("~"), "App_Data", "Specification_fin.xlsx");
                    using (var fs = System.IO.File.OpenRead(filePath))
                    {
                        var buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Count());
                        ms.Write(buffer, 0, buffer.Count());
                        ms.Seek(0, SeekOrigin.Begin);
                    }
                    //создание файла excel с инфой по расчетам
                    excBook = new XLWorkbook(ms);
                    var workSheet = excBook.Worksheet("WorkSheet");
                    workSheet.Name = "лот";
                    var claim = db.LoadTenderClaimById(claimId);
                    //>>>>>>>>Шапка - Заполнение инфы о заявке<<<<<<

                    //Менеджер из Москвы
                    bool managerIsMoscou = false;
                    string managerSid = GetUser().Sid;
                    var dtUserIsMoscou = Db.CheckManagerIsMoscou(managerSid);
                    if (dtUserIsMoscou.Rows.Count > 0)
                    {
                        managerIsMoscou = dtUserIsMoscou.Rows[0]["result"].ToString().Equals("1");
                    }
                    // />Менеджер из Москвы

                    var dealTypes = db.LoadDealTypes();
                    var deliveryTimes = db.LoadDeliveryTimes();
                    UserBase manager;
                    try
                    {
                        manager = UserHelper.GetUserById(claim.Manager.Id);
                    }
                    catch (Exception ex)
                    {
                        manager = new UserBase();
                    }
                    var dt = Db.GetExchangeRatesOnDate(DateTime.Now);

                    double? usdRate = null;
                    double? eurRate = null;

                    if (dt.Rows.Count >= 3)
                    {
                        usdRate = Convert.ToDouble(dt.Rows[1]["price"].ToString()) * 1.03;
                        eurRate = Convert.ToDouble(dt.Rows[2]["price"].ToString()) * 1.03;
                    }

                    int rowHead = 1;

                    var usdCell = workSheet.Cell(rowHead, 4);
                    usdCell.Value =
                        usdRate.HasValue
                            ? usdRate.Value.ToString("N2")
                            : string.Empty;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.Number;

                    var eurCell = workSheet.Cell(++rowHead, 4);
                    eurCell.Value = eurRate.HasValue
                            ? eurRate.Value.ToString("N2")
                            : string.Empty;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.Number;

                    var eurRicohCell = workSheet.Cell(++rowHead, 4);
                    var profitCell = workSheet.Cell(++rowHead, 4);//Рентабельность
                    workSheet.Cell(++rowHead, 4).Value = dealTypes.First(x => x.Id == claim.DealType).Value;
                    workSheet.Cell(++rowHead, 4).Value = manager != null ? manager.ShortName : string.Empty;
                    workSheet.Cell(++rowHead, 4).Value = claim.Customer;
                    //срок готовности цен от снабжения???
                    rowHead++;
                    workSheet.Cell(++rowHead, 4).Value = claim.Sum;//Максимальная цена контракта???
                    workSheet.Cell(++rowHead, 4).Value = claim.DeliveryDateString;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.DateTime;
                    workSheet.Cell(++rowHead, 4).Value = claim.DeliveryPlace;
                    workSheet.Cell(++rowHead, 4).Value = claim.KPDeadlineString;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.DateTime;
                    workSheet.Cell(++rowHead, 4).Value = claim.AuctionDateString;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.DateTime;
                    workSheet.Cell(++rowHead, 4).Value = claim.Comment;

                    var row = rowHead+2;
                    int firstRow = row;
                    var rowNumber = 1;
                    //строки расчета
                    foreach (var position in positions)
                    {
                        position.Name = position.Name.Replace("\n", "").Replace("\r", "");

                        if (position.Calculations != null && position.Calculations.Any())
                        {
                            int calcNum = 0;
                            foreach (var calculation in position.Calculations)
                            {
                                calcNum++;
                                calculation.Name = calculation.Name.Replace("\n", "").Replace("\r", "");

                                int col = 0;
                                workSheet.Cell(row, ++col).Value = rowNumber;
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                //Не показываем подряд одни и те же надписи названия и каталожника
                                workSheet.Cell(row, ++col).Value = calcNum > 1 ? String.Empty : position.CatalogNumber;
                                    //? position.CatalogNumber
                                    //: calculation.CatalogNumber;
                                double hPosCatNum = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 30);
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                //Не показываем подряд одни и те же надписи названия и каталожника
                                workSheet.Cell(row, ++col).Value = calcNum > 1 ? String.Empty : position.Name;
                                double hPosName = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 40);
                                    //String.IsNullOrEmpty(calculation.Name)? position.Name: calculation.Name;
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                                workSheet.Cell(row, ++col).Value = calculation.CatalogNumber;
                                double hCalcCatNum = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 30);
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                workSheet.Cell(row, ++col).Value = calculation.Name + (!String.IsNullOrEmpty(calculation.Name) ? "\r\n" : "")+  (!String.IsNullOrEmpty(calculation.Replace) ? "Замена:\r\n" + calculation.Replace : String.Empty);
                                double hCalcName = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 40);
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                                //workSheet.Cell(row, ++col).Value = calculation.Replace;
                                //workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                                
                                workSheet.Cell(row, ++col).Value = position.UnitName;
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                var countCell = workSheet.Cell(row, ++col);
                                countCell.Value = position.Value;
                                
                                countCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                if (!String.IsNullOrEmpty(position.ProductManagerName))
                                {
                                    //var prodManager = UserHelper.GetUserById(position.ProductManager.Id);
                                    //workSheet.Cell(row, ++col).Value = prodManager == null
                                    //    ? String.Empty
                                    //    : prodManager.ShortName;
                                    workSheet.Cell(row, ++col).Value = position.ProductManagerName;
                                }
                                else
                                {
                                    ++col;
                                }
                                double hProdManager = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 16);
                                workSheet.Cell(row, ++col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                if (calculation.DeliveryTime != null)
                                {
                                    var delivTime = deliveryTimes.First(x => x.Id == calculation.DeliveryTime.Id);
                                    workSheet.Cell(row, col).Value = delivTime == null ? String.Empty : delivTime.Value;
                                }
                                double hCalcDeliv = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 16);
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                string partNum4Online = String.IsNullOrEmpty(position.CatalogNumber)? calculation.CatalogNumber: position.CatalogNumber;
                                    string onlinePrice = String.Empty;
                                try
                                {onlinePrice = CatalogProduct.PriceRequest(partNum4Online);}
                                catch {
                                    
                                }
                                workSheet.Cell(row, ++col).Value = onlinePrice;//Цена B2B
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                var priceUsdCell = workSheet.Cell(row, ++col);
                                priceUsdCell.Value = calculation.PriceUsd;
                                priceUsdCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                priceUsdCell.Style.NumberFormat.Format = "$ #,##0.00";
                                var priceEurCell = workSheet.Cell(row, ++col);
                                priceEurCell.Value = calculation.PriceEur;
                                priceEurCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                priceEurCell.Style.NumberFormat.Format = "€ #,##0.00";
                                var priceEurRicohCell = workSheet.Cell(row, ++col);
                                priceEurRicohCell.Value = calculation.PriceEurRicoh;
                                priceEurRicohCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                priceEurRicohCell.Style.NumberFormat.Format = "€ #,##0.00";
                                var priceRublCell = workSheet.Cell(row, ++col);
                                priceRublCell.Value = calculation.PriceRubl;
                                priceRublCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                                //Вход за ед
                                if (String.IsNullOrEmpty(priceRublCell.Value.ToString().Trim()))
                                {
                                    if (!String.IsNullOrEmpty(priceEurRicohCell.Value.ToString().Trim()))
                                    {
                                        priceRublCell.FormulaR1C1 = String.Format("{0}*{1}", eurRicohCell.Address.ToStringFixed(), priceEurRicohCell.Address);
                                    }
                                    if (!String.IsNullOrEmpty(priceEurCell.Value.ToString().Trim()))
                                    {
                                        priceRublCell.FormulaR1C1 = String.Format("{0}*{1}", eurCell.Address.ToStringFixed(), priceEurCell.Address);
                                    }
                                    if (!String.IsNullOrEmpty(priceUsdCell.Value.ToString().Trim()))
                                    {
                                        priceRublCell.FormulaR1C1 = String.Format("{0}*{1}", usdCell.Address.ToStringFixed(), priceUsdCell.Address);
                                    }

                                }
                                // />Вход за ед
                                priceRublCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                //Сумма
                                var priceSumCell = workSheet.Cell(row, ++col);
                                priceSumCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                priceSumCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                priceSumCell.FormulaR1C1 = String.Format("{0}*{1}", countCell.Address,
                                    priceRublCell.Address);
                                // />Сумма

                                //Цена с ТЗР
                                var priceTzrCell = workSheet.Cell(row, ++col);
                                string formulaPriceTzrCell = String.Format("(({0}*1.02)*1.02)", priceRublCell.Address);
                                if (managerIsMoscou)
                                {
                                    formulaPriceTzrCell = String.Format("({0}*1.02)", priceRublCell.Address);
                                }
                                priceTzrCell.FormulaR1C1 = formulaPriceTzrCell;
                                priceTzrCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                priceTzrCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                // /> Цена с ТЗР

                                //Сумма с ТЗР
                                var sumTzrCell = workSheet.Cell(row, ++col);
                                string formulaSumTzrCell = String.Format("{0}*{1}", countCell.Address,priceTzrCell.Address);
                                sumTzrCell.FormulaR1C1 = formulaSumTzrCell;
                                sumTzrCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                sumTzrCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                // /> Сумма с ТЗР

                                //Цена С НДС
                                var priceNdsCell = workSheet.Cell(row, ++col);
                                string formulaPriceNdsCell = String.Format("{0}*(1+({1}/100))", priceTzrCell.Address, profitCell.Address);
                                priceNdsCell.FormulaR1C1 = formulaPriceNdsCell;
                                priceNdsCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                priceNdsCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                // /> Цена с НДС

                                //Сумма с НДС
                                var sumNdsCell = workSheet.Cell(row, ++col);
                                string formulaSumNdsCell = String.Format("{0}*{1}", countCell.Address, priceNdsCell.Address);
                                sumNdsCell.FormulaR1C1 = formulaSumNdsCell;
                                sumNdsCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                sumNdsCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                // /> Сумма с НДС

                                //workSheet.Cell(row, 5).Value = !calculation.PriceCurrency.Equals(0)
                                //    ? calculation.PriceCurrency.ToString("N2")
                                //    : string.Empty;
                                //workSheet.Cell(row, 6).Value = !calculation.SumCurrency.Equals(0)
                                //    ? calculation.SumCurrency.ToString("N2")
                                //    : string.Empty;
                                //workSheet.Cell(row, 7).Value = "";//currencies.First(x => x.Id == calculation.Currency).Value;
                                //workSheet.Cell(row, 7).Value = !calculation.PriceRub.Equals(0)
                                //    ? calculation.PriceRub.ToString("N2")
                                //    : string.Empty;
                                //workSheet.Cell(row, 8).Value = !calculation.SumRub.Equals(0)
                                //    ? calculation.SumRub.ToString("N2")
                                //    : string.Empty;

                                workSheet.Cell(row, ++col).Value = calculation.Provider;
                                workSheet.Cell(row, col)
                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                //workSheet.Cell(row, 9).Value = calculation.ProtectFact != null ?
                                //    facts.First(x => x.Id == calculation.ProtectFact.Id).Value : String.Empty;
                                //workSheet.Cell(row, 10).Value = calculation.ProtectCondition;
                                workSheet.Cell(row, ++col).Value = calculation.Comment;
                                workSheet.Cell(row, col).Style.Font.SetFontColor(XLColor.Red);
                                double hComent = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 40);


                                double[] arr = { hPosCatNum, hPosName, hCalcName, hCalcDeliv, hProdManager, hComent, hCalcCatNum };
                                workSheet.Row(row).Height = arr.Max();
                                row++;
                                rowNumber++;
                            }
                        }

                        else
                        {
                            int col = 0;
                            workSheet.Cell(row, ++col).Value = rowNumber;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, ++col).Value = position.CatalogNumber;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            
                            workSheet.Cell(row, ++col).SetValue(position.Name);
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                            workSheet.Row(row).Height = GetCellHeight(position.CatalogNumber.Length, 10);
                            workSheet.Row(row).Height = GetCellHeight(position.Name.Length, 40);
                            ++col;
                            ++col;
                            //workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                            workSheet.Cell(row, ++col).Value = position.UnitName;

                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, ++col).Value = position.Value;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            if (position.ProductManager != null)
                            {
                                var prodManager = UserHelper.GetUserById(position.ProductManager.Id);
                                workSheet.Cell(row, ++col).Value = prodManager == null
                                    ? String.Empty
                                    : prodManager.ShortName;
                            }
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                            ++col;//Цена B2B

                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, col).Style.NumberFormat.Format = "$ #,###";
                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, col).Style.NumberFormat.Format = "€ #,###";
                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, col).Style.NumberFormat.Format = "€ #,###";
                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, col).Style.NumberFormat.Format = "₽ #,###";
                            col = col + 6;
                            workSheet.Cell(row, col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            row++;
                            rowNumber++;
                        }
                    }


                    var range = workSheet.Range(workSheet.Cell(firstRow, 1), workSheet.Cell(row - 1, 21));
                    range.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetBottomBorderColor(XLColor.Gray);
                    range.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetTopBorderColor(XLColor.Gray);
                    range.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetRightBorderColor(XLColor.Gray);
                    range.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetLeftBorderColor(XLColor.Gray);

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
            catch (Exception ex)
            {
                error = true;
                message = "Ошибка сервера " + ex.Message;
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
                return null;
            }
        }

        [HttpGet]
        public ActionResult GetSpecificationFileOnlyCalculationTrans(int claimId, int cv)
        {
            XLWorkbook excBook = null;
            var ms = new MemoryStream();
            var error = false;
            var message = string.Empty;
            try
            {

                //получение позиций по заявке и расчетов к ним
                var db = new DbEngine();
                var positions = SpecificationPosition.GetListWithCalc(claimId, cv);//db.LoadSpecificationPositionsForTenderClaim(claimId, cv);
                //var facts = db.LoadProtectFacts();
                if (positions.Any())
                {
                    //var calculations = db.LoadCalculateSpecificationPositionsForTenderClaim(claimId, cv);
                    //if (calculations != null && calculations.Any())
                    //{
                    //    foreach (var position in positions)
                    //    {
                    //        if (position.State == 1) continue;
                    //        position.Calculations =
                    //            calculations.Where(x => x.IdSpecificationPosition == position.Id).ToList();
                    //    }
                    //}
                    var filePath = Path.Combine(Server.MapPath("~"), "App_Data", "Specification_finTrans.xlsx");
                    using (var fs = System.IO.File.OpenRead(filePath))
                    {
                        var buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Count());
                        ms.Write(buffer, 0, buffer.Count());
                        ms.Seek(0, SeekOrigin.Begin);
                    }
                    //создание файла excel с инфой по расчетам
                    excBook = new XLWorkbook(ms);
                    var workSheet = excBook.Worksheet("WorkSheet");
                    workSheet.Name = "лот";
                    var claim = db.LoadTenderClaimById(claimId);
                    //>>>>>>>>Шапка - Заполнение инфы о заявке<<<<<<

                    //Менеджер из Москвы
                    bool managerIsMoscou = false;
                    string managerSid = GetUser().Sid;
                    var dtUserIsMoscou = Db.CheckManagerIsMoscou(managerSid);
                    if (dtUserIsMoscou.Rows.Count > 0)
                    {
                        managerIsMoscou = dtUserIsMoscou.Rows[0]["result"].ToString().Equals("1");
                    }
                    // />Менеджер из Москвы

                    var dealTypes = db.LoadDealTypes();
                    var deliveryTimes = db.LoadDeliveryTimes();
                    UserBase manager;
                    try
                    {
                        manager = UserHelper.GetUserById(claim.Manager.Id);
                    }
                    catch (Exception ex)
                    {
                        manager = new UserBase();
                    }
                    var dt = Db.GetExchangeRatesOnDate(DateTime.Now);

                    double? usdRate = null;
                    double? eurRate = null;

                    if (dt.Rows.Count >= 3)
                    {
                        usdRate = Convert.ToDouble(dt.Rows[1]["price"].ToString()) * 1.03;
                        eurRate = Convert.ToDouble(dt.Rows[2]["price"].ToString()) * 1.03;
                    }

                    int rowHead = 1;

                    var usdCell = workSheet.Cell(rowHead, 4);
                    usdCell.Value =
                        usdRate.HasValue
                            ? usdRate.Value.ToString("N2")
                            : string.Empty;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.Number;

                    var eurCell = workSheet.Cell(++rowHead, 4);
                    eurCell.Value = eurRate.HasValue
                            ? eurRate.Value.ToString("N2")
                            : string.Empty;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.Number;

                    var eurRicohCell = workSheet.Cell(++rowHead, 4);
                    var profitCell = workSheet.Cell(++rowHead, 4);//Рентабельность
                    workSheet.Cell(++rowHead, 4).Value = dealTypes.First(x => x.Id == claim.DealType).Value;
                    workSheet.Cell(++rowHead, 4).Value = manager != null ? manager.ShortName : string.Empty;
                    workSheet.Cell(++rowHead, 4).Value = claim.Customer;
                    //срок готовности цен от снабжения???
                    rowHead++;
                    workSheet.Cell(++rowHead, 4).Value = claim.Sum;//Максимальная цена контракта???
                    workSheet.Cell(++rowHead, 4).Value = claim.DeliveryDateString;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.DateTime;
                    workSheet.Cell(++rowHead, 4).Value = claim.DeliveryPlace;
                    workSheet.Cell(++rowHead, 4).Value = claim.KPDeadlineString;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.DateTime;
                    workSheet.Cell(++rowHead, 4).Value = claim.AuctionDateString;
                    workSheet.Cell(rowHead, 4).DataType = XLCellValues.DateTime;
                    workSheet.Cell(++rowHead, 4).Value = claim.Comment;

                    var row = rowHead + 2;
                    int firstRow = row;
                    var rowNumber = 1;
                    //строки расчета
                    foreach (var position in positions)
                    {
                        position.Name = position.Name.Replace("\n", "").Replace("\r", "");

                        if (position.Calculations != null && position.Calculations.Any())
                        {
                            int calcNum = 0;
                            foreach (var calculation in position.Calculations)
                            {
                                calcNum++;
                                calculation.Name = calculation.Name.Replace("\n", "").Replace("\r", "");

                                int col = 0;
                                workSheet.Cell(row, ++col).Value = rowNumber;
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                workSheet.Cell(row, ++col).Value = position.ContractDeliveryTime;
                                workSheet.Cell(row, ++col).Value = position.Brand;
                                workSheet.Cell(row, ++col).Value = position.RecipientDetails;
                                workSheet.Cell(row, ++col).Value = position.QuestionnaireNum;
                                workSheet.Cell(row, ++col).Value = position.MaxPrice;
                               
                                //Не показываем подряд одни и те же надписи названия и каталожника
                                workSheet.Cell(row, ++col).Value = calcNum > 1 ? String.Empty : position.CatalogNumber;
                                //? position.CatalogNumber
                                //: calculation.CatalogNumber;
                                double hPosCatNum = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 30);
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                //Не показываем подряд одни и те же надписи названия и каталожника
                                workSheet.Cell(row, ++col).Value = calcNum > 1 ? String.Empty : position.Name;
                                double hPosName = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 40);
                                //String.IsNullOrEmpty(calculation.Name)? position.Name: calculation.Name;
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                                workSheet.Cell(row, ++col).Value = calculation.Brand;
                                workSheet.Cell(row, ++col).Value = calculation.CatalogNumber;
                                double hCalcCatNum = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 30);
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                workSheet.Cell(row, ++col).Value = calculation.Name;
                                //+ (!String.IsNullOrEmpty(calculation.Name) ? "\r\n" : "") + (!String.IsNullOrEmpty(calculation.Replace) ? "Замена:\r\n" + calculation.Replace : String.Empty);
                                double hCalcName = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 40);
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                                workSheet.Cell(row, ++col).Value = calculation.Replace.Replace("\n", "").Replace("\r", "");
                                //workSheet.Cell(row, ++col).Value = calculation.Replace;
                                //workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);


                                workSheet.Cell(row, ++col).Value = position.UnitName;
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                var countCell = workSheet.Cell(row, ++col);
                                countCell.Value = position.Value;

                                countCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                if (!String.IsNullOrEmpty(position.ProductManagerName))
                                {
                                    //var prodManager = UserHelper.GetUserById(position.ProductManager.Id);
                                    //workSheet.Cell(row, ++col).Value = prodManager == null
                                    //    ? String.Empty
                                    //    : prodManager.ShortName;
                                    workSheet.Cell(row, ++col).Value = position.ProductManagerName;
                                }
                                else
                                {
                                    ++col;
                                }
                                double hProdManager = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 16);
                                workSheet.Cell(row, ++col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                if (calculation.DeliveryTime != null)
                                {
                                    var delivTime = deliveryTimes.First(x => x.Id == calculation.DeliveryTime.Id);
                                    workSheet.Cell(row, col).Value = delivTime == null ? String.Empty : delivTime.Value;
                                }
                                double hCalcDeliv = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 16);
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                string partNum4Online = String.IsNullOrEmpty(position.CatalogNumber) ? calculation.CatalogNumber : position.CatalogNumber;
                                string onlinePrice = String.Empty;
                                try
                                { onlinePrice = CatalogProduct.PriceRequest(partNum4Online); }
                                catch
                                {

                                }
                                workSheet.Cell(row, ++col).Value = onlinePrice;//Цена B2B
                                workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                                var priceUsdCell = workSheet.Cell(row, ++col);
                                priceUsdCell.Value = calculation.PriceUsd;
                                priceUsdCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                priceUsdCell.Style.NumberFormat.Format = "$ #,##0.00";
                                var priceEurCell = workSheet.Cell(row, ++col);
                                priceEurCell.Value = calculation.PriceEur;
                                priceEurCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                priceEurCell.Style.NumberFormat.Format = "€ #,##0.00";
                                var priceEurRicohCell = workSheet.Cell(row, ++col);
                                priceEurRicohCell.Value = calculation.PriceEurRicoh;
                                priceEurRicohCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                priceEurRicohCell.Style.NumberFormat.Format = "€ #,##0.00";
                                var priceRublCell = workSheet.Cell(row, ++col);
                                priceRublCell.Value = calculation.PriceRubl;
                                priceRublCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                                //Вход за ед
                                if (String.IsNullOrEmpty(priceRublCell.Value.ToString().Trim()))
                                {
                                    if (!String.IsNullOrEmpty(priceEurRicohCell.Value.ToString().Trim()))
                                    {
                                        priceRublCell.FormulaR1C1 = String.Format("{0}*{1}", eurRicohCell.Address.ToStringFixed(), priceEurRicohCell.Address);
                                    }
                                    if (!String.IsNullOrEmpty(priceEurCell.Value.ToString().Trim()))
                                    {
                                        priceRublCell.FormulaR1C1 = String.Format("{0}*{1}", eurCell.Address.ToStringFixed(), priceEurCell.Address);
                                    }
                                    if (!String.IsNullOrEmpty(priceUsdCell.Value.ToString().Trim()))
                                    {
                                        priceRublCell.FormulaR1C1 = String.Format("{0}*{1}", usdCell.Address.ToStringFixed(), priceUsdCell.Address);
                                    }

                                }
                                // />Вход за ед
                                priceRublCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                //Сумма
                                var priceSumCell = workSheet.Cell(row, ++col);
                                priceSumCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                priceSumCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                priceSumCell.FormulaR1C1 = String.Format("{0}*{1}", countCell.Address,
                                    priceRublCell.Address);
                                // />Сумма

                                //Цена с ТЗР
                                var priceTzrCell = workSheet.Cell(row, ++col);
                                string formulaPriceTzrCell = String.Format("(({0}*1.02)*1.02)", priceRublCell.Address);
                                if (managerIsMoscou)
                                {
                                    formulaPriceTzrCell = String.Format("({0}*1.02)", priceRublCell.Address);
                                }
                                priceTzrCell.FormulaR1C1 = formulaPriceTzrCell;
                                priceTzrCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                priceTzrCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                // /> Цена с ТЗР

                                //Сумма с ТЗР
                                var sumTzrCell = workSheet.Cell(row, ++col);
                                string formulaSumTzrCell = String.Format("{0}*{1}", countCell.Address, priceTzrCell.Address);
                                sumTzrCell.FormulaR1C1 = formulaSumTzrCell;
                                sumTzrCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                sumTzrCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                // /> Сумма с ТЗР

                                //Цена С НДС
                                var priceNdsCell = workSheet.Cell(row, ++col);
                                string formulaPriceNdsCell = String.Format("{0}*(1+({1}/100))", priceTzrCell.Address, profitCell.Address);
                                priceNdsCell.FormulaR1C1 = formulaPriceNdsCell;
                                priceNdsCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                priceNdsCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                // /> Цена с НДС

                                //Сумма с НДС
                                var sumNdsCell = workSheet.Cell(row, ++col);
                                string formulaSumNdsCell = String.Format("{0}*{1}", countCell.Address, priceNdsCell.Address);
                                sumNdsCell.FormulaR1C1 = formulaSumNdsCell;
                                sumNdsCell.Style.NumberFormat.Format = "₽ #,##0.00";
                                sumNdsCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                // /> Сумма с НДС

                                //workSheet.Cell(row, 5).Value = !calculation.PriceCurrency.Equals(0)
                                //    ? calculation.PriceCurrency.ToString("N2")
                                //    : string.Empty;
                                //workSheet.Cell(row, 6).Value = !calculation.SumCurrency.Equals(0)
                                //    ? calculation.SumCurrency.ToString("N2")
                                //    : string.Empty;
                                //workSheet.Cell(row, 7).Value = "";//currencies.First(x => x.Id == calculation.Currency).Value;
                                //workSheet.Cell(row, 7).Value = !calculation.PriceRub.Equals(0)
                                //    ? calculation.PriceRub.ToString("N2")
                                //    : string.Empty;
                                //workSheet.Cell(row, 8).Value = !calculation.SumRub.Equals(0)
                                //    ? calculation.SumRub.ToString("N2")
                                //    : string.Empty;

                                workSheet.Cell(row, ++col).Value = calculation.Provider;
                                workSheet.Cell(row, col)
                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                //workSheet.Cell(row, 9).Value = calculation.ProtectFact != null ?
                                //    facts.First(x => x.Id == calculation.ProtectFact.Id).Value : String.Empty;
                                //workSheet.Cell(row, 10).Value = calculation.ProtectCondition;
                                workSheet.Cell(row, ++col).Value = calculation.Comment;
                                workSheet.Cell(row, col).Style.Font.SetFontColor(XLColor.Red);
                                double hComent = GetCellHeight(workSheet.Cell(row, col).Value.ToString().Length, 40);


                                double[] arr = { hPosCatNum, hPosName, hCalcName, hCalcDeliv, hProdManager, hComent, hCalcCatNum };
                                workSheet.Row(row).Height = arr.Max();
                                row++;
                                rowNumber++;
                            }
                        }

                        else
                        {
                            int col = 0;
                            workSheet.Cell(row, ++col).Value = rowNumber;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, ++col).Value = position.ContractDeliveryTime;
                            workSheet.Cell(row, ++col).Value = position.Brand;
                            workSheet.Cell(row, ++col).Value = position.RecipientDetails;
                            workSheet.Cell(row, ++col).Value = position.QuestionnaireNum;
                            workSheet.Cell(row, ++col).Value = position.MaxPrice;
                            
                            workSheet.Cell(row, ++col).Value = position.CatalogNumber;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                            workSheet.Cell(row, ++col).SetValue(position.Name);
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                            workSheet.Row(row).Height = GetCellHeight(position.CatalogNumber.Length, 10);
                            workSheet.Row(row).Height = GetCellHeight(position.Name.Length, 40);
                            ++col;//Бренд
                            ++col;//Кат номер
                            ++col;//Наименование
                            ++col;//Замена
                            //workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                            workSheet.Cell(row, ++col).Value = position.UnitName;

                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, ++col).Value = position.Value;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            if (position.ProductManager != null)
                            {
                                var prodManager = UserHelper.GetUserById(position.ProductManager.Id);
                                workSheet.Cell(row, ++col).Value = prodManager == null
                                    ? String.Empty
                                    : prodManager.ShortName;
                            }
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                            ++col;//Цена B2B

                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, col).Style.NumberFormat.Format = "$ #,###";
                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, col).Style.NumberFormat.Format = "€ #,###";
                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, col).Style.NumberFormat.Format = "€ #,###";
                            workSheet.Cell(row, ++col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            workSheet.Cell(row, col).Style.NumberFormat.Format = "₽ #,###";
                            col = col + 6;
                            workSheet.Cell(row, col).Value = String.Empty;
                            workSheet.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            row++;
                            rowNumber++;
                        }
                    }


                    var range = workSheet.Range(workSheet.Cell(firstRow, 1), workSheet.Cell(row - 1, 28));
                    range.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetBottomBorderColor(XLColor.Gray);
                    range.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetTopBorderColor(XLColor.Gray);
                    range.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetRightBorderColor(XLColor.Gray);
                    range.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                    range.Style.Border.SetLeftBorderColor(XLColor.Gray);

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
            catch (Exception ex)
            {
                error = true;
                message = "Ошибка сервера " + ex.Message;
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
                    FileDownloadName = "CalculationTrans_" + claimId + ".xlsx"
                };
            }
            else
            {
                ViewBag.Message = message;
                return null;
            }
        }

        public double GetCellHeight(int contentLength, int colMaxLen)
        {
            double res = Math.Ceiling((double)contentLength / (double)colMaxLen) * 15.75;
            if (res < 15.75) res = 15.75;
            return res;
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
                var tenderStatus = db.LoadTenderStatus();
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
                            claimProductManager.ShortName = managerFromAD.ShortName;
                        }
                    }
                    foreach (var claim in list)
                    {
                        var manager = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
                        if (manager != null)
                        {
                            claim.Manager = manager;
                        }
                    }
                    db.SetStatisticsForClaims(list);
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
                    workSheet.Cell(1, 6).Value = "Позиции";
                    workSheet.Cell(1, 7).Value = "Снабженцы";
                    workSheet.Cell(1, 8).Value = "Тип сделки";
                    workSheet.Cell(1, 9).Value = "Статус";
                    workSheet.Cell(1, 10).Value = "Создано";
                    workSheet.Cell(1, 11).Value = "Срок сдачи";
                    workSheet.Cell(1, 12).Value = "Статус конкурса";
                    workSheet.Cell(1, 13).Value = "Автор";
                    workSheet.Cell(1, 14).Value = "Просроченна";
                    var headRange = workSheet.Range(workSheet.Cell(1, 1), workSheet.Cell(1, 14));
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
                        workSheet.Cell(row, 5).Value = claim.Manager.ShortName;
                        workSheet.Cell(row, 6).Value = "Всего: " + claim.PositionsCount + "\rРасчетов: " +
                                                       claim.CalculatesCount;
                        workSheet.Cell(row, 7).Value = claim.ProductManagers != null
                            ? string.Join("\r", claim.ProductManagers.Select(x => x.ShortName + " " + x.PositionsCount + "/" + x.CalculatesCount))
                            : string.Empty;
                        workSheet.Cell(row, 8).Value = dealTypes.First(x => x.Id == claim.DealType).Value;
                        workSheet.Cell(row, 9).Value = status.First(x => x.Id == claim.ClaimStatus).Value;
                        workSheet.Cell(row, 10).Value = claim.RecordDate.ToString("dd.MM.yyyy");
                        workSheet.Cell(row, 10).DataType = XLCellValues.DateTime;
                        workSheet.Cell(row, 11).Value = claim.ClaimDeadline.ToString("dd.MM.yyyy");
                        workSheet.Cell(row, 11).DataType = XLCellValues.DateTime;
                        workSheet.Cell(row, 12).Value = tenderStatus.First(x => x.Id == claim.TenderStatus).Value;
                        workSheet.Cell(row, 13).Value = UserHelper.GetUserById(claim.Author.Sid).ShortName;
                        var overDie = "Нет";
                        if (claim.ClaimDeadline > DateTime.Now)
                        {
                            if (claim.ClaimStatus != 1 || claim.ClaimStatus != 8)
                            {
                                overDie = "Да";
                            }
                        }
                        workSheet.Cell(row, 14).Value = overDie;
                        row++;
                    }
                    workSheet.Columns(6, 7).Style.Alignment.WrapText = true;
                    workSheet.Columns(1, 14).AdjustToContents();
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
       
        //Excel
        //получение excel файла, с инфой по позициям заявки
        [HttpPost]
        public JsonResult UploadFileFormTrans(int claimId)
        {
            //int claimId;
            //int.TryParse(Request.Form["Id"], out claimId);
            var error = false;
            var message = string.Empty;
            XLWorkbook excBook = null;
            Stream inputStream = null;
            var positions = new List<SpecificationPosition>();
            var file = Request.Files[0];
            //try
            //{
            if (file == null || !file.FileName.EndsWith(".xlsx"))
            {
                error = true;
                message = "Файл не предоставлен или имеет неверный формат";
            }
            else
            {
                var productManagers = UserHelper.GetProductManagers();
                var units = PositionUnit.GetList();
                inputStream = file.InputStream;
                inputStream.Seek(0, SeekOrigin.Begin);
                excBook = new XLWorkbook(inputStream);
                //разбор файла
                var workSheet = excBook.Worksheet("Лот");
                if (workSheet != null)
                {
                    var user = GetUser();
                    //<<<<<<<Номер строки - начало разбора инфы>>>>>>
                    var row = 5;
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
                            ProductManager = new ProductManager() { Id = string.Empty, Name = string.Empty },

                            Replace = string.Empty,
                            IdClaim = claimId,
                            State = 1,
                            Author = user.Sid,
                            Currency = 1,
                        };
                        //получение ячеек с инфой по позициям
                       
                        var numberRange = workSheet.Cell(row, 1);
                        var deliveryTime = workSheet.Cell(row, 2);
                        var brand = workSheet.Cell(row, 3);
                        var recipientDetails = workSheet.Cell(row, 4);
                        var questionaryNum = workSheet.Cell(row, 5);
                        var maxPrice = workSheet.Cell(row, 6);
                        var catalogNumberRange = workSheet.Cell(row, 7);
                        var nameRange = workSheet.Cell(row, 8);
                        var unitRange = workSheet.Cell(row, 9);
                        var valueRange = workSheet.Cell(row, 10);
                        var managerRange = workSheet.Cell(row, 11);
                        var commentRange = workSheet.Cell(row, 12);
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

                        model.ContractDeliveryTime = deliveryTime.Value.ToString();
                        model.Brand = brand.Value.ToString();
                        model.RecipientDetails= recipientDetails.Value.ToString();
                        model.QuestionnaireNum = questionaryNum.Value.ToString();
                        model.MaxPrice = maxPrice.Value.ToString();

                        //разбор инфы по Каталожный номер, Замена, Единицы
                        if (catalogNumberRange != null && catalogNumberRange.Value != null)
                        {
                            model.CatalogNumber = catalogNumberRange.Value.ToString();
                        }

                        if (units != null && units.Any())
                        {
                            var unit = unitRange.Value.ToString();
                            model.Unit = units.Single(x => x.Name.Equals(unit)).Id;
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
                            if (managerFromAd != null)
                            {
                                model.ProductManager = managerFromAd;
                                model.ProductManagerId = managerFromAd.Id;
                            }
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


                    message = "Получено строк:    " + (row - 5);
                    if (repeatRowCount > 0)
                    {
                        message += "<br/>Из них повторных: " + repeatRowCount;
                    }
                    else
                    {
                        message += "<br/>Из них повторных: 0";
                    }
                    if (positions.Any())
                    {
                        message += "<br/>Сохранено строк:   " + positions.Count();
                    }
                    else
                    {
                        message += "<br/>Сохранено строк:   0";
                    }
                    var errorMessage = errorStringBuilder.ToString();
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        error = true;
                        message += "<br/>Ошибки:<br/>" + errorMessage + "<br />Сохранить изменения?";
                    }
                    else
                    {
                        message += "<br/>Ошибки: нет";
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



            return Json(new { error, message, positions });
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

        //Excel
        //получение excel файла, с инфой по позициям заявки
        [HttpPost]
        public JsonResult UploadFileForm(int claimId)
        {
            //int claimId;
            //int.TryParse(Request.Form["Id"], out claimId);
            var error = false;
            var message = string.Empty;
            XLWorkbook excBook = null;
            Stream inputStream = null;
            var positions = new List<SpecificationPosition>();
            var file = Request.Files[0];
            //try
            //{
                if (file == null || !file.FileName.EndsWith(".xlsx"))
                {
                    error = true;
                    message = "Файл не предоставлен или имеет неверный формат";
                }
                else
                {
                    var productManagers = UserHelper.GetProductManagers();
                    var units = PositionUnit.GetList();
                    inputStream = file.InputStream;
                    inputStream.Seek(0, SeekOrigin.Begin);
                    excBook = new XLWorkbook(inputStream);
                    //разбор файла
                    var workSheet = excBook.Worksheet("Лот");
                    if (workSheet != null)
                    {
                        var user = GetUser();
                        //<<<<<<<Номер строки - начало разбора инфы>>>>>>
                        var row = 5;
                        var errorStringBuilder = new StringBuilder();
                        var repeatRowCount = 0;
                        var db = new DbEngine();
                        //var currencies = db.LoadCurrencies();
                        //проход по всем строкам
                        while (true)
                        {
                            var rowValid = true;
                            var model = new SpecificationPosition()
                            {
                                CatalogNumber = string.Empty,
                                Comment = string.Empty,
                                Name = string.Empty,
                                ProductManager = new ProductManager() { Id = string.Empty, Name = string.Empty },
                                
                                Replace = string.Empty,
                                IdClaim = claimId,
                                State = 1,
                                Author = user.Sid,
                                Currency = 1,
                            };
                            //получение ячеек с инфой по позициям
                            var numberRange = workSheet.Cell(row, 1);
                            var catalogNumberRange = workSheet.Cell(row, 2);
                            var nameRange = workSheet.Cell(row, 3);
                            //var replaceRange = workSheet.Cell(row, 4);
                            var unitRange = workSheet.Cell(row, 4);
                            var valueRange = workSheet.Cell(row, 5);
                            var managerRange = workSheet.Cell(row, 6);
                            //var currencyRange = workSheet.Cell(row, 8);
                            //var priceRange = workSheet.Cell(row, 9);
                            //var sumRange = workSheet.Cell(row, 10);
                            //var priceTzrRange = workSheet.Cell(row, 11);
                            //var sumTzrRange = workSheet.Cell(row, 12);
                            //var priceNdsRange = workSheet.Cell(row, 13);
                            //var sumNdsRange = workSheet.Cell(row, 14);
                            var commentRange = workSheet.Cell(row, 7);
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
                            //if (replaceRange != null && replaceRange.Value != null)
                            //{
                            //    model.Replace = replaceRange.Value.ToString();
                            //}

                            if (units != null && units.Any())
                            {
                                var unit = unitRange.Value.ToString();
                                model.Unit = units.Single(x => x.Name.Equals(unit)).Id;
                            }

                            //if (unitRange != null && unitRange.Value != null)
                            //{
                            //    var value = unitRange.Value.ToString();
                            //    switch (value)
                            //    {
                            //        case "шт":
                            //            model.Unit = PositionUnit.Thing;
                            //            break;
                            //        case "упак":
                            //            model.Unit = PositionUnit.Package;
                            //            break;
                            //        case "м":
                            //            model.Unit = PositionUnit.Metr;
                            //            break;
                            //        default:
                            //            model.Unit = PositionUnit.Thing;
                            //            break;
                            //    }
                            //}
                            //else
                            //{
                            //    model.Unit = PositionUnit.Thing;
                            //}




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
                                if (managerFromAd != null)
                                {
                                    model.ProductManager = managerFromAd;
                                    model.ProductManagerId = managerFromAd.Id;
                                }
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
                            //разбор инфы по Ценам и Суммам
                            //if (priceRange != null && priceRange.Value != null)
                            //{
                            //    string priceValue = priceRange.Value.ToString();
                            //    if (!string.IsNullOrEmpty(priceValue))
                            //    {
                            //        double doubleValue;
                            //        var isValidDouble = double.TryParse(priceValue, out doubleValue);
                            //        if (!isValidDouble)
                            //        {
                            //            rowValid = false;
                            //            errorStringBuilder.Append("Строка: " + row +
                            //                                      ", значение '" + priceValue + "' в поле Цена за единицу не является числом<br/>");
                            //        }
                            //        else
                            //        {
                            //            model.Price = doubleValue;
                            //        }
                            //    }
                            //}
                            //if (sumRange != null && sumRange.Value != null)
                            //{
                            //    string sumValue = sumRange.Value.ToString();
                            //    if (!string.IsNullOrEmpty(sumValue))
                            //    {
                            //        double doubleValue;
                            //        var isValidDouble = double.TryParse(sumValue, out doubleValue);
                            //        if (!isValidDouble)
                            //        {
                            //            rowValid = false;
                            //            errorStringBuilder.Append("Строка: " + row +
                            //                                      ", значение '" + sumValue + "' в поле Сумма не является числом<br/>");
                            //        }
                            //        else
                            //        {
                            //            model.Sum = doubleValue;
                            //        }
                            //    }
                            //}
                            //if (priceTzrRange != null && priceTzrRange.Value != null)
                            //{
                            //    string priceTzrValue = priceTzrRange.Value.ToString();
                            //    if (!string.IsNullOrEmpty(priceTzrValue))
                            //    {
                            //        double doubleValue;
                            //        var isValidDouble = double.TryParse(priceTzrValue, out doubleValue);
                            //        if (!isValidDouble)
                            //        {
                            //            rowValid = false;
                            //            errorStringBuilder.Append("Строка: " + row +
                            //                                      ", значение '" + priceTzrValue + "' в поле Цена с ТЗР не является числом<br/>");
                            //        }
                            //        else
                            //        {
                            //            model.PriceTzr = doubleValue;
                            //        }
                            //    }
                            //}
                            //if (sumTzrRange != null && sumTzrRange.Value != null)
                            //{
                            //    string sumTzrValue = sumTzrRange.Value.ToString();
                            //    if (!string.IsNullOrEmpty(sumTzrValue))
                            //    {
                            //        double doubleValue;
                            //        var isValidDouble = double.TryParse(sumTzrValue, out doubleValue);
                            //        if (!isValidDouble)
                            //        {
                            //            rowValid = false;
                            //            errorStringBuilder.Append("Строка: " + row +
                            //                                      ", значение '" + sumTzrValue + "' в поле Сумма с ТЗР не является числом<br/>");
                            //        }
                            //        else
                            //        {
                            //            model.SumTzr = doubleValue;
                            //        }
                            //    }
                            //}
                            //if (priceNdsRange != null && priceNdsRange.Value != null)
                            //{
                            //    string priceNdsValue = priceNdsRange.Value.ToString();
                            //    if (!string.IsNullOrEmpty(priceNdsValue))
                            //    {
                            //        double doubleValue;
                            //        var isValidDouble = double.TryParse(priceNdsValue, out doubleValue);
                            //        if (!isValidDouble)
                            //        {
                            //            rowValid = false;
                            //            errorStringBuilder.Append("Строка: " + row +
                            //                                      ", значение '" + priceNdsValue + "' в поле Цена с НДС не является числом<br/>");
                            //        }
                            //        else
                            //        {
                            //            model.PriceNds = doubleValue;
                            //        }
                            //    }
                            //}
                            //if (sumNdsRange != null && sumNdsRange.Value != null)
                            //{
                            //    string sumNdsValue = sumNdsRange.Value.ToString();
                            //    if (!string.IsNullOrEmpty(sumNdsValue))
                            //    {
                            //        double doubleValue;
                            //        var isValidDouble = double.TryParse(sumNdsValue, out doubleValue);
                            //        if (!isValidDouble)
                            //        {
                            //            rowValid = false;
                            //            errorStringBuilder.Append("Строка: " + row +
                            //                                      ", значение '" + sumNdsValue + "' в поле Сумма с НДС числом<br/>");
                            //        }
                            //        else
                            //        {
                            //            model.SumNds = doubleValue;
                            //        }
                            //    }
                            //}
                            //if (currencyRange != null && currencyRange.Value != null && !string.IsNullOrEmpty(currencyRange.Value.ToString()))
                            //{
                            //    var value = currencyRange.Value.ToString();
                            //    var currency = currencies.FirstOrDefault(x => x.Value == value);
                            //    if (currency != null)
                            //    {
                            //        model.Currency = currency.Id;
                            //    }
                            //    else
                            //    {
                            //        rowValid = false;
                            //        errorStringBuilder.Append("Строка: " + row +
                            //                              ", не найдена Валюта: " + value + "<br/>");
                            //    }
                            //}
                            //else
                            //{
                            //    if (!model.Sum.Equals(0) || !model.Price.Equals(0) || !model.PriceTzr.Equals(0) || !model.SumTzr.Equals(0) || !model.SumNds.Equals(0) || !model.PriceNds.Equals(0))
                            //    {
                            //        rowValid = false;
                            //        errorStringBuilder.Append("Строка: " + row +
                            //                                  ", не задано обязательное значение Валюта<br/>");
                            //    }
                            //    else
                            //    {
                            //        model.Currency = 1;
                            //    }
                            //}
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
                        

                        message = "Получено строк:    " + (row - 5);
                        if (repeatRowCount > 0)
                        {
                            message += "<br/>Из них повторных: " + repeatRowCount;
                        }
                        else
                        {
                            message += "<br/>Из них повторных: 0";
                        }
                        if (positions.Any())
                        {
                            message += "<br/>Сохранено строк:   " + positions.Count();
                        }
                        else
                        {
                            message += "<br/>Сохранено строк:   0";
                        }
                        var errorMessage = errorStringBuilder.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            error = true;
                            message += "<br/>Ошибки:<br/>" + errorMessage + "<br />Сохранить изменения?";
                        }
                        else
                        {
                            message += "<br/>Ошибки: нет";
                            //foreach (SpecificationPosition position in positions)
                            //{
                            //    db.SaveSpecificationPosition(position);
                            //}
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
            //}
            //catch (Exception)
            //{
            //    error = true;
            //    message = "Ошибка сервера";
            //}
            //finally
            //{
            //    if (inputStream != null)
            //    {
            //        inputStream.Dispose();
            //    }
            //    if (excBook != null)
            //    {
            //        excBook.Dispose();
            //    }
            //}
            //ViewBag.FirstLoad = false;
            //ViewBag.Error = error.ToString().ToLowerInvariant();
            //ViewBag.Message = message;
            //ViewBag.Positions = positions;
            //ViewBag.IdClaim = claimId;

            

            return Json(new { error, message, positions });
            //return RedirectToAction("IndexManager", new {claimId});
            //return View();
        }

        //>>>>Уведомления
        //Сохранение заявки
        [HttpPost]
        public JsonResult SaveClaim(TenderClaim model)
        {
            var isComplete = false;
            ClaimStatusHistory statusHistory = null;
            int? idClaim;
            string errorText = String.Empty;
            try
            {
                
                model.KPDeadline = DateTime.ParseExact(model.KPDeadlineString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                model.ClaimDeadline = DateTime.ParseExact(model.ClaimDeadlineString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                model.TenderStart = DateTime.ParseExact(model.TenderStartString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                if (!string.IsNullOrEmpty(model.DeliveryDateString))
                    model.DeliveryDate = DateTime.ParseExact(model.DeliveryDateString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                if (!string.IsNullOrEmpty(model.DeliveryDateEndString))
                    model.DeliveryDateEnd = DateTime.ParseExact(model.DeliveryDateEndString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                if (!string.IsNullOrEmpty(model.AuctionDateString))
                    model.AuctionDate = DateTime.ParseExact(model.AuctionDateString, "dd.MM.yyyy", CultureInfo.CurrentCulture);
                var modelValid = true;
                if (string.IsNullOrEmpty(model.Customer) || model.Sum < 0) modelValid = false;

                if (modelValid)
                {
                    var user = GetUser();
                    var db = new DbEngine();
                    model.ClaimStatus = 1;
                    model.TenderStatus = 1;
                    model.Deleted = false;
                    model.RecordDate = DateTime.Now;
                    var usr = AdHelper.GetUserBySid(user.Sid);
                    model.Author.Sid = usr.AdSid;
                    model.Author.DisplayName = CurUser.DisplayName;
                    //model.Author = AdHelper.GetUserBySid(user.Sid);//UserHelper.GetUserById(user.Sid);
                    isComplete = db.SaveTenderClaim(ref model);
                    //if (model.DeliveryDateString == null) model.DeliveryDateString = string.Empty;
                    //if (model.DeliveryDateEndString == null) model.DeliveryDateEndString = string.Empty;
                    //if (model.AuctionDateString == null) model.AuctionDateString = string.Empty;
                    if (model.DeliveryPlace == null) model.DeliveryPlace = string.Empty;
                    if (isComplete)
                    {
                        //История изменения статуса
                        statusHistory = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdClaim = model.Id,
                            IdUser = user.Sid,
                            Status = new ClaimStatus() { Id = model.ClaimStatus },
                            Comment = "Автор: " + user.DisplayName
                        };
                        db.SaveClaimStatusHistory(statusHistory);
                        statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                        //>>>>Уведомления
                        if (model.Author.Sid != model.Manager.Id)
                        {
                            var manager = UserHelper.GetUserById(model.Manager.Id);
                            if (manager != null)
                            {
                                var host = ConfigurationManager.AppSettings["AppHost"];
                                var message = new StringBuilder();
                                message.Append("Добрый день!");
                                //message.Append(manager.Name);
                                message.Append("<br/>");
                                message.Append("Пользователь ");
                                message.Append(user.DisplayName);
                                message.Append(" создал заявку где Вы назначены менеджером.");
                                message.Append("<br/><br />");
                                message.Append(GetClaimInfo(model));
                                message.Append("<br/>");
                                message.Append("Ссылка на заявку: ");
                                message.Append("<a href='" + host + "/Claim/Index?claimId=" + model.Id + "'>" + host +
                                               "/Claim/Index?claimId=" + model.Id + "</a>");
                                //message.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(new List<UserBase>() { manager }, message.ToString(),
                                    String.Format("{0} - {1} - Новая заявка СпецРасчет", model.TenderNumber, model.Customer));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isComplete = false;
                errorText = ex.Message;
            }
            return Json(new { IsComplete = isComplete, Model = model, StatusHistory = statusHistory, errorText = errorText });
        }

        //Изменение курса валют
        [HttpPost]
        public JsonResult UpdateClaimCurrency(TenderClaim model)
        {
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                isComplete = db.UpdateClaimCurrency(model);
            }
            catch (Exception ex)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete });
        }

        //Изменение позиции
        [HttpPost]
        public PartialViewResult EditClaimPosition(SpecificationPosition model)
        {
            var isComplete = false;
            //try
            //{
                var user = GetUser();
                var db = new DbEngine();
                //var claimStatus = db.LoadLastStatusHistoryForClaim(model.IdClaim);
                //if (claimStatus == null || claimStatus.Status == null ||  claimStatus.Status.ToString() == "1")
                //    model.State = 1;
                //else model.State = 5;
                model.Author = user.Sid;
                var modelValid = true;
                //if (string.IsNullOrEmpty(model.Name)) modelValid = false;
                if (modelValid)
                {
                    isComplete = db.UpdateSpecificationPosition(model);
                }
            return PartialView("NewPosition", model);
            //}
            //catch (Exception)
            //{
            //    isComplete = false;
            //}
            //return Json(new { IsComplete = isComplete });
        }

        //удаление позиции
        public JsonResult DeleteClaimPosition(int id)
        {
            var isComplete = false;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                isComplete = db.DeleteSpecificationPosition(id, user);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteClaimPositions(int[] ids)
        {
            var isComplete = true;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                foreach (var id in ids)
                {
                  isComplete = isComplete &&         db.DeleteSpecificationPosition(id, user);  
                }
                
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
            bool isComplete;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                isComplete = db.DeleteTenderClaim(id, user);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult EditClaimDeadline(TenderClaim claim)
        {
            var isComplete = false;
            try
            {
                bool dateValid = !string.IsNullOrEmpty(claim.ClaimDeadline.ToShortDateString());
                if (dateValid)
                {
                    var db = new DbEngine();
                    db.UpdateClaimDeadline(claim.Id, claim.ClaimDeadline);
                    isComplete = true;
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete });
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
                var user = GetUser();
                var isController = user.Is(AdGroup.SpeCalcKontroler);//UserHelper.IsController(user);
                var isProduct = user.Is(AdGroup.SpeCalcProduct);//UserHelper.IsProductManager(user);
                var isManager = user.Is(AdGroup.SpeCalcManager);//UserHelper.IsManager(user);
                var db = new DbEngine();
                if (model.RowCount == 0) model.RowCount = 10;
                if (string.IsNullOrEmpty(model.IdManager) && isManager && !isController)
                {
                    var subMans = Employee.GetSubordinates(user.Sid);
                    model.IdManager = user.Sid + "," + string.Join(",", subMans);
                }
                if (string.IsNullOrEmpty(model.IdProductManager) && isProduct && !isController)
                {
                    var subProds = string.Join(",", Employee.GetSubordinates(user.Sid));
                    model.IdProductManager = user.Sid + "," + subProds;
                }
                list = db.FilterTenderClaims(model);
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
                            claimProductManager.ShortName = managerFromAD.ShortName;
                        }
                    }
                    foreach (var claim in list)
                    {
                        var manager = managers.FirstOrDefault(x => x.Id == claim.Manager.Id);
                        if (manager != null)
                        {
                            claim.Manager.ShortName = manager.ShortName;
                        }
                        var usr = AdHelper.GetUserBySid(claim.Author.Sid);
                        claim.Author = new AdUser() {FullName = usr.FullName, DisplayName = usr.DisplayName, Sid=usr.AdSid};
                            //UserHelper.GetUserById(claim.Author.Sid);
                    }
                    db.SetStatisticsForClaims(list);
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
        [HttpGet]
        public PartialViewResult GetNewPositions(int? claimId, int? cv, string ClaimType)
        {
            if (!claimId.HasValue) return null;
            if (!cv.HasValue) cv = 1;
            ViewBag.ClaimType = ClaimType;
            var list = SpecificationPosition.GetList(claimId.Value, cv.Value);// db.LoadSpecificationPositionsForTenderClaim(); ;
            return PartialView("NewPositions", list);
        }

        [HttpGet]
        public PartialViewResult GetCancelPositions(int? claimId, int? cv)
        {
            if (!claimId.HasValue) return null;
            if (!cv.HasValue) cv = 1;

            var list = SpecificationPosition.GetList(claimId.Value, cv.Value, stateId: new PositionState("CANCELPROD").Id);// db.LoadSpecificationPositionsForTenderClaim(); ;
            return PartialView("NewPositions", list);
        }

        [HttpGet]
        public PartialViewResult GetCalcPositions(int? claimId, int? cv, string ClaimType)
        {
            if (!claimId.HasValue) return null;
            if (!cv.HasValue) cv = 1;
            ViewBag.ClaimType = ClaimType;
            var list = SpecificationPosition.GetListWithCalc(claimId.Value, cv.Value);// db.LoadSpecificationPositionsForTenderClaim(); ;
            return PartialView("CalcPositions", list);
        }

        public PartialViewResult GetNewPosition(int? id)
        {
            if (!id.HasValue) return null;
            var model = new SpecificationPosition(id.Value);
            
            return PartialView("NewPosition", model);
        }

        [HttpPost]
        public PartialViewResult AddPosition(SpecificationPosition model)
        {
            model.Author = CurUser.Sid;
            var db = new DbEngine();
            db.SaveSpecificationPosition(model);
            return PartialView("NewPosition", model);
            //var isComplete = false;
            //try
            //{
            //    var user = GetUser();
            //    model.State = 1;
            //    model.Author = user.Sid;
            //    model.Currency = 1;
            //    var modelValid = true;
            //    if (string.IsNullOrEmpty(model.Name)) modelValid = false;
            //    if (modelValid)
            //    {
            //        var db = new DbEngine();

            //        if (string.IsNullOrEmpty(model.CatalogNumber)) model.CatalogNumber = string.Empty;
            //        if (string.IsNullOrEmpty(model.Replace)) model.Replace = string.Empty;
            //        if (string.IsNullOrEmpty(model.Comment)) model.Comment = string.Empty;
            //    }
            //}
            //catch (Exception)
            //{
            //    isComplete = false;
            //}
            //return Json(new { IsComplete = isComplete, Model = model });
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
                model.Author = user.Sid;
                model.Currency = 1;
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

        public PartialViewResult GetNewPositionEdit(int id)
        {
            var model = new SpecificationPosition(id);
            return PartialView("NewPositionEdit", model);
        }

        [HttpPost]
        public JsonResult AddClaimPositions(IEnumerable<SpecificationPosition> modelList)
        {

            var isComplete = false;
            try
            { 
                isComplete = modelList.Count() > 0;
                var db = new DbEngine();
                foreach (var model in modelList)
                {
                    //updatec
                    isComplete = isComplete && db.SaveSpecificationPosition(model);
                }
                
            }
            catch (Exception ex)
            {
                
                isComplete = false;
            }
            return Json(new {IsComplete = isComplete});
        }
        //>>>>Уведомления
        //передача заявки в работу
        [HttpPost]
        public JsonResult SetClaimOnWork(int id, int? cv, DateTime? deadlineDate)
        {
            cv = cv ?? 1;
            var isComplete = false;
            var message = string.Empty;
            ClaimStatusHistory model = null;
            //try
            //{
            

                var db = new DbEngine();
            if (deadlineDate.HasValue)
            {
                db.UpdateClaimDeadline(id, deadlineDate.Value);
            }

            var hasPosition = db.HasClaimPosition(id);
                if (hasPosition)
                {
                var state = new ClaimStatus("SEND");
                    isComplete = DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = id, ClaimStatus = state.Id });
                    var productManagers = db.LoadProductManagersForClaim(id, cv.Value);
                    if (productManagers != null && productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        foreach (var productManager in productManagers)
                        {
                            var productManagerFromAd =
                                productManagersFromAd.FirstOrDefault(x => x.Id == productManager.Id);
                            if (productManagerFromAd != null)
                            {
                                productManager.ShortName = productManagerFromAd.ShortName;
                            }
                        }
                        //истроия изменения статуса заявки
                        var user = GetUser();
                        var comment = "Продакты/снабженцы:<br />";
                        comment += string.Join("<br />", productManagers.Select(x => x.ShortName));
                        comment += "<br />Автор: " + user.DisplayName;
                        model = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdClaim = id,
                            IdUser = user.Sid,
                            Status = new ClaimStatus() { Id = 2 },
                            Comment = comment
                        };
                        db.SaveClaimStatusHistory(model);
                        model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                        //>>>>Уведомления
                        //var claimPositions = db.LoadSpecificationPositionsForTenderClaim(id, cv.Value);
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var claim = db.LoadTenderClaimById(id);
                        var host = ConfigurationManager.AppSettings["AppHost"];
                        foreach (var productManager in productInClaim)
                        {
                            //var positionCount = claimPositions.Count(x => x.ProductManager.Id == productManager.Id);
                            var messageMail = new StringBuilder();
                            messageMail.Append("Добрый день!");
                            messageMail.Append(String.Format("<br/>На имя {0} назначена заявка в системе СпецРасчет.", productManager.ShortName));
                            //messageMail.Append("<br/>Пользователь ");
                            //messageMail.Append(user.Name);
                            //messageMail.Append(
                            //    " создал заявку где Вам назначены позиции для расчета. Количество назначенных позиций: " +
                            //    positionCount + "<br/>");
                            messageMail.Append("<br/><br />");
                            messageMail.Append(GetClaimInfo(claim));
                            messageMail.Append("<br />Ссылка на заявку: ");
                            messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                           "/Calc/Index?claimId=" + claim.Id + "</a>");
                            //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                            Notification.SendNotification(new[] { productManager }, messageMail.ToString(),
                                String.Format("{0} - {1} - Новая заявка СпецРасчет", claim.TenderNumber, claim.Customer));
                        }
                    }
                }
                else
                {
                    message = "Невозможно передать заявку в работу без позиций спецификаций";
                }

            //}
            //catch (Exception)
            //{
            //    isComplete = false;
            //}
            //return RedirectToAction("IndexManager", new {claimId=id, cv});
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
                isComplete = DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = model.IdClaim, ClaimStatus = 5 });
                if (isComplete)
                {
                    model.Date = DateTime.Now;
                    model.IdUser = GetUser().Sid;
                    model.Status = new ClaimStatus() { Id = 5 };
                    db.SaveClaimStatusHistory(model);
                    model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                    //>>>>Уведомления
                    var claim = db.LoadTenderClaimById(model.IdClaim);
                    var productManagers = db.LoadProductManagersForClaim(model.IdClaim, model.Version);
                    if (productManagers != null && productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        var user = GetUser();
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var host = ConfigurationManager.AppSettings["AppHost"];
                        var messageMail = new StringBuilder();
                        messageMail.Append("Добрый день!");
                        messageMail.Append("<br/>");
                        messageMail.Append("Пользователь ");
                        messageMail.Append(user.FullName);
                        messageMail.Append(" отменил заявку где Вам назначены позиции для расчета.");
                        //messageMail.Append("<br/><br/>");
                        //messageMail.Append(GetClaimInfo(claim));
                        messageMail.Append("<br/>");
                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                       "/Calc/Index?claimId=" + claim.Id + "</a>");
                        //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(productInClaim, messageMail.ToString(),
                            String.Format("{0} - {1} - Отмена заявки СпецРасчет", claim.TenderNumber, claim.Customer));
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
                var state = new ClaimStatus("PAUSE");

                isComplete = DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = model.IdClaim, ClaimStatus = state.Id });
                if (isComplete)
                {
                    model.Date = DateTime.Now;
                    model.IdUser = GetUser().Sid;
                    model.Status = state;// { Id = 4 };

                    db.SaveClaimStatusHistory(model);
                    model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                    //>>>>Уведомления
                    var claim = db.LoadTenderClaimById(model.IdClaim);
                    var productManagers = db.LoadProductManagersForClaim(model.IdClaim, model.Version);
                    if (productManagers != null && productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        var user = GetUser();
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var host = ConfigurationManager.AppSettings["AppHost"];
                        var messageMail = new StringBuilder();
                        messageMail.Append("Добрый день!");
                        messageMail.Append("<br/>Пользователь ");
                        messageMail.Append(user.FullName);
                        messageMail.Append(" приостановил заявку где Вам назначены позиции для расчета.");
                        //messageMail.Append("<br/><br />");
                        //messageMail.Append(GetClaimInfo(claim));
                        messageMail.Append("<br/><br />");
                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                       "/Calc/Index?claimId=" + claim.Id + "</a>");
                        //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(productInClaim, messageMail.ToString(),
                            String.Format("{0} - {1} - Приостановка заявки СпецРасчет", claim.TenderNumber, claim.Customer));
                    }
                }
            }
            catch (Exception ex)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }
        [HttpPost]
        public ActionResult SaveFile(int? claimId, int? cv)
        {
            string message = "";
            if (Request.Files.Count > 0)
            {
               
                if (claimId.HasValue && claimId.Value > 0)
                {
                    //foreach (HttpPostedFileWrapper file in Request.Files)
                    //{
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        var fileFormats = WebConfigurationManager.AppSettings["FileFormat4TenderClaimFile"].Split(',').Select(s => s.ToLower()).ToArray();
                        byte[] fileData = null;
                        if (Array.IndexOf(fileFormats, Path.GetExtension(file.FileName).ToLower()) > -1)
                        {
                          using (var br = new BinaryReader(file.InputStream))
                        {
                            fileData = br.ReadBytes(file.ContentLength);
                        }
                        var db = new DbEngine();
                        var claimFile = new TenderClaimFile() { IdClaim = claimId.Value, File = fileData, FileName = file.FileName };
                        db.SaveTenderClaimFile(ref claimFile);  
                        }
                       else if (file.ContentLength > 0) message += String.Format("Файл {0} имеет недопустимое расширение.",file.FileName);
                    }
                    //}
                }
            }
            TempData["error"] = message;
            return RedirectToAction("Index", "Claim", new { claimId = claimId, cv=cv});
        }
 

//>>>>Уведомления
//возобновление заявки
[HttpPost]
        public JsonResult SetClaimContinued(int IdClaim, string Comment,int Version, DateTime? deadlineDate)
        {
            ClaimStatusHistory model = new ClaimStatusHistory() {IdClaim = IdClaim , Version = Version, Comment = Comment };
            var isComplete = false;
            try
            {
                var db = new DbEngine();
                if (deadlineDate.HasValue)
                {
                    db.UpdateClaimDeadline(IdClaim, deadlineDate.Value);
                }
                var statusHistory = db.LoadStatusHistoryForClaim(model.IdClaim);
                if (statusHistory.Count() > 1)
                {
                    //var lastValueValid = statusHistory.Last().Status.Id;
                    //if (lastValueValid == 4 || lastValueValid == 5)
                    //{
                        var actualStatus = new ClaimStatusHistory() {Status = new ClaimStatus(){Id = 3}};//statusHistory[statusHistory.Count() - 2];
                        isComplete = DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = model.IdClaim, ClaimStatus = actualStatus.Status.Id });
                        if (isComplete)
                        {
                            model.Date = DateTime.Now;
                            model.IdUser = GetUser().Sid;
                            model.Status = new ClaimStatus() { Id = actualStatus.Status.Id };
                            if (string.IsNullOrEmpty(model.Comment)) model.Comment = "Возобновление заявки" + "<br />Автор: " + GetUser().DisplayName;
                            db.SaveClaimStatusHistory(model);
                            model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                            //>>>>Уведомления
                            var claim = db.LoadTenderClaimById(model.IdClaim);
                            var productManagers = db.LoadProductManagersForClaim(model.IdClaim, model.Version);
                            if (productManagers != null && productManagers.Any())
                            {
                                var productManagersFromAd = UserHelper.GetProductManagers();
                                var user = GetUser();
                                var productInClaim =
                                    productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                                var host = ConfigurationManager.AppSettings["AppHost"];
                                var messageMail = new StringBuilder();
                                messageMail.Append("Добрый день!");
                                messageMail.Append("<br/>Пользователь ");
                                messageMail.Append(user.FullName);
                                messageMail.Append(" возобновил заявку для работы где Вам назначены позиции для расчета.");
                                messageMail.Append("<br/><br/>");
                                //messageMail.Append(GetClaimInfo(claim));
                                messageMail.Append("Ссылка на заявку: ");
                                messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                               "/Calc/Index?claimId=" + claim.Id + "</a>");
                                //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(productInClaim, messageMail.ToString(),
                                    String.Format("{0} - {1} - Возобновление заявки СпецРасчет", claim.TenderNumber, claim.Customer));
                            }
                        }
                    //}
                }
            }
            catch (Exception ex)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = model });
        }
        [HttpPost]
        
        //>>>>Уведомления
        //Отклонение позиций
       
        public JsonResult SetPositonRejected(List<int> positionsId, string comment, int idClaim, int cv)
        {
            var isComplete = false;
            ClaimStatusHistory model = null;
            //try
            //{
                var user = GetUser();
                var db = new DbEngine();
                isComplete = db.ChangePositionsState(positionsId, new PositionState("CANCELMAN").Id);
                var allPositions = db.LoadSpecificationPositionsForTenderClaim(idClaim, cv);
                int claimStatus = 3;
                bool isSameCalculate = allPositions.Any(x => x.State == 2 || x.State == 4);
                if (isSameCalculate) claimStatus = 6;
                var status = db.LoadLastStatusHistoryForClaim(idClaim).Status.Id;
                //изменение статуса заявки и истроиии изменения статусов
                if (status != claimStatus)
                {
                    var changeStatusComplete = DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = idClaim, ClaimStatus = claimStatus });
                    if (changeStatusComplete)
                    {
                        model = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdUser = user.Sid,
                            IdClaim = idClaim,
                            Comment = comment,
                            Status = new ClaimStatus() { Id = claimStatus }
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
                    var productManagers =
                        allPositions.Where(x => positionsId.Contains(x.Id)).Select(x => x.ProductManager).ToList();
                    if (productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var host = ConfigurationManager.AppSettings["AppHost"];
                        var messageMail = new StringBuilder();
                        messageMail.Append("Добрый день!");
                        messageMail.Append("<br/>");
                        messageMail.Append("Пользователь ");
                        messageMail.Append(user.DisplayName);
                        messageMail.Append(" отклонил Ваш расчет позиции по заявке № " + claim.Id + "<br/>");
                        if (!string.IsNullOrEmpty(comment)) messageMail.Append("Комментарий: " + comment);
                        messageMail.Append("<br/><br/>");
                        //messageMail.Append(GetClaimInfo(claim));
                        //messageMail.Append("<br/>");
                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                           "/Calc/Index?claimId=" + claim.Id + "</a>");
                        //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(productInClaim, messageMail.ToString(),
                             String.Format("{0} - {1} - Отклонение расчета позиций СпецРасчет", claim.TenderNumber, claim.Customer));
                    }
                }
            //}
            //catch (Exception)
            //{
            //    isComplete = false;
            //}
            return Json(new { IsComplete = isComplete, Model = model });
        }
        [HttpPost]

        //>>>>Уведомления
        //Отклонение позиций

        public JsonResult SendPositonOnWork(List<int> positionsId, string comment, int idClaim, int cv)
        {
            var isComplete = false;
            ClaimStatusHistory model = null;
            //try
            //{
                var user = GetUser();
                var db = new DbEngine();
                isComplete = db.ChangePositionsState(positionsId, new PositionState("SEND").Id);
                var lastStatus = db.LoadLastStatusHistoryForClaim(idClaim);
                int claimStatus; var allPositions = db.LoadSpecificationPositionsForTenderClaim(idClaim, cv);
                if (lastStatus.Status.Id == 9) claimStatus = 2;
                else claimStatus = lastStatus.Status.Id;
                var productManagers =
                        allPositions.Where(x => positionsId.Contains(x.Id)).Select(x => x.ProductManager).ToList();
                if (productManagers != null && productManagers.Any())
                {
                    var productManagersFromAd = UserHelper.GetProductManagers();
                    foreach (var productManager in productManagers)
                    {
                        var productManagerFromAd =
                            productManagersFromAd.FirstOrDefault(x => x.Id == productManager.Id);
                        if (productManagerFromAd != null)
                        {
                            productManager.ShortName = productManagerFromAd.ShortName;
                        }
                    }
                }
                var status = db.LoadLastStatusHistoryForClaim(idClaim).Status.Id;
                //изменение статуса заявки и истроиии изменения статусов
                var changeStatusComplete = true;
                if (lastStatus.Status.Id == 9) changeStatusComplete = DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = idClaim, ClaimStatus = 2 });
               if (changeStatusComplete)
                    {
                        model = new ClaimStatusHistory()
                        {
                            Date = DateTime.Now,
                            IdUser = user.Sid,
                            IdClaim = idClaim,
                            Comment = "Переданы позиции для повторного расчета для:<br />"
                    +string.Join("<br />", productManagers.Select(x => x.ShortName))+"<br /><br />",
                    
                            Status = new ClaimStatus() { Id = claimStatus}
                        };
                        if (!string.IsNullOrEmpty(comment)) model.Comment += "Комментарий: " + comment + "<br />";
                        model.Comment += "Автор: " + user.DisplayName;
                        db.SaveClaimStatusHistory(model);
                        model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                    }
                
                if (isComplete)
                {
                    //>>>>Уведомления
                    var claim = db.LoadTenderClaimById(idClaim);
                     productManagers =
                        allPositions.Where(x => positionsId.Contains(x.Id)).Select(x => x.ProductManager).ToList();
                    if (productManagers.Any())
                    {
                        var productManagersFromAd = UserHelper.GetProductManagers();
                        var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                        var host = ConfigurationManager.AppSettings["AppHost"];
                        var messageMail = new StringBuilder();
                        messageMail.Append("Добрый день!<br/>");
                        messageMail.Append("В заявке № " + claim.Id + " вам вновь переданы позиции для расчета пользователем " + user.FullName +"<br/>");
                        if (!string.IsNullOrEmpty(comment)) messageMail.Append("Комментарий: " + comment+"<br/>");
                        messageMail.Append("<br/>");
                        messageMail.Append("Ссылка на заявку: ");
                        messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                           "/Calc/Index?claimId=" + claim.Id + "</a>");
                        //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                        Notification.SendNotification(productInClaim, messageMail.ToString(),
                             String.Format("{0} - {1} - Повторная передача позиций СпецРасчет для расчета", claim.TenderNumber, claim.Customer));
                    }
                }
            //}
            //catch (Exception)
            //{
            //    isComplete = false;
            //}
            return Json(new { IsComplete = isComplete, Model = model });
        }
        //>>>>Уведомления
        //подтверждение позиций по заявке
        public JsonResult SetClaimAllPositonConfirmed(int idClaim, int cv)
        {
            var isComplete = false;
            ClaimStatusHistory model = null;
            var message = string.Empty;
            //try
            //{
                var user = GetUser();
                var db = new DbEngine();
                var positions = db.LoadSpecificationPositionsForTenderClaim(idClaim, cv);
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
                            DbEngine.ChangeTenderClaimClaimStatus(new TenderClaim() { Id = idClaim, ClaimStatus = 8 });
                            model = new ClaimStatusHistory()
                            {
                                Date = DateTime.Now,
                                IdUser = user.Sid,
                                IdClaim = idClaim,
                                Comment = string.Empty,
                                Status = new ClaimStatus() { Id = 8 }
                            };
                            db.SaveClaimStatusHistory(model);
                            model.DateString = model.Date.ToString("dd.MM.yyyy HH:mm");
                            //>>>>Уведомления
                            var claim = db.LoadTenderClaimById(idClaim);
                            var productManagers = positions.Select(x => x.ProductManager).ToList();
                            if (productManagers.Any())
                            {
                                var productManagersFromAd = UserHelper.GetProductManagers();
                                var productInClaim =
                                    productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                                var host = ConfigurationManager.AppSettings["AppHost"];
                                var messageMail = new StringBuilder();
                                messageMail.Append("Добрый день!");
                                messageMail.Append("<br/>");
                                messageMail.Append("Пользователь ");
                                messageMail.Append(user.DisplayName);
                                messageMail.Append(" подтвердил Ваш расчет позиции по заявке № " + claim.Id + " - версия " + cv);
                                messageMail.Append("<br/><br/>");
                                //messageMail.Append(GetClaimInfo(claim));
                                //messageMail.Append("<br/>");
                                //messageMail.Append("Ссылка на заявку: ");
                                //messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                //                   "/Calc/Index?claimId=" + claim.Id + "</a>");
                                //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                                Notification.SendNotification(productInClaim, messageMail.ToString(),
                                     String.Format("{0} - версия {2} - {1} - Подтверждение расчета позиций заявки СпецРасчет", claim.TenderNumber, claim.Customer, cv));
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
            //}
            //catch (Exception)
            //{
            //    isComplete = false;
            //}
            return Json(new { IsComplete = isComplete, Model = model, Message = message }, JsonRequestBehavior.AllowGet);
        }

        //Изменение статуса конкурса
        public JsonResult ChangeClaimTenderStatus(int idClaim, int tenderStatus)
        {
            var isComplete = false;
            try
            {
                isComplete = DbEngine.ChangeTenderClaimTenderStatus(idClaim, tenderStatus);
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //>>>>Уведомления
        //Добавление комментария
        public JsonResult AddCommentToClaim(string comment, int idClaim, int cv)
        {
            var isComplete = false;
            ClaimStatusHistory statusHistory = null;
            try
            {
                var user = GetUser();
                var db = new DbEngine();
                statusHistory = db.LoadLastStatusHistoryForClaim(idClaim);
                statusHistory.Date = DateTime.Now;
                statusHistory.IdUser = user.Sid;
                statusHistory.Comment = comment;
                statusHistory.DateString = statusHistory.Date.ToString("dd.MM.yyyy HH:mm");
                isComplete = db.SaveClaimStatusHistory(statusHistory);
                if (isComplete)
                {
                    var productManagers = db.LoadProductManagersForClaim(idClaim, cv);
                    var productManagersFromAd = UserHelper.GetProductManagers();
                    var productInClaim =
                            productManagersFromAd.Where(x => productManagers.Select(y => y.Id).Contains(x.Id)).ToList();
                    var claim = db.LoadTenderClaimById(idClaim);
                    var host = ConfigurationManager.AppSettings["AppHost"];
                    var messageMail = new StringBuilder();
                    messageMail.Append("Добрый день!");
                    messageMail.Append("<br/>");
                    messageMail.Append("В заявке № " + idClaim + ", где Вам назначены позиции для расчета, пользователь ");
                    messageMail.Append(user.DisplayName);
                    messageMail.Append(" создал комментарий: <br />" + comment);
                    messageMail.Append("<br/>");
                    //messageMail.Append(GetClaimInfo(claim));
                    //messageMail.Append("<br/><br/>");
                    messageMail.Append("Ссылка на заявку: ");
                    messageMail.Append("<a href='" + host + "/Calc/Index?claimId=" + claim.Id + "'>" + host +
                                   "/Calc/Index?claimId=" + claim.Id + "</a>");
                    //messageMail.Append("<br/>Сообщение от системы Спец расчет");
                    Notification.SendNotification(productInClaim, messageMail.ToString(),
                        String.Format("{0} - {1} - Комментарий к заявке СпецРасчет", claim.TenderNumber, claim.Customer));
                }
            }
            catch (Exception)
            {
                isComplete = false;
            }
            return Json(new { IsComplete = isComplete, Model = statusHistory }, JsonRequestBehavior.AllowGet);
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
                    model.PriceTzr.Equals(position.PriceTzr) &&
                    model.SumTzr.Equals(position.SumTzr) &&
                    model.PriceNds.Equals(position.PriceNds) &&
                    model.SumNds.Equals(position.SumNds) &&
                    model.Unit == position.Unit &&
                    model.Value == position.Value)
                {
                    isUnique = false;
                    break;
                }
            }
            return isUnique;
        }

        private AdUser GetUser()
        {
            return CurUser;
            //if (Session["CurUser"] != null)
            //{
            //    return (UserBase)Session["CurUser"];
            //}
            //var user = UserHelper.GetUser(User.Identity);
            //Session["CurUser"] = user;
            //return user;
        }

        //создание уникального имени снабженца, для excel файла загрузки позиций
        private string GetUniqueDisplayName(UserBase user)
        {
            var result = new StringBuilder();
            var name = user.Name;
            var nameArr = name.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
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

        private string GetClaimInfo(TenderClaim claim)
        {
            var db = new DbEngine();
            var dealTypes = db.LoadDealTypes();
            return "Заявка № " + claim.Id + "<br />Автор: " + UserHelper.GetUserById(claim.Author.Sid).ShortName +
                   "<br />Номер конкурса: " + claim.TenderNumber + "<br />Заказчик: " + claim.Customer + "<br />ИНН заказчика: " + claim.CustomerInn + "<br />Дата начала: " +
                   claim.TenderStart.ToString("dd.MM.yyyy") + "<br />Срок сдачи: "
                   + claim.ClaimDeadline.ToString("dd.MM.yyyy") + "<br />Менеджер: " +
                   UserHelper.GetUserById(claim.Manager.Id).ShortName + "<br />Подразделение менеджера: " +
                   claim.Manager.SubDivision +
                   "<br />Тип конкурса: " + dealTypes.First(x => x.Id == claim.DealType).Value + (claim.Sum > 0
                       ? "<br />Сумма: " + claim.Sum.ToString("N2")
                       : string.Empty) + (!string.IsNullOrEmpty(claim.TenderUrl)
                           ? "<br />Сcылка на конкурс: <a href='" + claim.TenderUrl + "'>[Ссылка]</a>]"
                           : "не указана");
        }

        [HttpPost]
        public JsonResult CheckCalcPositionChanges(int idCalcPosition)
        {
            var res = CalcPosition.GetChanges(idCalcPosition);
            return Json(res);
        }
        [HttpPost]
        public PartialViewResult GetClaimHistory(int? id, bool full=false)
        {
            if (!id.HasValue) return null;

            int totalCount;
            var list = TenderClaim.GetHistory(out totalCount, id.Value, full);
            ViewBag.TotalHistory = totalCount;
            return PartialView("History", list);
        }

        //public PartialViewResult GetClaimRejectedPositions(int? id, int version = 1)
        //{
        //    if (!id.HasValue) return null;
        //    int totalCount;
        //    var list = TenderClaim.GetRejectedPositions(out totalCount, id.Value, version);
        //    ViewBag.TotalCount = totalCount;
        //    return PartialView("Positions", list);
        //}
    }
}
