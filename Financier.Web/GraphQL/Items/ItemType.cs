using GraphQL.DataLoader;
using GraphQL.Types;

using Financier.Common.Expenses.Models;

namespace Financier.Web.GraphQL.Items
{
    public class ItemType : ObjectGraphType<Item>
    {
        public ItemType(IDataLoaderContextAccessor dataLoaderAccessor)
        {
            Field(t => t.Id, nullable: false, type: typeof(IdGraphType));
            Field(t => t.Description);
            Field(t => t.TransactedAt);
            Field(t => t.PostedAt);
            Field(t => t.TheRealAmount);
            Field(t => t.ItemId);
        }
    }
}