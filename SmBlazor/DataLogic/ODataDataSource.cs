using SmQueryOptionsNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmBlazor
{
    public class ODataDataSource: IDataSource
    {
        private string ApiBaseUri { get; set; } = null!;
        private string ApiNameUri { get; set; } = null!;
        private List<string> Expand { get; set; } = null!;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public ODataDataSource(string apiBaseUri, string apiNameUri, List<string> expand)
        {
            ApiBaseUri = apiBaseUri;
            ApiNameUri = apiNameUri;
            Expand = expand;
        }

        public async Task<List<dynamic?>> GetRows(SmQueryOptions qo)
        {
            var url = CalculateURL(qo);

            CancelAllRuningQueries();

            using var http = new HttpClient();

            //Runs query
            var res = new List<dynamic?>();



            var temp = await http.GetStreamAsync(ApiBaseUri + "\\" + ApiNameUri + url, cts.Token);

            using var jsd = JsonDocument.Parse(temp);
            JsonElement root = jsd.RootElement;
            root.TryGetProperty("value", out var rows);
            foreach (var row in rows.EnumerateArray())
            {
                var rowClone = row.Clone();
                res.Add(rowClone);
                //var rowClonestring = rowClone.ToString();

            }
            return res;

        }

        private void CancelAllRuningQueries()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
        }
        private string CalculateURL(SmQueryOptions qo)
        {
            var odataParams = new Dictionary<string, string?>();


            odataParams["$search"] = qo.Search;
            odataParams["$top"] = qo.Top?.ToString();
            odataParams["$skip"] = qo.Skip?.ToString();
            odataParams["$select"] = CalculateSelectString(qo.Select);
            odataParams["$expand"] = CalculateExpand(qo, Expand);


            var url = string.Join("&", odataParams.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => $"{x.Key}={x.Value}"));

            if (!string.IsNullOrEmpty(url))
                url = "?" + url;
            return url;
        }

        private static string? CalculateSelectString(HashSet<string>? columFieldNames)
        {
            if (columFieldNames == null)
                return null;
            var res = string.Join(",", (columFieldNames??new HashSet<string>()).Where(x => !x.Contains(".")));
            return res;
        }
        private static string CalculateExpand(SmQueryOptions qo, List<string> expand)
        {
            var res = string.Join(",", expand ?? new List<string>());
            return res;
        }


    }
}
