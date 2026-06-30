using DotNetPFClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pulse.Web.pages
{
    public partial class STAuthClient : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    if (!Request.QueryString["ReturnUrl"].StartsWith("/auth/login", StringComparison.OrdinalIgnoreCase))
                        Session["ReturnUrl"] = Request.QueryString["ReturnUrl"];
                }
                catch { }




                new STAuthenticationModule().init();

            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)

        {

            string strErrorMsg = string.Empty;

            lblErrorMsg.Text = "";

            if (new STAuthenticationModule().LdapProcessAuthentiaction(txtUserId.Text.Trim(),
            txtPassword.Text.Trim(),
            out strErrorMsg))
            {

                Response.Redirect(STAuthentication.StAppLoginUrl);

                //FormsAuthentication.RedirectToLoginPage();

            }

            else

            {

                lblErrorMsg.Text = strErrorMsg;

            }

        }
    }
}