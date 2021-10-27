using Microsoft.AspNetCore.Components;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Emit;

namespace SmBlazor
{
    public class Settings
    {
        public string Name { get; set; }
        public int FirstTopCount { get; set; } = 20;
        public int Height { get; set; } = 200;
        public string Search { get; set; } = "";
        public int Cursor { get; set; } = 0;
        public Columns Columns { get; set; } = new Columns();
        public Order Order { get; set; } = new Order();
        public Dictionary<string, string?> FilterValues { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        public ODataSource DataSource { get; set; } = null!;
        public StyleSettings StyleSettings { get; set; } = StyleSettingsHelper.GetDefaultStyleSettings();
        public Settings(string name)
        {
            Name = name;
        }
    }

}
