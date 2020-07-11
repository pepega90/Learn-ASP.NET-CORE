using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Learn_ASP.NET_CORE.Controllers
{
    public class ErrorController : Controller
    {
        /* logger (instance dari ILogger interface)
         * memiliki beberapa method untuk menampilkan beberapa pesan ke log
         * LogError(), untuk menampilkan exception
         * LogInformation(), untuk menampilkan informasi dari kode aplikasi kita
         * LogWarning(), untuk menampilkan warning
         */
        private readonly ILogger<ErrorController> logger;

        //jika kita ingin menampilkan information, warning, error. kita bisa inject ILogger interface
        // dengan bantuin ILogger interface, kita nantinya juga bisa membuat custom pesan errornya
        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {

            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested could not be found";
                    logger.LogWarning($"404 Error occured path = {statusCodeResult.OriginalPath} and query string = {statusCodeResult.OriginalQueryString}");
                    break;
            }

            return View("NotFound");
        }

        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            //ViewBag.ExceptionPath = exceptionDetails.Path;
            //ViewBag.ExceptionMessage = exceptionDetails.Error.Message;
            //ViewBag.StackTrace = exceptionDetails.Error.StackTrace;

            logger.LogError($"The path {exceptionDetails.Path} threw an exception {exceptionDetails.Error}");

            return View("Error");
        }
    }
}
