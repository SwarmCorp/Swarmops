using System;
using System.Globalization;
using Activizr.Logic.Cache;
using Activizr.Logic.Communications;
using Activizr.Logic.Financial;
using Activizr.Logic.Interfaces;
using Activizr.Logic.Pirates;
using Activizr.Logic.Support;
using Activizr.Basic.Types;
using Activizr.Database;
using Activizr.Basic.Enums;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Activizr.Logic.Structure
{
    public class Organization : BasicOrganization, ITreeNode, IComparable, ITreeNodeObject
    {
        #region Creation and Construction

        // ReSharper disable UnusedPrivateMember
        protected Organization ()
        // ReSharper restore UnusedPrivateMember
        {
        } // disallow public direct construction

        private Organization (BasicOrganization basic)
            : base(basic)
        {
        }

        protected Organization (Organization org)
            : base(org)
        {
        }

        public static Organization FromIdentity (int identity)
        {

            return FromBasic(OrganizationCache.GetOrganization(identity));
            //return FromBasic(PirateDb.GetDatabase().GetOrganization(identity));
        }

        public static Organization FromBasic (BasicOrganization basic)
        {
            return new Organization(basic);
        }

        #endregion

        private ObjectOptionalData _optionalData = null;
        private ObjectOptionalData OptionalData
        {
            get
            {
                if (_optionalData == null)
                {
                    Organization o = (Organization)this;
                    _optionalData = ObjectOptionalData.ForObject(o); //Added cast, otherwise it fails for subclasses
                }
                return _optionalData;
            }
        }


        #region Public methods

        public bool IsOrInherits (Organization prospectiveParent)
        {
            if (this.Identity == prospectiveParent.Identity)
                return true;
            return Inherits(prospectiveParent.Identity);
        }

        public bool Inherits (Organization prospectiveParent)
        {
            return Inherits(prospectiveParent.Identity);
        }

        public bool IsOrInherits (int prospectiveParentOrganizationId)
        {
            if (this.Identity == prospectiveParentOrganizationId)
                return true;
            return Inherits(prospectiveParentOrganizationId);
        }

        public bool Inherits (int prospectiveParentOrganizationId)
        {
            // Returns true if prospectiveParent is a parent of ours.

            Organizations line = GetLine();

            for (int index = 0; index < line.Count - 1; index++)
            {
                if (line[index].Identity == prospectiveParentOrganizationId)
                {
                    return true;
                }
            }

            return false;
        }

        public Organizations GetLine ()
        {
            return Organizations.FromArray(OrganizationCache.GetOrganizationLine(Identity));

            //return Organizations.FromArray(PirateDb.GetDatabase().GetOrganizationLine(Identity));
        }

        public Organizations GetTree ()
        {
            return Organizations.FromArray(OrganizationCache.GetOrganizationTree(Identity));
            //return Organizations.FromArray(PirateDb.GetDatabase().GetOrganizationTree(Identity));
        }

        public int GetMemberCount ()
        {
            return Memberships.GetMemberCountForOrganization(this);
        }

        public NewsletterFeeds GetNewsletterFeeds ()
        {
            return NewsletterFeeds.FromArray(PirateDb.GetDatabase().GetNewsletterFeedsForOrganization(Identity));
        }


        public Organizations GetMasterOrganizations ()
        {
            if (Identity == Organization.UPSEid || Inherits(Organization.UPSEid))
            {
                // HACK: Hardcoded

                return Organizations.FromSingle(FromIdentity(Organization.PPSEid));
            }
            else
            {
                return new Organizations();
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The purpose of this property is to hide the base property from the Organization object.
        /// DefaultCountry should be used instead.
        /// 
        /// Wich has been disabled because we need to be able to update it... /JL
        /// 
        /// </summary>
        //protected new int DefaultCountryId
        //{
        //    get { return base.DefaultCountryId; }
        //}

        /// <summary>
        /// Gets the default country for this organization.
        /// </summary>
        public Country DefaultCountry
        {
            get { return Country.FromIdentity(DefaultCountryId); }
        }

        /// <summary>
        /// Gets true if this is a fallback organization allocation (like UP Piratn�tet).
        /// </summary>
        public bool CatchAll
        {
            get
            {
                // Hardcoded for now, move to db when need arises

                if (NameShort == "UP Piratn�tet")
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the mail prefix for this organization. If there is none,
        /// traverses the hierarchy upward until one is found.
        /// </summary>
        public string MailPrefixInherited // Cached
        {
            get
            {
                if (MailPrefix.Length > 0)
                {
                    return MailPrefix;
                }

                if (string.IsNullOrEmpty(this.mailPrefixInherited))
                {
                    Organizations orgLine = GetLine();

                    orgLine.Reverse();

                    foreach (Organization org in orgLine)
                    {
                        if (org.MailPrefix.Length > 0)
                        {
                            this.mailPrefixInherited = org.MailPrefix;
                            return this.mailPrefixInherited;
                        }
                    }
                }

                return this.mailPrefixInherited;
            }
        }


        public Geography PrimaryGeography
        {
            get
            {
                if (this.anchorGeography == null)
                {
                    this.anchorGeography = Geography.FromIdentity(AnchorGeographyId);
                }

                return this.anchorGeography;
            }
        }
        #endregion

        #region UptakeGeographies

        public void AddUptakeGeography (Geography geo)
        {
            AddUptakeGeography(geo.Identity);
        }

        public void DeleteUptakeGeography (Geography geo)
        {
            DeleteUptakeGeography(geo.Identity);
        }

        public void AddUptakeGeography (int geoId)
        {
            OrganizationCache.AddOrgUptakeGeography(this.Identity, geoId);
        }

        public void DeleteUptakeGeography (int geoId)
        {
            OrganizationCache.DeleteOrgUptakeGeography(this.Identity, geoId);
        }

        private List<Geography> uptakeGeographies;

        public ReadOnlyCollection<Geography> UptakeGeographies
        {
            get
            {
                if (this.uptakeGeographies == null)
                {
                    this.uptakeGeographies =
                        Geographies.FromIdentities(PirateDb.GetDatabase().GetOrganizationUptakeGeographyIds(Identity));
                }

                return this.uptakeGeographies.AsReadOnly();
            }
        }

        public UptakeGeography[] GetUptakeGeographies (bool others)
        {
            BasicUptakeGeography[] basics = PirateDb.GetDatabase().GetOrganizationUptakeGeographies(Identity, others);
            List<UptakeGeography> retVal = new List<UptakeGeography>();
            foreach (BasicUptakeGeography b in basics)
            {
                try
                {
                    UptakeGeography ug = UptakeGeography.FromBasic(b);
                    if (ug.Organization != null && ug.Geography != null && ug.Organization.ParentIdentity != -1)
                        retVal.Add(UptakeGeography.FromBasic(b));
                }
                catch
                {
                    //catch bad references
                }
            }

            if (others)
            {
                Dictionary<int, BasicOrganization> allOrgs = OrganizationCache.GetOrganizationHashtable(Organization.RootIdentity);
                foreach (BasicOrganization bo in allOrgs.Values)
                {
                    if (bo.Identity > 0)
                    {
                        BasicUptakeGeography myAnchor = new BasicUptakeGeography(bo.Identity, bo.AnchorGeographyId);
                        retVal.Add(UptakeGeography.FromBasic(myAnchor));
                    }
                }
            }
            else
            {
                BasicUptakeGeography myAnchor = new BasicUptakeGeography(Identity, AnchorGeographyId);
                if (retVal.Find(delegate(UptakeGeography ug) { return ug.GeoId == AnchorGeographyId; }) == null)
                    retVal.Add(UptakeGeography.FromBasic(myAnchor));
            }

            return retVal.ToArray();
        }

        #endregion

        #region FunctionalMailAddresses

        private Dictionary<MailAuthorType, FunctionalMail.AddressItem> functionalMailDict = null;

        public FunctionalMail.AddressItem GetFunctionalMailAddressInh (MailAuthorType authorType)
        {
            LoadFunctionalMailAddress();

            if (functionalMailDict.ContainsKey(authorType))
                return functionalMailDict[authorType];
            else
            {
                if (this.ParentIdentity != 0)
                    return this.Parent.GetFunctionalMailAddressInh(authorType);
                else if (FunctionalMail.Address.ContainsKey(authorType))
                    return FunctionalMail.Address[authorType];// Default.
                else
                    return null;
            }
        }

        public FunctionalMail.AddressItem GetFunctionalMailAddress (MailAuthorType authorType)
        {
            LoadFunctionalMailAddress();

            if (functionalMailDict.ContainsKey(authorType))
                return functionalMailDict[authorType];
            else
            {
                return null;
            }
        }

        private void LoadFunctionalMailAddress ()
        {
            if (functionalMailDict == null)
            {
                functionalMailDict = new Dictionary<MailAuthorType, FunctionalMail.AddressItem>();
                string funcMails = OptionalData.GetOptionalDataString(ObjectOptionalDataType.OrgFunctionalMail);
                string[] rows = funcMails.Replace("\r", "\n").Replace("\n\n", "\n").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                Regex reSplitAddress = new Regex(@"^(?<Type>.+?):(?<Name>.+)[,;:]\s*(?<Address>\S+?@\S+?)$", RegexOptions.IgnoreCase);
                foreach (string row in rows)
                {
                    Match match = reSplitAddress.Match(row);
                    try
                    {
                        MailAuthorType maType = (MailAuthorType)Enum.Parse(typeof(MailAuthorType), match.Groups["Type"].Value);
                        string name = match.Groups["Name"].Value;
                        string address = match.Groups["Address"].Value;
                        functionalMailDict[maType] = new FunctionalMail.AddressItem(address, name);
                    }
                    catch { }
                }
            }
        }

        public void SetFunctionalMailAddress (MailAuthorType authorType, string name, string email)
        {
            if (string.IsNullOrEmpty(email.Trim()))
            {
                functionalMailDict.Remove(authorType);
            }
            else
            {
                functionalMailDict[authorType] = new FunctionalMail.AddressItem(email, name);
            }
            SaveFunctionalMailDict();
        }


        private void SaveFunctionalMailDict ()
        {
            StringBuilder sb = new StringBuilder();

            foreach (MailAuthorType ma in functionalMailDict.Keys)
            {
                sb.AppendFormat("{0}:{1};{2}\r\n", ma.ToString(), functionalMailDict[ma].Name, functionalMailDict[ma].Email);
            }
            OptionalData.SetOptionalDataString(ObjectOptionalDataType.OrgFunctionalMail, sb.ToString());
        }
        #endregion

        #region ShowNamesInNotifications
        public bool ShowNamesInNotificationsInh
        {
            get
            {
                if (OptionalData.HasData(ObjectOptionalDataType.OrgShowNamesInNotifications))
                    return OptionalData.GetOptionalDataBool(ObjectOptionalDataType.OrgShowNamesInNotifications);
                else if (this.ParentIdentity != 0)
                    return this.Parent.ShowNamesInNotificationsInh;
                else
                    return OptionalData.GetOptionalDataBool(ObjectOptionalDataType.OrgShowNamesInNotifications);
            }
        }

        public Nullable<bool> ShowNamesInNotifications
        {
            get
            {
                if (OptionalData.HasData(ObjectOptionalDataType.OrgShowNamesInNotifications))
                    return OptionalData.GetOptionalDataBool(ObjectOptionalDataType.OrgShowNamesInNotifications);
                else
                    return null;
            }
            set
            {
                if (value.HasValue)
                {
                    OptionalData.SetOptionalDataBool(ObjectOptionalDataType.OrgShowNamesInNotifications, value.Value);
                }
                else
                {
                    OptionalData.SetOptionalData(ObjectOptionalDataType.OrgShowNamesInNotifications, null);
                }
            }
        }
        #endregion
        #region UsePaymentStatus
        public bool UsePaymentStatusInh
        {
            get
            {
                if (OptionalData.HasData(ObjectOptionalDataType.OrgUsePaymentStatus))
                    return OptionalData.GetOptionalDataBool(ObjectOptionalDataType.OrgUsePaymentStatus);
                else if (this.ParentIdentity != 0)
                    return this.Parent.UsePaymentStatusInh;
                else
                    return OptionalData.GetOptionalDataBool(ObjectOptionalDataType.OrgUsePaymentStatus);
            }
        }

        public Nullable<bool> UsePaymentStatus
        {
            get
            {
                if (OptionalData.HasData(ObjectOptionalDataType.OrgUsePaymentStatus))
                    return OptionalData.GetOptionalDataBool(ObjectOptionalDataType.OrgUsePaymentStatus);
                else
                    return null;
            }
            set
            {
                if (value.HasValue)
                {
                    OptionalData.SetOptionalDataBool(ObjectOptionalDataType.OrgUsePaymentStatus, value.Value);
                }
                else
                {
                    OptionalData.SetOptionalData(ObjectOptionalDataType.OrgUsePaymentStatus, null);
                }
            }
        }
        #endregion

        private Geography anchorGeography;
        private string mailPrefixInherited;


        #region class globals

        public static readonly int SandboxIdentity = 3;
        public static readonly int RootIdentity = 5;
        public static readonly int PPSEid = 1;
        public static readonly int UPSEid = 2;
        public static readonly int PPFIid = 49;
        public static readonly int UPFIid = 95;
        public static readonly int PPNOid = 48;
        public static readonly int PPDKid = 82;
        public static readonly int UPDKid = 83;

        public static Organization Sandbox
        {
            get { return FromIdentity(SandboxIdentity); }
        }

        public static Organization Root
        {
            get { return FromIdentity(RootIdentity); }
        }

        public static Organization PPSE
        {
            get { return Organization.FromIdentity(Organization.PPSEid); }
        }

        #endregion

        #region IComparable Members

        public int CompareTo (object obj)
        {
            var org2 = obj as Organization;

            if (org2 == null)
            {
                return 0; // undefined
            }

            if (org2.CatchAll && !this.CatchAll)
            {
                return -1;
            }

            if (!org2.CatchAll && this.CatchAll)
            {
                return 1;
            }

            // TODO: Read culture from organization's default country instead of hardcoding like this

            return String.Compare(Name, org2.Name, true, new CultureInfo("sv-SE"));
        }

        #endregion

        #region ITreeNode Members

        public int ParentIdentity
        {
            get { return ParentOrganizationId; }
        }


        public int[] ChildrenIdentities
        {
            get { return Children.Identities; }
        }

        public ITreeNodeObject ParentObject
        {
            get { return this.Parent; }
        }

        public List<ITreeNodeObject> ChildObjects
        {
            get
            {
                List<ITreeNodeObject> retVal = new List<ITreeNodeObject>();
                foreach (Organization child in Children)
                    retVal.Add(child);
                return retVal;
            }
        }

        #endregion

        public Organization Parent
        {
            get
            {
                return OrganizationCache.FromCache(ParentIdentity);
            }
        }

        public Organizations Children
        {
            get
            {
                return Organizations.FromArray(OrganizationCache.GetOrganizationChildren(Identity));
            }
        }

        public Person GetTreasurer ()
        {
            Organizations line = GetLine();
            BasicPersonRole[] treasurers
                = PirateDb.GetDatabase().GetPeopleWithRoleType(RoleType.OrganizationTreasurer, line.Identities, new int[] { });

            if (treasurers.Length == 0)
                throw new Exception("No Treasurer Found");

            if (treasurers.Length == 1)
                return Person.FromIdentity(treasurers[0].PersonId);

            for (int i = line.Count - 1; i >= 0; --i)
            {
                //check orgline backwards, giving the most local role
                Organization org = line[i];
                foreach (BasicPersonRole br in treasurers)
                {
                    if (org.Identity == br.OrganizationId)
                        return Person.FromIdentity(br.PersonId);
                }
            }

            throw new Exception("No Treasurer Found");
        }


        public Memberships GetMemberships (bool includeTerminated)
        {
            return Memberships.ForOrganization(this, includeTerminated);
        }

        public Memberships GetMemberships ()
        {
            return GetMemberships(false);
        }


        public OrganizationFinancialAccounts FinancialAccounts
        {
            get
            {
                return new OrganizationFinancialAccounts(this.Identity);
            }
        }


        internal static int Create (int ParentOrganizationId, string NameInternational, string Name, string NameShort, string Domain, string MailPrefix, int AnchorGeographyId, bool AcceptsMembers, bool AutoAssignNewMembers, int DefaultCountryId)
        {
            return OrganizationCache.CreateOrganization(ParentOrganizationId,
                                     NameInternational,
                                     Name,
                                     NameShort,
                                     Domain,
                                     MailPrefix,
                                     AnchorGeographyId,
                                     AcceptsMembers,
                                     AutoAssignNewMembers,
                                     DefaultCountryId);
        }

        public void Delete ()
        {
            string problems = "";
            int ChildrenCount = this.Children.Count;
            int ActiveMembershipsCount = this.GetMemberships(false).Count;
            int HistoricalMembershipsCount = this.GetMemberships(true).Count - ActiveMembershipsCount;

            if (ChildrenCount > 0)
                problems += ChildrenCount.ToString() + " child organisations\n\r";

            if (ActiveMembershipsCount > 0)
                problems += ActiveMembershipsCount.ToString() + " active memberships\n\r";

            if (HistoricalMembershipsCount > 0)
                problems += HistoricalMembershipsCount.ToString() + " historical memberships\n\r";

            if (problems != "")
                throw new Exception("Can not delete because:\n\r" + problems);

            OrganizationCache.DeleteOrganization(this.Identity);

        }

        public static void UpdateOrganization (int ParentOrganizationId, string NameInternational, string Name,
                                            string NameShort, string Domain, string MailPrefix, int AnchorGeographyId, bool AcceptsMembers,
                                            bool AutoAssignNewMembers, int DefaultCountryId, int OrganizationId)
        {
            OrganizationCache.UpdateOrganization(ParentOrganizationId, NameInternational, Name, NameShort, Domain,
                MailPrefix, AnchorGeographyId, AcceptsMembers, AutoAssignNewMembers, DefaultCountryId,
                OrganizationId);
        }


        public OrganizationParameters Parameters
        {
            get { return new OrganizationParameters(this); }
        }


    }
}