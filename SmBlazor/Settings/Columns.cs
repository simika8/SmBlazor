using SmQueryOptionsNs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmBlazor
{
    public class Column
    {
        public string[] SplittedFieldName { get; }
        public string FieldName { get; }
        public string? Title { get; set; }
        public int Width { get; set; } = 100;
        public bool Visible { get; set; } = true;
        public bool RightAligned { get; set; }
        public FilterType FilterType { get; set; }
        public Func<object?, object?> CellFormatter { get; set; }
        public Column(string fieldName, Type propertyType)
        {
            FieldName = fieldName;
            RightAligned = ColumnHelper.DefaultRightAligned(propertyType);
            FilterType = propertyType == typeof(string) ? FilterType.StartsWithCaseInsensitive : FilterType.Equals;
            CellFormatter = ColumnHelper.DefaultCellFormatter(propertyType.Name);
            Title = FieldName;
            SplittedFieldName = fieldName.Split(".");

        }
        public Column(Type rowType, string fieldName) : this(fieldName, propertyType: rowType.GetProperty(fieldName)?.PropertyType??typeof(string)) { }
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
    public class Columns: List<Column>
    {

        //protected internal List<Column> ColumnList { get; set; } = new List<Column>();
        protected internal Dictionary<string, Column> ColumnDictionaryByFieldName { get; set; } = new Dictionary<string, Column>(System.StringComparer.OrdinalIgnoreCase);


        public List<Column> VisibleColumns()
        {
            return this.Where(x => x.Visible).ToList();
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
            if (base.Count == 0)
            {
                foreach (var prop in (rowType).GetProperties())
                {
                    base.Add(new Column(prop.Name, prop.PropertyType));
                }
            }
        }
        public new void Add(Column column)
        {
            base.Add(column);
            ColumnDictionaryByFieldName.Add(column.FieldName, column);
        }

    }


    public static class ColumnHelper
    {
        public readonly static HashSet<Type> RightAlignedTypes = new HashSet<Type> { typeof(double), typeof(int), typeof(decimal), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), };
        public static bool DefaultRightAligned(Type type)
        {
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
