using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace {{ prefix }}.Core.Common.Context
{
    public class IdentityContext
    {
        public static Identity? Get()
        {
            var payload = RpcContext.GetContext().GetAttachment("payload");
            if (payload != null)
            {
                var model = JsonSerializer.Deserialize<Identity>(payload?.ToString().Trim('"').Replace("\\", ""));
                return model;
            }
            return default(Identity);
        }
    }
}
