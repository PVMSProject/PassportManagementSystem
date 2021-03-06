﻿using System;
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

        //This method generates UserID,Password and CitizenType based on validations and conditions mentioned in the SRD and insert in database
        public static UserRegistration Registration(UserRegistration R)
        {
            try
            {
                string citizentype = string.Empty;
                string userid = string.Empty;

                //UserID Generation
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

                //Password Generation
                List<string> retrived_pass = (from c in P.UserRegistrations
                                              select c.Password.Substring(c.Password.Length - 3, c.Password.Length).ToString()).ToList();
                string randomid = string.Format("{0:000}", random.Next(0, 999));
                while (true)
                {
                    if (retrived_pass.Contains(randomid))
                        randomid = string.Format("{0:000}", random.Next(0, 999));
                    else
                        break;
                }
                char[] specialchar = { '#', '@', '$' };
                char sp = specialchar[random.Next(0, specialchar.Length)];
                DateTime today = DateTime.Today;
                string password = string.Format("{0:00}", today.Day) + today.ToString("MMM").ToLower() + sp + randomid;

                //CitizenType
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

                //Inserting into Database
                R.UserID = userid;
                R.Password = password;
                R.CitizenType = citizentype;
                P.UserRegistrations.Add(R);
                P.SaveChanges();
            }
            catch (Exception){}
            return R;
        }
        //This method checks seperately for 'Passport' and 'Visa' whether the EmailId is already registered 
        public static bool EmailValidation(UserRegistration R)
        {
            try
            {
                List<string> emailIDs = (from u in P.UserRegistrations
                                         where u.ApplyType == R.ApplyType
                                         select u.EmailAddress).ToList();
                if (emailIDs.Contains(R.EmailAddress))
                    return true;
                else
                    return false;
            }
            catch (Exception){}
            return false;
        }
        //It retrieves the ContactNumber w.r.t userid given from database and returns to Login view
        public static UserRegistration getContactNumber(string userid)
        {
            try
            {
                UserRegistration details = (from c in P.UserRegistrations
                                            where c.UserID == userid
                                            select c).FirstOrDefault();
                if (details != null)
                    return details;
                else
                    return null;
            }
            catch (Exception){  }
            return null;
        }
        //This method checks whether the given user details present in the database
        public static UserRegistration Login(UserRegistration R)
        {
            try
            {
                var user_present = (from u in P.UserRegistrations
                                    where u.UserID == R.UserID && u.ContactNumber == R.ContactNumber && u.Password == R.Password
                                    select u).FirstOrDefault();
                if (user_present != null)
                    return user_present;
                else
                    return null;
            }
            catch (Exception){}
            return null;
        }
        //Retrives list of states from database to 'ApplyPassport' and 'PassportReIssue' view on page load
        public static List<State> getState()
        {
            try
            {
                var state = from s in P.States
                            select s;
                return state.ToList();
            }
            catch (Exception) { }
            return null;
        }
        //Retrives list of cities based on state selected from database to 'ApplyPassport' and 'PassportReIssue' view
        public static List<City> getCity(string sname)
        {
            try
            {
                var sid = (from c in P.States
                           where c.STATE_NAME == sname
                           select c).FirstOrDefault();
                if (sid != null)
                {
                    var city = from c in P.Cities
                               where c.STATE_ID == sid.STATE_ID
                               select c;
                    return city.ToList();
                }
            }
            catch (Exception){}   
            return null;
        }
        //This method generates PassportNumber,RegistrationCost and ExpiryDate based on validations and conditions mentioned in the SRD and insert in database
        public static PassportApplication ApplyPassport(PassportApplication PA)
        {
            try
            {
                string passportid = string.Empty;
                //Calculates ExpiryDate based on IssueDate
                DateTime expiryDate = PA.IssueDate.AddYears(10);

                //Generates PassportNumber
                if (PA.BookletType == "30 Pages")
                {
                    var fps30 = (from c in P.PassportApplications
                                 select c.PassportNumber.Substring(c.PassportNumber.Length - 4, c.PassportNumber.Length)).Max();
                    if (fps30 == null)
                        fps30 = "0";
                    passportid = "FPS-30" + string.Format("{0:0000}", int.Parse(fps30) + 1);
                }
                else if (PA.BookletType == "60 Pages")
                {
                    var fps60 = (from c in P.PassportApplications
                                 select c.PassportNumber.Substring(c.PassportNumber.Length - 4, c.PassportNumber.Length)).Max();
                    if (fps60 == null)
                        fps60 = "0";
                    passportid = "FPS-60" + string.Format("{0:0000}", int.Parse(fps60) + 1);
                }

                //Calculates RegistrationCost based on Type of Service
                int registrationcost = 0;
                if (PA.TypeOfService == "Normal")
                    registrationcost = 2500;
                else if (PA.TypeOfService == "Tatkal")
                    registrationcost = 5000;

                //Inserting into database
                PA.PassportNumber = passportid;
                PA.ExpiryDate = expiryDate;
                PA.Amount = registrationcost;

                int usercount = (from c in P.PassportApplications
                                 where c.UserID == PA.UserID
                                 select c).Count();
                if (usercount == 0)//Checks whether the user already registered or not
                {
                    P.PassportApplications.Add(PA);
                    P.SaveChanges();
                }
                else
                    return null;
            }
            catch (Exception) { }
            return PA;
        }
        //This method generates NewPassportNumber,ReIssueCost and ExpiryDate based on validations and conditions mentioned in the SRD and insert in database
        //OldPassport data is also stored in database
        public static PassportApplication PassportReIssue(PassportApplication PA)
        {
            try
            {
                string newpassportid = string.Empty;
                //Calculates ExpiryDate based on IssueDate
                DateTime expiryDate = PA.IssueDate.AddYears(10);

                //Generates PassportNumber
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

                //Calculates ReIssueCost based on Type of Service
                int reissuecost = 0;
                if (PA.TypeOfService == "Normal")
                    reissuecost = 1500;
                else if (PA.TypeOfService == "Tatkal")
                    reissuecost = 3000;

                //Checks for the OldPassportNumber in database
                var oldpassport = (from c in P.PassportApplications
                                   where c.PassportNumber == PA.PassportNumber && c.UserID == PA.UserID
                                   select c).FirstOrDefault();
                if (oldpassport != null)
                {
                    //Removes the OldPassportData and stores in 'OldPassportData' table using trigger in SQLServer
                    P.PassportApplications.Remove(oldpassport);
                    PA.PassportNumber = newpassportid;
                    PA.ExpiryDate = expiryDate;
                    PA.Amount = reissuecost;

                    //Inserting New Passport details in database.
                    P.PassportApplications.Add(PA);
                    P.SaveChanges();

                    //Updates the ReasonForReIssue in 'OldPassportData' table
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
            catch (Exception) { }
            return null;
        }

        //This method generates VisaID,DateOfIssue,DateOfExpiry and RegistrationCost based on validations and conditions mentioned in the SRD and insert in database
        public static VisaApplication VisaApply(VisaApplication V)
        {
            try
            {
                string visaid = string.Empty;
                //Calculates DateOfIssue basedon DateOfApplication
                DateTime IssueDate = V.DateOfApplication.AddDays(10);
                DateTime ExpiryDate = DateTime.Today;
                int registrationcost = 0;

                //Checks whether the User Entered PassportNumber is in database or not
                PassportApplication PA = (from c in P.PassportApplications
                                          where c.PassportNumber == V.PassportNumber
                                          select c).FirstOrDefault();
                if (PA != null)
                {
                    //Based on Occupation of the User UserID,ExpiryDate and RegistrationCost is generated
                    if (V.Occupation == "Student")
                    {
                        int student_visaid = (from c in P.VisaApplications
                                              where c.Occupation == V.Occupation
                                              select c).Count() + 1;
                        visaid = V.Occupation.Substring(0, 3).ToUpper() + "-" + string.Format("{0:0000}", student_visaid);
                        ExpiryDate = IssueDate.AddYears(2);

                        if (V.Country == "USA")
                            registrationcost = 3000;
                        else if (V.Country == "China")
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

                    //If ExpiryDate of Visa is after the ExpiryDate of Passport then ExpiryDate of Visa is updated to ExpiryDate of Passport
                    if (ExpiryDate > PA.ExpiryDate)
                        ExpiryDate = PA.ExpiryDate;

                    //Inserting into database
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
            catch (Exception) { }
            return null;
        }
        //Security Question and answer of user should be matched with the Hint Question and Answer given by user while registering for Visa Cancellation
        public static string VisaAuthentication(UserRegistration U)
        {
            try
            {
                UserRegistration UR = (from c in P.UserRegistrations
                                       where c.UserID == U.UserID && c.HintQuestion == U.HintQuestion && c.HintAnswer == U.HintAnswer
                                       select c).FirstOrDefault();
                if (UR != null)
                    return "Success";
                else
                    return "Your security question and answer doesn't match";
            }
            catch (Exception) { }
            return null;
        }
        //This method validates the details given by the user to cancel the visa application
        //If validation is successfult then it calculates cancellation charges and updates status as 'Cancelled' in database
        public static VisaApplication VisaCancellation(VisaApplication V)
        {
            try
            {
                //Checks whether the details entered by the user matches in the database
                VisaApplication VA = (from c in P.VisaApplications
                                      where c.UserID == V.UserID && c.VisaID == V.VisaID && c.PassportNumber == V.PassportNumber && c.DateOfIssue == V.DateOfIssue
                                      select c).FirstOrDefault();
                if (VA != null)
                {
                    int cancellationcost = 0;
                    //Calculates difference between DateOfExpiry and Today's date in months
                    int Diff_mon = 0;
                    if (DateTime.Today < VA.DateOfExpiry)
                        Diff_mon = Math.Abs((VA.DateOfExpiry.Month - DateTime.Today.Month) + 12 * (VA.DateOfExpiry.Year - DateTime.Today.Year));

                    //Calculates cancellation cost based on occupation and 'Diff_mon'
                    if (VA.Occupation == "Student")
                    {
                        if (Diff_mon < 6)
                            cancellationcost = (int)(0.15 * VA.RegistrationCost);
                        else if (Diff_mon >= 6)
                            cancellationcost = (int)(0.25 * VA.RegistrationCost);
                    }
                    else if (VA.Occupation == "Private Employee")
                    {
                        if (Diff_mon < 6)
                            cancellationcost = (int)(0.15 * VA.RegistrationCost);
                        else if (Diff_mon >= 6 && Diff_mon < 12)
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

                    //Updating into database
                    VA.CancellationCharges = cancellationcost;
                    VA.Status = "Cancelled";
                    P.SaveChanges();
                    return VA;
                }
                else
                    return null;
            }
            catch (Exception) { }
            return null;
        }
    }
}