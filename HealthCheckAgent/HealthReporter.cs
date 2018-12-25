using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HealthCheckAgent
{
    public static class HealthReporter
    {
        private static HttpClient healthEndpointPinger = new HttpClient
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("APP_URL")),
            Timeout = TimeSpan.FromSeconds(30)
        };        

        [FunctionName("HealthReporter")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            HealthReport healthReport = await GetHealthReport();
            await DataDogClient.SendHealthReport(healthReport, log);            
        }

        private static async Task<HealthReport> GetHealthReport()
        {
            var response = await healthEndpointPinger.GetAsync("/health/status");
            var responseContent = await response.Content.ReadAsStringAsync();
            var healthReport = JsonConvert
                .DeserializeObject<HealthReport>(responseContent);

            return healthReport;
        }    
    }
}