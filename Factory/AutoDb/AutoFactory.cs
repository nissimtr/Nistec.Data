//licHeader
//===============================================================================================================
// System  : Nistec.Data - Nistec.Data Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of data library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using Nistec.Runtime;

namespace Nistec.Data.Factory
{

    /// <summary>
    /// This class contains a static method <see cref="CreateDB"/> 
    /// which generates class implementation from class 
    /// derived from <see cref="IAutoDb"/> and <see cref="IAutoDb"/> imterface.
    /// </summary>
    internal sealed class AutoFactory
    {

        #region Members
        /// <summary>
        /// ModuleBuilder object.
        /// </summary>
        private static ModuleBuilder m_modBuilder = null;

        /// <summary>
        /// AssemblyBuilder object.
        /// </summary>
        private static AssemblyBuilder m_asmBuilder = null;

        #endregion

        #region Constructors
        /// <summary>
        /// A static constructor.
        /// </summary>
        static AutoFactory()
        {
            AssemblyName asmName = new AssemblyName();
            asmName.Name = "DALDynamicAsm";
            m_asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
            m_modBuilder = m_asmBuilder.DefineDynamicModule("DALDynamicModule");
        }

        /// <summary>
        /// A private constructor designed in order that no one could create 
        /// an instance of this class.
        /// </summary>
        private AutoFactory()
        {
        }

        #endregion

        #region Create DB

        /// <summary>
        /// Create the class implementation of a class 
        /// derived from <see cref="IAutoDb"/> imterface.
        /// </summary>
        /// <param name="encapType">A class type</param>
        /// <returns></returns>
        public static IAutoDb CreateDB(Type encapType)
        {
            // Verifying base interface
            Type baseInterfaceType = typeof(IAutoDb);
            Type baseitf = encapType.GetInterface(baseInterfaceType.FullName);
            if (!encapType.IsSubclassOf(typeof(AutoDb))) { }


            if (baseitf != baseInterfaceType)
            {
                string msg = string.Format("interface {0} must inherit from base interface {1}", encapType.FullName, baseInterfaceType.FullName);
                throw new DalException(msg);
            }

            Type typeCreated = null;
            lock (typeof(AutoFactory))
            {
                // get a generated class name
                string strTypeName = "_$Impl" + encapType.FullName;

                // try to find the class among already generated classes
                typeCreated = m_modBuilder.GetType(strTypeName);

                if (typeCreated == null)
                {
                    TypeBuilder tb = null;
                    if (IsInterface(encapType))
                    {
                        // if the original type is an interface then create a new type derived from DalDB class
                        tb = m_modBuilder.DefineType(strTypeName, TypeAttributes.Class | TypeAttributes.Public, typeof(AutoDb));
                    }
                    else
                    {
                        // else create a new type derived from original type
                        tb = m_modBuilder.DefineType(strTypeName, TypeAttributes.Class | TypeAttributes.Public, encapType);
                    }

                    // add created type implementation
                    AddType(tb, encapType);

                    // get created type
                    typeCreated = tb.CreateType();
                }
            }

            // creat an instance of the created type
            IAutoDb target = (IAutoDb)ActivatorUtil.CreateInstance(typeCreated);

            return target;

        }

        #endregion

        #region Methods
        /// <summary>
        /// Adds an implementation to the created type.
        /// </summary>
        /// <param name="tb">Type builder object.</param>
        /// <param name="encapType">Created class.</param>
        private static void AddType(TypeBuilder tb, Type encapType)
        {
            // add implementation for the interface
            if (IsInterface(encapType))
            {
                tb.AddInterfaceImplementation(encapType);
            }

            // create implementation for each type abstract method
            MethodInfo[] parentMethodInfo = encapType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < parentMethodInfo.Length; ++i)
            {
                if (IsAbstractMethod(parentMethodInfo[i]))
                {
                    AddMethod(parentMethodInfo[i], tb);
                }
            }
        }

