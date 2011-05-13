using System;
using System.Collections.Generic;
using System.Text;
using Activizr.Logic.Pirates;
using Activizr.Logic.Structure;
using Activizr.Logic.Support;
using Activizr.Basic.Enums;
using Activizr.Basic.Types;
using Activizr.Database;

namespace Activizr.Logic.Communications
{
    public class PaperLetter: BasicPaperLetter
    {
        public static PaperLetter Create (Person creator, Organization organization, string fromName,
            string[] replyAddressLines, DateTime receivedDate, Person recipient, RoleType recipientRole, 
            bool personal)
        {
            return Create(creator.Identity, organization.Identity, fromName, replyAddressLines, receivedDate,
                          recipient.Identity, recipientRole, personal);
        }


        public static PaperLetter Create(int creatingPersonId, int organizationId, string fromName,
            string[] replyAddressLines, DateTime receivedDate, int toPersonId, RoleType toPersonInRole, bool personal)
        {
            return FromIdentity(PirateDb.GetDatabase().
                CreatePaperLetter(organizationId, fromName, String.Join("|", replyAddressLines),
                                  receivedDate, toPersonId, toPersonInRole, personal, creatingPersonId));
        }

        public static PaperLetter FromIdentity (int paperLetterId)
        {
            return FromBasic(PirateDb.GetDatabase().GetPaperLetter(paperLetterId));
        }

        public static PaperLetter FromBasic (BasicPaperLetter basic)
        {
            return new PaperLetter(basic);
        }

        private PaperLetter (BasicPaperLetter basic): base (basic)
        {
            // empty private constructor
        }

        public Documents Documents
        {
            get { return Support.Documents.ForObject(this); }
        }

#pragma warning disable 169
// ReSharper disable InconsistentNaming
        private new string ReplyAddress;  // hides ReplyAddress in base, quite on purpose
// ReSharper restore InconsistentNaming
#pragma warning restore 169

        public string[] ReplyAddressLines
        {
            get { return base.ReplyAddress.Split('|'); }
        }

        public Person Recipient
        {
            get
            {
                if (base.ToPersonId == 0)
                {
                    return null;
                }

                return Person.FromIdentity(base.ToPersonId);
            }
        }

    }
}