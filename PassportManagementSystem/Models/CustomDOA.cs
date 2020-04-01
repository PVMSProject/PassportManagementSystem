using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PassportManagementSystem.Models
{
    public class CustomDOA : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            VisaApplication V = (VisaApplication)validationContext.ObjectInstance;
            DateTime D = Convert.ToDateTime(value);
            if(V.Status=="Approved")
            {
                if (D >= DateTime.Today)
                    return ValidationResult.Success;
                else
                    return new ValidationResult("Date Of Application Cannot be before the current date");
            }
            else
                return ValidationResult.Success;
        }
    }
}