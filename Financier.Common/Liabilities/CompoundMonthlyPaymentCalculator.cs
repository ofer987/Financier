using System;
using System.Linq;
using System.Collections.Generic;

namespace Financier.Common.Liabilities
{
    public class MonthlyPaymentCalculator : IMonthlyPaymentCalculator
    {
        public IEnumerable<MonthlyPayment> GetMonthlyPayments(IMortgage mortgage)
        {
            return GetMonthlyPayments(mortgage, DateTime.MaxValue);
        }

        public IEnumerable<MonthlyPayment> GetMonthlyPayments(IMortgage mortgage, DateTime endAt)
        {
            if (endAt < mortgage.InitiatedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(endAt), $"Should be at or later than {mortgage.InitiatedAt}");
            }

            yield return new MonthlyPayment(mortgage, mortgage.InitiatedAt, mortgage.InitialValue, 0, 0);

            var monthlyPayment = Convert.ToDecimal(mortgage.MonthlyPayment);
            var balance = mortgage.InitialValue;
            var interestRate = mortgage.PeriodicAnnualInterestRate;

            var i = mortgage.InitiatedAt;
            for (; balance > 0 && i < endAt; i = i.AddDays(1))
            {
                if (mortgage.IsMonthlyPayment(i))
                {
                    var interestPayment = Convert.ToDecimal(Convert.ToDouble(balance) * interestRate / 12);
                    var principalPayment = monthlyPayment - interestPayment;

                    principalPayment = principalPayment > balance
                        ? balance
                        : principalPayment;

                    yield return new MonthlyPayment(mortgage, i, balance, interestPayment, principalPayment);
                    balance -= principalPayment;
                }

                if (balance == 0)
                {
                    yield break;
                }

                var extraPayment = mortgage.GetPrincipalOnlyPayments(i.Year, i.Month, i.Day)
                    .Sum();
                if (extraPayment != 0)
                {
                    extraPayment = extraPayment > balance
                        ? balance
                        : extraPayment;

                    yield return new MonthlyPayment(mortgage, i, balance, 0, extraPayment);
                    balance -= extraPayment;
                }
            }
        }
    }
}