        /// <summary>
        /// Adds attributes and an implementation to a method of the created class.
        /// </summary>
        /// <param name="method">MethodInfo object.</param>
        /// <param name="tb">Type builder object.</param>
        private static void AddMethod(MethodInfo method, TypeBuilder tb)
        {
            // get method return type
            Type RetType = method.ReturnType;

            if (RetType.FullName == "System.Void")
            {
                throw new Data.DalException("Return type void in abstract method not supported,Use one of types:" + AutoFactory.ReturnTypesString);
            }

            // get method paramete information
            ParameterInfo[] prms = method.GetParameters();

            int paramCount = prms.Length;

            // get method parameter types and names
            Type[] paramTypes = new Type[paramCount];
            string[] paramNames = new string[paramCount];
            ParameterAttributes[] paramAttrs = new ParameterAttributes[paramCount];
            for (int i = 0; i < paramCount; ++i)
            {
                paramTypes[i] = prms[i].ParameterType;
                paramNames[i] = prms[i].Name;
                paramAttrs[i] = prms[i].Attributes;
            }

            // define method body
            MethodBuilder methodBody = tb.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, paramTypes);

            // define method attribute if exists
            DBCommandAttribute CommandMethodAttr = (DBCommandAttribute)Attribute.GetCustomAttribute(method, typeof(DBCommandAttribute));
            if (CommandMethodAttr != null)
            {
                methodBody.SetCustomAttribute(DBCommandAttribute.GetAttributeBuilder(CommandMethodAttr));
            }

            // define method parameters with their attributes
            for (int i = 0; i < paramCount; ++i)
            {
                ParameterBuilder param = methodBody.DefineParameter(i + 1, paramAttrs[i], paramNames[i]);

                DbFieldAttribute[] ParameterAttr = (DbFieldAttribute[])prms[i].GetCustomAttributes(typeof(DbFieldAttribute), false);

                if (ParameterAttr.Length > 0) param.SetCustomAttribute(DbFieldAttribute.GetAttributeBuilder(ParameterAttr[0]));
            }

