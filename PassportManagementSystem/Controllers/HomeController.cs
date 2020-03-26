﻿using PassportManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PassportManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
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
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult LoginPage(UserRegistration R)
        {         
            if (ModelState.IsValidField("UserID") && ModelState.IsValidField("ContactNumber") && ModelState.IsValidField("Password"))
            {
                UserRegistration userdetails = DBOperations.Login(R);
                if(userdetails!=null)
                {
                    if (userdetails.ApplyType == "Passport")
                    {
                        ViewBag.welcome = "Welcome " + userdetails.FirstName + " " + userdetails.SurName;
                        return View("Contact");
                    }
                    else if (userdetails.ApplyType == "Visa")
                    {
                        ViewBag.welcome = "Welcome " + userdetails.FirstName + " " + userdetails.SurName;
                        return View("About");
                    }
                }
                else
                {
                    ViewBag.error = "Invalid Credentials";
                    ModelState.Clear();
                    return View("Login");
                }
                return View("Login");
            }
            else
                return View("Login");
        }
        public ActionResult GetContactNumber()
        {
            string userid = Request.QueryString["uid"];
            UserRegistration R = DBOperations.getContactNumber(userid);
            if (R != null)
                return View("Login", R);
            else
                ViewBag.error = "UserID "+userid + " doesn't exists";
            return View("Login");       
        }
    }
}