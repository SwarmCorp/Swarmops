using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Activizr.Basic.Enums;
using Activizr.Logic.Pirates;
using Activizr.Logic.Structure;
using Activizr.Logic.Support;
using Activizr.Logic.Security;

public partial class Pages_Public_DK_NewMember_New : System.Web.UI.Page
{
    DateTime SessionMemberDuplicateStop
    {
        get
        {
            if (Session["MemberDuplicateStop"] == null)
                Session["MemberDuplicateStop"] = DateTime.MinValue;
            return (DateTime)Session["MemberDuplicateStop"];
        }
        set
        {
            Session["MemberDuplicateStop"] = value;
        }
    }

    protected void Page_PreInit (object sender, EventArgs e)
    {
        this.UICulture = "da-DK";
    }

    protected void Page_Load (object sender, EventArgs e)
    {

        if (!Page.IsPostBack)
        {
            // Populate Countries

            Countries countries = Countries.GetAll();

            foreach (Country country in countries)
            {
                DropCountries.Items.Add(new ListItem(country.Code + " " + country.Name, country.Code));
            }

            //TODO: hardcoded denmark
            DropCountries.Items.FindByValue("DK").Selected = true;
            Wizard.ActiveStepIndex = 0;
            Wizard.SideBarStyle.CssClass = "Invisible";

            if (Request.UrlReferrer != null)
            {
                LabelReferrer.Text = Request.UrlReferrer.AbsoluteUri;
            }
            else
            {
                LabelReferrer.Text = string.Empty;
            }
            if (Request["repeat"] != null && Request["repeat"].ToLower().StartsWith("y"))
                Wizard.ActiveStepIndex = 5; // Startpage for repeat
            RepeatLink.NavigateUrl = Request.RawUrl;

            LabelMonthExample.Text = DropBirthMonths.Items[1].Text;
        }

        // Start by clearing all the error labels, for readability later.

        this.LabelNameError.Visible = false;
        this.LabelStreetError.Visible = false;
        this.LabelPostalError.Visible = false;
        this.LabelPhoneError.Visible = false;
        this.LabelEmailError.Visible = false;
        this.LabelBirthdateError.Visible = false;
        this.LabelGenderError.Visible = false;
        this.LabelCityError.Visible = false;


    }


