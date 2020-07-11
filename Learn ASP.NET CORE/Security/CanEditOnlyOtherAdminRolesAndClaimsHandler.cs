using Learn_ASP.NET_CORE.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Learn_ASP.NET_CORE.Security
{
    // Disini kita membuat custom handler, dengan requirement dari class ManageAdminRolesAndClaimsRequirement

    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : 
        AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        // lalu setelah itu kita melakukan override ke method HandleRequirementAsync()
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, 
            ManageAdminRolesAndClaimsRequirement requirement)
        {
            // properti resource, berisikan ManageRole dan ManageClaim action method
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if(authFilterContext == null)
            {
                return Task.CompletedTask;
            }

            // mendapatkan id admin yang login
            string loggedInAdminId = 
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            // mendapatkan id admin yang sedang diedit
            string adminInBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];

            // cek apakah user memiliki role admin DAN user claim itu adalah Edit Role dengan value true
            // DAN id admin yang login tidak sama dengan id admin yang sedang di edit
            if(context.User.IsInRole("Admin") && context.User.HasClaim(c => c.Type == "Edit Role" && c.Value == "true")
                && adminInBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;

        }
    }
}
