using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Utilities
{
    // disini kita membuat custom validation attribute, agar bisa. kita harus melakukan inherit
    // kepada class ValidationAttribute
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private readonly string allowedDomain;

        public ValidEmailDomainAttribute(string allowedDomain)
        {
            this.allowedDomain = allowedDomain;
        }

        public override bool IsValid(object value)
        {
            string[] emailDomain = value.ToString().Split("@");
            return emailDomain[1].ToUpper() == allowedDomain.ToUpper();
        }
    }
}
