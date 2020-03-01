using System;
using NUnit.Framework;

using Financier.Common.Liabilities;
using Financier.Common.Expenses;
using Financier.Common.Expenses.Actions;
using Financier.Common.Models;

namespace Financier.Common.Tests.Expenses
{
    // TODO:Rename this file and others to *Tests
    public class CashFlowStatementTest
    {
        public DateTime InitiatedAt => Subject.InitiatedAt;
        public ICashFlow CashFlow { get; private set; }
        public Activity Subject { get; private set; }
        public Home FirstHome { get; private set; }
        public Home SecondHome { get; private set; }

        [SetUp]
        public void Init()
        {
            var initiatedAt = new DateTime(2019, 1, 1);
            var downpayment = 82000.00M;
            var mortgageAmount = 328000.00M;
            var mortgageAmountMoney = new Money(mortgageAmount, initiatedAt);
            var preferredInterestRate = 0.0319M;

            var initialCash = new Money(10000.00M, initiatedAt);
            var initialDebt = new Money(5000.00M, initiatedAt);

            CashFlow = new DummyCashFlow(89.86M);
            Subject = new Activity(initialCash, initialDebt, CashFlow, initiatedAt);

            {
                var purchasedAt = initiatedAt;
                var mortgage = new FixedRateMortgage(
                    mortgageAmountMoney,
                    preferredInterestRate,
                    300,
                    purchasedAt
                );
                FirstHome = new Home(
                    "first home",
                    purchasedAt,
                    new Money(downpayment + mortgageAmountMoney, purchasedAt),
                    new Money(downpayment, purchasedAt),
                    mortgage
                );

                Subject.Buy(FirstHome, purchasedAt);
                Subject.Buy(FirstHome.Financing, purchasedAt);
            }

            // Sell the first home
            {
                var soldAt = new DateTime(2020, 1, 3);
                Subject.Sell(FirstHome, new Money(500000.00M, soldAt), soldAt);
                var leftOverMortgageBalance = 0 - FirstHome.Financing.GetBalance(soldAt).Value;
                Subject.Sell(
                    FirstHome.Financing,
                    new Money(leftOverMortgageBalance, soldAt),
                    soldAt
                );
            }

            {
                var purchasedAt = new DateTime(2020, 2, 3);
                var mortgage = new FixedRateMortgage(
                    mortgageAmountMoney,
                    preferredInterestRate,
                    300,
                    purchasedAt
                );
                SecondHome = new Home(
                    "second home",
                    purchasedAt,
                    new Money(downpayment + mortgageAmountMoney, purchasedAt),
                    new Money(downpayment, purchasedAt),
                    mortgage
                );
                Subject.Buy(SecondHome, purchasedAt);
                Subject.Buy(mortgage, purchasedAt);
            }
        }

        [TestCase(2019, 1, 1, 2019, 1, 1, 0.00)]
        [TestCase(2019, 1, 1, 2019, 1, 2, -82000 + 89.86 * 1)]
        [TestCase(2019, 1, 1, 2019, 1, 31, -82000 + 89.86 * 30)]
        [TestCase(2019, 1, 1, 2019, 2, 28, -82000 + 89.86 * 58)]
        [TestCase(2019, 2, 1, 2019, 2, 28, 0 + 89.86 * 27)]
        [TestCase(2019, 2, 1, 2020, 1, 4, 0 + 89.86 * 337 + 500000 - 318588.78)]
        [TestCase(2019, 2, 1, 2020, 2, 3, 0 + 89.86 * 367 + 500000 - 318588.78)]
        [TestCase(2019, 2, 1, 2020, 2, 4, 0 + 89.86 * 368 + 500000 - 318588.78 - 82000)]
        [TestCase(2020, 2, 1, 2020, 2, 4, 89.86 * 3 - 82000)]
        public void Test_GetCash(int startYear, int startMonth, int startDay,
            int endYear, int endMonth, int endDay,
            decimal expected)
        {
            var startAt = new DateTime(startYear, startMonth, startDay);
            var endAt = new DateTime(endYear, endMonth, endDay);

            Assert.That(
                Subject.GetCash(Inflations.GetInflation(InflationTypes.NoopInflation), startAt, endAt),
                Is.EqualTo(expected)
            );
        }
    }
}
