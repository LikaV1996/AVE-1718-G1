using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SqlReflect.Attributes;

namespace SqlReflect
{
    public class EmitDataMapper
    {

        private static readonly MethodInfo concatStrArray = typeof(string).GetMethod("Concat", new Type[] { typeof(string[]) });

        private static readonly MethodInfo concat4Str = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });

        private static readonly MethodInfo format1Str2Obj = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object), typeof(object) });


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
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Call, typeof(DynamicDataMapper).GetConstructor(new Type[] { typeof(Type), typeof(string), typeof(bool) }));
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);



            // Overrides AbstractDataMapper methods
            BuildMethod_Load(tb, klass);
            BuildMethod_SQLInsert(tb, klass);
            BuildMethod_SQLDelete(tb, klass);
            BuildMethod_SQLUpdate(tb, klass);

            Type type = tb.CreateType();

            ab.Save(aName.Name + ".dll");

            return (DynamicDataMapper)Activator.CreateInstance(type, new object[] { klass, connStr, false });

        }

        private static MethodBuilder BuildMethod(TypeBuilder tb, string methodName, Type retType, Type[] paramsType)
        {
            return tb.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
            retType,
            paramsType);
        }

        private static void BuildMethod_Load(TypeBuilder tb, Type klass)
        {
            Type[] parametersType = new Type[] { typeof(IDataReader) };
            MethodBuilder mbLoadMethod = BuildMethod(tb, "Load", typeof(object), parametersType);

            ILGenerator il = mbLoadMethod.GetILGenerator();
            LocalBuilder tobj = il.DeclareLocal(klass);

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldloca_S, tobj);
            il.Emit(OpCodes.Initobj, klass);

            foreach (PropertyInfo p in klass.GetProperties())
            {
                il.Emit(OpCodes.Ldloca_S, tobj);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldstr, p.PropertyType);
                il.Emit(OpCodes.Callvirt, p.GetGetMethod());
                il.Emit(OpCodes.Castclass, typeof(string));
                il.Emit(OpCodes.Call, p.GetSetMethod());
                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Box, klass);
            il.Emit(OpCodes.Stloc_1);

            Label label = il.DefineLabel();
            il.Emit(OpCodes.Br_S, label);
            il.MarkLabel(label);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
        }

        private static void BuildMethod_SQLInsert(TypeBuilder tb, Type klass)
        {
            Type[] parametersType = new Type[] { typeof(object) };
            MethodBuilder mbSQLInsertMethod = BuildMethod(tb, "SqlInsert", typeof(string), parametersType);

            ILGenerator il = mbSQLInsertMethod.GetILGenerator();

            LocalBuilder tobj = il.DeclareLocal(klass);

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, klass);
            il.Emit(OpCodes.Stloc, tobj);   //changed here
            //il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_S, 23);
            il.Emit(OpCodes.Newarr, typeof(string));
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldstr, "'");
            il.Emit(OpCodes.Stelem_Ref);

            PropertyInfo[] pi = klass.GetProperties();

            PropertyInfo pk = pi
                .First(p => p.IsDefined(typeof(PKAttribute)));

            for (int idx = 1, i = 0 ; i < pi.Length ; i++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                il.Emit(OpCodes.Ldloca_S, tobj);
                il.Emit(OpCodes.Call, pi[i].GetGetMethod());    //get method to return property value
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                il.Emit(OpCodes.Ldstr, (i < pi.Length-1) ? "' , '" : "'" ); //string to seperate properties
                il.Emit(OpCodes.Stelem_Ref);
            }
            
            il.Emit(OpCodes.Call, concatStrArray);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldarg_0);


            FieldInfo insertStmtField = typeof(DynamicDataMapper)
                .GetField("insertStmt", BindingFlags.Instance | BindingFlags.NonPublic);

            Label label = il.DefineLabel();


            il.Emit(OpCodes.Ldfld, insertStmtField);
            il.Emit(OpCodes.Ldstr, "(");
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldstr, ")");

            il.Emit(OpCodes.Call, concat4Str);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Br_S, label);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldloc_2);

            il.Emit(OpCodes.Ret);

        }
        
        private static void BuildMethod_SQLDelete(TypeBuilder tb, Type klass)
        {
            Type[] parametersType = new Type[] { typeof(object) };
            MethodBuilder mbSLQDeleteMethod = BuildMethod(tb, "SqlDelete", typeof(string), parametersType);
            
            FieldInfo deleteStmtField = typeof(DynamicDataMapper)
                .GetField("deleteStmt", BindingFlags.Instance | BindingFlags.NonPublic);

            ILGenerator il = mbSLQDeleteMethod.GetILGenerator();
            LocalBuilder tobj = il.DeclareLocal(klass);

            PropertyInfo pk = klass
                .GetProperties()
                .First(p => p.IsDefined(typeof(PKAttribute)));

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, klass);
            il.Emit(OpCodes.Stloc_0, tobj);
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ldfld, deleteStmtField);
            il.Emit(OpCodes.Ldstr, "'");
            il.Emit(OpCodes.Ldloca_S, tobj);
            il.Emit(OpCodes.Call, pk.GetGetMethod());   //get method to return property value
            il.Emit(OpCodes.Ldstr, "'");

            il.Emit(OpCodes.Call, concat4Str);
            il.Emit(OpCodes.Stloc_1);

            Label label = il.DefineLabel();

            il.Emit(OpCodes.Br_S, label);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);

        }

        private static void BuildMethod_SQLUpdate(TypeBuilder tb, Type klass)
        {
            Type[] parametersType = new Type[] { typeof(object) };
            MethodBuilder mbSQLUpdateMethod = BuildMethod(tb, "SqlUpdate", typeof(string), parametersType);

            ILGenerator il = mbSQLUpdateMethod.GetILGenerator();

            LocalBuilder tobj = il.DeclareLocal(klass);

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, klass);
            il.Emit(OpCodes.Stloc, tobj);   //changed here
            //il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_S, 30);
            il.Emit(OpCodes.Newarr, typeof(string));

            PropertyInfo[] pi = klass.GetProperties();

            PropertyInfo pk = pi
                .First(p => p.IsDefined(typeof(PKAttribute)));

            for (int idx = 0, i = 0; i < pi.Length; i++)    //only strings
            {
                PropertyInfo p = pi[i];
                if (p == pk) continue;

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                il.Emit(OpCodes.Ldstr, p.Name + "='");  //property name
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                il.Emit(OpCodes.Ldloca_S, tobj);
                il.Emit(OpCodes.Call, p.GetGetMethod());    //get method to return property value
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                il.Emit(OpCodes.Ldstr, (i < pi.Length - 1) ? "' , " : "'"); //string to seperate properties
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Call, concatStrArray);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldarg_0);


            FieldInfo updateStmtField = typeof(DynamicDataMapper)
                .GetField("updateStmt", BindingFlags.Instance | BindingFlags.NonPublic);

            Label label = il.DefineLabel();


            il.Emit(OpCodes.Ldfld, updateStmtField);
            il.Emit(OpCodes.Ldstr, "'");
            il.Emit(OpCodes.Ldloca_S, tobj);
            il.Emit(OpCodes.Call, pk.GetGetMethod());
            il.Emit(OpCodes.Ldstr, "'");

            il.Emit(OpCodes.Call, concat4Str);
            il.Emit(OpCodes.Call, format1Str2Obj);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Br_S, label);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldloc_2);

            il.Emit(OpCodes.Ret);

        }
    }
}