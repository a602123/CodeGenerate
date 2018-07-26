using Dapper;
using #%namespace%#.Model;
using #%namespace%#.Model.DBModels;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace #%namespace%#.DataProvider
{
    public abstract class BaseDataProvider
    {
        public BaseDataProvider(string tableName)
        {
            _tableName = tableName;
        }

        private string _tableName;


        protected abstract string InsertStr { get; }

        protected abstract string FieldsStr { get; }

        const string COUNTSQL = "SELECT COUNT(*) FROM {0} WHERE  1=1 {1};";

        const string PAGESQL = "SELECT * FROM ( SELECT {0}  FROM {2} WHERE 1=1 {3} {1})b limit {4} , {5}; ";

        const string SELECT = "SELECT {0} FROM {1} WHERE 1=1 {2} {3}";

        const string UPDATE = "UPDATE {0} SET {1} WHERE 1=1 {2}";

        const string DELETE = "DELETE FROM {0} WHERE 1=1 {1}";

        const string INSERT = "INSERT INTO {0} ({1}) VALUES ({2})";

        const string DISTRICT = "SELECT DISTINCT {0} FROM {1} WHERE 1=1 {2}";

        #region GetSelect
        protected string GetSelectSql(string fields, string condition)
        {
            return GetSelectSqlByName(_tableName, fields, condition, "");
        }

        protected string GetSelectSql(string fields, string condition, string order)
        {
            return GetSelectSqlByName(_tableName, fields, condition, order);
        }

        protected string GetSelectSqlByName(string tableName, string fields, string condition)
        {
            return string.Format(SELECT, fields, tableName, condition, "");
        }

        protected string GetSelectSqlByName(string tableName, string fields, string condition, string order)
        {
            return string.Format(SELECT, fields, tableName, condition, order);
        }

        protected string GetDeleteSql(string condition)
        {
            return GetDeleteSqlByName(_tableName, condition);
        }


        protected string GetDeleteSqlByName(string tableName, string condition)
        {
            return string.Format(DELETE, tableName, condition);
        }

        protected string GetUpdateSql(string fields, string condition)
        {
            return string.Format(UPDATE, _tableName, fields, condition);
        }

        protected string GetPageSql(string fields, string order, string condition, int begin, int end)
        {
            string countStr = string.Format(COUNTSQL, _tableName, condition);
            string rowsStr = string.Format(PAGESQL, fields, order, _tableName, condition, begin, end);
            return string.Format("{0}{1}", countStr, rowsStr);
        }

        protected string GetPageSqlFromView(string viewName, string fields, string order, string condition, int begin, int end)
        {
            string countStr = string.Format(COUNTSQL, viewName, condition);
            string rowsStr = string.Format(PAGESQL, fields, order, viewName, condition, begin, end);
            return string.Format("{0}{1}", countStr, rowsStr);
        }


        protected string GetMallPageSql(string fields, string order, string condition, int begin, int end)
        {
            string rowsStr = string.Format(PAGESQL, fields, order, _tableName, condition, begin.ToString(), end.ToString());
            return rowsStr;
        }

        protected string GetMallPageSqlFromView(string viewName, string fields, string order, string condition, int begin, int end)
        {
            string rowsStr = string.Format(PAGESQL, fields, order, viewName, condition, begin, end);
            return rowsStr;
        }

        protected string GetDistrictSql(string fieldName, string condition,string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                return string.Format(DISTRICT, fieldName, _tableName, condition);
            }
            else
            {
                return string.Format(DISTRICT, fieldName, viewName, condition);
            }
        }
        #endregion



        #region 公开的方法

        public bool Insert(object model)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Execute(InsertStr, model) > 0;
            }
        }

        public bool InsertList(IEnumerable<object> models)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Execute(InsertStr, models) == models.Count();
            }
        }

        public bool Insert(string fields, string values, object model = null)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Execute(string.Format(INSERT, _tableName, fields, values), model) > 0;
            }
        }

        public bool Delete(string condition)
        {
            string sql = GetDeleteSql(condition);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Execute(sql) > 0;
            }
        }

        public bool Delete(string condition,object model)
        {
            string sql = GetDeleteSql(condition);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Execute(sql,model) > 0;
            }
        }

        public bool Update(string condition, string fields = "", object model=null)
        {
            if (string.IsNullOrEmpty(fields))
            {
                fields = FieldsStr;
            }
            string sql = GetUpdateSql(fields, condition);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Execute(sql, model) > 0;
            }
        }

        public IEnumerable<T> GetList<T>(string condition, object searchObj = null)
        {
            string sql = GetSelectSql("*", condition);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Query<T>(sql, searchObj);
            }
        }


        public  T GetField<T>(string field, string condition, object searchObj = null)
        {
            string sql = GetSelectSql(field, condition);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                var result =  conn.Query<T>(sql, searchObj);
                return result.FirstOrDefault();
            }
        }

        public IEnumerable<T> GetList<T>(string condition, object searchObj, string viewName="",string order = "")
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = _tableName;
            }
       
            string sql = GetSelectSqlByName(viewName, "*", condition,order);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Query<T>(sql, searchObj);
            }
        }

        public int GetCount(string condition, object searchObj=null)
        {
            string sql = GetSelectSql("count(*)", condition);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                var result =  conn.Query<int>(sql, searchObj);
                return result.FirstOrDefault();
            }
        }


        public PageableData<T> GetPage<T>(string condition, object searchObj, string order, int begin, int end, string viewName="")
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = _tableName;
            }
            string sql = GetPageSqlFromView(viewName, "*", order, condition, begin, end);
            PageableData<T> result = null;
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                var reader = conn.QueryMultiple(sql, searchObj);
                result = new PageableData<T>()
                {
                    total = reader.Read<int>().FirstOrDefault(),
                    rows = reader.Read<T>()
                };
            }
            return result;
        }

        public T GetItem<T>(string condition, object searchModel, string viewName="")
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = _tableName;
            }
            string sql = GetSelectSqlByName(viewName, "*", condition);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                var result = conn.Query<T>(sql, searchModel);
                return result.FirstOrDefault();
            }
        }

        public void ExecSql(string sql)
        {
            
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                conn.Execute(sql);
            }
        }

        public IEnumerable<R> GetDistrict<R>(string fieldName,string condition = "", object searchModel = null,string viewName = "")
        {
            string sql = GetDistrictSql(fieldName,condition,viewName);
            using (MySqlConnection conn = new MySqlConnection(ConnStrManage.DB))
            {
                return conn.Query<R>(sql, searchModel);
            }
        }
        #endregion

        #region EF方法

        public T InsertByEF<T>(T model, Expression<Func<T, bool>> sameValueWhere) where T : class
        {
            using (var db = new DBContext())
            {
                if (sameValueWhere != null)
                {
                    if (db.Set<T>().Where(sameValueWhere).Count() > 0)
                    {
                        throw new SameValueException();
                    }
                }
                db.Set<T>().Add(model);
                db.SaveChanges();
                return model;
            }
        }

        public bool InsertListByEF<T>(List<T> models) where T : class
        {
            using (var db = new DBContext())
            {
                db.Set<T>().AddRange(models);
                return db.SaveChanges() == models.Count;
            }
        }

        public bool UpdateByEF<T>(T model, Expression<Func<T, bool>> sameValueWhere) where T : class
        {
            using (var db = new DBContext())
            {
                if (db.Set<T>().Where(sameValueWhere).Count() > 0)
                {
                    throw new SameValueException();
                }
                db.Set<T>().Attach(model);
                db.Entry<T>(model).State = EntityState.Modified;
                return db.SaveChanges() > 0;
            }

        }

        #endregion
    }
}
