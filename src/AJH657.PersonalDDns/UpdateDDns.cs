using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace AJH657.PersonalDDns
{
    public static class UpdateDDns
    {
        [FunctionName("UpdateDDns")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogDebug($"DNS record updating started");

            try
            {

                var reqBodyStream = req.Body;

                var ip = await new StreamReader(reqBodyStream).ReadToEndAsync();

                var ipRegexPattern = @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}\b";
                var ipRegex = new Regex(ipRegexPattern, RegexOptions.IgnoreCase);

                if (!ipRegex.IsMatch(ip))
                    throw new ArgumentException("IP not valid");

                var domainID = GetEnvironmentVariable("domainID");
                var clientID = GetEnvironmentVariable("clientID");
                var clientSecret = GetEnvironmentVariable("clientSecret");
                var subscriptionID = GetEnvironmentVariable("subscriptionID");

                var dnsRG = GetEnvironmentVariable("dnsResourceGroup");
                var dnsName = GetEnvironmentVariable("dnsName");
                var dnsRSName = GetEnvironmentVariable("dnsRecordSetName");


                var azureServiceCreds = await ApplicationTokenProvider.LoginSilentAsync(domainID, clientID, clientSecret);
                var dnsClient = new DnsManagementClient(azureServiceCreds)
                {
                    SubscriptionId = subscriptionID
                };

                var dnsRecordSet = dnsClient.RecordSets.Get(dnsRG, dnsName, dnsRSName, RecordType.A);

                if (dnsRecordSet.ARecords.Count > 0)
                    dnsRecordSet.ARecords.Clear();

                dnsRecordSet.ARecords.Add(new ARecord(ip));

                await dnsClient.RecordSets.UpdateAsync(dnsRG, dnsName, dnsRSName, RecordType.A, dnsRecordSet, dnsRecordSet.Etag);

                log.LogDebug($"DNS record updating was successfull");

                return new OkObjectResult("Updated Succesfully");
            }
            catch (Exception e)
            {
                log.LogError(e, "Dns record updating failed");
                return new BadRequestObjectResult(e);
            }


        }

        private static string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }
}
