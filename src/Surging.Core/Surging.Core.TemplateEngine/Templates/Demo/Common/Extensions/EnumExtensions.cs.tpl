using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace {{ prefix }}.Core.Common.Extensions
{
    /// <summary>
    /// 枚举扩展类
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举的自定义属性
        /// </summary>
        /// <typeparam name="T">自定义属性类型</typeparam>
        /// <param name="value">枚举</param>
        /// <returns>返回自定义属性</returns>
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            return field.GetCustomAttribute(typeof(T)) as T;

        }

        /// <summary>
        /// 获取枚举的值
        /// </summary>
        /// <param name="value">枚举</param>
        /// <returns>返回枚举的值</returns>
        public static int GetValue(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        #region 获取枚举特性
        /// <summary>
        /// 获取枚举显示名称
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <returns>枚举显示名称</returns>
        public static string GetEnumDisplayName(this Enum value)
        {
            if (value == null)
            {
                return "";
            }
            return GetEnumDisplayName(value.GetType(), value.ToString());
        }


        /// <summary>
        /// 获取枚举显示名称
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="value">枚举值</param>
        /// <returns>枚举显示名称</returns>
        public static string GetEnumDisplayName(Type enumType, string value)
        {
            string rv = "";
            FieldInfo field = null;

            if (enumType.IsEnum())
            {
                field = enumType.GetField(value);
            }
            //如果是nullable的枚举
            if (enumType.IsGeneric(typeof(Nullable<>)) && enumType.GetGenericArguments()[0].IsEnum())
            {
                field = enumType.GenericTypeArguments[0].GetField(value);
            }

            if (field != null)
            {

                var attribs = field.GetCustomAttributes(typeof(DisplayAttribute), true).ToList();
                if (attribs.Count > 0)
                {
                    rv = ((DisplayAttribute)attribs[0]).GetName();
                }
                else
                {
                    rv = value;
                }
            }
            return rv;
        }

        /// <summary>
        /// 判断是否是泛型
        /// </summary>
        /// <param name="self">Type类</param>
        /// <param name="innerType">泛型类型</param>
        /// <returns>判断结果</returns>
        public static bool IsGeneric(this Type self, Type innerType)
        {
            if (self.GetTypeInfo().IsGenericType && self.GetGenericTypeDefinition() == innerType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断是否为枚举
        /// </summary>
        /// <param name="self">Type类</param>
        /// <returns>判断结果</returns>
        public static bool IsEnum(this Type self)
        {
            return self.GetTypeInfo().IsEnum;
        }
        #endregion
    }
}