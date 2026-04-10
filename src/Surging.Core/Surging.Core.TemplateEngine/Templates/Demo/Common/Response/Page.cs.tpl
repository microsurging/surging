using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace {{ prefix }}.Core.Common.Response
{
    [DataContract]
    public class Page<T>
    {
        [DataMember]
        public int PageIndex { get; set; }


        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public int Total { get; set; }


        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public List<T> Items
        {
            get;
            set;
        }
    }
}
