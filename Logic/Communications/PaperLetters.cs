using System;
using System.Collections.Generic;
using System.Text;
using Activizr.Logic.Pirates;
using Activizr.Logic.Structure;
using Activizr.Basic.Types;
using Activizr.Database;
using Activizr.Logic.Support;

namespace Activizr.Logic.Communications
{
    public class PaperLetters: List<PaperLetter>
    {
        internal static PaperLetters FromArray(BasicPaperLetter[] basicArray)
        {
            // TODO: This function exists in too many places. It's GOT to be possible to make it generic,
            // through an interface or something like that.

            PaperLetters result = new PaperLetters() { Capacity = (basicArray.Length * 11 / 10) };

            foreach (BasicPaperLetter basic in basicArray)
            {
                result.Add(PaperLetter.FromBasic(basic));
            }

            return result;
        }

        public static PaperLetters ForOrganization (Organization organization)
        {
            return ForOrganization(organization.Identity);
        }

        public static PaperLetters ForOrganization (int organizationId)
        {
            return FromArray(PirateDb.GetDatabase().GetPaperLettersForOrganization(organizationId));
        }

        public static PaperLetters ForPerson (Person person)
        {
            return ForPerson(person.Identity);
        }

        public static PaperLetters ForPerson (int personId)
        {
            return FromArray(PirateDb.GetDatabase().GetPaperLettersForPerson(personId));
        }
    }
}