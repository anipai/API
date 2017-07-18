using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
namespace IBMApiAnaltycs
{
    using ServiceStack.Text;
    using System.Configuration;
    using System.Net;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var groupSummary = bool.Parse(ConfigurationManager.AppSettings["summary"]);
            //var count = bool.Parse(ConfigurationManager.AppSettings["count"]);
            var detail = bool.Parse(ConfigurationManager.AppSettings["detail"]);
            var startDateTime = ConfigurationManager.AppSettings["start"];
            var endDateTime = ConfigurationManager.AppSettings["end"];
            var env =  ConfigurationManager.AppSettings["env"];
            var org =  ConfigurationManager.AppSettings["org"];
            var uri = ConfigurationManager.AppSettings["uri"];
            var proxy = ConfigurationManager.AppSettings["proxy"];
            var planFilter = ConfigurationManager.AppSettings["APIFilterDetail"];
            var startTime = DateTime.Now;
            var useProxy = bool.Parse( ConfigurationManager.AppSettings["useProxy"]);
            var elastic = ConfigurationManager.AppSettings["elastic"];
            TimeRangeType timeRangeTypes = (TimeRangeType) Enum.Parse(typeof(TimeRangeType),ConfigurationManager.AppSettings["timerange"]);
            var limit = 20000;
            var lastXDays = int.Parse(ConfigurationManager.AppSettings["lastXDays"]);
            var target = ConfigurationManager.AppSettings["target"];

            if (target.Equals("AMAZON"))
            {
                var file = new System.IO.StreamReader("amazon.csv");
                var data = CsvSerializer.DeserializeFromReader<List<AmazonData>>(file);
                //var amazonData = CsvSerializer.DeserializeFromString<AmazonData>(file.ReadToEnd());
                data.RemoveAll(d => d.Record_type.Trim().Equals("RESPONSE"));
                data.ForEach(d => { if (d.Record_type.Trim().Equals("FAILURE")) { d.Record_type = "Amazon OLP"; d.Requests = d.Requests * 5; } });
                file.Close();
                file.Dispose();
                var callList = new List<CallSummary>();
                //data.ForEach(d => callList.Add(new CallSummary { Org= "Amazon", Api = "OLP", Count = )
                var node = new Uri(elastic);
                var settings = new ConnectionSettings(node);
                settings.DefaultIndex("amazon");
                var client = new ElasticClient(settings);

                var response = client.IndexMany<AmazonData>(data);
                

            }

