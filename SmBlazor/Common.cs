using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmBlazor
{
    public static class Common
    {
        

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
        

    }
}
