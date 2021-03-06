using System;
using Swarmops.Basic.Enums;
using Swarmops.Basic.Interfaces;

namespace Swarmops.Basic.Types.Financial
{
    public class BasicFinancialValidation: IHasIdentity
    {
        public BasicFinancialValidation (int financialValidationId, FinancialValidationType validationType, int personId, DateTime dateTime, FinancialDependencyType dependencyType, int foreignId)
        {
            this.FinancialValidationId = financialValidationId;
            this.ValidationType = validationType;
            this.PersonId = personId;
            this.DateTime = dateTime;
            this.DependencyType = dependencyType;
            this.ForeignId = foreignId;
        }

        public BasicFinancialValidation (BasicFinancialValidation original)
            :this (original.Identity, original.ValidationType, original.PersonId, original.DateTime, original.DependencyType, original.ForeignId)
        {
            // empty copy ctor
        }

        public int FinancialValidationId { get; private set; }
        public FinancialValidationType ValidationType { get; private set; }
        public int PersonId { get; private set; }
        public DateTime DateTime { get; private set; }
        public FinancialDependencyType DependencyType { get; private set; }
        public int ForeignId { get; private set; }
        public int Identity
        {
            get { return this.FinancialValidationId; }
        }
    }
}
