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
        //public int Cursor { get; set; } = 0;
        public StyleSettings StyleSettings { get; set; } = StyleSettingsHelper.GetDefaultStyleSettings();
        public string IdFieldName { get; set; } = "Id";
        public DataSourceSettings DataSourceSettings { get; set; } = new();
        public Dictionary<string, string> ExtraParams { get; set; } = new();
        public Columns Columns { get; set; } = new Columns();
    }

    public class DataSourceSettings
    {
        public string DataSourceApiBaseUri { get; set; } = null!;
        public string DataSourceApiPathUri { get; set; } = null!;
        public string DataSourceApiNameUri { get; set; } = null!;
        public string DataSourceApiExtraParams { get; set; } = null!;
    }

}
