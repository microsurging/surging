using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace {{ prefix }}.Core.Common.BaseParam
{
    public class BaseQuery
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 主键集合
        /// </summary>
        public List<int> Ids { get; set; } = new();

        /// <summary>
        /// 页面索引
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页显示数
        /// </summary>
        public int PageSize { get; set; } = 50;
    }
}
