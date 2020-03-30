using PassportManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PassportManagementSystem.Controllers
{
   // [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class PassportManagementSystemController : Controller
    {
        static List<State> slist=null;
        static List<City> clist = null;
        public ActionResult Home()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(UserRegistration R)
        {
            ModelState.Remove("UserID");
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                UserRegistration details = DBOperations.Registration(R);
                ViewBag.data = details;
                ModelState.Clear();
                return View();
            }
            else
                return View();
        }
        public ActionResult GetContactNumber()
        {
            string userid = Request.QueryString["uid"];
            UserRegistration R = DBOperations.getContactNumber(userid);
            if (R != null)
                return View("Login", R);
            else
                ViewBag.error = "UserID " + userid + " doesn't exists";
            return View("Login");
        }
        public ActionResult Login()
        {
            if (Session["UserID"] != null && Session["ApplyType"] != null)
            {
                Session.Abandon();
                Session.Clear();
            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(UserRegistration R)
        {         
            if (ModelState.IsValidField("UserID") && ModelState.IsValidField("ContactNumber") && ModelState.IsValidField("Password"))
            {
                UserRegistration userdetails = DBOperations.Login(R);
                if(userdetails!=null)
                {
                    Session["UserID"] = userdetails.UserID;
                    Session["ApplyType"] = userdetails.ApplyType;
                    if (userdetails.ApplyType == "Passport")
                    {
                        Session["Welcome"] = userdetails.FirstName + " " + userdetails.SurName;
                        return RedirectToAction("ApplyPassport");
                    }
                    else if (userdetails.ApplyType == "Visa")
                    {
                        Session["Welcome"] = userdetails.FirstName + " " + userdetails.SurName;
                        return RedirectToAction("ApplyVisa");
                    }
                }
                else
                {
                    ViewBag.error = "Invalid Credentials";
                    ModelState.Clear();
                    return View();
                }
                return View();
            }
            else
                return View();
        }     
        public ActionResult ApplyPassport()
        { 
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            slist = DBOperations.getState();
            ViewBag.state = slist;
            string stateid = Request.QueryString["sid"];
            clist = DBOperations.getCity(stateid);
            ViewBag.city = clist;
            ViewBag.statename = stateid;
            return View();
        }
        [HttpPost]
        public ActionResult ApplyPassport(PassportApplication P)
        {
            ModelState.Remove("ReasonForReIssue");
            ModelState.Remove("PassportNumber");
            if (ModelState.IsValid)
            {
                slist = DBOperations.getState();
                ViewBag.state = slist;
                string stateid = Request.QueryString["sid"];
                clist = DBOperations.getCity(stateid);
                ViewBag.city = clist;
                ViewBag.statename = stateid;

                PassportApplication details = DBOperations.ApplyPassport(P);
                if (details != null && Session["UserID"].ToString() == details.UserID)
                    ViewBag.data = details;
                else
                    ViewBag.error = "UserId doesn't match with current loginId";
                ModelState.Clear();
                return View();
            }
            else
            {
                slist = DBOperations.getState();
                ViewBag.state = slist;
                string stateid = Request.QueryString["sid"];
                clist = DBOperations.getCity(stateid);
                ViewBag.city = clist;
                ViewBag.statename = stateid;
                return View();
            }          
        }
        public ActionResult PassportReIssue()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            slist = DBOperations.getState();
            ViewBag.state = slist;
            string stateid = Request.QueryString["sid"];
            clist = DBOperations.getCity(stateid);
            ViewBag.city = clist;
            ViewBag.statename = stateid;
            return View();
        }
        [HttpPost]
        public ActionResult PassportReIssue(PassportApplication P)
        {
            if (ModelState.IsValid)
            {
                slist = DBOperations.getState();
                ViewBag.state = slist;
                string stateid = Request.QueryString["sid"];
                clist = DBOperations.getCity(stateid);
                ViewBag.city = clist;
                ViewBag.statename = stateid;

                PassportApplication details = DBOperations.PassportReIssue(P);
                if (details != null && Session["UserID"].ToString() == details.UserID)
                    ViewBag.data = details;
                else
                    ViewBag.error = "Passport Number/UserId doesn't exists";
                ModelState.Clear();
                return View();
            }
            else
            {
                slist = DBOperations.getState();
                ViewBag.state = slist;
                string stateid = Request.QueryString["sid"];
                clist = DBOperations.getCity(stateid);
                ViewBag.city = clist;
                ViewBag.statename = stateid;
                return View();
            }
        }
        public ActionResult ApplyVisa()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            return View();
        }
        public ActionResult VisaCancellation()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            return View();
        }
    }
}