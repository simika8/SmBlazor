using Microsoft.AspNetCore.Components;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Emit;

namespace SmBlazor
{
    public class GridData
    {
        public List<(int Index, dynamic? Row)>? Rows { get; set; }

        public async Task ReQuery(Settings settings)
        {
            var top = settings.FirstTopCount;
            Rows = null;
            var newRows = await GetRows(settings, top);
            AddToRows(newRows);
        }
        public void AddToRows(List<dynamic?> newRows)
        {
            if (Rows == null)
                Rows = new List<(int, dynamic?)>();
            var oldRowsCount = Rows.Count;
            for (int i = 0; i < newRows.Count; i++)
            {
                var row = newRows[i];
                Rows.Add(new(i + oldRowsCount, row));
            }
        }
        public async Task LoadMoreRecords(Settings settings)
        {
            var origRecCount = Rows?.Count??0;
            var redundantRecordCount = Math.Max((int)(origRecCount * 0.03), 1);
            var NeededNewLines = Math.Max(origRecCount, 1);
            var top = NeededNewLines + redundantRecordCount;
            var skip = origRecCount - redundantRecordCount;

            var newRows = await GetRows(settings, top, skip);


            var idField = settings.Columns.GetIdField();
            if (CanMerge(Rows, newRows, redundantRecordCount, idField, out var trueRedundantsCount))
            {
                var nonRedundantNewRows = newRows.Skip(trueRedundantsCount);
                AddToRows(nonRedundantNewRows.ToList());
                /*if (Rows == null)
                    Rows = nonRedundantNewRows.ToList();
                else
                    Rows.AddRange(nonRedundantNewRows);*/
            }
            else
            {
                //ha nem találtam redundáns sorokat a régi, és az új lista között,akkor túl sokat változott a lista, letöltöm az egészet újból.
                Rows = null;
                var newRows2 = await GetRows(settings, NeededNewLines + origRecCount);
                AddToRows(newRows2);
            }
           
        }
        private static async Task<List<dynamic?>> GetRows(Settings settings, int top, int? skip = null)
        {
            var qo = SmQueryOptionsHelper.CreateSmQueryOptions(settings, top, skip);
            var res = await settings.DataSource.GetRows(qo);
            return res;
        }

        private static bool CanMerge(List<(int id, dynamic? row)>? origRows, List<dynamic?> newRows, int redundantRecordCount, Column? idField, out int trueRedundantsCount)
        {
            if (origRows == null)
            {
                throw new Exception("MergeRows: origRows == null");
            }

            if (redundantRecordCount == 0)
            {
                throw new Exception("MergeRows: redundantRecordCount == 0");
            }


            var origRedundantRows = origRows.TakeLast(redundantRecordCount).Select(x => GetIdProperty(x.row, idField)).ToList();
            var newRedundantRows = newRows.Take(redundantRecordCount).Select(x => GetIdProperty(x, idField)).ToList();
            trueRedundantsCount = 0;
            foreach (var orow in origRedundantRows)
            {
                Console.WriteLine(orow);

                foreach (var nrow in origRedundantRows)
                {
                    Console.WriteLine(nrow);
                    if (orow.Equals(nrow))
                        trueRedundantsCount++;
                }
            }

            if (trueRedundantsCount == 0)
                return false;

            return true;
        }


        private static object? GetIdProperty(dynamic? row, Column idField)
        {
            var res = Get1LevelPropertyValue(row, idField.FieldName);
            return res;
        }

        /// <summary>
        /// visszaadja egy objektum propertyjét property név alapján
        /// </summary>
        /// <param name="src"></param>
        /// <param name="splittedFieldName"></param>
        /// <returns></returns>
        public static object? GetPropertyValue(object? src, string[] splittedFieldName)
        {
            if (splittedFieldName.Length > 1)
            {
                var firstLevelName = splittedFieldName.First();
                //var firstLevelProp = src?.GetType()?.GetProperty(firstLevelName)?.GetValue(src, null);
                var firstLevelProp = Get1LevelPropertyValue(src, firstLevelName);
                var remainingLevelNames = splittedFieldName.Skip(1).ToArray();
                var lastLevelProp = GetPropertyValue(firstLevelProp, remainingLevelNames);

                return lastLevelProp;
            }
            else
            {
                return Get1LevelPropertyValue(src, splittedFieldName.First());
            }
        }
        public static object? Get1LevelPropertyValue(object? src, string fieldName)
        {
            if (src is System.Text.Json.JsonElement)
            {
                var jse = (System.Text.Json.JsonElement)src;

                var property = jse.EnumerateObject()
                                      .FirstOrDefault(p => string.Compare(p.Name, fieldName,
                                                                          StringComparison.OrdinalIgnoreCase) == 0);

                var res = property.Value;

                //((System.Text.Json.JsonElement)src).TryGetProperty(fieldName.ToLowerInvariant(), out var res);
                return res;
            }
            else
            {
                var res = src?.GetType()?.GetProperty(fieldName)?.GetValue(src, null);
                return res;
            }

        }
    }




}
