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
        public DataSourceSettings DataSourceSettings { get; set; }
        public IDataSource DataSource { get; set; }
        private int _queryIdGenerator = 0;
        private bool _moreRecordsLoading = false;

        private void InitDataSource(DataSourceSettings dataSourceSettings)
        {
            DataSourceSettings = dataSourceSettings;
            switch (DataSourceSettings.DataSourceType)
            {
                case DataSourceType.SmQueryOptions:
                    DataSource = new SmQueryOptionsDataSource(DataSourceSettings.DataSourceApiBaseUri + DataSourceSettings.DataSourceApiPathUri, DataSourceSettings.DataSourceApiNameUri);
                    break;
                case DataSourceType.Odata:
                    DataSource = new ODataDataSource(DataSourceSettings.DataSourceApiBaseUri + DataSourceSettings.DataSourceApiPathUri, DataSourceSettings.DataSourceApiNameUri, DataSourceSettings.DataSourceOdataExpand);
                    break;
                default:
                    throw new NotSupportedException();

            }
        }

        public async Task ReQuery(SmGridSettings settings)
        {
            InitDataSource(settings.DataSourceSettings);
            var top = settings.FirstTopCount;
            Rows = null;
            var newRows = await GetRows(settings, top);
            if (newRows == null)
                return;

            Rows = null;
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
        public async Task LoadMoreRecords(SmGridSettings settings)
        {
            if (_moreRecordsLoading)
            {
                await Task.Delay(1);
                return;

            }

            try
            {
                _moreRecordsLoading = true;
                var origRecCount = Rows?.Count ?? 0;
                var redundantRecordCount = Math.Max((int)(origRecCount * 0.03), 1);
                var NeededNewLines = (int)(Math.Max(origRecCount, 1)*0.2+ settings.FirstTopCount);
                var top = NeededNewLines + redundantRecordCount;
                var skip = origRecCount - redundantRecordCount;

                var newRows = await GetRows(settings, top, skip);
                if (newRows == null)
                    return;



                if (CanMerge(Rows, newRows, redundantRecordCount, settings.IdFieldName, out var trueRedundantsCount))
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
                    if (newRows2 == null)
                        return;

                    AddToRows(newRows2);
                }
            } finally
            {
                _moreRecordsLoading = false;
            }
           
        }
        private async Task<List<dynamic?>?> GetRows(SmGridSettings settings, int top, int? skip = null)
        {
            int queryId = Interlocked.Increment(ref _queryIdGenerator);

            var qo = SmQueryOptionsHelper.CreateSmQueryOptions(settings, top, skip);
            var res = await DataSource.GetRows(qo);

            var queryIsObsolete = !Equals(_queryIdGenerator, queryId);
            if (queryIsObsolete)
                return null;


            return res;
        }

        private static bool CanMerge(List<(int id, dynamic? row)>? origRows, List<dynamic?> newRows, int redundantRecordCount, string idFieldName, out int trueRedundantsCount)
        {
            if (origRows == null)
            {
                throw new Exception("MergeRows: origRows == null");
            }

            if (redundantRecordCount == 0)
            {
                throw new Exception("MergeRows: redundantRecordCount == 0");
            }


            var origRedundantRows = origRows.TakeLast(redundantRecordCount).Select(x => GetIdProperty(x.row, idFieldName)).ToList();
            var newRedundantRows = newRows.Take(redundantRecordCount).Select(x => GetIdProperty(x, idFieldName)).ToList();
            trueRedundantsCount = 0;
            foreach (var orow in origRedundantRows)
            {
                //Console.WriteLine(orow);

                foreach (var nrow in origRedundantRows)
                {
                    //Console.WriteLine(nrow);
                    if (orow.Equals(nrow))
                        trueRedundantsCount++;
                }
            }

            if (trueRedundantsCount == 0)
                return false;

            return true;
        }


        private static object? GetIdProperty(dynamic? row, string idFieldName)
        {
            var res = Get1LevelPropertyValue(row, idFieldName);
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
