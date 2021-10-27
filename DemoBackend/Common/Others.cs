using Microsoft.AspNetCore.OData.Query.Wrapper;
using System.Collections;

namespace Common
{
    public static class Others
    {
        public static Guid GetGuidKey<T>(T o)
        {
            return (Guid)(o?.GetType().GetProperty("Id")?.GetValue(o) ?? Guid.Empty);
        }

        public static string GetSearchField(string requestQueryString)
        {
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(requestQueryString);
            if (queryDictionary == null)
                return "";
            else
                return GetSearchField(queryDictionary);
        }
        public static string GetSearchField(Dictionary<string, Microsoft.Extensions.Primitives.StringValues> queryDictionary)
        {
            if (queryDictionary.TryGetValue("search", out var search))
            {
                return search;
            }


            queryDictionary.TryGetValue("$search", out var dollarsearch);
            var res = dollarsearch.FirstOrDefault() ?? "";
            //syncfusion faszságainak javítása: (az összes múltbeli keresést felsorolja orral összekötve. utolsót használom csak.)
            res = res.Split("OR ").Last();
            return res;
        }
        public static bool IsCollection(Type type)
        {
            var interfaces = type.GetInterfaces();
            var nonStringEnumerable = interfaces.Contains(typeof(IEnumerable))
                && (type != typeof(string));
            return nonStringEnumerable;
        }

        /// <summary>
        /// Cleans Odata Wrapper shits, converts to typed collection
        /// </summary>
        public static IEnumerable<T> ToTypedCollection<T>(this IEnumerable<dynamic>? queryResult) where T : class
        {
            var res = new List<T>();
            if (queryResult == null)
                return res;

            foreach (var item in queryResult)
            {
                var itemUw = UnWrappObject(item);
                if (itemUw is T)
                {
                    var model = itemUw as T;
                    res.Add((T)itemUw);
                }
            }

            dynamic? UnWrappObject(object item)
            {
                if (item is null)
                    return null;
                if (IsISelectExpandWrapper(item.GetType()))
                {
                    ISelectExpandWrapper itemSelectExpandWrapper = (ISelectExpandWrapper)item;
                    var instance = itemSelectExpandWrapper.GetType().GetProperty("Instance");
                    var instancePropertyValue = itemSelectExpandWrapper?.GetType()?.GetProperty("Instance")?.GetValue(item, null);
                    var instancePropertyType = instance?.PropertyType;

                    //unshitting v1
                    if (instancePropertyValue != null)
                        return instancePropertyValue;

                    //unshitting v2
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    var dict = itemSelectExpandWrapper.ToDictionary();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    var itemValueType = itemSelectExpandWrapper.GetType();

                    if (instancePropertyType == null)
                        return null;
                    var model = ObjectFromDictionary(instancePropertyType, dict);

                    return model;
                }
                return item;
            }


            bool IsISelectExpandWrapper(Type type)
            {
                var interfaces = type.GetInterfaces();
                var containsISelectExpandWrapper = interfaces.Contains(typeof(ISelectExpandWrapper));
                return containsISelectExpandWrapper;
            }

            dynamic? ObjectFromDictionary(Type type, IDictionary<string, object> dict)
            {
                if (type == null)
                    return null;
                var result = Activator.CreateInstance(type);
                foreach (var item in dict)
                {
                    if (item.Value == null)
                        continue;
                    var isCollection = IsCollection(item.Value.GetType());
                    var isWrapper = IsISelectExpandWrapper(item.Value.GetType());
                    if (isWrapper)
                    {
                        var model = UnWrappObject(item.Value);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        type.GetProperty(item.Key)?.SetValue(result, model, null);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }
                    else if (isCollection)
                    {
                        var l = new List<string>();
                        l.Add("");
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        var list = type.GetProperty(item.Key)?.GetValue(result);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                        var addmethod = list?.GetType().GetMethod("Add");
                        var listtype = list?.GetType();
                        ;
                        foreach (var enumitemValue in (IEnumerable)item.Value)
                        {
                            var model = UnWrappObject(enumitemValue);
                            if (model != null)
                                addmethod?.Invoke(list, new object[] { model });
                        }
                    }
                    else
                    {
                        type?.GetProperty(item.Key)?.SetValue(result, item.Value, null);
                    }
                }
                return result;
                
            }

            return res;
        }
        /*
        public static IEnumerable<T> ToTypedCollectionWork2<T>(this IEnumerable<dynamic>? queryResult) where T : class
        {
            var res = new List<T>();
            if (queryResult == null)
                return res;

            //$select, $expand esetén nem az alap objektumokat (pl Models.Product) kapom vissza, hanem becsomagoltam kapom vissza speckó osztályba.
            //Ezeket kicsomagolom, hogy mindenképp alap típusban adhasasm vissza az eredményt
            foreach (var item in queryResult)
            {
                //var itemUw = UnWrappObject2(typeof(T), item);
                var itemUw = UnWrappObject3(item);
                if (itemUw is T)
                {
                    var model = itemUw as T;
                    res.Add((T)itemUw);
                }
            }

            dynamic UnWrappObject3(object itemValue)
            {
                if (IsWrapper(itemValue.GetType()))
                {
                    var instance = itemValue.GetType().GetProperty("Instance");
                    var instancePropertyValue = itemValue.GetType().GetProperty("Instance").GetValue(itemValue, null);
                    if (instancePropertyValue != null)
                        return instancePropertyValue;

                    var dict = ((ISelectExpandWrapper)itemValue).ToDictionary();

                    var itemValueType = itemValue.GetType();
                    var instancePropertyType = instance.PropertyType;

                    var model = ObjectFromDictionary3(instancePropertyType, dict);
*
                    return model;
                }
                return itemValue;
            }


            bool IsWrapper(Type type)
            {
                var ifs = type.GetInterfaces();
                var refs2 = ifs.Any(x => x == typeof(ISelectExpandWrapper));
                var res = type.GetInterfaces().Any(x => x.IsGenericType
                       && x.GetGenericTypeDefinition() == typeof(ISelectExpandWrapper));
                if (type.Name == "SelectAllAndExpand`1")
                    return true;
                if (type.Name == "SelectSomeAndInheritance`1")
                    return true;
                if (type.Name == "SelectSome`1")
                    return true;
                if (type.Name == "SelectAll`1")
                    return true;
                return res;
            }

            dynamic ObjectFromDictionary3(Type type, IDictionary<string, object> dict)
            {
                var result = Activator.CreateInstance(type);
                foreach (var item in dict)
                {
                    if (item.Value == null)
                        continue;
                    var itemValueTypeName = item.Value.GetType().Name;
                    var isCollection = IsCollection(item.Value.GetType());
                    var isWrapper = IsWrapper(item.Value.GetType());
                    if (isWrapper)
                    {
#if DEBUG
                        var debugdict = ((ISelectExpandWrapper)item.Value).ToDictionary();
                        var itemvaluetype = item.Value.GetType();
                        var prop = type.GetProperty(item.Key);
                        var proptype = prop.PropertyType.GetType();
#endif
                        var itemValue = item.Value;
                        var instance = itemValue.GetType().GetProperty("Instance");
                        var instancetype2 = instance.PropertyType;
                        var model = UnWrappObject3(item.Value);
                        type.GetProperty(item.Key).SetValue(result, model, null);
                        //var model = ObjectFromDictionary<T>(dict);
                        //do something
                        //res.Add(model);
                    }
                    else if (isCollection)
                    {
                        var l = new List<string>();
                        l.Add("");
                        var list = type.GetProperty(item.Key).GetValue(result);
                        var addmethod = list.GetType().GetMethod("Add");
                        var listtype = list.GetType();
                        ;
                        foreach (var enumitemValue in (IEnumerable)item.Value)
                        {
                            var instance = enumitemValue.GetType().GetProperty("Instance");
                            var instancetype2 = instance.PropertyType;
                            var model = UnWrappObject3(enumitemValue);
                            addmethod.Invoke(list, new object[] { model });
                            //list.Add(model);
                        }
                    }
                    else
                    {
                        type.GetProperty(item.Key).SetValue(result, item.Value, null);
                    }
                }
                return result;
                bool IsCollection(Type type)
                {
                    var interfaces = type.GetInterfaces();
                    //var genericCollection = interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
                    var nonStringEnumerable = interfaces.Contains(typeof(IEnumerable))
                        && (type != typeof(string));


                    return nonStringEnumerable;
                }

            }

            return res;
        }
        */

