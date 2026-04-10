using System;
using System.Collections.Generic;
using System.Text;

namespace {{ prefix }}.Core.Common.Repsitories.Models
{
    public class BaseModel
    {   
        public int Id { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int? Creater { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset? CreateDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public int? Updater { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTimeOffset? UpdateDate { get; set; }

        /// <summary>
        /// 删除
        /// </summary>
        public bool IsDeleted { get; set; } = false;


        public string? Remark { get; set; }
    }
}
