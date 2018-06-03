//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
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
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using Nistec.Generic;
using Nistec.Xml;
using Nistec.Runtime;
using System.Collections.Concurrent;
using Nistec.Data.Entities;
using Nistec.Config;

namespace Nistec.Data.Sqlite
{
   
    



    /// <summary>
    /// Represent a Config file as Dictionary key-value
    /// <example>
    /// <sppSttings>
    /// <myname value='nissim' />
    /// <mycompany value='mcontrol' />
    /// </sppSttings>
    /// </example>
    /// </summary>
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class PersistentDictionary:IDictionary<string,object>
    {

        static readonly PersistentDictionary _Instance = new PersistentDictionary(new XDictionaryettings());

        public static PersistentDictionary Instance
        {
            get { return _Instance; }
        }

        ConcurrentDictionary<string,object> dictionary;
        XDictionaryettings settings;
        string connectionString;
        const string DbProvider = DbLite.ProviderName;

        #region events

        bool ignorEvent = false;
        public event ConfigChangedHandler ItemChanged;
        public event EventHandler ConfigFileChanged;
        public event GenericEventHandler<string> ErrorOcurred;

        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
            {
                ErrorOcurred(this,e);
            }
        }

        protected virtual void OnItemChanged(string key, object value)
        {
            OnItemChanged(new ConfigChangedArgs("xconfig",key, value));
        }

        protected virtual void OnItemChanged(ConfigChangedArgs e)
        {
            if (settings.AutoSave)
            {
                Save();
            }
            if (ItemChanged != null)
                ItemChanged(this, e);
        }

         protected virtual void OnConfigFileChanged(EventArgs e)
        {
            FileToDictionary();

            if (ConfigFileChanged != null)
                ConfigFileChanged(this, e);
        }

        #endregion

        #region ctor
         /// <summary>
        /// PersistentDictionary ctor with a specefied filename
        /// </summary>
        /// <param name="filename"></param>
        public PersistentDictionary(string filename)
        {
            dictionary = new ConcurrentDictionary<string, object>();
            this.settings = new XDictionaryettings();
            if (string.IsNullOrEmpty(filename) == false)
                this.settings.Filename = filename;
            Init();
        }

        /// <summary>
        /// PersistentDictionary ctor with a specefied Dictionary
        /// </summary>
        /// <param name="dict"></param>
        public PersistentDictionary(IDictionary<string, object> dict)
        {
            this.settings = new XDictionaryettings();
            dictionary = dict;
            Init();
        }

        /// <summary>
        /// XConfig ctor with default filename CallingAssembly '.mconfig' in current direcory
        /// </summary>
        public PersistentDictionary(XDictionaryettings settings)
        {
            this.settings = settings;
            dictionary = new Dictionary<string, object>();
        }
        #endregion

        private void Init(string filename)
        {
            DbLite.CreateFile(filename);
            DbLite.ValidateConnection(filename, true);

        }

        #region ConcurrentDictionary<string,object>

        //
        // Summary:
        //     Gets a value that indicates whether the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     is empty.
        //
        // Returns:
        //     true if the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     is empty; otherwise, false.
        public bool IsEmpty { get { return dictionary.IsEmpty; } }

        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key already exists.
        //
        // Parameters:
        //   key:
        //     The key to be added or whose value should be updated
        //
        //   addValueFactory:
        //     The function used to generate a value for an absent key
        //
        //   updateValueFactory:
        //     The function used to generate a new value for an existing key based on the
        //     key's existing value
        //
        // Returns:
        //     The new value for the key. This will be either be the result of addValueFactory
        //     (if the key was absent) or the result of updateValueFactory (if the key was
        //     present).
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-addValueFactory is
        //     a null reference (Nothing in Visual Basic).-or-updateValueFactory is a null
        //     reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        public object AddOrUpdate(string key, Func<string, object> addValueFactory, Func<string, object, object> updateValueFactory)
        {
            return dictionary.AddOrUpdate(key, addValueFactory, updateValueFactory);
        }
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key already exists.
        //
        // Parameters:
        //   key:
        //     The key to be added or whose value should be updated
        //
        //   addValue:
        //     The value to be added for an absent key
        //
        //   updateValueFactory:
        //     The function used to generate a new value for an existing key based on the
        //     key's existing value
        //
        // Returns:
        //     The new value for the key. This will be either be addValue (if the key was
        //     absent) or the result of updateValueFactory (if the key was present).
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-updateValueFactory
        //     is a null reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        public object AddOrUpdate(string key, object addValue, Func<string, object, object> updateValueFactory)
        {
            return dictionary.AddOrUpdate(key,addValue,updateValueFactory);
        }

