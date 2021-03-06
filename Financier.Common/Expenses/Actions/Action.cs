using System;
using System.Collections.Generic;

using Financier.Common.Models;

namespace Financier.Common.Expenses.Actions
{
    public abstract class Action : IAction
    {
        public Types Type { get; }
        public IProduct Product { get; }
        public abstract decimal TransactionalPrice { get; }
        public virtual DateTime At { get; }

        public abstract bool IsSold { get; }
        public bool IsLastAction => Next.IsNull;
        public abstract bool CanBuy { get; }
        public abstract bool CanSell { get; }
        public virtual bool IsNull => false;

        protected decimal Price { get; }

        protected IAction next = NullAction.Instance;
        public virtual IAction Next
        {
            get
            {
                return next;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        protected Action(Types type, IProduct product, decimal price, DateTime at)
        {
            Type = type;
            Product = product;
            Price = price;
            At = at;
        }

        protected Action(Types type)
        {
            Type = type;
        }

        public IEnumerable<IAction> GetActions()
        {
            for (IAction i = this; !i.IsNull; i = i.Next)
            {
                yield return i;
            }
        }

        public virtual IEnumerable<decimal> GetValueAt(DateTime at)
        {
            yield break;
        }

        public virtual IEnumerable<decimal> GetCostAt(DateTime at)
        {
            yield break;
        }
    }
}
