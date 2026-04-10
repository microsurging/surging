using {{ prefix }}.Core.Common.Enums;
using {{ prefix }}.Core.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace {{ prefix }}.Core.Common.Response
{
    [DataContract]
    public class ApiResult<T> : ApiResult
    {
        /// <summary>
        /// 返回内容
        /// </summary>
        [DataMember]
        public T Result { get; set; }

        public ApiResult()
        {
        }

        public ApiResult(T t)
        {
            Result = t;
            Code = EnumReturnCode.SUCCESS;
            Msg = "ok";
        }

        public ApiResult(T t, string s = "")
        {
            Result = t;
            if (!string.IsNullOrEmpty(s))
            {
                Msg = s;
                Code = EnumReturnCode.BAD_REQUEST;
            }
        }

        public ApiResult(EnumReturnCode e, string s)
        {
            Result = default;
            if (!string.IsNullOrEmpty(s))
            {
                Msg = s;
                Code = e;
            }
            else
            {
                Msg = e.GetEnumDisplayName();
                Code = e;
            }
        }

        public ApiResult(EnumReturnCode t)
        {
            Result = default;
            if (t > 0)
            {
                Msg = t.GetEnumDisplayName();
                Code = EnumReturnCode.BAD_REQUEST;
            }
        }

        public ApiResult(T t, EnumReturnCode code, string s = "")
        {
            Result = t;
            if (!string.IsNullOrEmpty(s))
            {
                Msg = s;
                Code = EnumReturnCode.BAD_REQUEST;
                if (code != EnumReturnCode.BAD_REQUEST)
                    Code = code;
            }
        }

        public static ApiResult<T> Succeed(T t) => new ApiResult<T>(t);
        public static ApiResult<T> Failure(EnumReturnCode c) => new ApiResult<T>(c);
        public static ApiResult<T> Failure(T t, string s) => new ApiResult<T>(t, s);
        public static ApiResult<T> Failure(EnumReturnCode e, string s) => new ApiResult<T>(e, s);
        public static ApiResult<T> Failure(T t, EnumReturnCode c, string s) => new ApiResult<T>(t, c, s);

        public static void Succeed(object value)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract]
    public class ApiResult
    {

        public ApiResult() { }

        public ApiResult(string s)
        {

            if (!string.IsNullOrEmpty(s))
            {
                Msg = s;
                Code = EnumReturnCode.SUCCESS;
            }
        }
        public ApiResult(EnumReturnCode code, string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                Msg = s;
                Code = code;
            }
        }


        /// <summary>
        /// 响应编码
        /// </summary>
        [DataMember]
        public EnumReturnCode Code { get; set; } = EnumReturnCode.SUCCESS;



        /// <summary>
        /// 返回信息
        /// </summary>
        [DataMember]
        public string Msg { get; set; }

        public static ApiResult Failure(EnumReturnCode c, string s) => new ApiResult(c, s);
        public static ApiResult Succeed(string s) => new ApiResult(s);
    }
}