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
using System.Collections; 
using System.Reflection;
using System.Data;

using System.Runtime.InteropServices;
using System.Collections.Generic;
//using Nistec.Data.Common;

namespace Nistec.Data
{
  
    //public class Args: Dictionary<string,object>
    //{

    //    public static Args Get(params object[] keyValueParameters)
    //    {
    //        int count = keyValueParameters.Length;
    //        if (count % 2 != 0)
    //        {
    //            throw new ArgumentException("values parameter not correct, Not match key value arguments");
    //        }
    //        Args a = new Args();
    //        for (int i = 0; i < count; i++)
    //        {
    //            a.Add(keyValueParameters[i].ToString(), keyValueParameters[++i]);
    //        }
    //        return a;
    //    }

    //    public static IDbDataParameter[] Get(params object[] keyValueParameters)
    //    {

    //        int count = keyValueParameters.Length;
    //        if (count % 2 != 0)
    //        {
    //            throw new ArgumentException("values parameter not correct, Not match key value arguments");
    //        }
    //        List<IDbDataParameter> list = new List<IDbDataParameter>();
    //        for (int i = 0; i < count; i++)
    //        {
    //            list.Add(new DataParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
    //        }

    //        return list.ToArray();
    //    }
    //}

   
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyValueItem
    {
        public readonly string Key;
        public readonly object Value;
        public KeyValueItem(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        public bool Equals(KeyValueItem item)
        {
            return item.Key.Equals(Key) && item.Value.Equals(Value);
        }

        public bool Equals(string key, object value)
        {
            return key.Equals(Key) && value.Equals(Value);
        }

     }

    public class KeyValueItem<T> //: IEntityItem
    {

        public static List<KeyValueItem<T>> GetList(params object[] keyValueArgs)
        {
            List<KeyValueItem<T>> list = new List<KeyValueItem<T>>();
            if (keyValueArgs == null)
                return list;
            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("keyValueArgs is  incorrect, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                object val = keyValueArgs[i];
                string name = (string)keyValueArgs[++i];

                T value = GenericTypes.Convert<T>(val);
                var entity = new KeyValueItem<T>() { Value = value, Key = name };
                list.Add(entity);
            }
            return list;
        }


        public static KeyValueItem<T> Get(params object[] keyValueArgs)
        {
            if (keyValueArgs == null)
                return new KeyValueItem<T>();
            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("keyValueArgs is  incorrect, Not match key value arguments");
            }
            object val = keyValueArgs[0];
            string name = (string)keyValueArgs[1];
            T value = GenericTypes.Convert<T>(val);
            var entity = new KeyValueItem<T>() { Value = value, Key = name };
            return entity;
        }

        public T Value { get; set; }
        public string Key { get; set; }
    }


  
}
