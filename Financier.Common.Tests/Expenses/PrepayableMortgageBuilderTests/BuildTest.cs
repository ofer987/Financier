using System;
using System.Linq;
using NUnit.Framework;

using Financier.Common.Liabilities;
using Financier.Common.Expenses;
using Financier.Common.Models;

namespace Financier.Common.Tests.Expenses.PrepayableMortgageBuilderTests
{
    public class BuildTest
    {
        public PrepayableMortgageBuilder Subject { get; }
        public FixedRateMortgage BaseMortgage { get; }
        public PrepayableMortgage Result { get; }

        public BuildTest()
        {
            var purchasedAt = new DateTime(2019, 1, 1);
            var mortgageAmount = 328000.00M;
            var mortgageAmountMoney = new Money(mortgageAmount, purchasedAt);
            var preferredInterestRate = 0.0319M;

            BaseMortgage = new FixedRateMortgage(mortgageAmountMoney, preferredInterestRate, 300, purchasedAt);
            Subject = new PrepayableMortgageBuilder(BaseMortgage);
            Result = Subject.Build();
        }

        [TestCase(2019, 1, 1, 0.00)]
        [TestCase(2019, 1, 15, 1584.39)]
        [TestCase(2019, 1, 31, 1584.39)]
        [TestCase(2019, 2, 1, 1584.39)]
        [TestCase(2019, 2, 2, 1584.40)]
        [TestCase(2019, 12, 31, 1584.40)]
        [TestCase(2020, 1, 1, 32800.00)]
        [TestCase(2020, 1, 2, 1584.40)]
        [TestCase(2020, 2, 1, 1584.40)]
        [TestCase(2020, 12, 31, 1584.40)]
        [TestCase(2021, 1, 1, 32800)]
        [TestCase(2021, 1, 2, 1584.40)]
        [TestCase(2026, 4, 1, 1584.40)]
        [TestCase(2026, 5, 1, 1584.40)]
        [TestCase(2026, 6, 1, 1584.40)]
        [TestCase(2026, 7, 1, 45.93)]
        [TestCase(2026, 10, 1, 45.93)]
        public void Test_GetLatestPayment_IsAmount(int year, int month, int day, decimal expected)
        {
            Assert.That(
                Result.GetMonthlyPayments(new DateTime(year, month, day))
                    .Select(payment => payment.Amount)
                    .Select(amount => amount)
                    .Last(),
                Is.EqualTo(expected)
            );
        }

        [TestCase(2019, 1, 2, 2019, 1, 1)]
        [TestCase(2026, 5, 1, 2026, 4, 1)]
        [TestCase(2026, 6, 1, 2026, 5, 1)]
        [TestCase(2026, 7, 1, 2026, 6, 1)]
        [TestCase(2026, 10, 1, 2026, 6, 1)]
        public void Test_GetLatestPayment_IsLatestDate(int year, int month, int day, int latestYear, int latestMonth, int latestDay)
        {
            Assert.That(
                Result.GetMonthlyPayments(new DateTime(year, month, day))
                    .Select(payment => payment.At)
                    .Last(),
                Is.EqualTo(new DateTime(latestYear, latestMonth, latestDay))
            );
        }
    }
}