        public object AddOrUpdate(GenericEntity entity)
        {
            //var value=entity.Record.ToJson();
            string key = entity.PrimaryKey.ToString();
            int res = 0;
            using (var db = new DbLite(connectionString))
            {
                res = db.EntityUpsert(entity);
            }

            if (res > 0)
            {
                dictionary[key] = entity;
            }

            return res;
        }


        //
        // Summary:
        //     Removes all keys and values from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        public void Clear()
        {
            dictionary.Clear();
        }
        //
        // Summary:
        //     Determines whether the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     contains the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        //
        // Returns:
        //     true if the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     contains an element with the specified key; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }
        
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   valueFactory:
        //     The function used to generate a value for the key
        //
        // Returns:
        //     The value for the key. This will be either the existing value for the key
        //     if the key is already in the dictionary, or the new value for the key as
        //     returned by valueFactory if the key was not in the dictionary.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-valueFactory is a null
        //     reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        public object GetOrAdd(string key, Func<string, object> valueFactory)
        {
            return dictionary.GetOrAdd(key,valueFactory);
        }
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   value:
        //     the value to be added, if the key does not already exist
        //
        // Returns:
        //     The value for the key. This will be either the existing value for the key
        //     if the key is already in the dictionary, or the new value if the key was
        //     not in the dictionary.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        public object GetOrAdd(string key, object value)
        {
            return dictionary.GetOrAdd(key, value);

        }
        //
        // Summary:
        //     Copies the key and value pairs stored in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     to a new array.
        //
        // Returns:
        //     A new array containing a snapshot of key and value pairs copied from the
        //     System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        public KeyValuePair<string, object>[] ToArray()
        {
            return dictionary.ToArray();
        }
        //
        // Summary:
        //     Attempts to add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   value:
        //     The value of the element to add. The value can be a null reference (Nothing
        //     in Visual Basic) for reference types.
        //
        // Returns:
        //     true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     successfully. If the key already exists, this method returns false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        public bool TryAdd(string key, object value)
        {
            return dictionary.TryAdd(key, value);
        }
        //
        // Summary:
        //     Attempts to get the value associated with the specified key from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The key of the value to get.
        //
        //   value:
        //     When this method returns, value contains the object from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     with the specified key or the default value of , if the operation failed.
        //
        // Returns:
        //     true if the key was found in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>;
        //     otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).
        public bool TryGetValue(string key, out object value)
        {
            return dictionary.TryGetValue(key, out value);
        }
        //
        // Summary:
        //     Attempts to remove and return the value with the specified key from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The key of the element to remove and return.
        //
        //   value:
        //     When this method returns, value contains the object removed from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     or the default value of if the operation failed.
        //
        // Returns:
        //     true if an object was removed successfully; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).
        public bool TryRemove(string key, out object value)
        {
            return dictionary.TryRemove(key, out value);
        }
        //
        // Summary:
        //     Compares the existing value for the specified key with a specified value,
        //     and if they are equal, updates the key with a third value.
        //
        // Parameters:
        //   key:
        //     The key whose value is compared with comparisonValue and possibly replaced.
        //
        //   newValue:
        //     The value that replaces the value of the element with key if the comparison
        //     results in equality.
        //
        //   comparisonValue:
        //     The value that is compared to the value of the element with key.
        //
        // Returns:
        //     true if the value with key was equal to comparisonValue and replaced with
        //     newValue; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference.
        public bool TryUpdate(string key, object newValue, object comparisonValue)
        {
            return dictionary.TryUpdate(key, newValue, comparisonValue);

        }

