using System;
using System.Collections.Generic;
using Financier.Common.Expenses.Models;

namespace Financier.Common
{
    public partial class Factories
    {
        public static Statement GetSimpleStatement(Card card)
        {
            return new Statement 
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                PostedAt = new DateTime(2019, 1, 1),
                Items = new List<Item>()
            };
        }
    }
}

