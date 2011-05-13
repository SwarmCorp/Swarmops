using System;
using System.Collections.Generic;
using System.Text;
using Activizr.Logic.Support;
using Activizr.Basic.Types;
using Activizr.Database;

namespace Activizr.Logic.Financial
{
    public class PayrollAdjustments: PluralBase<PayrollAdjustments,PayrollAdjustment,BasicPayrollAdjustment>
    {
        static public PayrollAdjustments ForPayrollItem (PayrollItem payrollItem)
        {
            return FromArray(PirateDb.GetDatabase().GetPayrollAdjustments(payrollItem, DatabaseCondition.OpenTrue));
        }

        static public PayrollAdjustments ForSalary (Salary salary)
        {
            return FromArray(PirateDb.GetDatabase().GetPayrollAdjustments(salary));
        }
    }
}