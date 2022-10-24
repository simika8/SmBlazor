using Microsoft.AspNetCore.Components;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Emit;

namespace SmBlazor
{
    public class SmGridSettings
    {
        private string idFieldName;

        public string Name { get; set; }
        public int FirstTopCount { get; set; } = 20;
        public int Height { get; set; } = 200;
        public string Search { get; set; } = "";
        public int Cursor { get; set; } = 0;
        public Columns Columns { get; set; } = new Columns();
        public Order Order { get; set; } = new Order();

        public Dictionary<string, string?> FilterValues { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        public StyleSettings StyleSettings { get; set; } = StyleSettingsHelper.GetDefaultStyleSettings();
        public DataSourceSettings DataSourceSettings { get; set; }
        public string IdFieldName { get; set; } = "Id";

    }

    public class DataSourceSettings
    {
        public DataSourceType DataSourceType { get; set; } = DataSourceType.SmQueryOptions;
        public string DataSourceApiBaseUri { get; set; } = null!;
        public string DataSourceApiPathUri { get; set; } = null!;
        public string DataSourceApiNameUri { get; set; } = null!;
        public List<string> DataSourceOdataExpand { get; set; } = new();
    }
    public enum DataSourceType
    {
        SmQueryOptions = 0,
        Odata = 1,
    }

}
