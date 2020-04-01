using PassportManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PassportManagementSystem.Controllers
{
   [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
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
            return View();
        }
        public ActionResult GetCity(string STATE_NAME)
        {
            slist = DBOperations.getState();
            ViewBag.state = slist;
            clist = DBOperations.getCity(STATE_NAME);
            if(clist!=null)
            {
                ViewBag.city = new SelectList(clist, "CITY_NAME", "CITY_NAME");
                return PartialView("DisplayCities");
            }
            return View("ApplyPassport");
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
                if (Session["UserID"].ToString() == P.UserID)
                {
                    PassportApplication details = DBOperations.ApplyPassport(P);
                    if (details != null)
                        ViewBag.data = details;
                    else
                        ViewBag.error = "UserId already exists";
                }
                else
                    ViewBag.error = "UserId doesn't match with current loginId";
                ModelState.Clear();
                return View();
            }
            else
            {
                slist = DBOperations.getState();
                ViewBag.state = slist;
                return View();
            }                  
        }
        public ActionResult PassportReIssue()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            slist = DBOperations.getState();
            ViewBag.state = slist;
            return View();
        }
        [HttpPost]
        public ActionResult PassportReIssue(PassportApplication P)
        {
            if (ModelState.IsValid)
            {
                slist = DBOperations.getState();
                ViewBag.state = slist;
                if (Session["UserID"].ToString() == P.UserID)
                {
                    PassportApplication details = DBOperations.PassportReIssue(P);
                    if (details != null)
                        ViewBag.data = details;
                    else
                        ViewBag.error = "Passport Number w.r.t UserId doesn't exists";
                }
                else
                    ViewBag.error = "UserId doesn't match with current loginId";
                ModelState.Clear();
                return View();
            }
            else
            {
                slist = DBOperations.getState();
                ViewBag.state = slist;
                return View();
            }
        }
        public ActionResult ApplyVisa()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            return View();
        }
        [HttpPost]
        public ActionResult ApplyVisa(VisaApplication V)
        {
            ModelState.Remove("VisaID");
            ModelState.Remove("DateOfIssue");
            if(ModelState.IsValid)
            {
                if (Session["UserID"].ToString()== V.UserID)
                {
                    VisaApplication details = DBOperations.VisaApply(V);
                    if (details != null)
                        ViewBag.data = details;
                    else
                        ViewBag.error = "Passport Number doesn't exists";
                }
                else
                    ViewBag.error = "UserId doesn't match with current loginId";
                ModelState.Clear();
                return View();
            }
            else
                return View();
        }
        public ActionResult VisaAuthentication()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            Session["Authentication"] = "Unsuccessfull";
            return View();
        }
        [HttpPost]
        public ActionResult VisaAuthentication(UserRegistration U)
        {
            if (ModelState.IsValidField("HintQuestion") && ModelState.IsValidField("HintAnswer"))
            {
                U.UserID = Session["UserID"].ToString();
                string authentication = DBOperations.VisaAuthentication(U);
                if(authentication!= "Success")
                {
                    ModelState.Clear();
                    ViewBag.error = authentication;
                    return View();
                }
                else
                { 
                    Session.Remove("Authentication");
                    ModelState.Clear();
                    return RedirectToAction("VisaCancellation");
                }
            }
            else
                return View();
        }
        public ActionResult VisaCancellation()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            else if(Session["Authentication"]==null)
                return View();
            else
                return RedirectToAction("VisaAuthentication");
        }
        [HttpPost]
        public ActionResult VisaCancellation(VisaApplication V)
        {
            ModelState.Remove("Country");
            ModelState.Remove("Occupation");
            ModelState.Remove("DateOfApplication");
            if (ModelState.IsValid)
            {
                if (Session["UserID"].ToString() == V.UserID)
                {
                    VisaApplication details = DBOperations.VisaCancellation(V);
                    if (details != null)
                        ViewBag.data = details;
                    else
                        ViewBag.error = "Given details doesn't match in our database";
                }
                else
                    ViewBag.error = "UserId doesn't match with current loginId";
                ModelState.Clear();
                return View();
            }
            else
                return View();
        }
    }
}