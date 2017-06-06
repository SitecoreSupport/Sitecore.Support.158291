using Sitecore.ItemWebApi.Pipelines.Request;
using System.Linq;

namespace Sitecore.Support.ItemWebApi.Pipelines.Request
{
    public class FilterNullItems : RequestProcessor
    {
        public override void Process(RequestArgs args)
        {
            args.Items = args.Items.Where(i => i != null).ToArray();
        }       
    }
}