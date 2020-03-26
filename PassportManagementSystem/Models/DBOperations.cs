using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PassportManagementSystem.Models
{
    public class DBOperations
    {
        static PassportVisaManagementSystemEntities P = new PassportVisaManagementSystemEntities();
        static Random random = new Random();
        public static UserRegistration Registration(UserRegistration R)
        {
            string citizentype = String.Empty;
            string userid = String.Empty;
            int passid = (from c in P.UserRegistrations
                          where c.ApplyType == "Passport"
                          select c).Count() + 1;
            int visaid = (from c in P.UserRegistrations
                          where c.ApplyType == "Visa"
                          select c).Count() + 1;
            if (R.ApplyType == "Passport")
                userid = R.ApplyType.Substring(0, 4).ToUpper() + "-" + String.Format("{0:0000}", passid);
            else if (R.ApplyType == "Visa")
                userid = R.ApplyType.Substring(0, 4).ToUpper() + "-" + String.Format("{0:0000}", visaid);
            List<string> retrived_pass = (from c in P.UserRegistrations
                                         select c.Password.Substring(c.Password.Length-3,c.Password.Length).ToString()).ToList();
            string randomid=String.Format("{0:000}",random.Next(0, 999));
            while(true)
            {
                if (retrived_pass.Contains(randomid))
                    randomid = String.Format("{0:000}", random.Next(0, 999)); 
                else
                    break;
            }      
            char[] specialchar = { '#', '@', '$' };
            char sp = specialchar[random.Next(0, specialchar.Length)];
            DateTime today = DateTime.Today;
            string password = today.Day.ToString() + today.ToString("MMM").ToLower() + sp + randomid;
            int age = (int)(DateTime.Today.Subtract(R.DateOfBirth).TotalDays / 365);
            if (age >= 0 && age < 1)
                citizentype = "Infant";
            else if (age >= 1 && age < 10)
                citizentype = "Children";
            else if (age >= 10 && age < 20)
                citizentype = "Teen";
            else if (age >= 20 && age < 50)
                citizentype = "Adult";
            else if (age >= 50)
                citizentype = "Senior Citizen";
            R.UserID = userid;
            R.Password = password;
            R.CitizenType = citizentype;
            P.UserRegistrations.Add(R);
            P.SaveChanges();
            return R;
        }
        public static bool EmailValidation(UserRegistration R)
        {
            List<string> emailIDs = (from u in P.UserRegistrations
                                     where u.ApplyType==R.ApplyType
                                     select u.EmailAddress).ToList();
            if (emailIDs.Contains(R.EmailAddress))
                return true;
            else
                return false;
        }
        public static UserRegistration getContactNumber(string userid)
        {
            UserRegistration details = (from c in P.UserRegistrations
                               where c.UserID == userid
                               select c).FirstOrDefault();
            if (details != null)
                return details;
            else
                return null;
        }
        public static UserRegistration Login(UserRegistration R)
        {
            var user_present = (from u in P.UserRegistrations
                               where u.UserID == R.UserID && u.ContactNumber == R.ContactNumber && u.Password == R.Password
                               select u).FirstOrDefault();
            if (user_present != null)
                return user_present;
            else
                return null;                             
        }
    }
}