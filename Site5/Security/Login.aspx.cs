﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Activizr.Basic.Types;
using Activizr.Logic.Pirates;

namespace Activizr.Pages.Security
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.LabelLoginFailed.Visible = false;
            this.TextLogin.Focus();

            // Check for SSL

            if (!Request.IsSecureConnection)
            {
                if (!Request.Url.ToString().StartsWith("http://dev.activizr.com/") && !Request.Url.ToString().StartsWith("http://localhost:"))
                {
                    Response.Redirect(Request.Url.ToString().Replace("http:", "https:"));
                }
            }
        }

        protected void ButtonLogin_Click(object sender, EventArgs e)
        {
            string loginToken = this.TextLogin.Text;
            string password = this.TextPassword.Text;

            if (!string.IsNullOrEmpty(loginToken.Trim()) && !string.IsNullOrEmpty(password.Trim()))
            {
                try
                {
                    BasicPerson authenticatedPerson = Activizr.Logic.Security.Authentication.Authenticate(loginToken, password);
                    Person p = Person.FromIdentity(authenticatedPerson.PersonId);

                    if (p.PreferredCulture != Thread.CurrentThread.CurrentCulture.Name)
                        p.PreferredCulture = Thread.CurrentThread.CurrentCulture.Name;

                    // TODO: Determine logged-on organization; possibly ask for clarification

                    FormsAuthentication.RedirectFromLoginPage(authenticatedPerson.PersonId.ToString() + ",1", true);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.ToString());
                    this.LabelLoginFailed.Visible = true;
                    this.TextLogin.Focus();
                }
            }
        }
    }
}