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

            parametersType = new Type[]{ typeof(IDataReader) };
            MethodBuilder mbLoadMethod = buildMethod(tb, "Load", klass, parametersType);
                
            parametersType = new Type[]{ typeof(object) };
            MethodBuilder mbSQLInsertMethod = buildMethod(tb, "sqlInsert", typeof(string), parametersType);   
            MethodBuilder mbSQLDeleteMethod = buildMethod(tb, "sqlDelete", typeof(string), parametersType);  
            MethodBuilder mbSQLUpdateMethod = buildMethod(tb, "sqlUpdate", typeof(string), parametersType);
                
            Type type = tb.CreateType();

            DynamicDataMapper ddm = (DynamicDataMapper)Activator.CreateInstance(type);

            ab.Save(aName.Name + ".dll");

            return ddm;
            /*
            AssemblyName aName = new AssemblyName("DynamicAssemblyExample");
        AssemblyBuilder ab = 
            AppDomain.CurrentDomain.DefineDynamicAssembly(
                aName, 
                AssemblyBuilderAccess.RunAndSave);

        // For a single-module assembly, the module name is usually
        // the assembly name plus an extension.
        ModuleBuilder mb = 
            ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

        TypeBuilder tb = mb.DefineType(
            "MyDynamicType", 
             TypeAttributes.Public);

        // Add a private field of type int (Int32).
        FieldBuilder fbNumber = tb.DefineField(
            "m_number", 
            typeof(int), 
            FieldAttributes.Private);

        // Define a constructor that takes an integer argument and 
        // stores it in the private field. 
        Type[] parameterTypes = { typeof(int) };
        ConstructorBuilder ctor = tb.DefineConstructor(
            MethodAttributes.Public, 
            CallingConventions.Standard, 
            parameterTypes);
        
        ILGenerator il = ctor.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, fbNumber);
        il.Emit(OpCodes.Ret);
        
        // Overrides ToString() method
        MethodBuilder toStr = tb.DefineMethod("ToString",
             MethodAttributes.Public | MethodAttributes.ReuseSlot | 
             MethodAttributes.Virtual | MethodAttributes.HideBySig,
            typeof(string), 
            new Type[0]);
            
        MethodInfo concat = typeof(string)
            .GetMethod("Concat", new Type[]{typeof(object), typeof(object)});
        il = toStr.GetILGenerator();
        il.Emit(OpCodes.Ldstr, "m_number = ");
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, fbNumber);
        il.Emit(OpCodes.Box, fbNumber.FieldType);
        il.Emit(OpCodes.Call, concat);
        il.Emit(OpCodes.Ret);

        // Finish the type.
        Type t = tb.CreateType();

        // The following line saves the single-module assembly. This
        // requires AssemblyBuilderAccess to include Save. 
        // 
        ab.Save(aName.Name + ".dll");
        return t;
            */
        }

        private static MethodBuilder buildMethod(TypeBuilder tb, string methodName, Type retType, Type[] paramsType)
        {
            return tb.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
            retType,
            paramsType);
        }

        private static void buildLoadMethod()
        {
            throw new NotImplementedException();
        }

    }
}