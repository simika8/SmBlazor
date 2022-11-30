using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmQueryOptionsNs
{
    public static class Mapper
    {
        public static void CopyProperties<T, TU>(T source, TU dest, bool copyOnlyNonNullFields = false, bool copyCollections = true, HashSet<string>? fieldnames = null)
        {

            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                var toCopy = (fieldnames?.Contains(sourceProp.Name.ToLower()) ?? true);
                if (toCopy && destProps.Any(x => x.Name == sourceProp.Name))
                {
                    //var isCollection = sourceProp.PropertyType.Name.Contains("ICollection");
                    var isCollection = IsICollectionOfT(sourceProp.PropertyType);
                    var isEnumerable = IsIEnumerableOfT(sourceProp.PropertyType);
                    var isValueType = sourceProp.PropertyType.IsValueType || (sourceProp.PropertyType.Equals(typeof(string)));
                    var destProp = destProps.First(x => x.Name == sourceProp.Name);

                    var sourceValue = sourceProp.GetValue(source, null);
                    var destValue = destProp.GetValue(dest, null);
                    var propty = destProp.PropertyType;

                    var ut1 = destProp.PropertyType.IsNullableType() ? Nullable.GetUnderlyingType(destProp.PropertyType): destProp.PropertyType;
                    var ut2 = sourceProp.PropertyType.IsNullableType() ? Nullable.GetUnderlyingType(sourceProp.PropertyType) : sourceProp.PropertyType;

                    //if (destProp.PropertyType != sourceProp.PropertyType)
                    if (ut1 != ut2)
                    {
                        CopyProperties(sourceProp, destProp, copyOnlyNonNullFields, copyCollections);
                    }
                    else
                    {

                        if (isCollection)
                        {
                            if (copyCollections)
                            {
                                var list = (IEnumerable<dynamic>)sourceValue;
                                foreach (var item in (IEnumerable<dynamic>)sourceValue)
                                {
                                    ;
                                }
                                CopyProperties(sourceProp.GetValue(source, null), destProp.GetValue(dest, null), copyOnlyNonNullFields, copyCollections);
                            }
                        }
                        else
                        {
                            var sourceName = sourceProp.Name;
                            bool hasToCopy = (sourceValue == null) ^ (destValue == null);
                            hasToCopy = hasToCopy || (sourceValue != null && !sourceValue.Equals(destValue));
                            if (copyOnlyNonNullFields)
                            {
                                var def = GetDefaultValue(destProp.PropertyType);
                                var isdefault = sourceValue?.Equals(def) ?? true;
                                hasToCopy = hasToCopy && !isdefault;
                            }


                            if (destProp.CanWrite && hasToCopy)
                            {

                                destProp.SetValue(dest, sourceValue, null);
                            }
                        }
                    }
                }

            }
        }
        public static bool IsICollectionOfT(Type type)
        {
            var res = type.GetInterfaces().Any(x => x.IsGenericType
                   && x.GetGenericTypeDefinition() == typeof(ICollection<>));
            return res;
        }

        public static bool IsIEnumerableOfT(Type type)
        {
            var res = type.GetInterfaces().Any(x => x.IsGenericType
                   && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return res;
        }
        static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }

    }
}
