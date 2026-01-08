using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Services
{
    // Resolves LocalStack endpoint for SES service
    public static class LocalstackEndpointResolver
    {
        public static string ResolveSesServiceUrl()
        {
            // LocalStack Lambda runtime provides this
            var endpoint = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL");

            if (!string.IsNullOrWhiteSpace(endpoint))
                return endpoint.TrimEnd('/');

            // Host-run tests / local execution
            return "http://localhost:4566";
        }
    }

}
