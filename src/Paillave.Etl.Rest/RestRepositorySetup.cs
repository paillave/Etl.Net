using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Rest
{
    public class RestRepositorySetup
    {
        public string BaseAddress { get; }
        public virtual bool UseDefaultCredentials { get; } = false;
        public RestRepositorySetup(string baseAddress)
        {
            this.BaseAddress = baseAddress;
        }
        public virtual AuthenticationHeaderValue GetAuthenticationHeader()
        {
            return null;
        }
    }
}
