using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{

    public class EntityListItem<T> : IEntityItem
    {
        public T Value { get; set; }
        public string Label { get; set; }

    }
    public class EntityListContext<Dbc> where Dbc : IDbContext
    {

        public static string GetListJson(string valueField, string labelField, string mappingName, params object[] keyvalueParameters)
        {
            string sql = SqlFormatter.CreateCommandText(string.Format("{0} as Value,{1} as Label", valueField, labelField), mappingName, keyvalueParameters);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.QueryJson(sql, keyvalueParameters);
            }
        }
        public static JsonResults GetList(string valueField, string labelField, string mappingName, params object[] keyvalueParameters)
        {
            string sql = SqlFormatter.CreateCommandText(string.Format("{0} as Value,{1} as Label", valueField, labelField), mappingName, keyvalueParameters);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.QueryJsonResults(sql, keyvalueParameters);
            }
        }
        public static IList<string> GetList(string field, string mappingName, params object[] keyvalueParameters)
        {
            string sql = SqlFormatter.CreateCommandText(string.Format("{0} as Value", field), mappingName, keyvalueParameters);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.Query<string>(sql, keyvalueParameters);
            }
        }

    }
    public class EntityListContext<Dbc, T> where Dbc : IDbContext
    {
        public static IList<EntityListItem<T>> GetList(string valueField, string labelField, string mappingName, params object[] keyvalueParameters)
        {
            string sql = SqlFormatter.CreateCommandText(string.Format("{0} as Value,{1} as Label", valueField, labelField), mappingName, keyvalueParameters);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.Query<EntityListItem<T>>(sql, keyvalueParameters);
            }
        }
    }
}
