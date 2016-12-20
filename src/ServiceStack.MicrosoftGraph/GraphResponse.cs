using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ServiceStack.Azure
{
    [DataContract]
    public class GraphResponse<T>
    {
        [DataMember(Name= "@odata.context")]
        public string Context { get; set; }

        [DataMember(Name="value")]
        public T Value { get; set; }
    }
}
