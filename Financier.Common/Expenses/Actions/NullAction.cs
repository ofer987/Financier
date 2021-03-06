using System;

namespace Financier.Common.Expenses.Actions
{
    public class NullAction : Action
    {
        public override bool IsSold => false;
        public override bool CanBuy => true;
        public override bool CanSell => false;
        public override bool IsNull => true;
        public override DateTime At => DateTime.MinValue;
        public override decimal TransactionalPrice => 0.00M;

        public static NullAction Instance = new NullAction();

        private NullAction() : base(Types.Null)
        {
        }
    }
}