            // generate method body
            CreateMethodBody(methodBody.GetILGenerator(), RetType, prms);

        }

        /// <summary>
        /// Generates an implementation to the method.
        /// </summary>
        /// <param name="ilGen">ILGenerator object.</param>
        /// <param name="RetType">Method's return type</param>
        /// <param name="prms">Method's parameters.</param>
        private static void CreateMethodBody(ILGenerator ilGen, Type RetType, ParameterInfo[] prms)
        {
            // declaring local variables
            LocalBuilder V_0 = ilGen.DeclareLocal(typeof(MethodInfo));
            LocalBuilder V_1 = ilGen.DeclareLocal(typeof(Type));
            LocalBuilder V_2 = ilGen.DeclareLocal(typeof(object[]));
            LocalBuilder V_3 = ilGen.DeclareLocal(typeof(object));
            LocalBuilder V_4 = ilGen.DeclareLocal(RetType);
            LocalBuilder V_5 = ilGen.DeclareLocal(typeof(object[]));

            // V_0 = MethodBase.GetCurrentMethod();
            MethodInfo miLocalMethod = typeof(MethodBase).GetMethod("GetCurrentMethod");
            ilGen.Emit(OpCodes.Callvirt, miLocalMethod);
            ilGen.Emit(OpCodes.Castclass, typeof(MethodInfo));
            ilGen.Emit(OpCodes.Stloc_S, V_0);



            // V_5 = new object[prms.Length];
            ilGen.Emit(OpCodes.Ldc_I4, prms.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(object));
            ilGen.Emit(OpCodes.Stloc_S, V_5);

            for (int i = 0; i < prms.Length; ++i)
            {
                // V_5[i] = {method argument in position i+1};
                ilGen.Emit(OpCodes.Ldloc_S, V_5);
                ilGen.Emit(OpCodes.Ldc_I4, i);
                ilGen.Emit(OpCodes.Ldarg_S, i + 1);

                // if method argument is a value type then box it
                if (prms[i].ParameterType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Box, prms[i].ParameterType);
                }
                else if (prms[i].ParameterType.IsByRef)
                {
                    Type reftype = null;
                    // get type from reference type
                    reftype = InternalCmd.GetRefType(prms[i].ParameterType);

                    // and box by this type
                    if (reftype != null)
                    {
                        ilGen.Emit(OpCodes.Ldobj, reftype);
                        if (reftype.IsValueType)
                        {
                            ilGen.Emit(OpCodes.Box, reftype);
                        }
                    }
                }
                ilGen.Emit(OpCodes.Stelem_Ref);
            }

            // V_5 = V_2;
            ilGen.Emit(OpCodes.Ldloc_S, V_5);
            ilGen.Emit(OpCodes.Stloc_S, V_2);

            // load this.m_connection
            ilGen.Emit(OpCodes.Ldarg_0);// this
            FieldInfo fldConnection = typeof(AutoDb).GetField("m_connection", BindingFlags.Instance | BindingFlags.NonPublic);
            ilGen.Emit(OpCodes.Ldfld, fldConnection);
            // load this.m_transaction
            ilGen.Emit(OpCodes.Ldarg_0); // this
            FieldInfo fldTransaction = typeof(AutoDb).GetField("m_transaction", BindingFlags.Instance | BindingFlags.NonPublic);
            ilGen.Emit(OpCodes.Ldfld, fldTransaction);
            // load V_0
            ilGen.Emit(OpCodes.Ldloc_S, V_0);
            // load V_2
            ilGen.Emit(OpCodes.Ldloc_S, V_2);

            // load this.m_autoCloseConnection
            ilGen.Emit(OpCodes.Ldarg_0); // this
            FieldInfo fldAutoCloseConnection = typeof(AutoDb).GetField("m_autoCloseConnection", BindingFlags.Instance | BindingFlags.NonPublic);
            ilGen.Emit(OpCodes.Ldfld, fldAutoCloseConnection);

            //// load this.m_dbProvider
            //ilGen.Emit(OpCodes.Ldarg_0); // this
            //FieldInfo fldDBProvider = typeof(DalDB).GetField("m_DBProvider", BindingFlags.Instance | BindingFlags.NonPublic);
            //ilGen.Emit(OpCodes.Ldfld, fldDBProvider);

            // call Exec.DBCmd methos
            MethodInfo miExecuteMethod = null;
            miExecuteMethod = typeof(AutoCommand).GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);


            ilGen.Emit(OpCodes.Call, miExecuteMethod);
            // result -> V_3
            ilGen.Emit(OpCodes.Stloc_S, V_3);


            // returning parameters passed by reference
            for (int i = 0; i < prms.Length; ++i)
            {
                if (prms[i].ParameterType.IsByRef)
                {
                    Type reftype = null;
                    reftype = InternalCmd.GetRefType(prms[i].ParameterType);

                    if (reftype != null)
                    {
                        // {method argument in position i+1} = V_2[i]
                        ilGen.Emit(OpCodes.Ldarg_S, i + 1);
                        ilGen.Emit(OpCodes.Ldloc_S, V_2);
                        ilGen.Emit(OpCodes.Ldc_I4, i);
                        ilGen.Emit(OpCodes.Ldelem_Ref);
                        if (reftype.IsValueType)
                        {
                            ilGen.Emit(OpCodes.Unbox, reftype);
                            ilGen.Emit(OpCodes.Ldobj, reftype);
                        }
                        ilGen.Emit(OpCodes.Stobj, reftype);
                    }
                }
            }


            if (RetType.FullName != "System.Void")
            {
                // return (RetType)V_3;
                ilGen.Emit(OpCodes.Ldloc_S, V_3);
                if (RetType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Unbox, RetType);
                    ilGen.Emit(OpCodes.Ldobj, RetType);
                }
                else
                {
                    ilGen.Emit(OpCodes.Castclass, RetType);
                }
                ilGen.Emit(OpCodes.Stloc_S, V_4);
                ilGen.Emit(OpCodes.Ldloc_S, V_4);
            }
            ilGen.Emit(OpCodes.Ret);

        }

        #endregion

        #region Functions
        /// <summary>
        /// Checks if a type is an interface.
        /// </summary>
        /// <param name="encapType">Type object.</param>
        /// <returns></returns>
        private static bool IsInterface(Type encapType)
        {
            return encapType.IsInterface;
        }

        /// <summary>
        /// Checks if a method is abstract.
        /// </summary>
        /// <param name="method">MethodInfo object.</param>
        /// <returns></returns>
        private static bool IsAbstractMethod(MethodInfo method)
        {
            return method.IsAbstract;
        }


        /// <summary>
        /// Saves the created assembly to a file.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        public static void SaveAssemply(string fileName)
        {
            m_asmBuilder.Save(fileName);
        }


        #endregion

        /// <summary>
        /// DalMethods
        /// </summary>
        #region DalMethods

        /// <summary>
        /// list of return types
        /// </summary>
        public const string ReturnTypesString = "Int,Object,DalSchema,DataSet,DataTable,DataRow[],IDataAdapter,IDataReader,IDbCommand";


        /// <summary>
        /// GetReturnType
        /// </summary>
        /// <param name="typ"></param>
        /// <returns></returns>
        public static Type GetReturnType(DalReturnType typ)
        {
            switch (typ)
            {
                case DalReturnType.Int:
                    return typeof(int);
                case DalReturnType.DalSchema:
                    return Type.GetType("Nistec.Data.DalSchema");
                case DalReturnType.DataSet:
                    return typeof(System.Data.DataSet);
                case DalReturnType.DataTable:
                    return typeof(System.Data.DataTable);
                case DalReturnType.DataRows:
                    return typeof(System.Data.DataRow[]);
                case DalReturnType.DataReader:
                    return typeof(System.Data.IDataAdapter);
                case DalReturnType.DataAdapter:
                    return typeof(System.Data.IDataReader);
                case DalReturnType.Command:
                    return typeof(System.Data.IDbCommand);
                case DalReturnType.Object:
                    return Type.GetType("System.Object");
                default:// case DalReturnType.Default:
                    return Type.GetType("System.Object");
            }

        }

        /// <summary>
        /// GetReturnType
        /// </summary>
        /// <param name="typ"></param>
        /// <returns></returns>
        public static Type GetReturnType(string typ)
        {
            switch (typ)
            {
                case "System.Int":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    return typeof(int);
                case "Nistec.Data.DalSchema":
                    return Type.GetType("Nistec.Data.DalSchema");
                case "System.Data.DataSet":
                    return typeof(System.Data.DataSet);
                case "System.Data.DataTable":
                    return typeof(System.Data.DataTable);
                case "System.Data.DataRow[]":
                    return typeof(System.Data.DataRow[]);
                case "System.Data.DataRow":
                    return typeof(System.Data.DataRow);
                case "System.Object":
                    return Type.GetType("System.Object");
                case "System.Void":
                    return Type.GetType("System.Void");
                default:
                    return Type.GetType(typ);
            }

        }

        /// <summary>
        /// DBCommandTypeToCommandType
        /// </summary>
        /// <param name="dbcmd"></param>
        /// <returns></returns>
        public static System.Data.CommandType DBCommandTypeToCommandType(DBCommandType dbcmd)
        {
            switch (dbcmd)
            {
                case DBCommandType.StoredProcedure:
                    return CommandType.StoredProcedure;
                default:
                    return CommandType.Text;
            }
        }
        /// <summary>
        /// ParseEnum
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="value"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ParseEnum(Type enumType, string value, bool ignoreCase, int defaultValue)
        {
            try
            {
                object o = Enum.Parse(enumType, value, ignoreCase);
                return (int)o;
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion
    }
}
