using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SqlReflect
{
    public class EmitDataMapper
    {
        public static DynamicDataMapper Build(Type klass, string connStr, bool withCache)
        {
            AssemblyName aName = new AssemblyName(klass.Name + "DynamicDataMapper");
            AssemblyBuilder ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.RunAndSave
                );

            // For a single-module assembly, the module name is usually the assembly name plus an extension.
            ModuleBuilder mb =
                ab.DefineDynamicModule(
                    aName.Name,
                    aName.Name + ".dll"
                );

            TypeBuilder tb =
                mb.DefineType(
                    klass.Name + "DynamicDataMapperType",
                    TypeAttributes.Public,
                    typeof(DynamicDataMapper)
                );


            // Define a constructor that takes same arguments of DynamicDataMapper. 
            Type[] parameterTypes = { typeof(Type), typeof(string), typeof(bool) };
            ConstructorBuilder ctor =
                tb.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    parameterTypes
                );

            ILGenerator il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            //il.Emit(OpCodes.Stfld, /*fbNumber*/); //from dummy app
            il.Emit(OpCodes.Ret);

            // Overrides AbstractDataMapper methods

            Type[] parametersType;

            parametersType = new Type[] { typeof(IDataReader) };
            MethodBuilder mbLoadMethod = BuildLoadMethod(tb, "Load", klass);

            parametersType = new Type[] { typeof(object) };
            MethodBuilder mbSQLInsertMethod = buildMethod(tb, "sqlInsert", typeof(string), parametersType);
            MethodBuilder mbSQLDeleteMethod = buildMethod(tb, "sqlDelete", typeof(string), parametersType);
            MethodBuilder mbSQLUpdateMethod = buildMethod(tb, "sqlUpdate", typeof(string), parametersType);

            Type type = tb.CreateType();

            DynamicDataMapper ddm = (DynamicDataMapper)Activator.CreateInstance(type);

            ab.Save(aName.Name + ".dll");

            return ddm;

        }

        private static MethodBuilder buildMethod(TypeBuilder tb, string methodName, Type retType, Type[] paramsType)
        {
            return tb.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
            retType,
            paramsType);
        }

        private static void BuildLoadMethod(TypeBuilder tb, string methodName, Type klass)
        {
            Type[] parametersType = new Type[] { typeof(IDataReader) };
            MethodBuilder mbLoadMethod = buildMethod(tb, "Load", klass, parametersType);

            ILGenerator il = mbLoadMethod.GetILGenerator();

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldloca_S);
            il.Emit(OpCodes.Initobj);

            foreach (PropertyInfo p in klass.GetProperties())
            {
                il.Emit(OpCodes.Ldloca_S);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldstr, p.PropertyType);
                il.Emit(OpCodes.Callvirt, p.GetGetMethod());
                il.Emit(OpCodes.Castclass, klass);
                il.Emit(OpCodes.Call, );
            }
            il.Emit(OpCodes.Ret);

        }

    }
}