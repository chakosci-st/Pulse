using Pulse.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Web.Controllers
{
    public class ErrorController : Controller
    {

        public ActionResult Index(int? code)
        {
            var model = new vmError();

            // Read the error message from the cookie
            string errorMessage = null;
            if (Request.Cookies["AppErrorMessage"] != null)
            {
                errorMessage = Request.Cookies["AppErrorMessage"].Value;
                // Optionally, clear the cookie after reading
                Response.Cookies["AppErrorMessage"].Expires = DateTime.Now.AddDays(-1);
            }


            switch (code)
            {
                case 404:
                    model.Title = "Page Not Found";
                    model.Message = "Sorry, the page you are looking for does not exist.";
                    break;
                case 500:
                    model.Title = "Server Error";
                    model.Message = "Oops! Something went wrong on our end.";
                    break;
                default:
                    model.Title = "Error";
                    model.Message = "An unexpected error has occurred. " + errorMessage;
                    break;
            }

            Response.StatusCode = code ?? 500;
            return View("Error", model);
        }


        [Route("Error/400")]
        [Route("Error/BadRequest")]
        public ActionResult BadRequest()
        {
            // Read the error message from the cookie
            string errorMessage = null;
            if (Request.Cookies["AppErrorMessage"] != null)
            {
                errorMessage = Request.Cookies["AppErrorMessage"].Value;
                // Optionally, clear the cookie after reading
                Response.Cookies["AppErrorMessage"].Expires = DateTime.Now.AddDays(-1);
            }

            ViewBag.ErrorMessage = errorMessage;
            return View();
        }
 
        [Route("Error/401")]
        [Route("Error/Unauthorized")]
        public ActionResult Unauthorized() {
            // Read the error message from the cookie
            string errorMessage = null;
            if (Request.Cookies["AppErrorMessage"] != null)
            {
                errorMessage = Request.Cookies["AppErrorMessage"].Value;
                // Optionally, clear the cookie after reading
                Response.Cookies["AppErrorMessage"].Expires = DateTime.Now.AddDays(-1);
            }

            ViewBag.ErrorMessage = errorMessage;
            return View();
        }

        [Route("Error/403")]
        [Route("Error/Forbidden")]
        public ActionResult Forbidden()
        {
            // Read the error message from the cookie
            string errorMessage = null;
            if (Request.Cookies["AppErrorMessage"] != null)
            {
                errorMessage = Request.Cookies["AppErrorMessage"].Value;
                // Optionally, clear the cookie after reading
                Response.Cookies["AppErrorMessage"].Expires = DateTime.Now.AddDays(-1);
            }

            ViewBag.ErrorMessage = errorMessage;
            return View();
        }

        [Route("Error/404")]
        [Route("Error/NotFound")]
        public ActionResult NotFound()
        {
            // Read the error message from the cookie
            string errorMessage = null;
            if (Request.Cookies["AppErrorMessage"] != null)
            {
                errorMessage = Request.Cookies["AppErrorMessage"].Value;
                // Optionally, clear the cookie after reading
                Response.Cookies["AppErrorMessage"].Expires = DateTime.Now.AddDays(-1);
            }

            ViewBag.ErrorMessage = errorMessage;
            return View();
        }
 
        [Route("Error/500")]
        [Route("Error/ServerError")]
        public ActionResult ServerError()
        {
            // Read the error message from the cookie
            string errorMessage = null;
            if (Request.Cookies["AppErrorMessage"] != null)
            {
                errorMessage = Request.Cookies["AppErrorMessage"].Value;
                // Optionally, clear the cookie after reading
                Response.Cookies["AppErrorMessage"].Expires = DateTime.Now.AddDays(-1);
            }

            ViewBag.ErrorMessage = errorMessage;
            return View();
        }

        [Route("Error/503")]
        [Route("Error/ServiceUnavailable")]
        public ActionResult ServiceUnavailable()
        {
            return View();
        }

        public ActionResult General()
        {
            // Read the error message from the cookie
            string errorMessage = null;
            if (Request.Cookies["AppErrorMessage"] != null)
            {
                errorMessage = Request.Cookies["AppErrorMessage"].Value;
                // Optionally, clear the cookie after reading
                Response.Cookies["AppErrorMessage"].Expires = DateTime.Now.AddDays(-1);
            }

            ViewBag.ErrorMessage = errorMessage;
            return View();
        }
    }
}