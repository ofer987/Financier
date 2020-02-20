using System;
using System.Collections.Generic;
using System.Linq;

using Financier.Common.Extensions;
using Financier.Common.Models;
using Financier.Common.Expenses.Actions;

namespace Financier.Common.Expenses
{
    public class BalanceSheet
    {
        public DateTime InitiatedAt { get; set; }
        public Money InitialCash { get; set; } = Money.Zero;
        public Money InitialDebt { get; set; } = Money.Zero;
        public ICashFlow CashFlow { get; set; }
        public decimal DailyProfit => CashFlow.DailyProfit;
        public Dictionary<DateTime, IList<Money>> CashAdjustments = new Dictionary<DateTime, IList<Money>>();

        // private List<Home> homes { get; } = new List<Home>();
        // public IReadOnlyList<Home> Homes => homes.AsReadOnly();

        public Activity ProductHistory { get; } = new Activity();

        public BalanceSheet(ICashFlow cashFlow, DateTime initiatedAt)
        {
            CashFlow = cashFlow;
            InitiatedAt = initiatedAt;
        }

        public BalanceSheet(Money cash, Money debt, ICashFlow cashFlow, DateTime initiatedAt)
        {
            InitiatedAt = initiatedAt;
            InitialCash = cash;
            InitialDebt = debt;

            CashFlow = cashFlow;
        }

        public void Buy(Home home)
        {
            if (home.PurchasedAt < InitiatedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(home.PurchasedAt), home.PurchasedAt, $"The home's PurchasedAt cannot be before {InitiatedAt}");
            }

            ProductHistory.Buy(home, home.PurchasedAt);
        }

        public void Sell(Home home, Money salePrice, DateTime soldAt)
        {
            if (soldAt < InitiatedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(soldAt), soldAt, $"The home cannot be sold before before {InitiatedAt}");
            }

            // TODO: complete later
            ProductHistory.Sell(home, salePrice, soldAt);
        }

        public void AddCashAdjustment(DateTime at, Money cash)
        {
            if (!CashAdjustments.TryGetValue(at, out var list))
            {
                CashAdjustments.Add(at, new List<Money> { cash });
            }
            else
            {
                list.Add(cash);
            }
        }

        public IEnumerable<Product> GetOwnedProducts(DateTime at)
        {
            return ProductHistory.GetOwnedProducts(at);
        }

        public decimal GetAssets(IInflation inflation, DateTime at)
        {
            if (at < InitiatedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(at), $"Should be at or later than {InitiatedAt}");
            }

            var result = 0.00M;
            // TODO: add cash adjustments
            result += InitialCash.GetValueAt(inflation, at);
            result += CashFlow.DailyProfit * at.Subtract(InitiatedAt).Days;

            result += CashAdjustments
                .SelectMany(pair => pair.Value)
                .InflatedValue(inflation, at);

            foreach (var action in ProductHistory.GetHistories().SelectMany(history => history))
            {
                switch (action.Type)
                {
                    case Types.Purchase:
                        result -= action.Price.GetValueAt(inflation, at).Value;
                        result += action.Product.GetValueAt(at)
                            .InflatedValue(inflation, at);
                        break;
                    case Types.Sale:
                        result += action.Price.GetValueAt(inflation, at).Value;
                        break;
                    case Types.Null:
                        break;
                }
            }

            result += GetValueOfOwnedProducts(inflation, at);

            return decimal.Round(result, 2);
        }

        public decimal GetValueOfOwnedProducts(IInflation inflation, DateTime at)
        {
            return GetOwnedProducts(at)
                .SelectMany(product => product.GetValueAt(at))
                .InflatedValue(inflation, at);
        }

        public decimal GetCostOfOwnedProducts(IInflation inflation, DateTime at)
        {
            return GetOwnedProducts(at)
                .SelectMany(product => product.GetCostAt(at))
                .InflatedValue(inflation, at);
        }

        public decimal GetLiabilities(IInflation inflation, DateTime at)
        {
            if (at < InitiatedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(at), $"Should be at or later than {InitiatedAt}");
            }

            var result = 0.00M;
            result += InitialDebt.GetValueAt(inflation, at).Value;

            foreach (var action in ProductHistory.GetHistories().SelectMany(history => history))
            {
                switch (action.Type)
                {
                    case Types.Purchase:
                        // result += action.Product.GetCostAt(at)
                        //     .Total(inflation, at);
                        break;
                    case Types.Sale:
                        // TODO: Pay off debts
                        // Convert soldPrice to 1) Assets (add to cash), and
                        // 2) to liabilities (how much I owe others)
                        // 3) pay off liabilities right away (i.e., remove from cash)
                        // 4) convert remaining liabilities into debt (i.e., a debt product)
                        // result += action.Price.GetValueAt(inflation, at).Value;
                        break;
                    case Types.Null:
                        break;
                }
            }

            result += GetCostOfOwnedProducts(inflation, at);

            return decimal.Round(result, 2);
        }

        public decimal GetNetWorth(IInflation inflation, DateTime at)
        {
            if (at < InitiatedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(at), $"Should be at or later than {InitiatedAt}");
            }

            var result = 0.00M;
            result += GetAssets(inflation, at);
            result -= GetLiabilities(inflation, at);

            return result;
        }

        public decimal GetCashAt(DateTime at)
        {
            if (at < InitiatedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(at), $"Should be at or later than {InitiatedAt}");
            }

            return 0.00M
                + InitialCash.Value
                - InitialDebt
                + CashFlow.DailyProfit * at.Subtract(InitiatedAt).Days;
        }
    }
}
