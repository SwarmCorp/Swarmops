using System;
using System.Collections.Generic;
using System.Text;
using Activizr.Logic.Communications;
using Activizr.Logic.Financial;
using Activizr.Logic.Pirates;
using Activizr.Basic.Enums;
using Activizr.Basic.Interfaces;
using Activizr.Basic.Types;
using Activizr.Database;

namespace Activizr.Logic.Support
{
    public class Document: BasicDocument
    {
        private Document (BasicDocument document): base (document)
        {
            
        }

        public static Document FromIdentity (int documentId)
        {
            return FromBasic(PirateDb.GetDatabase().GetDocument(documentId));
        }

        public static Document FromBasic (BasicDocument basicDocument)
        {
            return new Document(basicDocument);
        }

        public static Document Create (string serverFileName, string clientFileName, Int64 fileSize, 
            string description, IHasIdentity identifiableObject, Person uploader)
        {
            int newDocumentId = PirateDb.GetDatabase().
                CreateDocument(serverFileName, clientFileName, fileSize, description, GetDocumentTypeForObject(identifiableObject), identifiableObject.Identity, uploader.Identity);

            return FromIdentity(newDocumentId);
        }

        public static DocumentType GetDocumentTypeForObject (IHasIdentity foreignObject)
        {
            if (foreignObject is Person)
            {
                return DocumentType.PersonPhoto;
            }
            else if (foreignObject is ExpenseClaim)
            {
                return DocumentType.ExpenseClaim;
            }
            else if (foreignObject is FinancialTransaction)
            {
                return DocumentType.FinancialTransaction;
            }
            else if (foreignObject is TemporaryIdentity)
            {
                return DocumentType.Temporary;
            }
            else if (foreignObject is InboundInvoice)
            {
                return DocumentType.InboundInvoice;
            }
            else if (foreignObject is PaperLetter)
            {
                return DocumentType.PaperLetter;
            }
            else if (foreignObject is ExternalActivity)
            {
                return DocumentType.ExternalActivityPhoto;
            }
            else
            {
                throw new ArgumentException("Unrecognized foreign object type:" + foreignObject.GetType().ToString());
            }
        }

        public new string ServerFileName
        {
            get { return base.ServerFileName; }
            set
            {
                PirateDb.GetDatabase().SetDocumentServerFileName(this.Identity, value);
                base.ServerFileName = value;
            }
        }

        public void SetForeignObject (IHasIdentity foreignObject)
        {
            PirateDb.GetDatabase().SetDocumentForeignObject(this.Identity, GetDocumentTypeForObject(foreignObject), foreignObject.Identity);
        }


        public IHasIdentity ForeignObject
        {
            get
            {
                switch (DocumentType)
                {
                    case DocumentType.ExpenseClaim:
                        return ExpenseClaim.FromIdentity(this.ForeignId);

                    case DocumentType.FinancialTransaction:
                        return FinancialTransaction.FromIdentity(this.ForeignId);
 
                    case DocumentType.InboundInvoice:
                        return InboundInvoice.FromIdentity(this.ForeignId);

                    case DocumentType.PaperLetter:
                        return PaperLetter.FromIdentity(this.ForeignId);

                    case DocumentType.PayrollItem:
                        return PayrollItem.FromIdentity(this.ForeignId);

                    default:
                        throw new NotImplementedException("DocumentType needs implementation: " + DocumentType.ToString());
                }
            }
        }

        public void Delete()
        {
            // Unlink, actually

            SetForeignObject(new TemporaryIdentity(0));
        }
    }
}