//licHeader
//===============================================================================================================
// System  : Nistec.Data - Nistec.Data Class Library
// Author  : 
// Updated : 01/07/2015
// Note    : 
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
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Nistec.Data.Advanced
{

    public class GroupByHelper
    {

        #region static methods

        /// <summary>
        /// SelectGroupByInto
        /// </summary>
        /// <param name="aggregateMode"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="groupByFields"></param>
        /// <param name="sumFields"></param>
        /// <param name="aliasFields"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static DataTable DoSelectGroupByInto(Aggregate mode, DataTable dt, string tableName, string[] groupByFields, string[] sumFields, string[] aliasFields, string filter)
        {
            if (sumFields.Length != aliasFields.Length)
            {
                throw new ArgumentException("sumFields.Length should be equal to aliasFields.Length");
            }
            string fieldList = "";
            string GroupList = "";
            StringBuilder sb = new StringBuilder();
            StringBuilder sbGroup = new StringBuilder();
            foreach (string s in groupByFields)
            {
                sb.AppendFormat("{0},", s);

            }
            GroupList = sb.ToString().TrimEnd(',');
            if (mode != Aggregate.None)
            {
                int i = 0;
                foreach (string s in sumFields)
                {
                    sb.AppendFormat("{0}({1}) {2},", mode.ToString(), s, aliasFields[i]);
                    i++;
                }
            }
            fieldList = sb.ToString().TrimEnd(',');
            return DoSelectGroupByInto(tableName, dt, fieldList, filter, GroupList);

        }

        /// <summary>
        /// SelectGroupByInto
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <param name="groupByField"></param>
        /// <param name="sumField"></param>
        /// <param name="aliasField"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static DataTable DoSelectGroupByInto(Aggregate mode, DataTable dt, string tableName, string groupByField, string sumField, string aliasField, string filter)
        {
            string fieldList = "";
            if (mode == Aggregate.None)
            {
                fieldList = string.Format("{0}", groupByField);
            }
            else
            {
                fieldList = string.Format("{0},{1}({2}) {3}", groupByField, mode.ToString(), sumField, aliasField);
            }
            return DoSelectGroupByInto(tableName, dt, fieldList, filter, groupByField);
        }

        /// <summary>
        /// CreateGroupByTable
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="TableSource"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        public static DataTable DoCreateGroupByTable(string TableName, DataTable TableSource, string FieldList)
        {
            GroupByHelper dsHelper = new GroupByHelper();

            return dsHelper.CreateGroupByTable(TableName, TableSource, FieldList);
        }
        /// <summary>
        /// InsertGroupByInto
        /// </summary>
        /// <param name="DestTable"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="GroupBy"></param>
        public static void DoInsertGroupByInto(DataTable DestTable, DataTable SourceTable, string FieldList, string RowFilter, string GroupBy)
        {
            GroupByHelper dsHelper = new GroupByHelper();

            dsHelper.InsertGroupByInto(DestTable, SourceTable, FieldList, RowFilter, GroupBy);
        }
        /// <summary>
        /// SelectGroupByInto
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="GroupBy"></param>
        /// <returns></returns>
        public static DataTable DoSelectGroupByInto(string TableName, DataTable SourceTable, string FieldList,
    string RowFilter, string GroupBy)
        {
            GroupByHelper dsHelper = new GroupByHelper();

            return dsHelper.SelectGroupByInto(TableName, SourceTable, FieldList, RowFilter, GroupBy);
        }

        #endregion

        private System.Collections.ArrayList m_FieldInfo;
        private string m_FieldList;
        private System.Collections.ArrayList GroupByFieldInfo;
        private string GroupByFieldList;

        /// <summary>
        /// DataSet
        /// </summary>
        public DataSet ds;
        /// <summary>
        /// GroupByHelper ctor
        /// </summary>
        /// <param name="DataSet"></param>
        public GroupByHelper(ref DataSet DataSet)
        {
            ds = DataSet;
        }
        /// <summary>
        /// GroupByHelper ctor
        /// </summary>
        public GroupByHelper()
        {
            ds = null;
        }


        private class FieldInfo
        {
            public string RelationName;
            public string FieldName;	//source table field name
            public string FieldAlias;	//destination table field name
            public string Aggregate;
        }

        private void ParseFieldList(string FieldList, bool AllowRelation)
        {
            /*
             * This code parses FieldList into FieldInfo objects  and then 
             * adds them to the m_FieldInfo private member
             * 
             * FieldList systax:  [relationname.]fieldname[ alias], ...
            */
            if (m_FieldList == FieldList) return;
            m_FieldInfo = new System.Collections.ArrayList();
            m_FieldList = FieldList;
            FieldInfo Field; string[] FieldParts;
            string[] Fields = FieldList.Split(',');
            int i;
            for (i = 0; i <= Fields.Length - 1; i++)
            {
                Field = new FieldInfo();
                //parse FieldAlias
                FieldParts = Fields[i].Trim().Split(' ');
                switch (FieldParts.Length)
                {
                    case 1:
                        //to be set at the end of the loop
                        break;
                    case 2:
                        Field.FieldAlias = FieldParts[1];
                        break;
                    default:
                        throw new Exception("Too many spaces in field definition: '" + Fields[i] + "'.");
                }
                //parse FieldName and RelationName
                FieldParts = FieldParts[0].Split('.');
                switch (FieldParts.Length)
                {
                    case 1:
                        Field.FieldName = FieldParts[0];
                        break;
                    case 2:
                        if (AllowRelation == false)
                            throw new Exception("Relation specifiers not permitted in field list: '" + Fields[i] + "'.");
                        Field.RelationName = FieldParts[0].Trim();
                        Field.FieldName = FieldParts[1].Trim();
                        break;
                    default:
                        throw new Exception("Invalid field definition: " + Fields[i] + "'.");
                }
                if (Field.FieldAlias == null)
                    Field.FieldAlias = Field.FieldName;
                m_FieldInfo.Add(Field);
            }
        }


        /// <summary>
        /// CreateJoinTable
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        public DataTable CreateJoinTable(string TableName, DataTable SourceTable, string FieldList)
        {
            /*
             * Creates a table based on fields of another table and related parent tables
             * 
             * FieldList syntax: [relationname.]fieldname[ alias][,[relationname.]fieldname[ alias]]...
            */
            if (FieldList == null)
            {
                throw new ArgumentException("You must specify at least one field in the field list.");
                //return CreateTable(TableName, SourceTable);
            }
            else
            {
                DataTable dt = new DataTable(TableName);
                ParseFieldList(FieldList, true);
                foreach (FieldInfo Field in m_FieldInfo)
                {
                    if (Field.RelationName == null)
                    {
                        DataColumn dc = SourceTable.Columns[Field.FieldName];
                        dt.Columns.Add(dc.ColumnName, dc.DataType, dc.Expression);
                    }
                    else
                    {
                        DataColumn dc = SourceTable.ParentRelations[Field.RelationName].ParentTable.Columns[Field.FieldName];
                        dt.Columns.Add(dc.ColumnName, dc.DataType, dc.Expression);
                    }
                }
                if (ds != null)
                    ds.Tables.Add(dt);
                return dt;
            }
        }
        /// <summary>
        /// InsertJoinInto
        /// </summary>
        /// <param name="DestTable"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="Sort"></param>
        public void InsertJoinInto(DataTable DestTable, DataTable SourceTable,
            string FieldList, string RowFilter, string Sort)
        {
            /*
            * Copies the selected rows and columns from SourceTable and inserts them into DestTable
            * FieldList has same format as CreatejoinTable
            */
            if (FieldList == null)
            {
                throw new ArgumentException("You must specify at least one field in the field list.");
                //InsertInto(DestTable, SourceTable, RowFilter, Sort);
            }
            else
            {
                ParseFieldList(FieldList, true);
                DataRow[] Rows = SourceTable.Select(RowFilter, Sort);
                foreach (DataRow SourceRow in Rows)
                {
                    DataRow DestRow = DestTable.NewRow();
                    foreach (FieldInfo Field in m_FieldInfo)
                    {
                        if (Field.RelationName == null)
                        {
                            DestRow[Field.FieldName] = SourceRow[Field.FieldName];
                        }
                        else
                        {
                            DataRow ParentRow = SourceRow.GetParentRow(Field.RelationName);
                            DestRow[Field.FieldName] = ParentRow[Field.FieldName];
                        }
                    }
                    DestTable.Rows.Add(DestRow);
                }
            }
        }
        /// <summary>
        /// SelectJoinInto
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="Sort"></param>
        /// <returns></returns>
        public DataTable SelectJoinInto(string TableName, DataTable SourceTable, string FieldList, string RowFilter, string Sort)
        {
            /*
             * Selects sorted, filtered values from one DataTable to another.
             * Allows you to specify relationname.fieldname in the FieldList to include fields from
             *  a parent table. The Sort and Filter only apply to the base table and not to related tables.
            */
            DataTable dt = CreateJoinTable(TableName, SourceTable, FieldList);
            InsertJoinInto(dt, SourceTable, FieldList, RowFilter, Sort);
            return dt;
        }

        /*
        Test the Application
        1. Save and then compile the DataSetHelper class that you created in the previous sections.  
        2. Follow these steps to create a new Visual C# Windows Application: a.  Start Visual Studio .NET. 
        b.  On the File menu, point to New, and then click Project. 
        c.  In the New Project dialog box, click Visual C# Projects under Project Types, and then click Windows Application under Templates. 
 
        3. In Solution Explorer, right-click the solution, and then click Add Existing Project. Add the DataSetHelper project.  
        4. On the Project menu, click Add Reference. 
        5. In the Add Reference dialog box, click the Projects tab, and then add a reference to the DataSetHelper project to the Windows Form application. 
        6. In the form designer, drag three Button controls and a DataGrid control from the toolbox to the form. Name the buttons btnCreateJoin, btnInsertJoinInto, and btnSelectJoinInto. Keep the default name for the DataGrid control (dataGrid1).  
        7. In the form code, add the following Using statement to the top of the Code window:using System.Data;
					
 
        8. Add the following variable declarations to the form definition:DataSet ds; DataSetHelper.DataSetHelper dsHelper;
					
 
        9. Add the following code to the Form_Load event:ds = new DataSet();
        dsHelper = new DataSetHelper.DataSetHelper(ref ds);
        //Create source tables
        DataTable dt = new DataTable("Employees");
        dt.Columns.Add("EmployeeID",Type.GetType("System.Int32") );
        dt.Columns.Add("FirstName", Type.GetType("System.String"));
        dt.Columns.Add("LastName", Type.GetType("System.String"));
        dt.Columns.Add("BirthDate", Type.GetType("System.DateTime"));
        dt.Columns.Add("JobTitle", Type.GetType("System.String"));
        dt.Columns.Add("DepartmentID", Type.GetType("System.Int32"));
        dt.Rows.Add(new object[] {1, "Tommy", "Hill", new DateTime(1970, 12, 31),  "Manager", 42});
        dt.Rows.Add(new object[] {2, "Brooke", "Sheals", new DateTime(1977, 12, 31), "Manager", 23});
        dt.Rows.Add(new object[] {3, "Bill", "Blast", new DateTime(1982, 5, 6), "Sales Clerk", 42});
        dt.Rows.Add(new object[] {1, "Kevin", "Kline", new DateTime(1978, 5, 13), "Sales Clerk", 42});
        dt.Rows.Add(new object[] {1, "Martha", "Seward", new DateTime(1976, 7, 4), "Sales Clerk", 23});
        dt.Rows.Add(new object[] {1, "Dora", "Smith", new DateTime(1985, 10, 22), "Trainee", 42});
        dt.Rows.Add(new object[] {1, "Elvis", "Pressman", new DateTime(1972, 11, 5), "Manager", 15});
        dt.Rows.Add(new object[] {1, "Johnny", "Cache", new DateTime(1984, 1, 23), "Sales Clerk", 15});
        dt.Rows.Add(new object[] {1, "Jean", "Hill", new DateTime(1979, 4, 14), "Sales Clerk", 42});
        dt.Rows.Add(new object[] {1, "Anna", "Smith", new DateTime(1985, 6, 26), "Trainee", 15});
        ds.Tables.Add(dt);

        dt = new DataTable("Departments");
        dt.Columns.Add("DepartmentID", Type.GetType("System.Int32"));
        dt.Columns.Add("DepartmentName", Type.GetType("System.String"));
        dt.Rows.Add(new object[] {15, "Men's Clothing"});
        dt.Rows.Add(new object[] {23, "Women's Clothing"});
        dt.Rows.Add(new object[] {42, "Children's Clothing"});
        ds.Tables.Add(dt);

        ds.Relations.Add("DepartmentEmployee",     ds.Tables["Departments"].Columns["DepartmentID"], 
            ds.Tables["Employees"].Columns["DepartmentID"]);
					
 
        10. Add the following code to the btnCreateJoin_Click event:dsHelper.CreateJoinTable("EmpDept",ds.Tables["Employees"], 
            "FirstName FName,LastName LName,BirthDate,DepartmentEmployee.DepartmentName Department");
        dataGrid1.SetDataBinding(ds, "EmpDept");
					
 
        11. Add the following code to the btnInsertJoinInto_Click event:dsHelper.InsertJoinInto(ds.Tables["EmpDept"], ds.Tables["Employees"], 
            "FirstName FName,LastName LName,BirthDate,DepartmentEmployee.DepartmentName Department",
            "JobTitle='Sales Clerk'", "DepartmentID");
        dataGrid1.SetDataBinding(ds, "EmpDept");
					
 
        12. Add the following code to the btnSelectJoinInto_Click event:dsHelper.SelectJoinInto("EmpDept2", ds.Tables["Employees"],
            "FirstName,LastName,BirthDate BDate,DepartmentEmployee.DepartmentName Department", 
            "JobTitle='Manager'", "DepartmentID");
        dataGrid1.SetDataBinding(ds, "EmpDept2");
					
 
        13. Run the application, and then click each of the buttons. Notice that the DataGrid is populated with the tables and the data from the code.

        NOTE: You can only click the btnCreateJoin and the btnSelectJoinInto buttons one time. If you click either of these buttons more than one time, you receive an error message that you are trying to add the same table two times. Additionally, you must click btnCreateJoin before you click btnInsertJoinInto; otherwise, the destination DataTable is not created. If you click the btnInsertJoinInto button multiple times, you populate the DataGrid with duplicate records.  
        /*
        /*
        Enhancement Ideas
        � The ColumnName and the DataType properties are the only properties that are copied to the destination DataTable. You can extend the CreateTable method to copy additional properties, such as the MaxLength property, or you can create new key columns. 
        � The Expression property is not copied; instead, the evaluated result is copied. Therefore, you do not have to add fields that are referenced by the expression to the destination table. Additionally, the destination column can appear earlier in the result list than any of the columns that this column depends on otherwise. You can modify the CreateTable method to copy the Expression (the InsertInto column ignores columns with an Expression), although this is subject to the limitations that are mentioned earlier in this paragraph. 
        � You can merge the functionality of the CreateJoinTable, the InsertJoinInto, and the SelectJoinInto methods into the CreateTable, the InsertInto, and the SelectInto methods. For additional information about the CreateTable, the InsertInto, and the SelectInto methods, click the article number below to view the article in the Microsoft Knowledge Base: 
        326009 (http://support.microsoft.com/kb/326009/EN-US/) HOWTO: Implement a DataSet SELECT INTO helper class in Visual C# .NET 
        If you do not want to merge these methods, but if you have both sets of methods in a single class, you can enable the CreateJoinTable and the InsertJoinInto methods to handle an empty field list by removing the Throw statements and by uncommenting the calls to the CreateTable and the InsertInto methods in the following lines of code:    if (FieldList==null)
            {
                throw new ArgumentException("You must specify at least one field in the field list.");
                //return CreateTable(TableName, SourceTable);
            }
					
        -and-     if (FieldList==null)
            {
                throw new ArgumentException("You must specify at least one field in the field list.");
                //InsertInto(DestTable, SourceTable, RowFilter, Sort);
            }

        */


        private void ParseGroupByFieldList(string FieldList)
        {
            /*
            * Parses FieldList into FieldInfo objects and adds them to the GroupByFieldInfo private member
            * 
            * FieldList syntax: fieldname[ alias]|operatorname(fieldname)[ alias],...
            * 
            * Supported Operators: count,sum,max,min,first,last
            */
            if (GroupByFieldList == FieldList) return;
            GroupByFieldInfo = new System.Collections.ArrayList();
            FieldInfo Field; string[] FieldParts; string[] Fields = FieldList.Split(',');
            for (int i = 0; i <= Fields.Length - 1; i++)
            {
                Field = new FieldInfo();
                //Parse FieldAlias
                FieldParts = Fields[i].Trim().Split(' ');
                switch (FieldParts.Length)
                {
                    case 1:
                        //to be set at the end of the loop
                        break;
                    case 2:
                        Field.FieldAlias = FieldParts[1];
                        break;
                    default:
                        throw new ArgumentException("Too many spaces in field definition: '" + Fields[i] + "'.");
                }
                //Parse FieldName and Aggregate
                FieldParts = FieldParts[0].Split('(');
                switch (FieldParts.Length)
                {
                    case 1:
                        Field.FieldName = FieldParts[0];
                        break;
                    case 2:
                        Field.Aggregate = FieldParts[0].Trim().ToLower();    //we're doing a case-sensitive comparison later
                        Field.FieldName = FieldParts[1].Trim(' ', ')');
                        break;
                    default:
                        throw new ArgumentException("Invalid field definition: '" + Fields[i] + "'.");
                }
                if (Field.FieldAlias == null)
                {
                    if (Field.Aggregate == null)
                        Field.FieldAlias = Field.FieldName;
                    else
                        Field.FieldAlias = Field.Aggregate + "of" + Field.FieldName;
                }
                GroupByFieldInfo.Add(Field);
            }
            GroupByFieldList = FieldList;
        }


        // CreateGroupByTable Method
        //This section contains the code for the CreateGroupByTable method. 

        //The following is the calling convention for the CreateGroupByTable method: 
        //dt = dsHelper.CreateGroupByTable("OrderSummary", ds.Tables["Orders"], "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max");
        //This call sample creates a new DataTable with a TableName of OrderSummary and four fields (EmployeeID, Total, Min, and Max). The four fields have the same data type as the EmployeeID and the Amount fields in the Orders table.

        //Use the following syntax to specify fields in the field list: 
        //fieldname[ alias]|aggregatefunction(fieldname)[ alias], ...
        //Note the following for this syntax: �	The ColumnName and the DataType properties are the only properties that are copied to the destination DataTable.
        //�	You can rename a field in the destination DataTable by specifying an alias name.
        //�	The field list can contain a subset of field names that are listed in a different order than in the source DataTable. If the field list is blank, an exception is thrown.
        //�	Relation specifiers are not supported as part of the field name. All fields must come from the same DataTable.

        //To call the CreateGroupByTable method, add the following method to the DataSetHelper class that you created in the "" section:

        /// <summary>
        /// CreateGroupByTable
        /// The following is the calling convention for the CreateGroupByTable method: 
        /// dt = dsHelper.CreateGroupByTable("OrderSummary", ds.Tables["Orders"], "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max");
        ///This call sample creates a new DataTable with a TableName of OrderSummary and four fields (EmployeeID, Total, Min, and Max). The four fields have the same data type as the EmployeeID and the Amount fields in the Orders table.
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        public DataTable CreateGroupByTable(string TableName, DataTable SourceTable, string FieldList)
        {
            /*
             * Creates a table based on aggregates of fields of another table
             * 
             * RowFilter affects rows before GroupBy operation. No "Having" support
             * though this can be emulated by subsequent filtering of the table that results
             * 
             *  FieldList syntax: fieldname[ alias]|aggregatefunction(fieldname)[ alias], ...
            */
            if (FieldList == null)
            {
                throw new ArgumentException("You must specify at least one field in the field list.");
                //return CreateTable(TableName, SourceTable);
            }
            else
            {
                DataTable dt = new DataTable(TableName);
                ParseGroupByFieldList(FieldList);
                foreach (FieldInfo Field in GroupByFieldInfo)
                {
                    DataColumn dc = SourceTable.Columns[Field.FieldName];
                    if (Field.Aggregate == null)
                        dt.Columns.Add(Field.FieldAlias, dc.DataType, dc.Expression);
                    else
                    {
                        Type type = dc.DataType;
                        if (type == typeof(byte))
                            type = typeof(int);

                        dt.Columns.Add(Field.FieldAlias, type);
                    }
                }
                if (ds != null)
                    ds.Tables.Add(dt);
                return dt;
            }
        }


        //InsertGroupByInto Method
        //This section contains code for the InsertGroupByInto method.

        //The results are sorted on the fields that are listed in the GroupBy argument. The GroupBy argument must comply with a valid Sort field list (minus ASC and DESC modifiers). If the GroupBy argument is blank, the target DataTable contains only a single record that aggregates all the input. When you call the ParseGroupByFieldList and the ParseFieldList properties, you can parse lists that were previously parsed, if these lists are available. If the field list is blank, an exception is thrown. 

        //This is the calling convention for the InsertGroupByInto method: 
        //dsHelper.InsertGroupByInto(ds.Tables["OrderSummary"], ds.Tables["Orders"],
        //    "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max", "EmployeeID<5", "EmployeeID");

        //This call sample reads records from the DataTable that is named Orders and writes records to the DataTable that is named OrderSummary. The OrderSummary DataTable contains the EmployeeID field and three different aggregates of the Amount field that are filtered on "EmployeeID<5" and that are grouped on (and sorted by) EmployeeID.

        //Note The filter expression is applied before any aggregate functionality. To implement HAVING-type functionality, filter the resultant DataTable. 

        //To call the InsertGroupByInto method, add the following method to the DataSetHelper class that you created in the "" section: 

        /// <summary>
        /// InsertGroupByInto
        /// The results are sorted on the fields that are listed in the GroupBy argument. The GroupBy argument must comply with a valid Sort field list (minus ASC and DESC modifiers). If the GroupBy argument is blank, the target DataTable contains only a single record that aggregates all the input. When you call the ParseGroupByFieldList and the ParseFieldList properties, you can parse lists that were previously parsed, if these lists are available. If the field list is blank, an exception is thrown. 
        /// </summary>
        /// <param name="DestTable"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="GroupBy"></param>
        public void InsertGroupByInto(DataTable DestTable, DataTable SourceTable, string FieldList,
    string RowFilter, string GroupBy)
        {
            /*
             * Copies the selected rows and columns from SourceTable and inserts them into DestTable
             * FieldList has same format as CreateGroupByTable
            */
            if (FieldList == null)
                throw new ArgumentException("You must specify at least one field in the field list.");
            ParseGroupByFieldList(FieldList);	//parse field list
            ParseFieldList(GroupBy, false);			//parse field names to Group By into an arraylist
            DataRow[] Rows = SourceTable.Select(RowFilter, GroupBy);
            DataRow LastSourceRow = null, DestRow = null; bool SameRow; int RowCount = 0;
            foreach (DataRow SourceRow in Rows)
            {
                SameRow = false;
                if (LastSourceRow != null)
                {
                    SameRow = true;
                    foreach (FieldInfo Field in m_FieldInfo)
                    {
                        if (Field.FieldName != "" && !ColumnEqual(LastSourceRow[Field.FieldName], SourceRow[Field.FieldName]))
                        {
                            SameRow = false;
                            break;
                        }
                    }
                    if (!SameRow)
                        DestTable.Rows.Add(DestRow);
                }
                if (!SameRow)
                {
                    DestRow = DestTable.NewRow();
                    RowCount = 0;
                }
                RowCount += 1;
                foreach (FieldInfo Field in GroupByFieldInfo)
                {
                    switch (Field.Aggregate)    //this test is case-sensitive
                    {
                        case null:        //implicit last
                        case "":        //implicit last
                        case "last":
                            DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                            break;
                        case "first":
                            if (RowCount == 1)
                                DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                            break;
                        case "count":
                            DestRow[Field.FieldAlias] = RowCount;
                            break;
                        case "sum":
                            DestRow[Field.FieldAlias] = Add(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                            break;
                        case "max":
                            DestRow[Field.FieldAlias] = Max(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                            break;
                        case "min":
                            if (RowCount == 1)
                                DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
                            else
                                DestRow[Field.FieldAlias] = Min(DestRow[Field.FieldAlias], SourceRow[Field.FieldName]);
                            break;
                    }
                }
                LastSourceRow = SourceRow;
            }
            if (DestRow != null)
                DestTable.Rows.Add(DestRow);
        }


        private FieldInfo LocateFieldInfoByName(System.Collections.ArrayList FieldList, string Name)
        {
            //Looks up a FieldInfo record based on FieldName
            foreach (FieldInfo Field in FieldList)
            {
                if (Field.FieldName == Name)
                    return Field;
            }
            return null;
        }

        private bool ColumnEqual(object a, object b)
        {
            /*
             * Compares two values to see if they are equal. Also compares DBNULL.Value.
             * 
             * Note: If your DataTable contains object fields, you must extend this
             * function to handle them in a meaningful way if you intend to group on them.
            */
            if ((a is DBNull) && (b is DBNull))
                return true;    //both are null
            if ((a is DBNull) || (b is DBNull))
                return false;    //only one is null
            //bool ok = a.Equals(b);
            return a.Equals(b); //(a == b);    //value type standard comparison
        }

        private object Min(object a, object b)
        {
            //Returns MIN of two values - DBNull is less than all others
            if ((a is DBNull) || (b is DBNull))
                return DBNull.Value;
            if (((IComparable)a).CompareTo(b) == -1)
                return a;
            else
                return b;
        }

        private object Max(object a, object b)
        {
            //Returns Max of two values - DBNull is less than all others
            if (a is DBNull)
                return b;
            if (b is DBNull)
                return a;
            if (((IComparable)a).CompareTo(b) == 1)
                return a;
            else
                return b;
        }

        private object Add(object a, object b)
        {
            //Adds two values - if one is DBNull, then returns the other
            if (a is DBNull)
                return b;
            if (b is DBNull)
                return a;
            return (decimal.Parse(a.ToString())) + (decimal.Parse(b.ToString()));
            //return ((decimal)a + (decimal)b);
        }


        //SelectGroupByInto Method
        //This section contains the code for the SelectGroupByInto method. This method is a combination of the CreateGroupByTable and the InsertGroupByInto methods. The SelectGroupByInto method creates a new DataTable based on existing DataTable objects, and copies the records that are sorted and filtered to the new DataTable. 

        //The following is the calling convention for the SelectGroupByInto method: 
        //dt = dsHelper.SelectGroupByInto("OrderSummary", ds.Tables["Employees"],
        //    "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max", "EmployeeID<5", "EmployeeID");
        //This call sample creates a new DataTable with a TableName of OrderSummary and four fields (EmployeeID, Total, Min, and Max). These four fields have the same data type as the EmployeeID and the Amount fields in the Orders table. Then this sample reads records from the Orders DataTable, and writes records to the OrderSummary DataTable. The OrderSummary DataTable contains the EmployeeID field and three different aggregates of the Amount field that are filtered on "EmployeeID<5" and that are grouped on (and sorted by) EmployeeID. If the GroupBy argument is blank, the target DataTable contains only a single record that aggregates all the input.

        //Note The filter expression is applied before any aggregate functionality. To implement HAVING-type functionality, filter the DataTable that results. 

        //To call the SelectGroupByInto method, add the following method to the DataSetHelper class that you created in the "" section:

        /// <summary>
        /// SelectGroupByInto 
        /// This method is a combination of the CreateGroupByTable and the InsertGroupByInto methods. The SelectGroupByInto method creates a new DataTable based on existing DataTable objects, and copies the records that are sorted and filtered to the new DataTable. 
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="SourceTable"></param>
        /// <param name="FieldList"></param>
        /// <param name="RowFilter"></param>
        /// <param name="GroupBy"></param>
        /// <returns></returns>
        public DataTable SelectGroupByInto(string TableName, DataTable SourceTable, string FieldList,
           string RowFilter, string GroupBy)
        {
            /*
             * Selects data from one DataTable to another and performs various aggregate functions
             * along the way. See InsertGroupByInto and ParseGroupByFieldList for supported aggregate functions.
             */
            DataTable dt = CreateGroupByTable(TableName, SourceTable, FieldList);
            InsertGroupByInto(dt, SourceTable, FieldList, RowFilter, GroupBy);
            return dt;
        }



        // Test the Application1.	Save, and then compile the DataSetHelper class that you created in the previous sections.
        //2.	Follow these steps to create a new Visual C# Windows Application: 	a. 	Start Visual Studio .NET.
        //b. 	On the File menu, point to New, and then click Project.
        //c. 	In the New Project dialog box, click Visual C# Projects under Project Types, and then click Windows Application under Templates.

        //3.	In Solution Explorer, right-click the solution, and then click Add Existing Project. Add the DataSetHelper project. 
        //4.	On the Project menu, click Add Reference.
        //5.	In the Add Reference dialog box, click the Projects tab, and then add a reference to the DataSetHelper project to the Windows Form application.
        //6.	In the form designer, drag three Button controls and a DataGrid control from the toolbox to the form. Name the buttons btnCreateGroupBy, btnInsertGroupByInto, and btnSelectGroupByInto. Keep the default name for the DataGrid control (DataGrid1). 
        //7.	In the form code, add the following using statement to the top of the code window: 
        //using System.Data;
        //8.	Add the following variable declarations to the form definition: 
        //DataSet ds; DataSetHelper.DataSetHelper dsHelper;
        //9.	Add the following code to the Form_Load event: 
        //ds = new DataSet();
        //dsHelper = new DataSetHelper.DataSetHelper(ref ds);
        ////Create the source table
        //DataTable dt = new DataTable("Orders");
        //dt.Columns.Add("EmployeeID", Type.GetType("System.String"));
        //dt.Columns.Add("OrderID", Type.GetType("System.Int32"));
        //dt.Columns.Add("Amount", Type.GetType("System.Decimal"));
        //dt.Rows.Add(new object[] {"Sam", 5, 25.00});
        //dt.Rows.Add(new object[] {"Tom", 7, 50.00});
        //dt.Rows.Add(new object[] {"Sue", 9, 11.00});
        //dt.Rows.Add(new object[] {"Tom", 12, 7.00});
        //dt.Rows.Add(new object[] {"Sam", 14, 512.00});
        //dt.Rows.Add(new object[] {"Sue", 15, 17.00});
        //dt.Rows.Add(new object[] {"Sue", 22, 2.50});
        //dt.Rows.Add(new object[] {"Tom", 24, 3.00});
        //dt.Rows.Add(new object[] {"Tom", 33, 78.75});
        //ds.Tables.Add(dt);
        //10.	Add the following code to the btnCreateGroupBy.Click event: 
        //dsHelper.CreateGroupByTable("OrderSummary", ds.Tables["Orders"], 
        //    "EmployeeID,count(EmployeeID) Orders,Sum(Amount) OrderTotal,max(Amount) BestOrder,min(Amount) WorstOrder");
        //dataGrid1.SetDataBinding(ds, "OrderSummary");
        //11.	Add the following code to the btnInsertGroupByInto.Click event: 
        //dsHelper.InsertGroupByInto(ds.Tables["OrderSummary"], ds.Tables["Orders"], 
        //    "EmployeeID,count(EmployeeID) Orders,sum(Amount) OrderTotal,max(Amount) BestOrder,min(Amount) WorstOrder", 
        //    "", "EmployeeID");
        //dataGrid1.SetDataBinding(ds, "OrderSummary");
        //12.	Add the following code to the btnSelectGroupByInto.Click event: 
        //dsHelper.SelectGroupByInto("OrderSummary2", ds.Tables["Orders"], 
        //    "EmployeeID,count(EmployeeID) Orders,sum(Amount) OrderTotal,max(Amount) BestOrder,min(Amount) WorstOrder", 
        //    "OrderID>10", "EmployeeID");
        //dataGrid1.SetDataBinding(ds, "OrderSummary2");
        //13.	Run the application, and then click each of the buttons. Notice that the DataGrid is populated with the tables and data from the code.
        //Note You can only click the btnCreateGroupBy and the btnSelectGroupByInto buttons one time. If you click either of these buttons more than one time, you receive an error message that you are trying to add the same table two times. Additionally, you must click btnCreateGroupBy before you click btnInsertGroupByInto; otherwise, the destination DataTable is not created. If you click the btnInsertGroupByInto button multiple times, you populate the DataGrid with duplicate records. 

    }
}