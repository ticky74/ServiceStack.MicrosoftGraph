using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
using ServiceStack.Text;

namespace ServiceStack.Azure
{
    [DataContract]
    public class GraphResponse<T>
    {
        [DataMember(Name= "@odata.context")]
        public string Context { get; set; }

        [DataMember(Name="value")]
        public T Value { get; set; }

        public static GraphResponse<T> Parse(string responseData)
        {
            if (string.IsNullOrWhiteSpace(responseData))
                return new GraphResponse<T>
                {
                    Value = default(T)
                };

            return JsonSerializer.DeserializeFromString<GraphResponse<T>>(responseData);
        }
    }
}
