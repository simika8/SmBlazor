using System.Collections;
using System.Linq.Expressions;

namespace Common
{
    public static class Others
    {
        


        public static Guid GetGuidKey<T>(T o)
        {
            return (Guid)(o?.GetType().GetProperty("Id")?.GetValue(o) ?? Guid.Empty);
        }

        public static string GetSearchField(string requestQueryString)
        {
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(requestQueryString);
            if (queryDictionary == null)
                return "";
            else
                return GetSearchField(queryDictionary);
        }
        public static string GetSearchField(Dictionary<string, Microsoft.Extensions.Primitives.StringValues> queryDictionary)
        {
            if (queryDictionary.TryGetValue("search", out var search))
            {
                return search;
            }


            queryDictionary.TryGetValue("$search", out var dollarsearch);
            var res = dollarsearch.FirstOrDefault() ?? "";
            //syncfusion faszságainak javítása: (az összes múltbeli keresést felsorolja orral összekötve. utolsót használom csak.)
            res = res.Split("OR ").Last();
            return res;
        }
        public static bool IsCollection(Type type)
        {
            var interfaces = type.GetInterfaces();
            var nonStringEnumerable = interfaces.Contains(typeof(IEnumerable))
                && (type != typeof(string));
            return nonStringEnumerable;
        }


    }


}