        #endregion

        #region IDictionary implemenation

        // Summary:
        //     Gets an System.Collections.Generic.ICollection<T> containing the keys of
        //     the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Returns:
        //     An System.Collections.Generic.ICollection<T> containing the keys of the object
        //     that implements System.Collections.Generic.IDictionary<TKey,TValue>.
        public ICollection<string> Keys { get { return dictionary.Keys; } }
        //
        // Summary:
        //     Gets an System.Collections.Generic.ICollection<T> containing the values in
        //     the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Returns:
        //     An System.Collections.Generic.ICollection<T> containing the values in the
        //     object that implements System.Collections.Generic.IDictionary<TKey,TValue>.
        ICollection<object> Values { get { return dictionary.Values; } }

        // Summary:
        //     Gets or sets the element with the specified key.
        //
        // Parameters:
        //   key:
        //     The key of the element to get or set.
        //
        // Returns:
        //     The element with the specified key.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.Collections.Generic.KeyNotFoundException:
        //     The property is retrieved and key is not found.
        //
        //   System.NotSupportedException:
        //     The property is set and the System.Collections.Generic.IDictionary<TKey,TValue>
        //     is read-only.
        public object this[string key] { get { return dictionary[key]; } set { dictionary[key] = value; } }

        // Summary:
        //     Adds an element with the provided key and value to the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The object to use as the key of the element to add.
        //
        //   value:
        //     The object to use as the value of the element to add.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.ArgumentException:
        //     An element with the same key already exists in the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        //   System.NotSupportedException:
        //     The System.Collections.Generic.IDictionary<TKey,TValue> is read-only.
        public void Add(string key, object value)
        {
            dictionary.Add(key,value);
        }
        //
        // Summary:
        //     Determines whether the System.Collections.Generic.IDictionary<TKey,TValue>
        //     contains an element with the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Returns:
        //     true if the System.Collections.Generic.IDictionary<TKey,TValue> contains
        //     an element with the key; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }
        //
        // Summary:
        //     Removes the element with the specified key from the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The key of the element to remove.
        //
        // Returns:
        //     true if the element is successfully removed; otherwise, false. This method
        //     also returns false if key was not found in the original System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.NotSupportedException:
        //     The System.Collections.Generic.IDictionary<TKey,TValue> is read-only.
        public bool Remove(string key)
        {
            return dictionary.Remove(key);
        }
        //
        // Summary:
        //     Gets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key whose value to get.
        //
        //   value:
        //     When this method returns, the value associated with the specified key, if
        //     the key is found; otherwise, the default value for the type of the value
        //     parameter. This parameter is passed uninitialized.
        //
        // Returns:
        //     true if the object that implements System.Collections.Generic.IDictionary<TKey,TValue>
        //     contains an element with the specified key; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        public bool TryGetValue(string key, out object value)
        {
            return dictionary.TryGetValue(key,out value);
        }

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        // Summary:
        //     Gets the number of elements contained in the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     The number of elements contained in the System.Collections.Generic.ICollection<T>.
        public int Count { get { return dictionary.Count; } }
        //
        // Summary:
        //     Gets a value indicating whether the System.Collections.Generic.ICollection<T>
        //     is read-only.
        //
        // Returns:
        //     true if the System.Collections.Generic.ICollection<T> is read-only; otherwise,
        //     false.
        public bool IsReadOnly { get { return dictionary.IsReadOnly; } }

