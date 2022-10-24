using SmQueryOptionsNs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmBlazor
{
    public class SmColumn
    {
        private string fieldName;
        private string[] splittedFieldName;
        private string? propertyTypeName;
        private bool? rightAligned;
        private FilterType? filterType;
        private Func<object?, object?>? cellFormatter;

        internal string[] SplittedFieldName { get => splittedFieldName; }
        public string FieldName
        {
            get => fieldName;
            set
            {
                fieldName = value;
                splittedFieldName = fieldName.Split(".");
                Title = Title ?? FieldName;
            }
        }
        public string PropertyTypeName
        {
            get => propertyTypeName??"string";
            set
            {
                propertyTypeName = value;
                rightAligned = rightAligned ?? ColumnHelper.DefaultRightAligned(propertyTypeName);
                FilterType = filterType ?? (propertyTypeName == "string" ? FilterType.StartsWithCaseInsensitive : FilterType.Equals);
                CellFormatter = ColumnHelper.DefaultCellFormatter(propertyTypeName);
            }
        }
        public string? Title { get; set; }
        public int Width { get; set; } = 100;
        public bool Visible { get; set; } = true;
        public bool RightAligned { get => rightAligned ?? false; set => rightAligned = value; }
        public FilterType FilterType { get => filterType ?? FilterType.Equals; set => filterType = value; }
        [System.Text.Json.Serialization.JsonIgnore]
        internal Func<object?, object?> CellFormatter { get => cellFormatter?? ColumnHelper.DefaultCellFormatter(PropertyTypeName); set => cellFormatter = value; }
        public SmColumn(string fieldName, string propertyTypeName)
        {
            FieldName = fieldName;
            PropertyTypeName = propertyTypeName;

            //RightAligned = ColumnHelper.DefaultRightAligned(propertyTypeName);
            //FilterType = propertyTypeName == "string" ? FilterType.StartsWithCaseInsensitive : FilterType.Equals;
            //CellFormatter = ColumnHelper.DefaultCellFormatter(propertyTypeName);
            //Title = FieldName;
            //SplittedFieldName = fieldName.Split(".");

        }
        public SmColumn(Type rowType, string fieldName) : this(fieldName, propertyTypeName: rowType.GetProperty(fieldName)?.PropertyType.Name ?? "string") { }
        public SmColumn() 
        { 
        }
    }
    /*public class Columns
    {

        protected internal List<Column> ColumnList { get; set; } = new List<Column>();
        protected internal Dictionary<string, Column> ColumnDictionaryByFieldName { get; set; } = new Dictionary<string, Column>(System.StringComparer.OrdinalIgnoreCase);


        public List<Column> VisibleColumns()
        {
            return ColumnList.Where(x => x.Visible).ToList();
        }
        public Column? GetColumn(string fieldName)
        {
            if (ColumnDictionaryByFieldName.TryGetValue(fieldName, out var column2))
                return column2;
            else
                return null;

        }

        public Column? GetIdField()
        {
            var res = GetColumn("Id");

            return res;
        }

        public void AddAllColumnsByRowType(Type rowType)
        {
            //ha nincsenek oszlopok megadva akkor az osztály típus alapján elkészítem.
            if (ColumnList.Count == 0)
            {
                foreach (var prop in (rowType).GetProperties())
                {
                    ColumnList.Add(new Column(prop.Name, prop.PropertyType));
                }
            }
        }
        public void Add(Column column)
        {
            ColumnList.Add(column);
            ColumnDictionaryByFieldName.Add(column.FieldName, column);
        }

    }*/
    public class Columns: List<SmColumn>
    {
        protected internal Dictionary<string, SmColumn> ColumnDictionaryByFieldName { get; set; } = new Dictionary<string, SmColumn>(System.StringComparer.OrdinalIgnoreCase);


        public List<SmColumn> VisibleColumns()
        {
            return this.Where(x => x.Visible).ToList();
        }
        public SmColumn? GetColumn(string fieldName)
        {
            if (ColumnDictionaryByFieldName.TryGetValue(fieldName, out var column2))
                return column2;
            else
                return null;

        }

        public SmColumn? GetIdField()
        {
            var res = GetColumn("Id");

            return res;
        }

        public void AddAllColumnsByRowType(Type rowType)
        {
            //ha nincsenek oszlopok megadva akkor az osztály típus alapján elkészítem.
            if (base.Count == 0)
            {
                foreach (var prop in (rowType).GetProperties())
                {
                    base.Add(new SmColumn(prop.Name, prop.PropertyType.Name));
                }
            }
        }
        public new void Add(SmColumn column)
        {
            base.Add(column);
            ColumnDictionaryByFieldName.Add(column.FieldName, column);
        }

    }


    public static class ColumnHelper
    {
        public readonly static HashSet<Type> RightAlignedTypes = new HashSet<Type> { typeof(double), typeof(int), typeof(decimal), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), };
        public static bool DefaultRightAligned(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                return false;
            var nullableType = Nullable.GetUnderlyingType(type) ?? typeof(string);
            var res = RightAlignedTypes.Contains(type) || RightAlignedTypes.Contains(nullableType);
            return res;
        }
        public static Func<object?, object?> DefaultCellFormatter(string propertyTypeName)
        {
            switch (propertyTypeName)
            {
                case nameof(Double):
                    return x => GetDouble(x?.ToString(), 0);
                case nameof(DateTime):
                    return x => GetDate(x?.ToString());
                default:
                    return x => x;
            };
        }
        public static double GetDouble(string? value, double defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            double result;

            // Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                // Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                // Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }
            return result;
        }


        public static DateTime? GetDate(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
            /*
            string[] formats = { 
            // Basic formats
            "yyyyMMddTHHmmsszzz",
            "yyyyMMddTHHmmsszz",
            "yyyyMMddTHHmmssZ",
            // Extended formats
            "yyyy-MM-ddTHH:mm:sszzz",
            "yyyy-MM-ddTHH:mm:sszz",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.zzZ",
            // All of the above with reduced accuracy
            "yyyyMMddTHHmmzzz",
            "yyyyMMddTHHmmzz",
            "yyyyMMddTHHmmZ",
            "yyyy-MM-ddTHH:mmzzz",
            "yyyy-MM-ddTHH:mmzz",
            "yyyy-MM-ddTHH:mmZ",
            // Accuracy reduced to hours
            "yyyyMMddTHHzzz",
            "yyyyMMddTHHzz",
            "yyyyMMddTHHZ",
            "yyyy-MM-ddTHHzzz",
            "yyyy-MM-ddTHHzz",
            "yyyy-MM-ddTHHZ",
            "s",
            };
            
            
            return DateTime.ParseExact(str, formats,
                CultureInfo.InvariantCulture, DateTimeStyles.None);*/
        }

    }
}
