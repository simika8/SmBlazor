using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SmData
{
    public class DtoTypeBuilder
    {
        

        public static Type BuildMultiLevelType(IEnumerable<KeyValuePair<string[], Type?>>? props)
        {
            var normalProps = props.Where(x => x.Key.Count() == 1).ToList();
            var subClassPropNames = props.Where(x => x.Key.Count() > 1).Select(x => x.Key.First()).Distinct().ToArray();
            subClassPropNames?.ToList().ForEach(subPropName =>
            {
                var subprops = props.Where(x => x.Key.First() == subPropName)
                    .Select(x => 
                        new KeyValuePair<string[], Type?>(x.Key.Skip(1).ToArray(), x.Value));


                var type = BuildRowTypeByPropsOld(subprops);
                var newPropkvp = new KeyValuePair<string[], Type?>(new string[1] { subPropName }, type);
                normalProps.Add(newPropkvp);
            });

            var res = BuildRowTypeByPropsOld(normalProps);
            return res;
        }
        public static Type BuildRowTypeByPropsOld(IEnumerable<KeyValuePair<string[], Type?>>? props)
        {
            var newTypeName = Guid.NewGuid().ToString();
            var assemblyName = new AssemblyName(newTypeName);
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("Main");
            var tb = dynamicModule.DefineType(newTypeName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);     // This is the type of class to derive from. Use null if there isn't one
            tb.DefineDefaultConstructor(MethodAttributes.Public |
                                                MethodAttributes.SpecialName |
                                                MethodAttributes.RTSpecialName);
            foreach (var property in props)
            {
                AddProperty(tb, property.Key.First(), property.Value);
            }
            
            var res = tb.CreateType();
            return res;
        }
        public static Type BuildRowTypeByProps(List<string[]> fieldNames, Type[] fieldTypes)
        {
            var newTypeName = Guid.NewGuid().ToString();
            var assemblyName = new AssemblyName(newTypeName);
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("Main");
            var tb = dynamicModule.DefineType(newTypeName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);     // This is the type of class to derive from. Use null if there isn't one
            tb.DefineDefaultConstructor(MethodAttributes.Public |
                                                MethodAttributes.SpecialName |
                                                MethodAttributes.RTSpecialName);


            List<MethodBuilder> propertySetters = new List<MethodBuilder>();
            for (var i = 0; i < fieldNames.Count(); i++)
            {
                var propertySetter = AddProperty(tb, fieldNames[i].First(), fieldTypes[i]);
                propertySetters.Add(propertySetter);
            }

            CreateConstructor(tb, fieldNames, fieldTypes, propertySetters);

            var res = tb.CreateType();
            return res;
        }
        private static void CreateConstructor(TypeBuilder tb, List<string[]> propertyNames, Type[] propertyTypes, List<MethodBuilder> propertySetters)
        {
            var constructor = tb.DefineConstructor(
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                propertyTypes.ToArray());

            var conObj = typeof(object).GetConstructor(new Type[0]);

            ILGenerator il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, conObj);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Nop);

            for (var i = 0; i < propertyTypes.Count(); i++)
            {
                var property = propertyTypes[i];

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Call, propertySetters[i]);
                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);
        }

        private static MethodBuilder AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            var getMethod = typeBuilder.DefineMethod("get_" + propertyName,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getMethodIL = getMethod.GetILGenerator();
            getMethodIL.Emit(OpCodes.Ldarg_0);
            getMethodIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getMethodIL.Emit(OpCodes.Ret);

            var setMethod = typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });
            var setMethodIL = setMethod.GetILGenerator();
            Label modifyProperty = setMethodIL.DefineLabel();
            Label exitSet = setMethodIL.DefineLabel();

            setMethodIL.MarkLabel(modifyProperty);
            setMethodIL.Emit(OpCodes.Ldarg_0);
            setMethodIL.Emit(OpCodes.Ldarg_1);
            setMethodIL.Emit(OpCodes.Stfld, fieldBuilder);
            setMethodIL.Emit(OpCodes.Nop);
            setMethodIL.MarkLabel(exitSet);
            setMethodIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethod);
            propertyBuilder.SetSetMethod(setMethod);
            return setMethod;
        }
    }
}
