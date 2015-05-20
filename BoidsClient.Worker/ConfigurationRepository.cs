using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BoidsClient.Worker
{
    public class ConfigurationRepository
    {
        private string GetRoleId()
        {
            return RoleEnvironment.CurrentRoleInstance.Id;
        }
        public async Task Initialize()
        {
            var storageClient = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConfigStorage"));
            var client = storageClient.CreateCloudTableClient();

            var tableRef = client.GetTableReference("boidsconfig");
            await tableRef.CreateIfNotExistsAsync();
            _table = tableRef;
            _statusTable = client.GetTableReference("boidsstatus");
            await _statusTable.CreateIfNotExistsAsync();

        }

        private CloudTable _statusTable;

        private CloudTable _table;
        public async Task<int> GetTargetInstancesCount()
        {
            if (_table == null)
            {
                await Initialize();
            }
            bool success = false;
            var peersCount = 0;
            while (!success)
            {
                var id = this.GetRoleId();
                var result = await _table.ExecuteAsync(TableOperation.Retrieve<RoleConfig>(id, id));
                RoleConfig config;
                if (result.HttpStatusCode == 404)
                {
                    config = new RoleConfig { PartitionKey = id, RowKey = id };
                    await _table.ExecuteAsync(TableOperation.InsertOrReplace(config));
                }
                else
                {
                    config = (RoleConfig)result.Result;
                }


                await _statusTable.ExecuteAsync(TableOperation.InsertOrReplace(new Status { LastKeepAlive = DateTime.UtcNow, PartitionKey = id, RowKey = id }));



                if (result.HttpStatusCode == 200)
                {
                    success = true;
                    peersCount = config.PeersCount;
                }
            }
            return peersCount;

        }
    }

    public class RoleConfig : TableEntity
    {

        public int PeersCount { get; set; }
    }

    public class Status : TableEntity
    {
        public DateTime LastKeepAlive { get; set; }
    }
}
