using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Stormancer.Plugins;
using Stormancer.Server.Components;

namespace Server.Database
{
    internal class ESClientPlugin : IHostPlugin
    {
        private object synclock = new object();
        private IESClientFactory factory = null;
        public void Build(HostPluginBuildContext ctx)
        {


            ctx.HostStarting += h =>
            {
                h.DependencyResolver.Register<IESClientFactory, ESClientFactory>();
            };
        }
    }
    public interface IESClientFactory
    {
        Task<Nest.IElasticClient> CreateClient(string index);
    }
    class ESClientFactory : IESClientFactory
    {
        private IEnvironment _environment;
        public ESClientFactory(IEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<IElasticClient> CreateClient(string indexName)
        {
            var indices = await _environment.ListIndices();

            var index = indices.FirstOrDefault(i => i.name == indexName);
            var endpoint = "https://api.stormancer.com";
            var connection = new Elasticsearch.Net.Connection.HttpClientConnection(
                 new ConnectionSettings(),
                 new AuthenticatedHttpClientHandler(index));

            return new Nest.ElasticClient(new ConnectionSettings(new Uri(endpoint + "/" + index.accountId + "/_indices/_q"), index.name), connection);
        }
    }
}