    protected void Wizard_NextButtonClick (object sender, WizardNavigationEventArgs e)
    {
        // Validate data for the various steps, or (on page 4) finalize the membership.


        // -------------------------------------
        // VALIDATE PAGE: CHOICE OF ORGANIZATION 
        // -------------------------------------

        if (e.CurrentStepIndex == 0)
        {
            // Validate the choice of organization.

            if (this.RadioParty.Checked == false && this.RadioYouthLeague.Checked == false && this.RadioYouthLeagueOnly.Checked == false)
            {
                this.LabelOrganizationError.Text = "Du m&aring;ste v&auml;lja en organisation!";
                e.Cancel = true;
            }
            else
            {
                this.LabelOrganizationError.Text = string.Empty; // Clear on success
            }

            // If youth league is selected, make sure that the payment page is skipped.

            if (this.RadioYouthLeague.Checked == true || this.RadioYouthLeagueOnly.Checked == true)
            {
                this.LabelSmsMessageText.Text = "PP UNG";
                this.LabelOrganization.Text = "Ung Pirat";
                Wizard.ActiveStepIndex = 2; // Remove this on 2007 expiry

                // Set the focus to the name control on the member details page.

                this.TextName.Focus();
            }

            else if (this.RadioParty.Checked == true)
            {
                this.LabelSmsMessageText.Text = "PP MEDLEM";
                this.LabelOrganization.Text = "Piratpartiet";

                Wizard.ActiveStepIndex = 2; // Zero cost payment

                // Set the focus to the payment code control.

                this.TextPaymentCode.Focus();
            }


        }


        // ---------------------------
        // VALIDATE PAGE: PAYMENT CODE 
        // ---------------------------

        if (e.CurrentStepIndex == 1)
        {
            // Validate the payment code page.

            PaymentCode code = null;

            try
            {
                code = PaymentCode.FromCode(this.TextPaymentCode.Text.Trim());
            }
            catch (Exception)
            {
                // If we can't get the payment code, it will remain null, which is fine.
            }

            if (code == null || code.Claimed)
            {
                this.LabelPaymentError.Text = "Hittar inte kvittokoden!";
                e.Cancel = true;
            }
            else
            {
                this.LabelPaymentError.Text = string.Empty; // Clear on success

                if (code.IssuedToPhoneNumber.Length > 0)
                {
                    this.TextPhone.Text = code.IssuedToPhoneNumber;
                }
            }

            // Set the focus to the name control on the member details page.

            this.TextName.Focus();
        }


        // -----------------------------
        // VALIDATE PAGE: MEMBER DETAILS
        // -----------------------------

        if (e.CurrentStepIndex == 2)
        {
            // Validate member details. This is going to be quite a long validation.

            // Check for errors.

            // Name

            if (this.TextName.Text.Length < 3 || !this.TextName.Text.Contains(" "))
            {
                this.LabelNameError.Visible = true;
                e.Cancel = true;
            }

            // Street

            if (this.TextStreet.Text.Length < 4)
            {
                this.LabelStreetError.Visible = true;
                e.Cancel = true;
            }

            // Postal code & city -- also validate postal code length for given country

            if (this.TextPostal.Text.Length < 4)
            {
                this.LabelPostalError.Visible = true;
                e.Cancel = true;
            }
            else
            {
                Country country = Country.FromCode(DropCountries.SelectedValue);

                if (country.PostalCodeLength != 0)
                {
                    string postalCode = this.TextPostal.Text;
                    postalCode = postalCode.Replace(" ", "");
                    if (postalCode.Length != country.PostalCodeLength)
                    {
                        this.LabelPostalError.Visible = true; ;
                        e.Cancel = true;
                    }
                }
            }
            if (this.TextCity.Text.Length < 3 && this.LabelPostalError.Text.Length < 2)
            {
                this.LabelCityError.Visible = true;
                e.Cancel = true;
            }

            // Phone number

            if (this.TextPhone.Text.Length < 7)
            {
                this.LabelPhoneError.Visible = true;
                e.Cancel = true;
            }

            // Email

            this.TextEmail.Text = this.TextEmail.Text.Trim();
            if (this.TextEmail.Text.Length < 5)
            {
                this.LabelEmailError.Visible = true;
                e.Cancel = true;
            }
            else if (!Formatting.ValidateEmailFormat(this.TextEmail.Text))
            {
                this.LabelEmailError.Visible = true;
                e.Cancel = true;
            }


            // Birthdate

            try
            {
                int day = Convert.ToInt32(this.TextBirthDay.Text);
                int year = Convert.ToInt32(this.TextBirthYear.Text);
                int month = Convert.ToInt32(this.DropBirthMonths.SelectedValue);

                DateTime test = new DateTime(year, month, day);

                if (test > DateTime.Now)
                {
                    throw new Exception("No, you can't be born on a future date.");
                }

                if (test < DateTime.Now.AddYears(-125))
                {
                    throw new Exception("And you're not over 125 years old, either.");
                }
            }
            catch (Exception)
            {
                this.LabelBirthdateError.Visible = true;
                e.Cancel = true;
            }

            // Gender

            try
            {
                PersonGender gender = (PersonGender)Enum.Parse(typeof(PersonGender), this.DropGenders.SelectedValue);

                if (gender == PersonGender.Unknown)
                {
                    throw new Exception(); // Gender not selected - just throw something to produce the error message.
                }
            }
            catch (Exception)
            {
                this.LabelGenderError.Visible = true;
                e.Cancel = true;
            }


            if (!e.Cancel)
            {
                this.TextPassword1.Focus();
            }
        }



        // ---------------------------------
        // VALIDATE PAGE: PIRATEWEB PASSWORD
        // ---------------------------------

        if (e.CurrentStepIndex == 3 && DateTime.Now.Subtract(SessionMemberDuplicateStop).TotalSeconds > 2)
        {
            string password1 = this.TextPassword1.Text;
            string password2 = this.TextPassword2.Text;

            if (password1 != password2)
            {
                this.LabelPasswordErrorSame.Visible = true;
                e.Cancel = true;
            }
            else if (password1 == string.Empty)
            {
                this.LabelPasswordError.Visible = true;
                e.Cancel = true;
            }
            else if (password1.Length < 5)
            {
                this.LabelPasswordErrorLength.Visible = true;
                e.Cancel = true;
            }

            if (e.Cancel == true)
            {
                this.TextPassword1.Focus(); // Set focus to first (now empty) text box
            }
            else
            {
                ViewState["pw"] = password1;

                //Skipping activist and officer right now.
                CreateMember();
                Wizard.ActiveStepIndex = 5;
            }
        }

        if (e.CurrentStepIndex == 4 && DateTime.Now.Subtract(SessionMemberDuplicateStop).TotalSeconds > 2)
        {

            // This is the final page. When we get here, all data is good. This code
            // creates and commits the member.

            CreateMember();
            SessionMemberDuplicateStop = DateTime.Now;

            Wizard.ActiveStepIndex = 6;//to skip the repeat page

        }
        if (e.CurrentStepIndex == 5)
            Wizard.ActiveStepIndex = 0;
    }