        /*
        public static IEnumerable<T> ToTypedCollectionWork1<T>(this IEnumerable<dynamic>? queryResult) where T : class
        {
            var res = new List<T>();
            if (queryResult == null)
                return res;

            //$select, $expand esetén nem az alap objektumokat (pl Models.Product) kapom vissza, hanem becsomagoltam kapom vissza speckó osztályba.
            //Ezeket kicsomagolom, hogy mindenképp alap típusban adhasasm vissza az eredményt
            foreach (var item in queryResult)
            {
                var itemUw = UnWrappObject2(typeof(T), item);
                //var itemUw = UnWrappObject2(item);
                if (itemUw is T)
                {
                    var model = itemUw as T;
                    res.Add((T)itemUw);
                }
            }

            T ObjectFromDictionary<T>(IDictionary<string, object> dict) where T : class
            {
                Type type = typeof(T);
                T result = (T)Activator.CreateInstance(type);
                foreach (var item in dict)
                {
                    if (item.Value.GetType().Name == "SelectAll`1")
                    {
                        var dictSelectAll1 = ((ISelectExpandWrapper)item.Value).ToDictionary();
                        var itemvaluetype = item.Value.GetType();
                        var prop = type.GetProperty(item.Key);
                        var proptype = prop.GetType();
                        var model = UnWrappObject2(proptype, item.Value);
                        //var model = ObjectFromDictionary<T>(dict);
                        //do something
                        //res.Add(model);
                    }
                    else
                    {
                        type.GetProperty(item.Key).SetValue(result, item.Value, null);
                    }
                }
                return result;
            }
            dynamic ObjectFromDictionary2(Type type, IDictionary<string, object> dict)
            {
                var result = Activator.CreateInstance(type);
                foreach (var item in dict)
                {
                    if (item.Value == null)
                        continue;
                    var itemValueTypeName = item.Value.GetType().Name;
                    var coll = IsICollectionOfT(item.Value.GetType());
                    if (IsWrapper(item.Value.GetType()))
                    {
#if DEBUG

                        var debugdict = ((ISelectExpandWrapper)item.Value).ToDictionary();
                        var itemvaluetype = item.Value.GetType();
                        var prop = type.GetProperty(item.Key);
                        var proptype = prop.PropertyType.GetType();
#endif
                        var itemValue = item.Value;
                        var instance = itemValue.GetType().GetProperty("Instance");
                        var instancetype2 = instance.PropertyType;
                        var model = UnWrappObject3(item.Value);
                        type.GetProperty(item.Key).SetValue(result, model, null);
                        //var model = ObjectFromDictionary<T>(dict);
                        //do something
                        //res.Add(model);
                    }
                    else if (coll)
                    {
                        var l = new List<string>();
                        l.Add("");
                        var list = type.GetProperty(item.Key).GetValue(result);
                        var addmethod = list.GetType().GetMethod("Add");
                        var listtype = list.GetType();
                        ;
                        foreach (var enumitemValue in (IEnumerable)item.Value)
                        {
                            var instance = enumitemValue.GetType().GetProperty("Instance");
                            var instancetype2 = instance.PropertyType;
                            var model = UnWrappObject3(enumitemValue);
                            addmethod.Invoke(list, new object[] { model });
                            //list.Add(model);
                        }
                    }
                    else
                    {
                        type.GetProperty(item.Key).SetValue(result, item.Value, null);
                    }
                }
                return result;
            }

            IDictionary<string, object> ObjectToDictionary<T>(T item) where T : class
            {
                Type myObjectType = item.GetType();
                IDictionary<string, object> dict = new Dictionary<string, object>();
                var indexer = new object[0];
                System.Reflection.PropertyInfo[] properties = myObjectType.GetProperties();
                foreach (var info in properties)
                {
                    var value = info.GetValue(item, indexer);
                    dict.Add(info.Name, value);
                }
                return dict;
            }

            bool IsICollectionOfT(Type type)
            {
                var res = type.GetInterfaces().Any(x => x.IsGenericType
                       && x.GetGenericTypeDefinition() == typeof(ICollection<>));
                return res;
            }

            object GetDefaultValue(Type t)
            {
                if (t.IsValueType)
                    return Activator.CreateInstance(t);

                return null;
            }

            T UnWrappObject<T>(dynamic item) where T : class
            {
                Type type = typeof(T);
                if (IsWrapper(type))
                {
                    var dict = ((ISelectExpandWrapper)item).ToDictionary();
                    var model = ObjectFromDictionary<T>(dict);
                    //do something
                    return model;
                }
                return item;
            }

            dynamic UnWrappObject2(Type type, dynamic item)
            {
                if (IsWrapper(item.GetType()))
                {
                    var itemw = (ISelectExpandWrapper)item;
                    var x = itemw.GetType().GetProperty("Instance");
                    var instance = item.GetType().GetProperty("Instance");
                    //var a = entityProperty.PropertyType
                    //var instance = itemw.Instance;
                    var dict = ((ISelectExpandWrapper)item).ToDictionary();
                    var model = ObjectFromDictionary2(type, dict);
                    //do something
                    return model;
                }
                return item;
            }
            dynamic UnWrappObject3(object itemValue)
            {
                if (IsWrapper(itemValue.GetType()))
                {
                    var instance = itemValue.GetType().GetProperty("Instance");
                    var instanceV = itemValue.GetType().GetProperty("Instance").GetValue(itemValue, null);
                    var instancetype2 = instance.PropertyType;


                    var itemw = (ISelectExpandWrapper)itemValue;
                    //var a = entityProperty.PropertyType
                    //var instance2 = itemw.Instance;
                    var dict = ((ISelectExpandWrapper)itemValue).ToDictionary();
                    var model = ObjectFromDictionary2(instancetype2, dict);
                    return instanceV;
                }
                return itemValue;
            }
            bool IsWrapper(Type type)
            {
                var ifs = type.GetInterfaces();
                var refs2 = ifs.Any(x => x == typeof(ISelectExpandWrapper));
                var res = type.GetInterfaces().Any(x => x.IsGenericType
                       && x.GetGenericTypeDefinition() == typeof(ISelectExpandWrapper));
                if (type.Name == "SelectAllAndExpand`1")
                    return true;
                if (type.Name == "SelectSomeAndInheritance`1")
                    return true;
                if (type.Name == "SelectSome`1")
                    return true;
                if (type.Name == "SelectAll`1")
                    return true;
                return res;
            }

            return res;
        }*/

    }


}
