using Dapper;
using General.Data.SQLConditionConverter;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.DAL.Extension
{
    public static class ObjectExtension
    {
        public static dynamic ToDynamic<T>(this T obj)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var currentValue = propertyInfo.GetValue(obj);
                expando.Add(propertyInfo.Name, currentValue);
            }
            return expando as ExpandoObject;
        }
        /// <summary>
        /// 物件複製
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Clone<T>(this object original)
        {
            var raw = (T)original;
            return raw.DeepCopyByExpressionTree<T>();
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }
        public static DynamicParameters ConvertDapperParameters(this QueryParameters param)
        {
            DynamicParameters dapperparams = new DynamicParameters();
            foreach (var item in param)
            {
                dapperparams.Add(item.Name, item.Value, item.DbType);
            }
            return dapperparams;
        }
    }
}
