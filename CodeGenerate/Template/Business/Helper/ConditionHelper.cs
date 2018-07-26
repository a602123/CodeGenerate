using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace #%namespace%#.Business
{
    /// <summary>
    /// 拼接条件的帮助类
    /// </summary>
    public class ConditionHelper
    {
        private string _conditon = "";
        const string LIKE = " {0} Like @{0}";
        //const string LIKE = " {0} Like '%' +@{0}+'%'";
        const string EQUAL = " {0} = @{0}";
        const string UNEQUAL = " {0} <> @{0}";
        const string IN = " {0} IN @{0}";
        const string BIGGER = " {0} > @{0}";
        const string LESS = " {0} < @{0}";
        const string NOLESS = "{0} >= @{0}";
        const string NOBIGGER = "{0} <= @{0}";

        public ConditionHelper()
        {

        }

        public ConditionHelper And(string name, object value, CompareType type)
        {
            if (CheckValue(value))
            {
                if (type == CompareType.Like)
                {
                    value = string.Format("%{0}%", value);
                }
                _conditon = string.Format("{0} AND {1}", _conditon, GetConditionStr(name, type));
            }
            return this;
        }

        public ConditionHelper And(string name,ref string value, CompareType type)
        {
            if (CheckValue(value))
            {
                if (type == CompareType.Like)
                {
                    value = string.Format("%{0}%", value);
                }
                _conditon = string.Format("{0} AND {1}", _conditon, GetConditionStr(name, type));
            }
            return this;
        }


        public ConditionHelper And(ConditionHelper set)
        {
            if (!string.IsNullOrEmpty(set.ToSet()))
            {
                _conditon = string.Format("{0} AND {1}", _conditon, set.ToSet());
            }
            return this;
        }

        public ConditionHelper And(string condition)
        {
            if (!string.IsNullOrEmpty(condition))
            {
                _conditon = string.Format("{0} AND {1}", _conditon, condition);
            }
            return this;
        }

        public ConditionHelper Or(string name, object value, CompareType type)
        {
            if (CheckValue(value))
            {
                _conditon = string.Format("{0} OR {1}", _conditon, GetConditionStr(name, type));
            }

            return this;
        }

        public ConditionHelper Or(ConditionHelper set)
        {
            if (!string.IsNullOrEmpty(set.ToSet()))
            {
                _conditon = string.Format("{0} OR {1}", _conditon, set.ToSet());
            }
            return this;
        }

        public ConditionHelper Or(string condition)
        {
            if (!string.IsNullOrEmpty(condition))
            {
                _conditon = string.Format("{0} OR {1}", _conditon, condition);
            }
            return this;
        }

        private bool CheckValue(object value)
        {
            if (value == null)
            {
                return false;
            }
            if (value is Array && ((Array)value).Length == 0)
            {
                return false;
            }
            if (string.IsNullOrEmpty(value.ToString()))
            {
                return false;
            }
            return true;
        }


        private string GetConditionStr(string name, CompareType type)
        {
            string result = "";
            switch (type)
            {
                case CompareType.Equal:
                    result = string.Format(EQUAL, name);
                    break;
                case CompareType.Unequal:
                    result = string.Format(UNEQUAL, name);
                    break;
                case CompareType.Like:
                    result = string.Format(LIKE, name);
                    break;
                case CompareType.In:
                    result = string.Format(IN, name);
                    break;
                case CompareType.Less:
                    result = string.Format(LESS, name);
                    break;
                case CompareType.Bigger:
                    result = string.Format(BIGGER, name);
                    break;
                case CompareType.NoBigger:
                    result = string.Format(NOBIGGER, name);
                    break;
                case CompareType.NoLess:
                    result = string.Format(NOLESS, name);
                    break;
                default:
                    break;
            }
            return result;
        }

        public override string ToString()
        {
            return _conditon;
        }

        public string ToSet()
        {
            if (string.IsNullOrEmpty(_conditon))
            {
                return "";
            }
            else
            {
                return string.Format("({0})", _conditon.Trim().TrimStart("AND".ToCharArray()).TrimStart(("OR").ToCharArray()));
            }
        }

        public static ConditionHelper Build(object searchObj)
        {
            Type objType = searchObj.GetType();
            ConditionHelper result = new ConditionHelper();
            foreach (PropertyInfo propInfo in objType.GetProperties())
            {
                var attrs = propInfo.GetCustomAttributes(typeof(QueryTypeAttribute), true);
                if (attrs.Length > 0)
                {
                    var value = propInfo.GetValue(searchObj, null);
                    if (value is string)
                    {
                        var str = value as string;
                        result.And(propInfo.Name,ref str, (attrs[0] as QueryTypeAttribute).QueryType);
                    }
                    else
                    {
                        result.And(propInfo.Name, value, (attrs[0] as QueryTypeAttribute).QueryType);
                    }

                   
                }
            }
            return result;

        }
    }

    public enum CompareType
    {
        Equal,
        Unequal,
        Like,
        In,
        Bigger,
        Less,
        NoBigger,
        NoLess
    }

    [AttributeUsage(AttributeTargets.Property ,Inherited = true)]
    public class QueryTypeAttribute : Attribute
    {
        private CompareType _type;
        public QueryTypeAttribute(CompareType type)
        {
            _type = type;
        }

        public CompareType QueryType { get { return _type; } }
    }
}
