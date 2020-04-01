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
            if (R.ApplyType == "Passport")
            {
                int passid = (from c in P.UserRegistrations
                              where c.ApplyType == R.ApplyType
                              select c).Count() + 1;
                userid = R.ApplyType.Substring(0, 4).ToUpper() + "-" + string.Format("{0:0000}", passid);
            }      
            else if (R.ApplyType == "Visa")
            {
                int visaid = (from c in P.UserRegistrations
                              where c.ApplyType == R.ApplyType
                              select c).Count() + 1;
                userid = R.ApplyType.Substring(0, 4).ToUpper() + "-" + string.Format("{0:0000}", visaid);
            }
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
            string password = string.Format("{0:00}",today.Day) + today.ToString("MMM").ToLower() + sp + randomid;
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
        public static List<City> getCity(string sname)
        {
            var sid = (from c in P.States
                      where c.STATE_NAME == sname
                      select c).FirstOrDefault();
            if(sid!=null)
            {
                var city = from c in P.Cities
                           where c.STATE_ID == sid.STATE_ID
                           select c;
                return city.ToList();
            }
            return null;
        }
        public static PassportApplication ApplyPassport(PassportApplication PA)
        {
            string passportid = string.Empty;
            DateTime expiryDate = PA.IssueDate.AddYears(10);
            if (PA.BookletType=="30 Pages")
            {
                var fps30 = (from c in P.PassportApplications
                             select c.PassportNumber.Substring(c.PassportNumber.Length - 4, c.PassportNumber.Length)).Max();
                if (fps30 == null)
                    fps30 = "0";
                passportid = "FPS-30" + string.Format("{0:0000}", int.Parse(fps30)+1);
            }             
            else if(PA.BookletType=="60 Pages")
            {
                var fps60 = (from c in P.PassportApplications
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

            int usercount = (from c in P.PassportApplications
                             where c.UserID == PA.UserID
                             select c).Count();
            if (usercount == 0)
            {
                P.PassportApplications.Add(PA);
                P.SaveChanges();
            }
            else
                return null;

            return PA;
        }
        public static PassportApplication PassportReIssue(PassportApplication PA)
        {
            string newpassportid = string.Empty;
            DateTime expiryDate = PA.IssueDate.AddYears(10);
            if (PA.BookletType == "30 Pages")
            {
                var fps30 = (from c in P.PassportApplications
                             select c.PassportNumber.Substring(c.PassportNumber.Length - 4, c.PassportNumber.Length)).Max();
                if (fps30 == null)
                    fps30 = "0";
                newpassportid = "FPS-30" + string.Format("{0:0000}", int.Parse(fps30) + 1);
            }
            else if (PA.BookletType == "60 Pages")
            {
                var fps60 = (from c in P.PassportApplications
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

                P.PassportApplications.Add(PA);
                P.SaveChanges();
                OldPassportData O = (from c in P.OldPassportDatas
                                     where c.PassportNumber == oldpassport.PassportNumber
                                     select c).FirstOrDefault();
                O.ReasonForReIssue = PA.ReasonForReIssue;
                P.SaveChanges();
                return PA;
            }
            else
                return null;
        }
        public static VisaApplication VisaApply(VisaApplication V)
        {
            string visaid = string.Empty;
            DateTime IssueDate = V.DateOfApplication.AddDays(10);
            DateTime ExpiryDate = DateTime.Today;
            int registrationcost = 0;
           
            PassportApplication PA = (from c in P.PassportApplications
                                      where c.PassportNumber == V.PassportNumber
                                      select c).FirstOrDefault();
            if (PA != null)
            {
                if (V.Occupation == "Student")
                {
                    int student_visaid = (from c in P.VisaApplications
                                          where c.Occupation == V.Occupation
                                          select c).Count() + 1;
                    visaid = V.Occupation.Substring(0, 3).ToUpper() + "-" + string.Format("{0:0000}", student_visaid);
                    ExpiryDate = IssueDate.AddYears(2);

                    if (V.Country == "USA")
                        registrationcost = 3000;
                    else if(V.Country=="China")
                        registrationcost = 1500;
                    else if (V.Country == "Japan")
                        registrationcost = 3500;
                }
                else if (V.Occupation == "Private Employee")
                {
                    int pe_visaid = (from c in P.VisaApplications
                                     where c.Occupation == V.Occupation
                                     select c).Count() + 1;
                    visaid = "PE-" + string.Format("{0:0000}", pe_visaid);
                    ExpiryDate = IssueDate.AddYears(3);

                    if (V.Country == "USA")
                        registrationcost = 4500;
                    else if (V.Country == "China")
                        registrationcost = 2000;
                    else if (V.Country == "Japan")
                        registrationcost = 4000;
                }
                else if (V.Occupation == "Government Employee")
                {
                    int ge_visaid = (from c in P.VisaApplications
                                     where c.Occupation == V.Occupation
                                     select c).Count() + 1;
                    visaid = "GE-" + string.Format("{0:0000}", ge_visaid);
                    ExpiryDate = IssueDate.AddYears(4);

                    if (V.Country == "USA")
                        registrationcost = 5000;
                    else if (V.Country == "China")
                        registrationcost = 3000;
                    else if (V.Country == "Japan")
                        registrationcost = 4500;
                }
                else if (V.Occupation == "Self Employed")
                {
                    int se_visaid = (from c in P.VisaApplications
                                     where c.Occupation == V.Occupation
                                     select c).Count() + 1;
                    visaid = "SE-" + string.Format("{0:0000}", se_visaid);
                    ExpiryDate = IssueDate.AddYears(1);

                    if (V.Country == "USA")
                        registrationcost = 6000;
                    else if (V.Country == "China")
                        registrationcost = 4000;
                    else if (V.Country == "Japan")
                        registrationcost = 9000;
                }
                else if (V.Occupation == "Retired Employee")
                {
                    int re_visaid = (from c in P.VisaApplications
                                     where c.Occupation == V.Occupation
                                     select c).Count() + 1;
                    visaid = "RE-" + string.Format("{0:0000}", re_visaid);
                    ExpiryDate = IssueDate.AddYears(1).AddMonths(6);

                    if (V.Country == "USA")
                        registrationcost = 2000;
                    else if (V.Country == "China")
                        registrationcost = 2000;
                    else if (V.Country == "Japan")
                        registrationcost = 1000;
                }
                if (ExpiryDate > PA.ExpiryDate)
                    ExpiryDate = PA.ExpiryDate;

                V.VisaID = visaid;
                V.DateOfIssue = IssueDate;
                V.DateOfExpiry = ExpiryDate;
                V.RegistrationCost = registrationcost;
                P.VisaApplications.Add(V);
                P.SaveChanges();
                return V;
            }
            else
                return null;
        }
        public static string VisaAuthentication(UserRegistration U)
        {
            UserRegistration UR = (from c in P.UserRegistrations
                                  where c.UserID == U.UserID && c.HintQuestion == U.HintQuestion && c.HintAnswer == U.HintAnswer
                                  select c).FirstOrDefault();
            if (UR != null)
                return "Success";
            else
                return "Your security question and answer doesn't match";
        }
        public static VisaApplication VisaCancellation(VisaApplication V)
        {   
            VisaApplication VA = (from c in P.VisaApplications
                                  where c.UserID == V.UserID && c.VisaID == V.VisaID && c.PassportNumber == V.PassportNumber && c.DateOfIssue == V.DateOfIssue
                                  select c).FirstOrDefault();
            if (VA!=null)
            {
                int cancellationcost = 0;
                int Diff_mon = 0;
                if (DateTime.Today<VA.DateOfExpiry)
                    Diff_mon = Math.Abs((VA.DateOfExpiry.Month - DateTime.Today.Month) + 12 * (VA.DateOfExpiry.Year - DateTime.Today.Year));
                if (VA.Occupation == "Student")
                {
                    if (Diff_mon < 6)
                        cancellationcost = (int)(0.15 * VA.RegistrationCost);
                    else if(Diff_mon>=6)
                        cancellationcost = (int)(0.25 * VA.RegistrationCost);
                }
                else if (VA.Occupation == "Private Employee")
                {
                    if (Diff_mon < 6)
                        cancellationcost = (int)(0.15 * VA.RegistrationCost);
                    else if (Diff_mon >=6 && Diff_mon < 12)
                        cancellationcost = (int)(0.25 * VA.RegistrationCost);
                    else if (Diff_mon >= 12)
                        cancellationcost = (int)(0.20 * VA.RegistrationCost);
                }
                else if (VA.Occupation == "Government Employee")
                {
                    if (Diff_mon >= 6 && Diff_mon < 12)
                        cancellationcost = (int)(0.20 * VA.RegistrationCost);
                    else if (Diff_mon >= 12)
                        cancellationcost = (int)(0.25 * VA.RegistrationCost);
                    else if (Diff_mon < 6)
                        cancellationcost = (int)(0.15 * VA.RegistrationCost);
                }
                else if (VA.Occupation == "Self Employed")
                {
                    if (Diff_mon < 6)
                        cancellationcost = (int)(0.15 * VA.RegistrationCost);
                    else if (Diff_mon >= 6)
                        cancellationcost = (int)(0.25 * VA.RegistrationCost);
                }
                else if (VA.Occupation == "Retired Employee")
                {
                    if (Diff_mon < 6)
                        cancellationcost = (int)(0.10 * VA.RegistrationCost);
                    else if (Diff_mon >= 6)
                        cancellationcost = (int)(0.20 * VA.RegistrationCost);
                }
                VA.CancellationCharges = cancellationcost;
                VA.Status = "Cancelled";
                P.SaveChanges();
                return VA;
            }
            else
                return null;           
        }
    }
}