using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Data.SqlClient;
using Nistec.Data;
using Nistec.Data.Factory;
using System.Data;
using Nistec;
using System.Data.SqlClient;

namespace DataEntityDemo.DalDB
{

    public class DalRuleTest
    {
        public static void RuleTest()
        {
            int AccountId = 7000; int Method = 7; int Count = 1; int ItemUnit = 1;
            decimal price = 0;
            decimal credit = 0;
            int creditState = 0;


            using (DalRule dal = new DalRule())
            {
                dal.tst_trans_InsertExists(943986, DateTime.Now);

            }
            using (DalRule dal = new DalRule())
            {
                dal.AutoCloseConnection = false;
                dal.ValidateCredit(AccountId, (int)Method, Count * ItemUnit, ref price, ref credit, ref creditState);
                
                DataSet ds = dal.AccountsAndUsers(70000);
                foreach (DataTable dt in ds.Tables)
                {

                }

                DataTable dtAcc= dal.Accounts();
                DataTable dtU = dal.Users();

            }

            using (DalRule dal = new DalRule())
            {
                dal.AutoCloseConnection = false;
                DataTable dtAcc = dal.Accounts();
                DataTable dtU = dal.Users();

            }

            Console.WriteLine("creditState:" + creditState.ToString());
        }

    }

    public class DalRule : DbCommand
    {
        const string cnn = "Data Source=???; Initial Catalog=NetcellDB; User ID=???; Password=????; Connection Timeout=30";
        #region ctor

        public DalRule()
            : base(cnn)
        {
        }

        public static DalRule Instance
        {
            get
            {
                return new DalRule();
            }
        }
        #endregion

        [DBCommand(DBCommandType.StoredProcedure, "sp_Credit_Validation")]
        public int Validate_Credit
            (
            [DbField()]int AccountId,
            [DbField()] int MtId,
            [DbField()]int Units,
            [DbField(DbType.Decimal, 12, 4)]ref decimal Price,
            [DbField(DbType.Decimal, 12, 4)]ref decimal Credit,
            [DbField()]ref int CreditStatus
            )
        {
            object[] values = new object[] { AccountId, MtId, Units, Price, Credit, CreditStatus };
            int res = (int)base.Execute(values);
            Price = Types.ToDecimal(values[3], 0M);
            Credit = Types.ToDecimal(values[4], 0M);
            CreditStatus = Types.ToInt(values[5], 0);
            return res;
        }

        public int ValidateCredit3(int AccountId, int Method, int Units, ref decimal Price, ref decimal Credit)
        {
            SqlParameter parameter = new SqlParameter("Price", SqlDbType.Decimal);
            parameter.Direction = ParameterDirection.Output;
            parameter.Precision = 12; parameter.Scale = 4;
            SqlParameter parameter2 = new SqlParameter("Credit", SqlDbType.Decimal);
            parameter2.Direction = ParameterDirection.Output;
            parameter2.Precision = 12; parameter2.Scale = 4;
            object result = null;
            using (SqlCommand cmd = new SqlCommand("sp_Credit_Validation", new SqlConnection(cnn)))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(new SqlParameter[] { new SqlParameter("AccountId", AccountId), new SqlParameter("MtId", Method), new SqlParameter("Units", Units), parameter, parameter2 });
                cmd.Connection.Open();
                result = cmd.ExecuteNonQuery();
            }


            Price = Types.ToDecimal(parameter.Value, 0M);
            Credit = Types.ToDecimal(parameter2.Value, 0M);
            return GenericTypes.Convert<int>(result, 0);
        }

        public int ValidateCredit2(int AccountId, int Method, int Units, ref decimal Price, ref decimal Credit)
        {
            SqlParameter parameter = new SqlParameter("Price", SqlDbType.Decimal);
            parameter.Direction = ParameterDirection.Output;
            parameter.Precision = 4; parameter.Scale = 4;
            SqlParameter parameter2 = new SqlParameter("Credit", SqlDbType.Decimal);
            parameter2.Direction = ParameterDirection.Output;
            parameter2.Precision = 4; parameter2.Scale = 4;
            int res = base.ExecuteScalar<int>("sp_Credit_Validation", new SqlParameter[] { new SqlParameter("AccountId", AccountId), new SqlParameter("MtId", Method), new SqlParameter("Units", Units), parameter, parameter2 },0, CommandType.StoredProcedure, 0);
            Price = Types.ToDecimal(parameter.Value, 0M);
            Credit = Types.ToDecimal(parameter2.Value, 0M);
            return res;
        }

        [DBCommand(DBCommandType.StoredProcedure, "sp_Credit_Validation")]
        public int ValidateCredit
            (
            [DbField()]int AccountId,
            [DbField()] int MtId,
            [DbField()]int Units,
            [DbField(DbType.Decimal, 12, 4)]ref decimal Price,
            [DbField(DbType.Decimal, 12, 4)]ref decimal Credit,
            [DbField()]ref int CreditStatus
            )
        {
            object[] values = new object[] { AccountId, MtId, Units, Price, Credit, CreditStatus };
            int res = (int)base.Execute(values);
            Price = Types.ToDecimal(values[3], 0M);
            Credit = Types.ToDecimal(values[4], 0M);
            CreditStatus = Types.ToInt(values[5], 0);

            return res;
        }

        [DBCommand(DBCommandType.Text, "select * from Accounts")]
        public DataTable Accounts()
        {
            return (DataTable)base.Execute("Accounts", false, null);
        }

        [DBCommand(DBCommandType.Text, "select * from Users")]
        public DataTable Users()
        {
            return (DataTable)base.Execute("Users",false , null);
        }


        [DBCommand(DBCommandType.Text, "select * from Accounts ; select * from Users")]
        public DataSet AccountsAndUsers()
        {
            DataSet ds = (DataSet)base.Execute();
            return base.DataSetTableMapping(ds, true, "Accounts", "Users");

        }

        [DBCommand(DBCommandType.Text, 
            @"select * from Accounts where AccountId=@AccountId; 
            select * from Users where AccountId=@AccountId"
        )]
        public DataSet AccountsAndUsers(int AccountId)
        {
            DataSet ds = (DataSet)base.Execute(AccountId);
            return base.DataSetTableMapping(ds, true, "Accounts", "Users");

        }


       [DBCommand(DBCommandType.InsertOrUpdate, "tst_trans")]
       public int tst_trans_InsertUpdate
      (
      [DbField( DalParamType.Key)] int Id,
      [DbField()]DateTime Creation
      )
        {
            return (int)base.Execute(Id,Creation);

        }

       [DBCommand(DBCommandType.InsertNotExists, "tst_trans")]
       public int tst_trans_InsertExists
      (
      [DbField(DalParamType.Key)] int Id,
      [DbField()]DateTime Creation
      )
       {
           return (int)base.Execute(Id, Creation);

       }
    }
}