        // Summary:
        //     Adds an item to the System.Collections.Generic.ICollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to add to the System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public void Add(KeyValuePair<string, object> item)
        {
            dictionary.Add(item);
        }
        //
        // Summary:
        //     Removes all items from the System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public void Clear();
        //
        // Summary:
        //     Determines whether the System.Collections.Generic.ICollection<T> contains
        //     a specific value.
        //
        // Parameters:
        //   item:
        //     The object to locate in the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     true if item is found in the System.Collections.Generic.ICollection<T>; otherwise,
        //     false.
        public bool Contains(KeyValuePair<string, object> item)
        {
            return dictionary.Contains(item);
        }
        //
        // Summary:
        //     Copies the elements of the System.Collections.Generic.ICollection<T> to an
        //     System.Array, starting at a particular System.Array index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional System.Array that is the destination of the elements
        //     copied from System.Collections.Generic.ICollection<T>. The System.Array must
        //     have zero-based indexing.
        //
        //   arrayIndex:
        //     The zero-based index in array at which copying begins.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     arrayIndex is less than 0.
        //
        //   System.ArgumentException:
        //     The number of elements in the source System.Collections.Generic.ICollection<T>
        //     is greater than the available space from arrayIndex to the end of the destination
        //     array.
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array,arrayIndex);
        }
        //
        // Summary:
        //     Removes the first occurrence of a specific object from the System.Collections.Generic.ICollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to remove from the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     true if item was successfully removed from the System.Collections.Generic.ICollection<T>;
        //     otherwise, false. This method also returns false if item is not found in
        //     the original System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public bool Remove(KeyValuePair<string, object> item)
        {
            return dictionary.Remove(item);
        }

        #endregion

        #region IEnumerable<out T> : IEnumerable
       
            // Summary:
            //     Returns an enumerator that iterates through the collection.
            //
            // Returns:
            //     A System.Collections.Generic.IEnumerator<T> that can be used to iterate through
            //     the collection.
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
        

        #endregion

        public void Load(IDictionary<string, object> dict)
        {
            dictionary = dict;
            ignorEvent = true;
            try
            {
                DictionaryToFile();
                FileToDictionary();
            }
            finally
            {
                ignorEvent = false;
            }
        }

        #region watcher

        FileSystemWatcher watcher;

        /// <summary>
        /// Initilaize the file system watcher
        /// </summary>
        void InitWatcher()
        {
            string fpath = Path.GetDirectoryName(settings.Filename);
            string filter = Path.GetExtension(settings.Filename);

             // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = fpath;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter ="*"+ filter;// "*.txt";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Created += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Deleted += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Renamed += new RenamedEventHandler(WatchFile_Renamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            //Console.WriteLine("Press \'q\' to quit the sample.");
            //while (Console.Read() != 'q') ;
        }

        /// <summary>
        /// Occoured when file changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileChanged(FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                OnConfigFileChanged(EventArgs.Empty);
            }
        }
        /// <summary>
        /// Occoured when file renamed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileRenamed(RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            if (e.FullPath == settings.Filename)
            {
                OnConfigFileChanged(EventArgs.Empty);
            }
        }


        void WatchFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == settings.Filename)
            {
                if (ignorEvent == false)
                    OnFileChanged(e);
            }
        }
        void WatchFile_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.OldFullPath == settings.Filename || e.FullPath == settings.Filename)
            {
                OnFileRenamed(e);
            }
        }
        #endregion

        #region properties


        /// <summary>
        /// Get or Set value by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get 
            {
                if (!dictionary.ContainsKey(key))
                {
                    return null;
                }
                return dictionary[key]; 
            }

            set
            {
                if (dictionary.ContainsKey(key))
                {
                    if (dictionary[key] != value)
                    {
                        dictionary[key] = value;
                        OnItemChanged(key, value);
                    }
                }
                else
                {
                    dictionary[key] = value;
                    OnItemChanged(key, value);
                }

            }
        }

        /// <summary>
        /// Get value indicating if the config file exists
        /// </summary>
        public bool FileExists
        {
            get
            {
                return File.Exists(settings.Filename);
            }
        }

 
        /// <summary>
        /// Get all items as IDictionary
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> ToDictionary()
        {
            return dictionary;
        }

        #endregion

        /// <summary>
        /// Read Config File
        /// </summary>
        public void Read()
        {
            try
            {
                FileToDictionary();
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Error occured when try to read Config To Dictionary, Error: " + ex.Message));
            }
        }

        /// <summary>
        /// Save Changes 
        /// </summary>
        public void Save()
        {
            try
            {
                DictionaryToFile();
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Error occured when try to save Dictionary To Config, Error: " + ex.Message));
            }
        }
        /// <summary>
        /// Print all item to Console
        /// </summary>
        public void PrintConfig()
        {
            Console.WriteLine("<" + settings.RootTag + ">");

            foreach (KeyValuePair<string, object> entry in dictionary)
            {
                Console.WriteLine("key={0}, value={1}, Type={2}", entry.Key , entry.Value,entry.Value==null? "String": entry.Value.GetType().ToString());
            }
            Console.WriteLine("</" + settings.RootTag + ">");

        }
   
    
        /// <summary>
        /// Init new config file from dictionary
        /// </summary>
        /// <param name="dict"></param>
        private void Init()
        {
 
            XmlBuilder builder = new XmlBuilder();
            builder.AppendXmlDeclaration();
            builder.AppendEmptyElement(settings.RootTag, 0);
            foreach (KeyValuePair<string,object> entry in dictionary)
            {
                if (entry.Value == null)
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[]{"key", entry.Key,"value", string.Empty, "type", "String"});
                }
                else
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[] { "key", entry.Key, "value", entry.Value.ToString(), "type", entry.Value.GetType().ToString() });
                }
            }

            if (settings.Encrypted)
                EncryptFile(builder.Document.OuterXml);
            else
                builder.Document.Save(settings.Filename);

            if (settings.UseFileWatcher)
                InitWatcher();
        }

        private void FileToDictionary()
        {

            Dictionary<string,object> dict=new Dictionary<string,object>();

            XmlDocument doc = new XmlDocument();

            if (settings.Encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(settings.Filename);

            Console.WriteLine("Load Config: " + settings.Filename);
            //XmlParser parser=new XmlParser(filename);

            XmlNode app = doc.SelectSingleNode("//" + settings.RootTag);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
                var attkey = node.Attributes["key"];
                if (attkey != null)
                    dict[attkey.Value] = GetValue(node);

            }
            dictionary = dict;
        }

        private object GetValue(XmlNode node)
        {
            string value = node.Attributes["value"].Value;
            string type ="string";
            XmlAttribute attrib= node.Attributes["type"];

            if (attrib == null)
            {
                return value;
            }

            type = attrib.Value;

            return Types.StringToObject(type, value);
         }

        private void ValidateConfig()
        {
            if (!FileExists && dictionary.Count > 0)
            {
                Init();
            }
        }

        private void DictionaryToFile()
        {
            ValidateConfig();

            XmlDocument doc = new XmlDocument();
            if (settings.Encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(settings.Filename);

            XmlNode app = doc.SelectSingleNode("//" + settings.RootTag);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
              
                string key=node.Attributes["key"].Value;
                object value=dictionary[key];

                if (value != null)
                {
                  node.Attributes["value"].Value =value.ToString();
                  node.Attributes["type"].Value = value.GetType().ToString();
                }
                else
                {
                    node.Attributes["value"].Value = string.Empty;
                    node.Attributes["type"].Value ="String";
                }
            }

            if (settings.Encrypted)
            {
                EncryptFile(doc.OuterXml);
            }
            else
                doc.Save(settings.Filename);
        }

       

        private string DecryptFile()
        {
            Encryption en = new Encryption(settings.Password);
            return en.DecryptFileToString(settings.Filename, true);
        }

        private bool EncryptFile(string ouput)
        {
            Encryption en = new Encryption(settings.Password);
            return en.EncryptStringToFile(ouput, settings.Filename, true);
        }

     
    }
}
