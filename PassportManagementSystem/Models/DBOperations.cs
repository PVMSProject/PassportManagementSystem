using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
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
            string citizentype = string.Empty;
            string userid = string.Empty;
            int passid = (from c in P.UserRegistrations
                          where c.ApplyType == "Passport"
                          select c).Count() + 1;
            int visaid = (from c in P.UserRegistrations
                          where c.ApplyType == "Visa"
                          select c).Count() + 1;
            if (R.ApplyType == "Passport")
                userid = R.ApplyType.Substring(0, 4).ToUpper() + "-" + string.Format("{0:0000}", passid);
            else if (R.ApplyType == "Visa")
                userid = R.ApplyType.Substring(0, 4).ToUpper() + "-" + string.Format("{0:0000}", visaid);
            List<string> retrived_pass = (from c in P.UserRegistrations
                                         select c.Password.Substring(c.Password.Length-3,c.Password.Length).ToString()).ToList();
            string randomid=string.Format("{0:000}",random.Next(0, 999));
            while(true)
            {
                if (retrived_pass.Contains(randomid))
                    randomid = string.Format("{0:000}", random.Next(0, 999)); 
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
        public static List<State> getState()
        {
            var state = from s in P.States
                        select s;
            return state.ToList();
        }
        public static List<City> getCity(string sid)
        {
            var city = from c in P.Cities
                       where c.STATE_ID==sid
                       select c;
            return city.ToList();
        }
        public static PassportApplication ApplyPassport(PassportApplication PA)
        {
            string passportid = string.Empty;
            DateTime expiryDate = PA.IssueDate.AddYears(10);
            if (PA.BookletType=="30 Pages")
            {
                var fps30 = (from c in P.PassportApplications
                             where c.BookletType == "30 Pages"
                             select c.PassportNumber.Substring(c.PassportNumber.Length - 4, c.PassportNumber.Length)).Max();
                if (fps30 == null)
                    fps30 = "0";
                passportid = "FPS-30" + string.Format("{0:0000}", int.Parse(fps30)+1);
            }             
            else if(PA.BookletType=="60 Pages")
            {
                var fps60 = (from c in P.PassportApplications
                             where c.BookletType == "60 Pages"
                             select c.PassportNumber.Substring(c.PassportNumber.Length - 4, c.PassportNumber.Length)).Max();
                if (fps60 == null)
                    fps60 = "0";
                passportid = "FPS-60" + string.Format("{0:0000}", int.Parse(fps60)+1);
            }
            int registrationcost = 0;
            if (PA.TypeOfService == "Normal")
                registrationcost = 2500;
            else if (PA.TypeOfService == "Tatkal")
                registrationcost = 5000;
            PA.PassportNumber = passportid;
            PA.ExpiryDate = expiryDate;
            PA.Amount = registrationcost;
            try
            {
                P.PassportApplications.Add(PA);
                P.SaveChanges();
            }
            catch(DbUpdateException E)
            {
                SqlException ex = E.GetBaseException() as SqlException;
                if (ex.Message.Contains("FK_PassportUserID"))
                    return null;
            }
            return PA;
        }
        public static PassportApplication PassportReIssue(PassportApplication PA)
        {
            string newpassportid = string.Empty;
            DateTime expiryDate = PA.IssueDate.AddYears(10);
            if (PA.BookletType == "30 Pages")
            {
                var fps30 = (from c in P.PassportApplications
                             where c.BookletType == "30 Pages"
                             select c.PassportNumber.Substring(c.PassportNumber.Length - 4, c.PassportNumber.Length)).Max();
                if (fps30 == null)
                    fps30 = "0";
                newpassportid = "FPS-30" + string.Format("{0:0000}", int.Parse(fps30) + 1);
            }
            else if (PA.BookletType == "60 Pages")
            {
                var fps60 = (from c in P.PassportApplications
                             where c.BookletType == "60 Pages"
                             select c.PassportNumber.Substring(c.PassportNumber.Length - 4, c.PassportNumber.Length)).Max();
                if (fps60 == null)
                    fps60 = "0";
                newpassportid = "FPS-60" + string.Format("{0:0000}", int.Parse(fps60) + 1);
            }
            int reissuecost = 0;
            if (PA.TypeOfService == "Normal")
                reissuecost = 1500;
            else if (PA.TypeOfService == "Tatkal")
                reissuecost = 3000;

            var oldpassport = (from c in P.PassportApplications
                                 where c.PassportNumber == PA.PassportNumber && c.UserID==PA.UserID
                                 select c).FirstOrDefault();
            if (oldpassport != null)
            {
                P.PassportApplications.Remove(oldpassport);
                PA.PassportNumber = newpassportid;
                PA.ExpiryDate = expiryDate;
                PA.Amount = reissuecost;
                try
                {
                    P.PassportApplications.Add(PA);
                    P.SaveChanges();
                    OldPassportData O = (from c in P.OldPassportDatas
                                         where c.PassportNumber == oldpassport.PassportNumber
                                         select c).FirstOrDefault();
                    O.ReasonForReIssue = PA.ReasonForReIssue;
                    P.SaveChanges();
                }
                catch (DbUpdateException E)
                {
                    SqlException ex = E.GetBaseException() as SqlException;
                    if (ex.Message.Contains("FK_PassportUserID"))
                        return null;
                }
                return PA;
            }
            else
                return null;
        }
    }
}