    private void CreateMember ()
    {
        // If youthOrg is true, then the person opted for youth league membership.

        bool youthOrgSelected = this.RadioYouthLeague.Checked || this.RadioYouthLeagueOnly.Checked == true;

        PaymentCode paymentCode = null;

        /*
         * // Payments disabled
         * 
        if (!youthOrgSelected) 
        {
            paymentCode = PaymentCode.FromCode(this.TextPaymentCode.Text);
        }*/

        string name = this.TextName.Text;
        string password = ViewState["pw"] as string;
        string email = this.TextEmail.Text.Replace(",", ".");  // for some reason 1 in 500 mistypes this
        string phone = this.TextPhone.Text;
        string street = this.TextStreet.Text;
        string postal = this.TextPostal.Text;
        string city = this.TextCity.Text;
        string countryCode = this.DropCountries.SelectedValue;

        int birthDay = Convert.ToInt32(this.TextBirthDay.Text);
        int birthMonth = Convert.ToInt32(this.DropBirthMonths.SelectedValue);
        int birthYear = Convert.ToInt32(this.TextBirthYear.Text);

        DateTime birthdate = new DateTime(birthYear, birthMonth, birthDay);
        PersonGender gender = (PersonGender)Enum.Parse(typeof(PersonGender), this.DropGenders.SelectedValue);

        // This will stop the Create to be called twice even if doubleclick
        SessionMemberDuplicateStop = DateTime.Now; ;

        // We are ready to create the new person.

        Person person = Person.Create(name, email, password, phone, street, postal, city, countryCode, birthdate, gender);

        // The creation resolves the appropriate geography, so we can determine the correct
        // organization to join the person to.

        // In some cases PostalCode does not indicate the geography uniquely
        if (DropDownKommun.SelectedIndex >= 0)
        {
            person.Geography = Geography.FromIdentity(int.Parse(DropDownKommun.SelectedValue));
        }

        // Add memberships, as appropriate.

        Organization youthOrg = null;
        this.LabelMemberOfOrganizations.Text = string.Empty;

        if (youthOrgSelected)
        {
            try
            {
                youthOrg = Organizations.GetMostLocalOrganization(person.GeographyId, Organization.UPDKid);
            }
            catch (ArgumentException ex)
            {
                //didnt find any, Does the org itself accept members?
                youthOrg = Organization.FromIdentity(Organization.UPDKid);
                if (!youthOrg.AcceptsMembers)
                {
                    throw ex; //rethrow
                }

            }
            Activizr.Logic.Pirates.Membership newMembership = person.AddMembership(youthOrg.Identity, DateTime.Now.AddYears(1));
            newMembership.SetPaymentStatus(MembershipPaymentStatus.NewlyRegistered, DateTime.Now);
            this.LabelMemberOfOrganizations.Text = "<br><b>" + Server.HtmlEncode(youthOrg.Name) + "</b> og <br/>";
        }

        if (!this.RadioYouthLeagueOnly.Checked)
        {
            person.AddMembership(Organization.PPDKid, DateTime.Now.AddYears(1));
            this.LabelMemberOfOrganizations.Text += "<b>Piratpartiet</b>";
        }

        // Create events.

        if (RadioYouthLeague.Checked)
        {
            Activizr.Logic.Support.PWEvents.CreateEvent(EventSource.SignupPage, EventType.AddedMember,
                person.Identity, youthOrg.Identity, person.GeographyId, person.Identity, 0, youthOrg.Identity.ToString() + " " + Organization.PPDKid.ToString() + "," + Request.UserHostAddress + "," + this.LabelReferrer.Text);
            PWLog.Write(PWLogItem.Person, person.Identity, PWLogAction.MemberAdd, "New member joined organization " + youthOrg.NameShort + " and Piratpartiet DK.", "The self-signup came from IP address " + Request.UserHostAddress + ".");
        }
        else if (RadioYouthLeagueOnly.Checked)
        {
            Activizr.Logic.Support.PWEvents.CreateEvent(EventSource.SignupPage, EventType.AddedMember,
                person.Identity, youthOrg.Identity, person.GeographyId, person.Identity, 0, youthOrg.Identity.ToString() + "," + Request.UserHostAddress + "," + this.LabelReferrer.Text);
            PWLog.Write(PWLogItem.Person, person.Identity, PWLogAction.MemberAdd, "New member joined organizations " + youthOrg.NameShort, "The self-signup came from IP address " + Request.UserHostAddress + ".");
        }
        else
        {
            Activizr.Logic.Support.PWEvents.CreateEvent(EventSource.SignupPage, EventType.AddedMember,
                person.Identity, Organization.PPDKid, person.GeographyId, person.Identity, 0, Organization.PPDKid.ToString() + "," + Request.UserHostAddress + "," + this.LabelReferrer.Text);
            PWLog.Write(PWLogItem.Person, person.Identity, PWLogAction.MemberAdd, "New member joined Piratpartiet DK.", "The self-signup came from IP address " + Request.UserHostAddress + ".");
        }

        // Claim the payment.

        if (paymentCode != null)
        {
            paymentCode.Claim(person);
        }

        // If activity level was not passive (activist or officer), then register as activist.

        if (!this.RadioActivityPassive.Checked)
        {
            person.CreateActivist(false, false);

            Activizr.Logic.Support.PWEvents.CreateEvent(EventSource.SignupPage, EventType.NewActivist,
                person.Identity, Organization.PPDKid, person.GeographyId, person.Identity, 0, string.Empty);
            PWLog.Write(PWLogItem.Person, person.Identity, PWLogAction.MemberAdd, "Joined as activist.", string.Empty);
        }

        // If activity level was officer, register as officer volunteer.

        if (this.RadioActivityOfficer.Checked)
        {
            int[] chairman = Authorization.PersonsWithRoleInOrg(RoleType.OrganizationChairman, Organization.PPFIid, false);
            if (chairman.Length > 0)
            {
                Person defaultOwner = Person.FromIdentity(chairman[0]);

                Volunteer volunteer = Volunteer.Create(person, defaultOwner); // RF owns new volunteers

                //Autoassign will try to assign to ElectoralCircuit lead or 
                //if not possible, to its parent org lead, or if not possible to defaultOwner
                //TODO:Ugly hack sweden hardcoded (30)
                volunteer.AutoAssign(person.Geography, Organization.PPDKid, defaultOwner, Geography.SwedenId);

                volunteer.AddRole(Organization.PPDKid, person.GeographyId, RoleType.LocalLead);
                volunteer.AddRole(Organization.PPDKid, person.GeographyId, RoleType.LocalDeputy);
                volunteer.AddRole(Organization.PPDKid, person.GeographyId, RoleType.LocalAdmin);

                string textParameter = person.Name.Replace("|", "") + "|" +
                                       person.Phone.Replace("|", "") +
                                       "|Yes|KL1 KL2 KLA";

                Activizr.Logic.Support.PWEvents.CreateEvent(EventSource.SignupPage, EventType.NewVolunteer, 0, Organization.PPDKid, person.GeographyId, 0, 0, textParameter);
                PWLog.Write(PWLogItem.Person, person.Identity, PWLogAction.MemberAdd, "Volunteered for local officership.", string.Empty);
            }
        }
    }


