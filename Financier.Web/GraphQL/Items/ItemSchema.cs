using GraphQL;
using GraphQL.Types;

namespace Financier.Web.GraphQL.Items
{
    public class ItemSchema : Schema
    {
        public ItemSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<ItemQuery>();
            Mutation = resolver.Resolve<ItemMutation>();
        }
    }
}
