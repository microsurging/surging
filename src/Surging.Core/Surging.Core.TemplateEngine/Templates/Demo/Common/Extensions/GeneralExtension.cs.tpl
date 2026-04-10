using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace {{ prefix }}.Core.Common.Extensions
{
    public static class GeneralExtension
    {
        /// <summary>
        /// 扩展Where方法 IQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereIF<T>(
            this IQueryable<T> query,
            bool condition,
                Expression<Func<T, bool>> predicate)
        {
            return condition
                ? query.Where(predicate)
                : query;
        }

        /// <summary>
        /// 扩展Where方法 IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> WhereIF<T>(
            this IEnumerable<T> query,
            bool condition,
                Func<T, bool> predicate)
        {
            return condition
                ? query.Where(predicate)
                : query;
        }

        #region 非空验证
        /// <summary>
        /// 检查给定的对象是否为null，或者为空
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count <= 0;
        }

        /// <summary>
        /// 指示此字符串是否为NULL，或者是一个 System.String.Empty
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        /// <summary>
        /// 指示此字符串是否为NULL，或者是一个 System.String.Empty或空字符串
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
        /// <summary>
        /// 指示此字符串是否为NULL，或者是一个 Guid.Empty
        /// </summary>
        public static bool IsNullOrEmpty(this Guid? str)
        {
            return str == Guid.Empty || str is null;
        }

        /// <summary>
        /// 指示此字符串是否为NULL，或者是一个 Guid.Empty
        /// </summary>
        public static bool IsNullOrEmpty(this Guid str)
        {
            return str == Guid.Empty;
        }
        #endregion

        #region 分页辅助
        /// <summary>
        /// 获取总页数（数字向上）
        /// </summary>
        /// <param name="count"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int GetCeiling(this int count, int size)
        {
            if (count == 0 || size == 0)
                return 0;
            return count > 0 ? (int)Math.Ceiling(Convert.ToDecimal(count) / Convert.ToDecimal(size)) : 0;
        }
        #endregion
    }
}