    protected void Wizard_FinishButtonClick (object sender, WizardNavigationEventArgs e)
    {
        // Once we get here, we're all done, so just redirect to the home page.
        if (this.RadioParty.Checked)
            Response.Redirect("http://www.piratpartiet.dk/");
        else
            Response.Redirect("http://www.ungpirat.dk/");

    }


    protected void ButtonAbort_Click (object sender, EventArgs e)
    {
        if (this.RadioParty.Checked)
            Response.Redirect("http://www.piratpartiet.dk/");
        else
            Response.Redirect("http://www.ungpirat.dk/");
    }

    protected void TextCity_TextChanged (object sender, EventArgs e)
    {
    }

    protected void TextPostal_TextChanged (object sender, EventArgs e)
    {
        RebuildFromPostalCode();

    }

    private void RebuildFromPostalCode ()
    {
        Cities citiylist = Cities.FromPostalCode(TextPostal.Text, DropCountries.SelectedValue);
        LabelPostalErrorUnknown.Visible = false;
        string selectedKommun = DropDownKommun.SelectedValue;
        DropDownKommun.Items.Clear();
        foreach (City city in citiylist)
        {
            TextCity.Text = city.Name;
            ListItem newItem = new ListItem(city.Geography.Name, city.Geography.GeographyId.ToString());
            DropDownKommun.Items.Add(newItem);
            if (selectedKommun == city.Geography.GeographyId.ToString())
                newItem.Selected = true;

        }
        if (citiylist.Count > 1)
        {
            LabelKommun.Visible = true;
            DropDownKommun.Visible = true;
            LabelKommunError.Visible = true;
        }
        else
        {
            LabelKommun.Visible = false;
            DropDownKommun.Visible = false;
            LabelKommunError.Visible = false;
        }

        if (citiylist.Count == 0)
        {
            LabelPostalErrorUnknown.Visible = true;
        }
        else
        {
            if (DropDownKommun.SelectedIndex < 0)
            {
                DropDownKommun.SelectedIndex = 0;
            }
        }
    }
    protected void DropDownKommun_SelectedIndexChanged (object sender, EventArgs e)
    {

    }
    protected void DropCountries_SelectedIndexChanged (object sender, EventArgs e)
    {
        RebuildFromPostalCode();
    }
}
