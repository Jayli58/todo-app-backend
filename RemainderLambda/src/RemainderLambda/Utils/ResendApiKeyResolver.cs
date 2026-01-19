using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Utils
{
    public static class ResendApiKeyResolver
    {
        public static string ResolveResendApiKey()
        {
            // local & test
            var direct = Environment.GetEnvironmentVariable("RESEND_API_KEY");
            if (!string.IsNullOrWhiteSpace(direct))
                return direct.Trim();

            // AWS
            // look up Resend api key from SSM Parameter Store
            var paramName = Environment.GetEnvironmentVariable("RESEND_API_KEY_PARAM")
                ?? throw new InvalidOperationException("Missing RESEND_API_KEY or RESEND_API_KEY_PARAM");

            using var ssm = new AmazonSimpleSystemsManagementClient();

            var resp = ssm.GetParameterAsync(new GetParameterRequest
            {
                Name = paramName,
                WithDecryption = true
            }).GetAwaiter().GetResult();

            return resp.Parameter.Value;
        }
    }
}
