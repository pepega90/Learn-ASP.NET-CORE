using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Security
{
    // Disini kita membuat custom requirement
    public class ManageAdminRolesAndClaimsRequirement : IAuthorizationRequirement
    {

    }
}
