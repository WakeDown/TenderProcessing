using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Stuff.Objects;
using TenderProcessing.Helpers;
using TenderProcessing.Models;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Controllers
{
    public class QuestionController : Controller
    {
        private UserBase GetCurUser()
        {
            return UserHelper.GetUser(User.Identity);
        }

        private void DisplayCurUser()
        {
            var user = GetCurUser();
            if (user == null || !UserHelper.IsUserAccess(user))
            {
                var dict = new RouteValueDictionary();
                dict.Add("message", "У Вас нет доступа к приложению");
                RedirectToAction("ErrorPage", "Auth", dict);
            }
            ViewBag.UserName = user.Name;
        }
        
        [HttpGet]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Question que)
        {
            DisplayCurUser();

            try
            {
                ResponseMessage responseMessage;
                que.Creator = new Employee() { AdSid = GetCurUser().Id };
                bool complete = que.Save(out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);

                return RedirectToAction("Positions", "Question", new { id = responseMessage.Id });
            }
            catch (Exception ex)
            {
                ViewData["ServerError"] = ex.Message;
                return View("New", que);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Positions(Question que)
        {
            

            try
            {
                ResponseMessage responseMessage;
                que.NewPosition.Creator = new Employee() { AdSid = GetCurUser().Id };
                que.NewPosition.Question = new Question(que.Id);
                bool complete = que.NewPosition.Save(out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);

                return RedirectToAction("Positions", new { id = que.Id });
            }
            catch (Exception ex)
            {
                ViewData["ServerError"] = ex.Message;
                return RedirectToAction("Positions", que.NewPosition.Question);
            }

            return View();
        }

        [HttpGet]
        public ActionResult Positions(int? id)
        {
            DisplayCurUser();
            Question que = new Question();
            if (id.HasValue)
            {
                que = new Question(id.Value, true);
            }
            else
            {
                RedirectToAction("New");
            }

            return View(que);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            DisplayCurUser();

            try
            {
                ResponseMessage responseMessage;
                bool complete = QuePosition.Delete(id, out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
            return null;
        }

        [HttpPost]
        public ActionResult SetQuestion2Work(int? id)
        {
            try
            {
                ResponseMessage responseMessage;
                //que.Creator = new Employee() { AdSid = GetCurUser().Id };
                //que.NewPosition.Question = new Question(que.Id);
                bool complete = Question.SetQuestion2Work(id.Value, out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);

                return RedirectToAction("Index", "Claim");
            }
            catch (Exception ex)
            {
                ViewData["ServerError"] = ex.Message;
                return RedirectToAction("Positions", new { id = id.Value });
            }
        }
	}
}