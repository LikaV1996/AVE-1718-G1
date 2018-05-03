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

        private static readonly MethodInfo concatStrArr = typeof(string).GetMethod("Concat", new Type[] { typeof(string[]) });
        private static readonly MethodInfo concat4Str = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
        private static readonly MethodInfo concat3Str = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string) });
        private static readonly MethodInfo concat2Obj = typeof(string).GetMethod("Concat", new Type[] { typeof(object), typeof(object) });

        private static readonly MethodInfo format1Str2Obj = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object), typeof(object) });

        private static Type baseType = typeof(DynamicDataMapper);

        private static bool classOrStruct = false;
        private static bool autoIncrement = true;
        private static bool PK_IsString = false;

        private static string connectionStr;

        public static DynamicDataMapper Build(Type klass, string connStr, bool withCache)
        {
            connectionStr = connStr;

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
                    baseType
                );




            if (klass.IsClass) classOrStruct = true;

            PropertyInfo pk = klass
                .GetProperties()
                .First(p => p.IsDefined(typeof(PKAttribute)));

            PKAttribute PKatt = (PKAttribute)pk.GetCustomAttribute(typeof(PKAttribute));

            autoIncrement = PKatt.AutoIncrement;

            PK_IsString = (pk.PropertyType == typeof(string));

            // Define a constructor that takes same arguments of DynamicDataMapper. 
            BuildConstructor(tb);

            // Overrides AbstractDataMapper methods
            BuildMethod_Load(tb, klass);

            int SQLInsertArrayLength = 0, SQLUpdateArrayLength = 0;
            foreach (PropertyInfo p in klass.GetProperties())
            {
                if (p != pk) {
                    SQLInsertArrayLength++;
                    SQLUpdateArrayLength++;
                }
                    if(!autoIncrement) SQLInsertArrayLength++;
            }
            SQLInsertArrayLength = SQLInsertArrayLength * 2 + 1;
            SQLUpdateArrayLength = SQLUpdateArrayLength * 3;

            BuildMethod_SQLInsert(tb, klass, SQLInsertArrayLength);
            BuildMethod_SQLDelete(tb, klass);
            BuildMethod_SQLUpdate(tb, klass, SQLUpdateArrayLength);

            Type type = tb.CreateType();

            ab.Save(aName.Name + ".dll");

            return (DynamicDataMapper)Activator.CreateInstance(type, new object[] { klass, connStr, false });
        }

        private static void BuildConstructor(TypeBuilder tb)
        {
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
            il.Emit(OpCodes.Call, baseType.GetConstructor(parameterTypes));
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);


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

            LocalBuilder c = il.DeclareLocal(klass);
            LocalBuilder localVariable_1 = il.DeclareLocal(klass);
            LocalBuilder localVariable_2 = il.DeclareLocal(typeof(object));

            il.Emit(OpCodes.Nop);

            if (!classOrStruct)
            {
                il.Emit(OpCodes.Ldloca_S, localVariable_1);
                il.Emit(OpCodes.Initobj, klass);
            }
            else if (classOrStruct)
            {
                il.Emit(OpCodes.Newobj, klass.GetConstructor(new Type[] { }));
                il.Emit(OpCodes.Stloc_1);
            }
            //else { /*throw error ??*/ }

            foreach (PropertyInfo p in klass.GetProperties())
            {
                bool propertyTypeIsObject = !(p.PropertyType.IsPrimitive || p.PropertyType == typeof(string));

                if ( !classOrStruct )
                    il.Emit(OpCodes.Ldloca_S, localVariable_1);
                else if( classOrStruct )
                    il.Emit(OpCodes.Ldloc_1);
                //else { /*throw error ??*/ }

                if( propertyTypeIsObject )
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, p.GetGetMethod()/*PropertyType*/);//EmitDataMapper.Build(p.PropertyType, connectionStr, false));
                }

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldstr, p.Name);
                il.Emit(OpCodes.Callvirt, typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(k => k.GetIndexParameters().Any() && k.GetIndexParameters()[0].ParameterType == typeof(string))     //find get_Item()
                         .Select(k => k.GetGetMethod()).First());   //get getmethod

                if (propertyTypeIsObject)
                    il.Emit(OpCodes.Callvirt, typeof(IDataMapper).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(k => k.GetIndexParameters().Any() && k.GetIndexParameters()[0].ParameterType == typeof(object))
                        .Select(k => k.GetGetMethod()).First());

                if (p.PropertyType.IsPrimitive)
                    il.Emit(OpCodes.Unbox_Any, p.PropertyType);
                else
                    il.Emit(OpCodes.Isinst, p.PropertyType);

                if ( !classOrStruct )
                    il.Emit(OpCodes.Call, p.GetSetMethod());
                else if( classOrStruct )
                il.Emit(OpCodes.Callvirt, p.GetSetMethod());
                //else { /*throw error ??*/ }

                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            if( !classOrStruct )
                il.Emit(OpCodes.Box, klass);
            //else if( /*is class*/ ) // nothing happens
            //else { /*throw error ??*/ }
            il.Emit(OpCodes.Stloc_2);

            Label label = il.DefineLabel();
            
            il.Emit(OpCodes.Br_S, label);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ret);
        }

        private static void BuildMethod_SQLInsert(TypeBuilder tb, Type klass, int arrayLength)
        {
            Type[] parametersType = new Type[] { typeof(object) };
            MethodBuilder mbSQLInsertMethod = BuildMethod(tb, "SqlInsert", typeof(string), parametersType);

            ILGenerator il = mbSQLInsertMethod.GetILGenerator();

            LocalBuilder c = il.DeclareLocal(klass);
            LocalBuilder localVariable_1 = il.DeclareLocal(typeof(string));
            LocalBuilder localVariable_2 = il.DeclareLocal(typeof(string));

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1);
            if( !classOrStruct )
                il.Emit(OpCodes.Unbox_Any, klass);
            else if( classOrStruct )
                il.Emit(OpCodes.Castclass, klass);
            //else { /*throw error ??*/ }
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_S, arrayLength);
            il.Emit(OpCodes.Newarr, typeof(string));
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldstr, "'");
            il.Emit(OpCodes.Stelem_Ref);

            int idx = 1;
            foreach(PropertyInfo p in klass.GetProperties())
            {
                if (autoIncrement && p.IsDefined(typeof(PKAttribute))) continue;

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                if( !classOrStruct )
                    il.Emit(OpCodes.Ldloca_S, c);
                else if( classOrStruct )
                    il.Emit(OpCodes.Ldloc_0);
                //else { /*throw error ??*/ }
                if( !classOrStruct )
                    il.Emit(OpCodes.Call, p.GetGetMethod());
                else if( classOrStruct )
                    il.Emit(OpCodes.Callvirt, p.GetGetMethod());    //get method to return property value
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                il.Emit(OpCodes.Ldstr, ( p != klass.GetProperties().Last() ) ? "' , '" : "'"); //string to seperate properties
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Call, concatStrArr);
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
            LocalBuilder c = il.DeclareLocal(klass);
            LocalBuilder localVariable_1 = il.DeclareLocal(typeof(string));

            PropertyInfo pk = klass
                .GetProperties()
                .First(p => p.IsDefined(typeof(PKAttribute)));

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1);
            if( !classOrStruct )
                il.Emit(OpCodes.Unbox_Any, klass);
            else if( classOrStruct )
                il.Emit(OpCodes.Castclass, klass);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ldfld, deleteStmtField);

            if(PK_IsString)
                il.Emit(OpCodes.Ldstr, "'");

            if (!classOrStruct)
                il.Emit(OpCodes.Ldloca_S, c);
            else if (classOrStruct)
                il.Emit(OpCodes.Ldloc_0);
            if( !classOrStruct )
                il.Emit(OpCodes.Call, pk.GetGetMethod());
            else if( classOrStruct )
                il.Emit(OpCodes.Callvirt, pk.GetGetMethod());   //get method to return property value

            if (PK_IsString)
            {
                il.Emit(OpCodes.Ldstr, "'");
                il.Emit(OpCodes.Call, concat4Str);
            }
            else
            {
                il.Emit(OpCodes.Box, pk.PropertyType);
                il.Emit(OpCodes.Call, concat2Obj);
            }
            il.Emit(OpCodes.Stloc_1);

            Label label = il.DefineLabel();

            il.Emit(OpCodes.Br_S, label);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldloc_1);

            il.Emit(OpCodes.Ret);

        }

        private static void BuildMethod_SQLUpdate(TypeBuilder tb, Type klass, int arrayLength)
        {
            Type[] parametersType = new Type[] { typeof(object) };
            MethodBuilder mbSQLUpdateMethod = BuildMethod(tb, "SqlUpdate", typeof(string), parametersType);

            ILGenerator il = mbSQLUpdateMethod.GetILGenerator();

            LocalBuilder c = il.DeclareLocal(klass);
            LocalBuilder setString = il.DeclareLocal(typeof(string));
            LocalBuilder localVariable_2 = il.DeclareLocal(typeof(string));

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1);
            if( !classOrStruct )
                il.Emit(OpCodes.Unbox_Any, klass);
            else if( classOrStruct )
                il.Emit(OpCodes.Castclass, klass);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_S, arrayLength);
            il.Emit(OpCodes.Newarr, typeof(string));

            PropertyInfo pk = klass.GetProperties()
                .First(p => p.IsDefined(typeof(PKAttribute)));

            int idx = 0;
            foreach(PropertyInfo p in klass.GetProperties())    //only strings
            {
                if (p.IsDefined(typeof(PKAttribute)) /*p == pk*/) continue;

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                il.Emit(OpCodes.Ldstr, p.Name + "='");  //property name
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);

                if (!classOrStruct)
                    il.Emit(OpCodes.Ldloca_S, c);
                else if( classOrStruct )
                    il.Emit(OpCodes.Ldloc_0);

                if( !classOrStruct)
                    il.Emit(OpCodes.Call, p.GetGetMethod());
                else if( classOrStruct )
                    il.Emit(OpCodes.Callvirt, p.GetGetMethod());    //get method to return property value

                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_S, idx++);
                il.Emit(OpCodes.Ldstr, ( p != klass.GetProperties().Last() ) ? "' , " : "'"); //string to seperate properties
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Call, concatStrArr);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldarg_0);


            FieldInfo updateStmtField = typeof(DynamicDataMapper)
                .GetField("updateStmt", BindingFlags.Instance | BindingFlags.NonPublic);

            Label label = il.DefineLabel();


            il.Emit(OpCodes.Ldfld, updateStmtField);
            il.Emit(OpCodes.Ldloc_1);

            if(PK_IsString)
                il.Emit(OpCodes.Ldstr, "'");

            if ( !classOrStruct ) {
                il.Emit(OpCodes.Ldloca_S, c);
                il.Emit(OpCodes.Call, pk.GetGetMethod());
            }
            else if ( classOrStruct ) {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Callvirt, pk.GetGetMethod());
            }

            if (PK_IsString)
            {
                il.Emit(OpCodes.Ldstr, "'");
                il.Emit(OpCodes.Call, concat3Str);
            }
            else
            {
                il.Emit(OpCodes.Box, pk.PropertyType);
                
            }
            il.Emit(OpCodes.Call, format1Str2Obj);
            

            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Br_S, label);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldloc_2);

            il.Emit(OpCodes.Ret);

        }
    }
}