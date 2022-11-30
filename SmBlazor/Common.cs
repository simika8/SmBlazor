using Microsoft.JSInterop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmBlazor
{
    public static class Common
    {
        public static JsonSerializerOptions smJso = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };


        public static async ValueTask<T[]> WhenAll<T>(params ValueTask<T>[] tasks)
        {
            // We don't allocate the list if no task throws
            List<Exception>? exceptions = null;

            var results = new T[tasks.Length];
            for (var i = 0; i < tasks.Length; i++)
                try
                {
                    results[i] = await tasks[i].ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>(tasks.Length);
                    exceptions.Add(ex);
                }

            return exceptions is null
                ? results
                : throw new AggregateException(exceptions);
        }

        public static bool BlazorIsWasm(IJSRuntime jsRuntime)
        {
            var res = jsRuntime is IJSInProcessRuntime;
            return res;
        }


        public static async Task<T> GetFromJsonAsync<T>(IJSRuntime jsRuntime, HttpClient http, string pathFromWwwRoot)
        {
            T? res;
            if (BlazorIsWasm(jsRuntime))
            {
                res = await http.GetFromJsonAsync<T>(pathFromWwwRoot, smJso);
            } else
            {
                var jsonString = await File.ReadAllTextAsync("wwwroot/" + pathFromWwwRoot);
                res = JsonSerializer.Deserialize<T>(jsonString, smJso);

                //return await http.GetFromJsonAsync<T>("https://localhost:7153/" + pathFroWwwwRoot);
            }
            if (res != null)
                return res;
            else
                throw new FileNotFoundException($"File not found: { pathFromWwwRoot }. BlazorIsWasm : {BlazorIsWasm(jsRuntime)}");
        }


    }
}
