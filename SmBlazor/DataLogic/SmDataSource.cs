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
    public class SmDataSource
    {
        private string ApiBaseUri { get; set; } = null!;
        private string ApiNameUri { get; set; } = null!;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public SmDataSource(string apiBaseUri, string apiNameUri)
        {
            ApiBaseUri = apiBaseUri;
            ApiNameUri = apiNameUri;
        }

        public async Task<List<dynamic?>> GetRows(SmQueryOptions qo, Dictionary<string, string>? extraParams)
        {
            var qoUri = SmQueryOptionsUrlHelper.CalculateURL(qo, extraParams);
            ;
            CancelAllRuningQueries();

            using var http = new HttpClient();

            //Runs query
            var res = new List<dynamic?>();


            var url = ApiBaseUri + "/" + ApiNameUri + qoUri;
            var temp = await http.GetStreamAsync(url, cts.Token);
            using var jsd = JsonDocument.Parse(temp);
            JsonElement root = jsd.RootElement;
            foreach (var row in root.EnumerateArray())
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

    }
}