            else
            {
                if (timeRangeTypes == TimeRangeType.Specific)
                {
                    for (var day = 0; day < lastXDays; day++)
                    {
                        var count = bool.Parse(ConfigurationManager.AppSettings["count"]);
                        
                        var thisStartDateTime = DateTime.Parse(startDateTime).AddDays(-day);
                        var thisEndDateTime = DateTime.Parse(endDateTime).AddDays(-day);

                        var summary = new List<CallSummary>();

                        var fileNamePart = string.Format(
                            "{0}_{1}",
                            thisStartDateTime.ToString("ddMMyyHHmm"),
                            thisEndDateTime.ToString("ddMMyyHHmm"));
                        Console.WriteLine("Preparing for first call from {0}  to {1}. Each call will process {2} rows", thisStartDateTime, thisEndDateTime, limit);
                        var nextRef = string.Format(uri, org, env, thisEndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"), thisStartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"), limit); //2016-11-17T13:00:00

                        var callsProcesssed = 0;
                        var totalCalls = 0;
                        bool first = true;
                        while (!string.IsNullOrEmpty(nextRef) && callsProcesssed <= totalCalls & (groupSummary || detail || count))
                        {
                            try
                            {
#if debug
                                Console.WriteLine("NextRef: " + nextRef);
#endif
                                var data = FromUri(nextRef, args[0], proxy, useProxy);
                                var log = JsonConvert.DeserializeObject<Log>(data);
                                if (log.nextHref != null)
                                {
                                    nextRef = log.nextHref;
                                }
                                if (first)
                                {
                                    Console.WriteLine(" Total calls from {0} to {1} = {2}", thisStartDateTime, thisEndDateTime, log.totalCalls.ToString("N0"));
                                    totalCalls = log.totalCalls;
                                    //LoadDataIntoElastic(totalCalls, thisStartDateTime.Date, elastic);
                                    first = false;
                                    count = false;
                                }
                                else
                                {
                                    Console.WriteLine("{0} rows processed. Time passed {1}", callsProcesssed.ToString("N0"), DateTime.Now - startTime);
                                }
                                callsProcesssed += limit;

                                if (groupSummary)
                                {
                                    summary.AddRange(
                                        log.calls.GroupBy(c => new { c.apiName, c.devOrgName })
                                            .Select(
                                                cl =>
                                                new CallSummary
                                                {
                                                    Api = cl.First().apiName,
                                                    Org = cl.First().devOrgName,
                                                    Count = cl.Count(),
                                                    Ok200 = cl.Count(x => x.statusCode.StartsWith("200")),
                                                    Error4X = cl.Count(x => x.statusCode.Contains("4")),
                                                    Error5X = cl.Count(x => x.statusCode.Contains("5"))//,
                                                                                                       //Others = cl.Count(x => (!x.statusCode.StartsWith("200") && !x.statusCode.StartsWith("500")))
                                            })
                                            .ToList());
                                    Console.WriteLine("{0} grouping rows added. Time passed {1}", summary.Count, DateTime.Now - startTime);
                                    WriteToFile(summary, fileNamePart);
                                }

                                if (detail)
                                {
                                    var selectCalls = log.calls.Where(c => c.apiName.Contains(planFilter) || string.IsNullOrWhiteSpace(planFilter));
                                    WriteToFile(selectCalls, fileNamePart, elastic);
                                    Console.WriteLine("{0} rows added. Time passed {1}", selectCalls.Count(), DateTime.Now - startTime);
                                }
                            }
                            catch (Exception e)
                            {
                                nextRef = null;
                                Console.WriteLine("something went wrong. Continuing to summary. " + e.Message);
                            }

                        }

                        if (groupSummary)
                        {
                            var finalList =
                                summary.GroupBy(c => new { c.Api, c.Org })
                                    .Select(
                                        cl =>
                                        new CallSummary
                                        {
                                            datetime = thisStartDateTime.Date,
                                            Api = cl.First().Api,
                                            Org = cl.First().Org,
                                            Count = cl.Sum(clt => clt.Count),
                                            Ok200 = cl.Sum(clt => clt.Ok200),
                                            Error4X = cl.Sum(clt => clt.Error4X),
                                            Error5X = cl.Sum(clt => clt.Error5X)//,
                                                                                //SuccessPercent = Math.Round((((decimal)item.Ok200 / (decimal)item.Count) * 100), 2)
                                    }).ToList();

                            finalList.ForEach(f => f.SuccessPercent = Math.Round((((decimal)f.Ok200 / (decimal)f.Count) * 100), 2));
                            if (finalList.Count > 0)
                            {
                                //LoadDataIntoElastic(finalList, elastic);
                                WriteToFile(finalList, fileNamePart);

                            }
                            Console.WriteLine("Final group count" + finalList.Count());
                        }
                    }
                }
            }
            Console.WriteLine("press any key to exit.");
            Console.ReadLine();
        }

        private static void LoadDataIntoElastic(int totalCalls, DateTime date, string elastic)
        {
            var node = new Uri(elastic);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex("apic-volumes");
            var client = new ElasticClient(settings);

            var response = client.Index(new { dateTime = date, day = date.DayOfWeek.ToString(), count= totalCalls });
            if(response.Created)
            {
                Console.WriteLine("Index with id: " + response.Id);
            }
            else
            {
                Console.WriteLine(response.ServerError.Error.Reason);
            }
            //client.CloseIndex("apic-volumes");
        }

        private static string FromUri(string uri, string credential, string proxy, bool useProxy)
        {
            string json;
            using (var client = new WebClient())
            {
                var auth = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credential));
                client.Headers.Add("Accept-Language", " en-US");
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)");
                client.Headers.Add("authorization", auth);

                if (!string.IsNullOrEmpty(proxy) && useProxy)
                {
                    client.Proxy = new WebProxy(proxy);
                    ////client.Proxy =
                    client.UseDefaultCredentials = true;
                }


                json = client.DownloadString(uri);
            }
            return json;
        }

        private static void WriteToFile(IEnumerable<CallSummary> data, string fileNamePart)
        {
            using (var file = new System.IO.StreamWriter(string.Format("apiSummary_{0}.csv", fileNamePart)))
            {
                var csv = CsvSerializer.SerializeToCsv(data);
                file.Write(csv);
            }

        }

        private static void WriteToFile(IEnumerable<Call> data, string fileNamePart, string elastic)
        {
            using (var file = new System.IO.StreamWriter(string.Format("apiDetail_{0}.csv", fileNamePart), true))
            {

                var csv = CsvSerializer.SerializeToCsv(data.Select(i => new { i.apiName, i.apiVersion, i.appName, i.datetime, i.devOrgName, i.envName, i.planName, i.planVersion, i.statusCode, i.timeToServeRequest, i.latency, i.requestBody, i.queryString }));
                file.Write(csv);
            }
            //if (!string.IsNullOrWhiteSpace(elastic))
            //{
            //    LoadDataIntoElastic(data, elastic);
            //}
        }

        private static void SpecificDateRangeCall()
        {

        }

        private static void LoadDataIntoElastic(IEnumerable<CallSummary> data, string server)
        {
            var node = new Uri(server);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex("apic-lit");
            var client = new ElasticClient(settings);

            var response = client.IndexMany<CallSummary>(data);
            //(data.Select( d => new CallLit
            //{ apiName = d.apiName, datetime = d.datetime, devOrgName = d.devOrgName, productName = d.productName, timeToServeRequest = d.timeToServeRequest }));
            

            
        }

        private static void LoadDataIntoElastic(IEnumerable<Call> data, string server)
        {
            var node = new Uri(server);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex("apic-detail");
            var client = new ElasticClient(settings);
            
            var dataLit = (data.Select(d => new CallLit
            { apiName = d.apiName, datetime = d.datetime, devOrgName = d.devOrgName, productName = d.productName, timeToServeRequest = d.timeToServeRequest }));

            var response = client.IndexMany<CallLit>(dataLit);
            Console.WriteLine(response.Items.Count() + " added into the elastic index apic-detail");
        }
    }

    public class AmazonData
    {
        public DateTime Day_of_Year { get; set; }
        public string Operation_name { get; set; }
        public string Record_type { get; set; }
        public int Requests { get; set; }
        
    }



    

    public class AmazonSummary

    {
        public DateTime Day_of_Year { get; set; }
        public string Operation_name { get; set; }
        public string Record_type { get; set; }
        public int Requests { get; set; }
        public int Sum { get; set; }

    }

    public class Call
    {
        public string datetime { get; set; }
        public string apiName { get; set; }
        public string apiVersion { get; set; }
        public string appName { get; set; }
        public string envName { get; set; }
        public string planName { get; set; }
        public string planVersion { get; set; }
        public string devOrgName { get; set; }
        public string resourceName { get; set; }
        public int timeToServeRequest { get; set; }
        public int bytesSent { get; set; }
        public string requestProtocol { get; set; }
        public string requestMethod { get; set; }
        public string uriPath { get; set; }
        public string queryString { get; set; }
        public string statusCode { get; set; }
        public string requestHeaders { get; set; }
        public string userAgent { get; set; }
        public string requestBody { get; set; }
        public string responseHeaders { get; set; }
        public string responseBody { get; set; }
        public string latency { get; set; }
        public int? bytesReceived { get; set; }
        public string productName { get; set; }
    }

    public class CallLit
    {
        public string datetime { get; set; }
        public string apiName { get; set; }
        public string productName { get; set; }
        public string devOrgName { get; set; }
        public int timeToServeRequest { get; set; }
    }

    public class CallSummary
    {
        //public string Date { get; set; }
        
        public string Org { get; set; }
        public string Api { get; set; }
       //public string Status { get; set; }
        public int Count { get; set; }
        public int Ok200 { get; set; }
        public int Error5X { get; set; }
        public int Error4X { get; set; }
        //public int Others { get; set; }
        public decimal SuccessPercent { get; set; }
        public DateTime datetime { get;  set; }
    }

    public class Log
    {
        public int totalCalls { get; set; }
        public string next { get; set; }
        public string nextHref { get; set; }
        public List<Call> calls { get; set; }
    }

    public enum TimeRangeType
    {
        Specific,
        LastXDays
    }
}


