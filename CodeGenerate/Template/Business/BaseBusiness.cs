using Dapper;
using #%namespace%#.DataProvider;
using #%namespace%#.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace #%namespace%#.Business
{
    public class BaseBusiness<T> where T : class
    {
        protected BaseDataProvider _provider;

        public event Action<T> BeginInsert;

        public event Action<T> EndInsert;

        public event Action<T> BeginUpdate;

        public event Action<T> EndUpdate;

        public event Action<int[]> BeginDel;

        public event Action EndDel;





        /// <summary>
        /// 获取，只适用于主键为Id的获取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewName">试图名</param>
        /// <returns></returns>
        public T GetItem(long id, string viewName = "")
        {
            string condition = "AND Id = @Id";
            return _provider.GetItem<T>(condition, new { Id = id }, viewName);
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="idFiledName">id的字段名</param>
        /// <param name="viewName">试图名</param>
        /// <returns></returns>
        public T GetItem(object id, string idFiledName, string viewName = "")
        {
            string condition = new ConditionHelper().And(idFiledName, id, CompareType.Equal).ToString();
            DynamicParameters p = new DynamicParameters();
            p.Add($"@{idFiledName}", id);
            return _provider.GetItem<T>(condition, p, viewName);
        }


        /// <summary>
        /// 删除，只适用于主键为Id的删除
        /// </summary>
        /// <param name="ids"></param>
        public bool Del(int[] ids)
        {
            string condition = new ConditionHelper().And("Id", new { Id = ids }, CompareType.In).ToString();
            return _provider.Delete(condition, new { Id = ids });
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="idFiledName">id的字段名</param>
        /// <param name="ids">ids</param>
        /// <returns></returns>
        public bool Del<R>(string idFiledName, params R[] ids) where R : struct
        {
            if (BeginDel != null)
            {
                BeginDel.Invoke(ids as int[]);
            }
            string condition = new ConditionHelper().And(idFiledName, ids, CompareType.In).ToString();
            DynamicParameters p = new DynamicParameters();
            p.Add($"@{idFiledName}", ids);
            if (_provider.Delete(condition, p))
            {
                if (EndDel != null)
                {
                    EndDel.Invoke();
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(T model, string incrementFieldName = null, string sameValuePropertyName = null)
        {
            if (!string.IsNullOrEmpty(sameValuePropertyName))
            {
                PropertyInfo property = model.GetType().GetProperty(sameValuePropertyName);
                string check = new ConditionHelper().And(sameValuePropertyName, property.GetValue(model, null), CompareType.Equal).ToString();

                DynamicParameters p = new DynamicParameters();
                p.Add($"@{sameValuePropertyName}", property.GetValue(model, null));

                if (_provider.GetItem<T>(check, p) != null)
                {
                    throw new SameValueException();
                }
            }
            List<string> fieldsBuilder = new List<string>();
            List<string> valuesBuilder = new List<string>();
            if (string.IsNullOrEmpty(incrementFieldName))
            {
                Type type = typeof(T);
                PropertyInfo[] propertys = type.GetProperties();

                foreach (var property in propertys)
                {

                    fieldsBuilder.Add($"{property.Name}");
                    valuesBuilder.Add($"@{property.Name}");
                }
            }
            else
            {
                Type type = typeof(T);
                PropertyInfo[] propertys = type.GetProperties();
                foreach (var property in propertys)
                {
                    if (property.Name != incrementFieldName)
                    {
                        fieldsBuilder.Add($"{property.Name}");
                        valuesBuilder.Add($"@{property.Name}");
                    }
                }
            }
            return _provider.Insert(string.Join(",", fieldsBuilder), string.Join(",", valuesBuilder), model);
        }

        /// <summary>
        /// 使用EF进行添加
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sameValueWhere"></param>
        public void InsertByEF(T model, Expression<Func<T, bool>> sameValueWhere)
        {
            if (BeginInsert != null)
            {
                BeginInsert.Invoke(model);
            }

            var inserted = _provider.InsertByEF(model, sameValueWhere);
            if (EndInsert != null)
            {
                EndInsert.Invoke(inserted);
            }
        }



        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model">对象</param>
        /// <param name="fields">更新字段</param>
        /// <param name="idPropertyName">Id字段名</param>
        /// <param name="sameValuePropertyName">检测相同值字段名</param>
        /// <returns></returns>
        public bool Update(T model, string[] fields = null, string idPropertyName = "Id", string sameValuePropertyName = "")
        {
            if (BeginUpdate != null)
            {
                BeginUpdate.Invoke(model);
            }

            PropertyInfo editProperty = model.GetType().GetProperty("EditTime");
            if (editProperty != null)
            {
                editProperty.SetValue(model, DateTime.Now, null);
            }

            PropertyInfo propertyId = model.GetType().GetProperty(idPropertyName);
            if (!string.IsNullOrEmpty(sameValuePropertyName))
            {
                PropertyInfo propertySameValue = model.GetType().GetProperty(sameValuePropertyName);
                string check = new ConditionHelper().And(idPropertyName, propertyId.GetValue(model, null), CompareType.Unequal)
                            .And(sameValuePropertyName, propertySameValue.GetValue(model, null), CompareType.Equal).ToString();

                DynamicParameters p = new DynamicParameters();
                p.Add($"@{idPropertyName}", propertyId.GetValue(model, null));
                p.Add($"@{sameValuePropertyName}", propertySameValue.GetValue(model, null));

                if (_provider.GetItem<T>(check, p) != null)
                {
                    throw new SameValueException();
                }

            }
            string fieldsStr = "";
            if (fields != null)
            {
                List<string> builder = new List<string>();
                foreach (var field in fields)
                {
                    builder.Add($"{field} = @{field}");
                }
                fieldsStr = string.Join(",", builder.ToArray());
            }
            else
            {
                Type type = typeof(T);
                PropertyInfo[] propertys = type.GetProperties();
                List<string> builder = new List<string>();
                foreach (var property in propertys)
                {
                    if (property.Name != sameValuePropertyName)
                    {
                        builder.Add($"{property.Name} = @{property.Name}");
                    }
                }
                fieldsStr = string.Join(",", builder);

            }
            string condition = new ConditionHelper().And(idPropertyName, propertyId.GetValue(model, null), CompareType.Equal).ToString();
            if (_provider.Update(condition, fieldsStr, model))
            {
                if (EndUpdate != null)
                {
                    EndUpdate.Invoke(model);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 通过EF进行更新
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sameValueWhere"></param>
        public void UpdateByEF(T model, Expression<Func<T, bool>> sameValueWhere)
        {
            if (BeginUpdate != null)
            {
                BeginUpdate.Invoke(model);
            }

            PropertyInfo editProperty = model.GetType().GetProperty("EditTime");

            if (editProperty != null)
            {
                editProperty.SetValue(model, DateTime.Now, null);
            }

            if (_provider.UpdateByEF(model, sameValueWhere))
            {
                if (EndUpdate != null)
                {
                    EndUpdate.Invoke(model);
                }
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="offset">略过</param>
        /// <param name="limit">取</param>
        /// <param name="searchModel">查询对象(需要查询的字段需要添加QueryType属性)</param>
        /// <param name="order">排序（order by id desc）</param>
        /// <returns></returns>
        public PageableData<T> GetPage(int offset, int limit, object searchModel, string order = "")
        {
            string condition = ConditionHelper.Build(searchModel).ToString();

            return _provider.GetPage<T>(condition, searchModel, order, offset, limit);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        public IEnumerable<T> GetList(string condition)
        {
            return _provider.GetList<T>(condition);
        }

        public IEnumerable<T> GetList(string condition, object searchModel, string viewName = "")
        {
            return _provider.GetList<T>(condition, searchModel, viewName);
        }
        public IEnumerable<T> GetList(string condition, DynamicParameters searchModel, string viewName = "", string order = "")
        {
            return _provider.GetList<T>(condition, searchModel, viewName, order);
        }

        public IEnumerable<R> GetDistrictField<R>(string fieldName, string condition = null, object searchModel = null, string viewName = "")
        {
            return _provider.GetDistrict<R>(fieldName, condition, searchModel, viewName);
        }
    }
}
