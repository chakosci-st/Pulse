using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pulse.SharedUtilities.Helpers
{
    public static class JsonHelper
    {
        public static T ParseFormJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
