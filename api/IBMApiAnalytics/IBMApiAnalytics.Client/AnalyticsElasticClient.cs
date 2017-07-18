using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using IBMApiAnalytics.Models;
using IBMApiAnalytics.Utils;
using Nest;
using NLog;

namespace IBMApiAnalytics.Client
{
    public class AnalyticsElasticClient
    {
        private const string IndexNameFormat = "{0}-{1}";

        private readonly string _defaultSummaryIndexName = "apic-lit" ;
        private readonly string _defaultSummaryForOrgIndexName = "apic-orglit";
        private readonly string _defaultVolumesIndexName = "apic-volumes";
        private readonly string _defaultDetailIndexName = "apic-detail";

        private readonly string _configVolumeIndexKey = "volumeIndexPrefix";
        private readonly string _configSummaryIndexKey = "summaryIndexPrefix";
        private readonly string _configSummaryForOrgIndexKey = "summaryOrgIndexPrefix";
        private readonly string _configDetailIndexKey = "detailIndexPrefix";

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public IBulkResponse IndexSummaryWithOrg(IEnumerable<CallSummaryForOrg> data, string server)
        {
            var indexName = ConfigurationManager.AppSettings.SafeGet(_configSummaryForOrgIndexKey, _defaultSummaryForOrgIndexName);
            var response = IndexManyInElastic(server, indexName, data.ToList());
            _logger.Debug("Summary records indexed {0}", response.Items.Count());
            return response;
        }

        public IBulkResponse IndexSummary(IEnumerable<CallSummary> data, string server)
        {
            data = data.ToList();
            if (!data.Any())
            {
                _logger.Debug("No records found to index");
                return null;
            }
            var indexName = ConfigurationManager.AppSettings.SafeGet(_configSummaryIndexKey, _defaultSummaryIndexName);
            var response = IndexManyInElastic(server, indexName, data.ToList());
            _logger.Debug("Summary records indexed {0}", response.Items.Count());
            return response;
        }


        public IBulkResponse IndexDetail(IEnumerable<Call> data, DateTime date, string server)
        {
            data = data.ToList();
            if (!data.Any())
            {
                _logger.Debug("No records found to index");
                return null;
            }
            var indexName = string.Format(IndexNameFormat, ConfigurationManager.AppSettings.SafeGet(_configDetailIndexKey, _defaultDetailIndexName), date.ToString("yyyyMMdd"));

            var dataLit = data.Select(d => new CallLit
                                        { Id = d.Id, apiName = d.apiName, datetime = d.datetime,
                                            devOrgName = d.devOrgName, productName = d.productName,
                                            statusCode = d.statusCode, timeToServeRequest = d.timeToServeRequest,
                                            requestBody = (d.apiName.Contains("live") ? string.Empty : d.requestBody),
                                            responseBody = (d.apiName.Contains("live") ? string.Empty : d.responseBody),
                                            planName = d.planName
            }).ToList();

            var response = IndexManyInElastic(server, indexName, dataLit);
            _logger.Debug("Detail records added to Index {0} - {1}", response.Items.First().Index, response.Items.Count());

            return response;

        }

        public IIndexResponse IndexVolumes(int totalCalls, DateTime date, string server)
        {
            var indexName = ConfigurationManager.AppSettings.SafeGet(_configVolumeIndexKey, _defaultVolumesIndexName);
            var callVolume = new CallVolumePerDay { Id = $"Vol{date.ToString("yyyyMMdd")}", datetime = date, day = date.DayOfWeek.ToString(), count = totalCalls };

            var response = IndexInElastic(server, indexName, callVolume);
            _logger.Debug("Volume records added to Index {0}", response.Index);
            return response;
        }

        private static IBulkResponse IndexManyInElastic<T>(string server, string indexName, List<T> objectsToBeIndexed) where T : class
        {
            if (!objectsToBeIndexed.Any())
            {
                return null;
            }
            var node = new Uri(server);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex(indexName);
            var client = new ElasticClient(settings);
            return client.IndexMany(objectsToBeIndexed);

        }

        private static IIndexResponse IndexInElastic<T>(string server, string indexName, T objectToBeIndexed) where T : class
        {
            var node = new Uri(server);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex(indexName);
            var client = new ElasticClient(settings);
            return client.Index(objectToBeIndexed);
        }

    }
}
