using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Services.Implementations
{
   public class ApplicationService: IApplicationService
    {
        private readonly string _urlHome;

        public ApplicationService(string urlHome)
        { 
            _urlHome = urlHome;
        }

        public string GetHomeUrl()
        {
            return _urlHome;
        }
    }
}
