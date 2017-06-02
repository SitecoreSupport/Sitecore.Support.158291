using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.Data.Items;
using Sitecore.ItemWebApi.Pipelines.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Sitecore.Support.ItemWebApi.Pipelines.Search
{
    public class FilterNullItems : DefinitionBasedSearchProcessor
    {
        public override void Process(SearchArgs args)
        {
            int numberOfMedia = args.Queryable.GetResults<ConvertedSearchResultItem>().TotalSearchResults;

            List<string> ids = new List<string>();
            List<Item> items = new List<Item>();

            SearchResults<ConvertedSearchResultItem> results = args.Queryable.Page(0, numberOfMedia).GetResults<ConvertedSearchResultItem>();
            items.AddRange(from s in results.Hits select s.Document.GetItem());

            foreach (Item i in items)
            {
                if (i != null)
                {
                    ids.Add(i.ID.ToShortID().ToString().ToLower());
                }
            }

            Expression<Func<ConvertedSearchResultItem, bool>> expression = null;

            foreach (string id in ids)
            {
                Expression<Func<ConvertedSearchResultItem, bool>> expression2 = (ConvertedSearchResultItem i) => i.Id == id;
                expression = ((expression == null) ? expression2 : expression.Or(expression2));
            }

            args.Queryable = args.Queryable.Where<ConvertedSearchResultItem>(expression);
        }
    }
}