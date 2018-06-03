using System;
using System.Data;
using System.ComponentModel;
using System.Collections;
using System.Reflection;

using Nistec.Data;

namespace DataEntityDemo.Ole
{
	using System.Data.OleDb;
    using Nistec.Data.OleDb;
    using Nistec.Data.Factory;
    using DataEntityDemo.DB;
    using Nistec.Generic;


     public class DBConfig
    {
 
        public static string ConnectionString
        {
            get {return NetConfig.ConnectionString("Norhwind"); }
        }

        public static string GetConnectionString()
        {
            string ComoonFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
            string DBpath = ComoonFolder + @"\Nistec\Data\NorhwindDB.mdb";
            return string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};", DBpath);
        }

    }
	
	#region OleDb.DALBase

	/// <summary>
	/// Data class.
	/// </summary>
     public sealed class NorhwindBase : Nistec.Data.Factory.AutoBase<Norhwind>  
	{
         public static readonly NorhwindBase DB = new NorhwindBase();

         static NorhwindBase()
        {
            //bool isGac = System.Reflection.Assembly.GetAssembly(typeof(Nistec.Data.Common.DalDB)).GlobalAssemblyCache;
            //DB.Init(DBProvider.OleDb, DBConfig.ConnectionString, true, true);
            //DB.Init("AdventureWorks", true, true);
            //DB.Init<AdventureWorks>( true, true);
        }

         public NorhwindDB Norhwind { get { return DB.CreateInstance<NorhwindDB>(); } }

	}

	#endregion

	#region OleDb.DBBase

	/// <summary>
	/// dalApplication class.
	/// </summary>
    public abstract class NorhwindDB : Nistec.Data.Factory.AutoDb
	{

		#region Sample

		[DBCommand("SELECT * FROM [TblIni]")]
		public abstract DataTable TblIni();


		[DBCommand("SELECT * FROM [Customers]")]
		public abstract DataTable Customers();

		[DBCommand("SELECT * FROM [Orders]")]
		public abstract DataTable Orders();

		[DBCommand("SELECT * FROM [Shippers]")]
		public abstract DataTable Shippers();

		[DBCommand("SELECT * FROM [Order Details]")]
		public abstract DataTable OrdersDetails();

		[DBCommand(DBCommandType.Text,"UPDATE [Products] SET [UnitPrice]=@UnitPrice WHERE [ProductID]=@ProductID")]
		public abstract int UpdateProductUnitPrice (double UnitPrice,int ProductID);

		[DBCommand(DBCommandType.Insert, "Shippers")]
		public abstract int ShiperInsert
			(
			[DbField(40)]string CompanyName,
			[DbField(24)]string Phone
			);

		[DBCommand(DBCommandType.Update , "Shippers")]
		public abstract int ShiperUpdate
			(
			[DbField(DalParamType.Key)] int ShipperID,
			[DbField(40)]string CompanyName,
			[DbField(24)]string Phone
			);

		#endregion
		
	}
	#endregion

}



