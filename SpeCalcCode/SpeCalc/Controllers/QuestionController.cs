using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Stuff.Objects;
using SpeCalc.Helpers;
using SpeCalc.Models;
using SpeCalc.Objects;
using SpeCalcDataAccessLayer.Models;

namespace SpeCalc.Controllers
{
    public class QuestionController : BaseController
    {
        //private UserBase GetCurUser()
        //{

        //    if (Session["CurUser"] != null)
        //    {
                
        //        return (UserBase)Session["CurUser"];
        //    }
        //    var user = UserHelper.GetUser(User.Identity);
        //    Session["CurUser"] = user;
        //    return user;
        //}

        //private void DisplayCurUser()
        //{
        //    var user = GetCurUser();
        //    if (user == null || !UserHelper.IsUserAccess(user))
        //    {
        //        var dict = new RouteValueDictionary();
        //        dict.Add("message", "У Вас нет доступа к приложению");
        //        RedirectToAction("ErrorPage", "Auth", dict);
        //    }
        //    ViewBag.UserName = user.Name;
        //    ViewBag.CurUser = user;
        //}
        
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
                que.Creator = new Employee() { AdSid = GetCurUser().Sid };
                bool complete = que.Save(out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);

                return RedirectToAction("Index", "Question", new { id = responseMessage.Id });
            }
            catch (Exception ex)
            {
                ViewData["ServerError"] = ex.Message;
                return View("New", que);
            }
        }

        //[HttpPost]
        //public ActionResult Index(Question que)
        //{
            

        //    try
        //    {
        //        ResponseMessage responseMessage;
        //        que.NewPosition.Creator = new Employee() { AdSid = GetCurUser().Id };
        //        que.NewPosition.Question = new Question(que.Id);
        //        bool complete = que.NewPosition.Save(out responseMessage);
        //        if (!complete) throw new Exception(responseMessage.ErrorMessage);

        //        return RedirectToAction("Index", new { id = que.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ServerError"] = ex.Message;
        //        return RedirectToAction("Index", que.NewPosition.Question);
        //    }

        //    return View();
        //}

        [HttpGet]
        public ActionResult Index(int? id)
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
        public JsonResult DeletePos(int id)
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
            [MultipleButton(Name = "action", Argument = "SetQuestionAnswered")]
        public ActionResult SetQuestionAnswered(Question que)
        {
            try
            {
                ResponseMessage responseMessage;
                bool complete = Question.SetQuestionAnswered(que.Id, out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);

                return RedirectToAction("List", "Question");
            }
            catch (Exception ex)
            {
                TempData["ServerError"] = ex.Message;
                return RedirectToAction("Index", que);
            }
        }
        
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SetQuestionSent")]
        public ActionResult SetQuestionSent(Question que)
        {
            try
            {
                ResponseMessage responseMessage;
                //que.Creator = new Employee() { AdSid = GetCurUser().Id };
                //que.NewPosition.Question = new Question(que.Id);
                bool complete = Question.SetQuestionSent(que.Id, out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);

                return RedirectToAction("List", "Question");
            }
            catch (Exception ex)
            {
                TempData["ServerError"] = ex.Message;
                return RedirectToAction("Index", que);
            }
        }

        public ActionResult PositionList(int? id)
        {
            if (!id.HasValue) return HttpNotFound();



            return View();
        }

        [HttpGet]
        public ActionResult AddPosition(int? idQuestion)
        {
            //if (!idQuestion.HasValue) return HttpNotFound();

            //ViewData["idQuestion"] = idQuestion;

            return View();
        }

        [HttpPost]
        public ActionResult AddPosition(QuePosition pos)
        {
            try
            {
                ResponseMessage responseMessage;
                bool complete = pos.Save(out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);

                return RedirectToAction("Index", new { id = pos.Question.Id });
            }
            catch (Exception ex)
            {
                TempData["ServerError"] = ex.Message;
                return RedirectToAction("AddPosition", pos);
            }
        }

        [HttpGet]
        public ActionResult AddPosAnswer(int? idQuePosition)
        {
            //if (!idQuePosition.HasValue) return HttpNotFound();

            //ViewData["idQuePosition"] = idQuePosition;

            return View();
        }

         [HttpPost]
        public ActionResult AddPosAnswer(QuePosAnswer ans)
        {
            try
            {
                ResponseMessage responseMessage;
                bool complete = ans.Save(out responseMessage);
                if (!complete) throw new Exception(responseMessage.ErrorMessage);

                return RedirectToAction("Index", new { id = ans.QuePosition.Question.Id });
                //return RedirectToAction("Index", new { id = new QuePosition(ans.QuePosition.Id).Question.Id }); //RedirectToAction("Index", new { id = ans.QuePosition.Question.Id });
            }
            catch (Exception ex)
            {
                TempData["ServerError"] = ex.Message;
                return View("AddPosAnswer", ans);
            }
        }

         [HttpPost]
         public JsonResult DeleteAns(int id)
         {
             DisplayCurUser();

             try
             {
                 ResponseMessage responseMessage;
                 bool complete = QuePosAnswer.Delete(id, out responseMessage);
                 if (!complete) throw new Exception(responseMessage.ErrorMessage);
             }
             catch (Exception ex)
             {
                 return Json(ex.Message);
             }
             return null;
         }

         [HttpPost]
         public JsonResult DeleteQue(int id)
         {
             DisplayCurUser();

             try
             {
                 ResponseMessage responseMessage;
                 bool complete = Question.Delete(id, out responseMessage);
                 if (!complete) throw new Exception(responseMessage.ErrorMessage);
             }
             catch (Exception ex)
             {
                 return Json(ex.Message);
             }
             return null;
         }

        public ActionResult List()
        {
            //var list = Question.GetList();
            ViewBag.Sid = Question.GetSid();

            var filter = new QuestionFilter();

            return View(filter);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "QueFilter")]
        public ActionResult QueFilter(QuestionFilter qFilter)
        {
            return View("List", qFilter);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "QueFilterClear")]
        public ActionResult QueFilterClear(QuestionFilter qFilter)
        {
            var filter = new QuestionFilter();
            return View("List", filter);
        }
	}
}