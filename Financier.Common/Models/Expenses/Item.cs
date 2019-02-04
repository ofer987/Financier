using System;
using System.Text;

namespace Financier.Common.Models.Expenses
{
    public class Item
    {
        public Guid Id { get; set; }

        public Guid StatementId { get; set; }

        public Statement Statement { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime TransactedAt { get; set; }

        public DateTime PostedAt { get; set; }

        public Item(string description, DateTime transactedAt, DateTime postedAt, decimal amount)
        {
            Description = description;
            TransactedAt = transactedAt;
            PostedAt = postedAt;
            Amount = amount;
        }

        public Item()
        {
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Item;
            if (other == null)
            {
                return false;
            }

            if ((Description ?? string.Empty) != (other.Description ?? string.Empty)
                    || Amount != other.Amount 
                    || TransactedAt != other.TransactedAt 
                    || PostedAt != other.PostedAt)
            {
                return false;
            }

            return true;
        }

        public static bool operator ==(Item x, Item y)
        {
            if (object.ReferenceEquals(x, null))
            {
                return (object.ReferenceEquals(y, null));
            }

            return x.Equals(y);
        }

        public static bool operator !=(Item x, Item y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{nameof(Id)}: ({Id})");
            sb.AppendLine($"{nameof(Description)}: ({Description ?? string.Empty})");
            sb.AppendLine($"{nameof(Amount)}: ({Amount})");
            sb.AppendLine($"{nameof(PostedAt)}: ({PostedAt})");
            sb.AppendLine($"{nameof(TransactedAt)}: ({TransactedAt})");

            return sb.ToString();
        }
    }
}
