using System;
using System.Linq;
using System.Collections.Generic;

using Financier.Common.Actions;

namespace Financier.Common.Models
{
    public class BalanceSheet
    {
        public List<IAsset> Assets { get; }

        public List<ILiability> Liabilities { get; }

        public decimal Cash { get; }

        public DateTime From { get; }

        public DateTime To { get; }

        public BalanceSheet(IEnumerable<IProduct> products, DateTime from, DateTime to)
        {
            if (to < from)
            {
                throw new Exception($"Balance Sheet cannot be computed in reverse order from ({from}) to ({to})");
            }

            From = from;
            To = to;

            var sortedSoldProducts = products
                .Where(product => product.IsSold)
                .Where(product => product.PurchasedAt >= from)
                .Where(product => product.PurchasedAt <= to)
                .Where(product => product.SoldAt <= to)
                .OrderBy(product => product.PurchasedAt);

            var sortedUnsoldProducts = products
                .Where(product => !product.IsSold)
                .Where(product => product.PurchasedAt >= from)
                .Where(product => product.PurchasedAt <= to)
                .OrderBy(product => product.PurchasedAt);

            var sortedYetToBeSoldProducts = products
                .Where(product => product.IsSold)
                .Where(product => product.PurchasedAt >= from)
                .Where(product => product.PurchasedAt <= to)
                .Where(product => product.SoldAt > to)
                .OrderBy(product => product.PurchasedAt);

            // TODO Figure out how to factor in inflation!
            foreach (var product in sortedSoldProducts)
            {
                Cash -= product.PurchasePrice;
                Cash += product.ValueBy(to);
                Cash -= product.CostBy(to);
            }

            foreach (var product in sortedUnsoldProducts.Concat(sortedYetToBeSoldProducts))
            {
                Cash -= product.PurchasePrice;
                Assets.AddRange(product.Assets);
                Liabilities.AddRange(product.Liabilities);
            }
        }

        public decimal TotalValue()
        {
            var result = Cash;

            var assetValue = Assets
                .Select(asset => asset.ValueBy(To))
                .Aggregate(0.00M, (total, val) => total += val);

            var liabilityCost = Liabilities
                .Select(liability => liability.CostBy(To))
                .Aggregate(0.00M, (total, val) => total += val);

            return Cash + assetValue + liabilityCost;
        }
    }
}
