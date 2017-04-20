using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ItemWebApi;
using Sitecore.ItemWebApi.Pipelines.Request;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sitecore.Support.ItemWebApi.Pipelines.Request
{
    public class ResolveScope : RequestProcessor
    {
        private static bool CanReadItem(Item item)
        {
            if (!item.Access.CanRead())
            {
                return false;
            }
            return ((Sitecore.Context.Site.Name != "shell") || item.Access.CanReadLanguage());
        }

        private static string[] GetAxes()
        {
            string queryString = WebUtil.GetQueryString("scope", null);
            if (queryString == null)
            {
                return new string[] { "s" };
            }
            string[] strArray = queryString.Split(new char[] { '|' });
            Regex regex = new Regex("^[cps]{1}$");
            List<string> list = new List<string>();
            foreach (string str2 in strArray)
            {
                string input = str2.ToLower().Trim();
                if (regex.IsMatch(input) && !list.Contains(input))
                {
                    list.Add(input);
                }
            }
            if (list.Count <= 0)
            {
                return new string[] { "s" };
            }
            return list.ToArray();
        }

        private static IEnumerable<Item> GetItemsByAxe(Item item, string axe)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(axe, "axe");
            switch (axe)
            {
                case "c":
                    return item.GetChildren();

                case "p":
                    if (item.Parent == null)
                    {
                        return new Item[0];
                    }
                    return new Item[] { item.Parent };

                case "s":
                    return new Item[] { item };
            }
            throw new FormatException("Unknown axe value.");
        }

        private static Item[] GetScope(Item[] items)
        {
            Assert.ArgumentNotNull(items, "items");
            if (items.Length == 0)
            {
                Logger.Warn("Cannot resolve the scope because the item set is empty.");
                return new Item[0];
            }
            List<Item> source = new List<Item>();
            string[] axes = GetAxes();
            foreach (Item item in items)
            {
                if (item != null)
                    source.AddRange(GetScope(item, axes));
            }
            return source.Where<Item>(new Func<Item, bool>(ResolveScope.CanReadItem)).ToArray<Item>();
        }

        private static IEnumerable<Item> GetScope(Item item, IEnumerable<string> axes)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(axes, "axes");
            List<Item> list = new List<Item>();
            foreach (string str in axes)
            {
                list.AddRange(GetItemsByAxe(item, str));
            }
            return list.ToArray();
        }

        public override void Process(RequestArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            arguments.Scope = GetScope(arguments.Items);
        }
    }
}