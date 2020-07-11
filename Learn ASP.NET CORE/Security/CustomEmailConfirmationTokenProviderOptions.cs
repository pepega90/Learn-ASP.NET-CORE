using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Security
{
    // disini kita membuat custom class, untuk EmailConfirmationTokenProviderOptions. 
    // lalu lakukan inherit ke DataProtectionTokenProviderOptions, dimana kita mengiginkan properti TokenLifespan
    public class CustomEmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {

    }
}
