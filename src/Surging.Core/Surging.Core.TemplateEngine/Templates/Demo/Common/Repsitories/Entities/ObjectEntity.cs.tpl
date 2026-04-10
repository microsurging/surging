using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace {{ prefix }}.Core.Common.Repsitories.Entities
{    /// <summary>
     /// 基础主键字段
     /// </summary>
    public class ObjectEntity : IObjectEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// 基础主键字段接口
    /// </summary>
    public interface IObjectEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Description("主键")]
        public int Id { get; set; }
    }
}

