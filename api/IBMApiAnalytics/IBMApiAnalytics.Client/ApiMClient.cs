using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using IBMApiAnalytics.Models;
using IBMApiAnalytics.Utils;
using Newtonsoft.Json;
using NLog;

namespace IBMApiAnalytics.Client
{
    public class ApiMClient
    {

        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public IEnumerable<Call> GetCalls(DateTime startDate, DateTime endDate, string credentials)
        {
            var env = ConfigurationManager.AppSettings["env"];
            var org = ConfigurationManager.AppSettings["org"];
            var uri = ConfigurationManager.AppSettings["uri"];
            var proxy = ConfigurationManager.AppSettings["proxy"];
            var useProxy = bool.Parse(ConfigurationManager.AppSettings["useProxy"]);
            var limit = ConfigurationManager.AppSettings.SafeGet("maxRowsToProcessInLoop", 5000);
            var callsProcesssed = 0;
            int totalCalls;
            var calls = new List<Call>();
            var nextRef = string.Format(uri, org, env, endDate.ToString("yyyy-MM-ddTHH:mm:ss"), startDate.ToString("yyyy-MM-ddTHH:mm:ss"), limit);
            _logger.Debug("Fetching records from {0}  to {1}.", startDate, endDate);
            while (!string.IsNullOrEmpty(nextRef))
            {
                var data = FromUri(nextRef, credentials, proxy, useProxy);
                var log = JsonConvert.DeserializeObject<Log>(data);
                calls.AddRange(log.calls);
                callsProcesssed += log.calls.Count;
                totalCalls = log.totalCalls;
                nextRef = log.nextHref;
                _logger.Trace("NextHRef {0}", nextRef);
                _logger.Debug("{0} out of {1} rows processed", callsProcesssed, totalCalls);
                if (callsProcesssed >= totalCalls)
                {
                    break;
                }
            }
            return calls;
        }

        public int GetVolume(DateTime startDate, DateTime endDate, string credentials)
        {
            var env = ConfigurationManager.AppSettings["env"];
            var org = ConfigurationManager.AppSettings["org"];
            var uri = ConfigurationManager.AppSettings["uri"];
            var proxy = ConfigurationManager.AppSettings["proxy"];
            var useProxy = bool.Parse(ConfigurationManager.AppSettings["useProxy"]);
            var nextRef = string.Format(uri, org, env, endDate.ToString("yyyy-MM-ddTHH:mm:ss"), startDate.ToString("yyyy-MM-ddTHH:mm:ss"), 1); // Send limit as 1 since we are only interested in the Value of the Total Calls 
            var data = FromUri(nextRef, credentials, proxy, useProxy);
            var log = JsonConvert.DeserializeObject<Log>(data);
            return log.totalCalls;
        }

        private static string FromUri(string uri, string credential, string proxy, bool useProxy)
        {
            string json;
            using (var client = new WebClient())
            {
                var auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(credential));
                client.Headers.Add("Accept-Language", " en-US");
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)");
                client.Headers.Add("authorization", auth);

                if (!string.IsNullOrEmpty(proxy) && useProxy)
                {
                    client.Proxy = new WebProxy(proxy);
                    client.UseDefaultCredentials = true;
                }

                json = client.DownloadString(uri);
            }
            return json;
        }
    }
}
