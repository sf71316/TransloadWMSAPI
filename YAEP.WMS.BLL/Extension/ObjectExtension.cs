using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YAEP.Interfaces;

namespace YAEP.WMS.BLL.Extension
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
        public static T CloneByJSON<T>(this object obj)
        {

            string data = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(data);
        }
        public static bool AllComplete(this IActionResult<bool> obj)
        {
            return obj.Content && obj.Success;
        }
        public static bool AllComplete(this IEnumerable<IActionResult<bool>> obj)
        {
            return obj.All(x => x.Success);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }
    }
}
