using Dapper;
using #%namespace%#.Business;
using #%namespace%#.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace #%namespace%#.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<T,B>: ControllerBase where T:class where B:BaseBusiness<T>
    {
        protected B _business;


        public BaseController(string viewName = null, string delFiledName = "Id", string orderStr = "")
        {
            _viewName = viewName;
            _delFiledName = delFiledName;
            _orderStr = orderStr;
            _business = System.Activator.CreateInstance<B>();
        }

        #region 属性
        private string _viewName;
        public string ViewName
        {
            get
            {
                return _viewName;
            }

        }

        public string DelFiledName
        {
            get
            {
                return _delFiledName;
            }
        }

        private string _delFiledName;

        private string _orderStr;
        public string OrderStr
        {
            get
            {
                return _orderStr;
            }
        }
        #endregion

        
        [HttpGet]
        public GeneralResultModel GetAllItems()
        {
            try
            {
                if (string.IsNullOrEmpty(OrderStr))
                {
                    return new GeneralResultModel() { Code = ResultCodeEnum.Success, Data = _business.GetList("", null, viewName: _viewName) };
                }
                else
                {
                    return new GeneralResultModel() { Code = ResultCodeEnum.Success, Data = _business.GetList("", null, viewName: _viewName, order: OrderStr) };
                }
            }
            catch (Exception ex)
            {

                return new GeneralResultModel() { Code = ResultCodeEnum.Fail, Message = ex.Message };
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">这里id指的INCID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public GeneralResultModel GetItem(int id)
        {
            try
            {
                if (string.IsNullOrEmpty(ViewName))
                {
                    return new GeneralResultModel() { Code = ResultCodeEnum.Success, Data = _business.GetItem(id, idFiledName: "Id") };
                }
                else
                {
                    return new GeneralResultModel() { Code = ResultCodeEnum.Success, Data = _business.GetItem(id, "Id", ViewName) };
                }

            }
            catch (Exception ex)
            {

                return new GeneralResultModel() { Code = ResultCodeEnum.Fail, Message = ex.Message };
            }
        }

        /// <summary>
        /// 111
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nameFieldName">不用填即可</param>
        /// <returns></returns>
        [HttpGet("GetItemByName/{name}")]
        public virtual GeneralResultModel GetItemByName(string name,bool state=true,string nameFieldName="Name")
        {
            try
            {
                var p = new DynamicParameters();
                p.Add($"{nameFieldName}", $"%{name}%");
                if (string.IsNullOrEmpty(ViewName))
                {
                    return new GeneralResultModel() { Code = ResultCodeEnum.Success, Data = _business.GetList(new ConditionHelper().And(nameFieldName, name, CompareType.Like).ToString(), p) };
                }
                else
                {
                    return new GeneralResultModel() { Code = ResultCodeEnum.Success, Data = _business.GetList(new ConditionHelper().And(nameFieldName, name, CompareType.Like).ToString(), p, _viewName) };
                }

            }
            catch (Exception ex)
            {

                return new GeneralResultModel() { Code = ResultCodeEnum.Fail, Message = ex.Message };
            }
        }

        [HttpDelete]
        public GeneralResultModel Del(params int[] ids)
        {
            try
            {
                if (ids != null && ids.Count() != 0 && _business.Del(_delFiledName, ids))
                {
                    return new GeneralResultModel() { Code = ResultCodeEnum.Success };
                }
                else
                {
                    return new GeneralResultModel() { Code = ResultCodeEnum.Fail };
                }

            }
            catch (Exception ex)
            {

                return new GeneralResultModel() { Code = ResultCodeEnum.Fail, Message = ex.Message };
            }

        }

    }
}
