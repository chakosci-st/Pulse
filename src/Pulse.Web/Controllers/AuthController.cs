using Pulse.Auth.Identity.Interfaces;
using Pulse.Auth.Identity.Models;
using Pulse.Auth.Identity.Services;
using Pulse.Core.Interfaces;
using Pulse.SharedUtilities.Helpers;
using Pulse.SharedUtilities.Extensions;

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Web.Security;
using System.Xml.Linq;

using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Pulse.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserGroupService _usergroupService;
        private readonly IPlantUserGroupMemberRepository _plantusergroupmemberService;
        private readonly Core.Interfaces.IActiveDirectoryService _activedirectoryService;

        public AuthController(
            IUserService userService,
            Core.Interfaces.IActiveDirectoryService activedirectoryService,
             IPlantUserGroupMemberRepository plantusergroupmemberService,
            IUserGroupService usergroupService)
        {
            _activedirectoryService = activedirectoryService;
            _userService = userService;
            _plantusergroupmemberService = plantusergroupmemberService;
            _usergroupService = usergroupService;
        }

        // ============================================================
        // 1) Me: returns current user info based on cookie identity
        // ============================================================
        [HttpGet]
        [Route("Account/Me")]
        public ActionResult Me()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new HttpUnauthorizedResult();
            }

            var employeeid = User.Identity.GetClaim("employeeid");
            var username = User.Identity.GetClaim("username");
            var email = User.Identity.GetClaim("email");
            var firstname = User.Identity.GetClaim("firstname");
            var lastname = User.Identity.GetClaim("lastname");
            var usergroups = User.Identity.GetClaim("usergroups");
            var modulecodes = User.Identity.GetClaim("modulecodes");

            var dto = new
            {
                UserName = username,
                EmployeeId = employeeid,
                Email = email,
                FirstName = firstname,
                LastName = lastname,
                DisplayName = (firstname + " " + lastname).Trim(),
                UserGroups = usergroups,
                ModuleCodes = modulecodes
            };

            return Json(dto, JsonRequestBehavior.AllowGet);
        }

        // ============================================================
        // 2) GetToken: generate JWT from existing cookie identity
        //    - NO re-authentication, you already trust User.Identity
        // ============================================================
        [Authorize]
        [HttpGet]
        [Route("auth/token")]
        public ActionResult GetToken()
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity == null || !identity.IsAuthenticated)
                {
                    return new HttpUnauthorizedResult("User not authenticated");
                }

                // Extract claims from the cookie identity
                var loggeduser = identity.GetClaim("username") ?? identity.Name;
                var employeeId = identity.GetClaim("employeeid") ?? string.Empty;
                var email = identity.GetClaim("email") ?? string.Empty;
                var firstName = identity.GetClaim("firstname") ?? string.Empty;
                var lastName = identity.GetClaim("lastname") ?? string.Empty;
                var modulecodes = identity.GetClaim("modulecodes") ?? string.Empty;
                var usergroupids = identity.GetClaim("usergroupids") ?? string.Empty;
                var usergroups = identity.GetClaim("usergroups") ?? string.Empty;

                var jwtSecret = System.Configuration.ConfigurationManager.AppSettings["Jwt:SecretKey"];
                var jwtIssuer = System.Configuration.ConfigurationManager.AppSettings["Jwt:Issuer"];
                var jwtAudience = System.Configuration.ConfigurationManager.AppSettings["Jwt:Audience"];

                var customClaims = new Dictionary<string, string>
                {
                    { "username",    loggeduser },
                    { "employeeid",  employeeId },
                    { "email",       email },
                    { "firstname",   firstName },
                    { "lastname",    lastName },
                    { "modulecodes", modulecodes },
                    { "usergroupids", usergroupids },
                    { "usergroups",  usergroups }
                };

                var token = JwtHelper.GenerateToken(
                    username: loggeduser,
                    usergroups: usergroupids,
                    modulecodes: modulecodes,
                    customClaims: customClaims,
                    secretkey: jwtSecret,
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    expireMinutes: 480);

                return Json(
                    new
                    {
                        access_token = token,
                        token_type = "Bearer",
                        expires_in = 480 * 60
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "GetToken error: " + ex.Message);
            }
        }

        // ============================================================
        // MVC basics
        // ============================================================
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Relogin(string url)
        {
            return View();
        }

        [HttpGet]
        [Route("auth/logout")]
        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            FormsAuthentication.SignOut();

            Session.Clear();
            Session.Abandon();

            ClearSession();


            var appLoginUrl = Url.Action("Login", "Auth", null, Request?.Url?.Scheme);
            if (string.IsNullOrWhiteSpace(appLoginUrl))
            {
                appLoginUrl = (ConfigurationManager.AppSettings["web:rootpath"] ?? string.Empty).TrimEnd('/') + "/auth/login";
            }

            var ssoLogoutUrl = GetSsoLogoutBaseUrl();
            if (string.IsNullOrWhiteSpace(ssoLogoutUrl))
            {
                return Redirect(appLoginUrl);
            }

            var separator = ssoLogoutUrl.Contains("?") ? string.Empty : "?TargetResource=";
            var redirectUrl = ssoLogoutUrl + separator + HttpUtility.UrlEncode(appLoginUrl);
            return Redirect(redirectUrl);


           
 
             

        }


        [HttpPost]
        [Route("auth/forcelogout")]
        public ActionResult ForceLogout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            FormsAuthentication.SignOut();

            Session.Clear();
            Session.Abandon();

            ClearSession();


            return new HttpStatusCodeResult(200); 
        }

        private void ClearSession()
        {

            Session.Clear();
            Session.Abandon();

            Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-30);
            Response.Cookies["stAuthClient"].Expires = DateTime.Now.AddDays(-30);
            Response.Cookies[".ASPXAUTH"].Expires = DateTime.Now.AddDays(-30);

            FormsAuthentication.SetAuthCookie(null, false);
            FormsAuthentication.SignOut();


            System.Web.HttpCookie authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, "");
            authCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(authCookie);

            DotNetPFClient.STAuthenticationModule.SingleSignOut();



        }

        private static string GetSsoLogoutBaseUrl()
        {
            var configuredAppSetting = ConfigurationManager.AppSettings["websso:logoutUrl"];
            if (!string.IsNullOrWhiteSpace(configuredAppSetting))
            {
                return configuredAppSetting;
            }

            try
            {
                var webConfig = WebConfigurationManager.OpenWebConfiguration("~");
                var section = webConfig.GetSection("stClientConfig") as DefaultSection;
                var rawXml = section?.SectionInformation?.GetRawXml();

                if (string.IsNullOrWhiteSpace(rawXml))
                {
                    return null;
                }

                var element = XElement.Parse(rawXml);
                return element.Attribute("stLogoutUrl")?.Value;
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // Login: create local user, load groups, issue cookie
        // ============================================================
        [Route("auth/login")]
        [Route("auth/login/{code}")]
        public async Task<ActionResult> Login(string code)
        {
            if (!Request.IsAuthenticated)
            {
                return new HttpUnauthorizedResult();
            }

            var loggeduser = User.Identity.Name;

            // 1) Load/create user profile
            var userprofile = await _userService.GetUserByUserNameAsync(loggeduser);

            if (userprofile == null)
            {
                var obj = _activedirectoryService.FindUser(
                    loggeduser,
                    Core.Enums.ActiveDirectoryKeyType.Username);

                userprofile = new Core.Entities.User
                {
                    UserId = obj.EmployeeId,
                    UserName = obj.Username,
                    FirstName = obj.FirstName,
                    LastName = obj.LastName,
                    Email = obj.Email,
                    CreatedBy = obj.EmployeeId
                };

                await _userService.AddUserAsync(userprofile);
            }

            // 2) Load user access (groups + modules)
            var usergroups = await _userService.GetUserGroupsAsync(userprofile.UserId);

            var modules = new List<string>();
            foreach (var usergroup in usergroups)
            {
                var modulesperusergroup = (await _usergroupService
                    .GetModulesAsync(usergroup.UserGroupId.Value))
                    .Where(ug => ug.IsSelected == 1);

                foreach (var module in modulesperusergroup)
                {
                    modules.Add(module.ModuleCode);
                }
            }

            var plantusergroup = await _plantusergroupmemberService.GetListByUserIdAsync(userprofile.UserId);
            foreach (var usergroup in plantusergroup)
            {
                var modulesperusergroup = (await _usergroupService
                    .GetModulesAsync(usergroup.UserGroupId))
                    .Where(ug => ug.IsSelected == 1);

                foreach (var module in modulesperusergroup)
                {
                    modules.Add(module.ModuleCode);
                }
            }



            var modulecodes = string.Join(",", modules.Distinct());
            var usergroupids = string.Join(",", usergroups.Select(ug => ug.UserGroupId));
            var usergroupnames = string.Join(",", usergroups.Select(ug => ug.UserGroup.UserGroupName));

            // 3) Build claims for MVC cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loggeduser),
                new Claim(ClaimTypes.NameIdentifier, userprofile.UserId ?? string.Empty),
                new Claim("username",    loggeduser),
                new Claim("employeeid",  userprofile.UserId ?? string.Empty),
                new Claim("email",       userprofile.Email ?? string.Empty),
                new Claim("firstname",   userprofile.FirstName ?? string.Empty),
                new Claim("lastname",    userprofile.LastName ?? string.Empty),
                new Claim("modulecodes", modulecodes ?? string.Empty),
                new Claim("usergroupids", usergroupids ?? string.Empty),
                new Claim("usergroups",  usergroupnames ?? string.Empty),
                new Claim("plantcodes", string.Join(",",  plantusergroup.Select(p=>p.PlantCode))  ?? string.Empty)
            };

            var identity = new ClaimsIdentity(
                claims,
                DefaultAuthenticationTypes.ApplicationCookie);

            var authManager = HttpContext.GetOwinContext().Authentication;

            authManager.SignIn(new AuthenticationProperties
            {
                IsPersistent = false
            }, identity);

            var returnUrl = Session["ReturnUrl"] as string;
            Session["ReturnUrl"] = null;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

 
    }
}