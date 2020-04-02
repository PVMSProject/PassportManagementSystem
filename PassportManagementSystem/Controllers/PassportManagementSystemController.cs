using PassportManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PassportManagementSystem.Controllers
{
   [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]//Prevents Page from going back when back button is pressed
    public class PassportManagementSystemController : Controller
    {
        static List<State> slist=null;
        static List<City> clist = null;
        //Displays Home View
        public ActionResult Home()
        {
            return View();
        }
        //Displays About View
        public ActionResult About()
        {
            return View();
        }
        //Displays Contact View
        public ActionResult Contact()
        {
            return View();
        }
        //Displays Register View
        public ActionResult Register()
        {
            return View();
        }
        //When user submits the form in register page it validates
        //If validation is successfull then it goes to DBOperations Class and
        //fetches the data and store in ViewBag.data(used in .cshtml to display) and empty the fiels in the form
        //Else it returns to the same view with validation messages
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
        //Retrieving contact number from database to login view based on userid
        //If contact number w.r.t userid is present then R is sent to view to display the details in textbox
        //Else message is sent saying that UserID doesn't exists
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
        //Displays Login View
        //Session is used to remove that particular session when user logouts
        public ActionResult Login()
        {
            if (Session["UserID"] != null && Session["ApplyType"] != null)
            {
                Session.Abandon();
                Session.Clear();
            }
            return View();
        }
        //When user submits the form in Login page it validates
        //If validation is successfull then it goes to DBOperations Class and 
        //fetches the data and session is created for userid and applytype
        //based on applytype it redirects to that particular page
        //Else it returns to the same view with validation messages
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
        //when user logins if user type is passport then it redirects to this Action
        //DBOperations fetches state data on page load to view inorder to select state by the user
        //Used Session to restrict users to directly access the link without login
        public ActionResult ApplyPassport()
        { 
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            slist = DBOperations.getState();
            ViewBag.state = slist;
            return View();
        }
        //Retrives Cities list from database to view based on state name
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
        //When user apply the passport it validates
        //If validation is successfull then it goes to DBOperations Class and 
        //fetches the data and sends data or error messages to view 
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
        //Displays Passport ReIssue View
        //DBOperations fetches state data on page load to view inorder to select state by the user
        //Used Session to restrict users to directly access the link without login
        public ActionResult PassportReIssue()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            slist = DBOperations.getState();
            ViewBag.state = slist;
            return View();
        }
        //When user submit the passport data for reissue it validates
        //If validation is successfull then it goes to DBOperations Class and 
        //fetches the data and sends data or error messages to view 
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
        //when user logins if user type is Visa then it redirects to this Action
        //Used Session to restrict users to directly access the link without login
        public ActionResult ApplyVisa()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            return View();
        }
        //When user apply the visa then it validates
        //If validation is successfull then it goes to DBOperations Class and 
        //fetches the data and sends data or error messages to view 
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
        //Displays VisaAuthentication View and Created session called 'Authentication'
        //Used Session to restrict users to directly access the link without login
        public ActionResult VisaAuthentication()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            Session["Authentication"] = "Unsuccessfull";
            return View();
        }
        //When user submits the security question and answer then it validates
        //If validation is successfull then it goes to DBOperations Class and 
        //returns 'Success' and removes the 'Authentication' session and redirects to VisaCancellation
        //else returns to same view
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
        //Displays VisaCancellation view
        //Used Session to restrict users to directly access the link without login
        //Used 'Authentication' session to restricts users to directly access the link without VisaAuthentication
        public ActionResult VisaCancellation()
        {
            if (Session["UserID"] == null && Session["ApplyType"] == null)
                return RedirectToAction("Login");
            else if(Session["Authentication"]==null)
                return View();
            else
                return RedirectToAction("VisaAuthentication");
        }
        //When user cancels the visa then it validates
        //If validation is successfull then it goes to DBOperations Class and 
        //fetches the data and sends data or error messages to view 
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