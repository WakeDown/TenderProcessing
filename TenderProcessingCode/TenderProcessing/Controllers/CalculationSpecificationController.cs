using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TenderProcessing.Models;
using TenderProcessingDataAccessLayer;
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
            var claimStatusString = string.Empty;
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
                    var statusList = db.LoadClaimStatus();
                    var status = statusList.FirstOrDefault(x => x.Id == claim.ClaimStatus);
                    if (status != null)
                    {
                        claimStatusString = status.Value;
                    }
                }
            }
            ViewBag.Claim = claim;
            ViewBag.Status = claimStatusString;
            ViewBag.DealType = dealTypeString;
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
	}
}