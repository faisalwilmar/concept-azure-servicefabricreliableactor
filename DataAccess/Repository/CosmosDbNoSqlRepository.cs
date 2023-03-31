using DataAccess.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class CosmosDbNoSqlRepository<T> where T : BaseModel, new()
    {
        /// <summary>
        /// CosmosDB client connection handler 
        /// </summary>
        private readonly Lazy<CosmosClient> _client;

        /// <summary>
        /// CosmosDB database id
        /// </summary>
        private string _databaseId;

        /// <summary>
        /// CosmosDB collection id
        /// </summary>
        private readonly string _collectionId = BaseModel.GenerateDocumentType(typeof(T));

        /// <summary>
        /// Additional partition key beside collection name
        /// </summary>
        private bool _partitionPropertyDefined = false;
        private List<string> _partitionPropertyNames = new();
        private readonly List<PropertyInfo> _partitionProperties = new();

        public CosmosDbNoSqlRepository(string databaseId,
            CosmosClient cosmosDBClient, string partitionProperties = "")
        {
            _client ??= new Lazy<CosmosClient>(cosmosDBClient);

            SetUp(databaseId, partitionProperties);
        }

        #region Private
        private void SetUp(string databaseId, string partitionProperties = "")
        {
            _databaseId = databaseId;

            if (string.IsNullOrEmpty(_databaseId))
                throw new ArgumentException("Invalid Database Id", "databaseId");
            if (string.IsNullOrEmpty(_collectionId))
                throw new ArgumentException("Invalid Collection Id", "collectionId");

            if (!string.IsNullOrWhiteSpace(partitionProperties))
            {
                _partitionPropertyDefined = true;

                _partitionPropertyNames = partitionProperties.Split(",").ToList();

                foreach (var propertyName in _partitionPropertyNames)
                {
                    _partitionProperties.Add(typeof(T).GetProperty(propertyName.Trim()));
                }
            }
        }

        /// <summary>
        /// Compose Partition Key when querying document.
        /// </summary>
        /// <param name="partitionKeys"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ApplicationException"></exception>
        private string ComposePartitionKeys(Dictionary<string, string> partitionKeys)
        {
            var partitionKey = _collectionId;

            if (_partitionPropertyDefined && partitionKey == null)
                throw new ArgumentException("Partition key must be defined");

            if (partitionKeys != null)
            {
                if (!_partitionPropertyDefined)
                    return partitionKey;
                for (int ii = 0; ii < _partitionPropertyNames.Count; ii++)
                {
                    try
                    {
                        partitionKey += "/" + partitionKeys[_partitionPropertyNames[ii]];
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException($"Missing partition key parameter {_partitionPropertyNames[ii]}");
                    }
                }
            }

            return partitionKey;
        }

        /// <summary>
        /// Generate partition key from already created document.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string GeneratePartitionKey(T item)
        {
            var result = _collectionId;

            if (_partitionPropertyNames != null)
            {
                foreach (var p in _partitionProperties)
                {
                    result += "/" + p.GetValue(item);
                }
            }

            return result;
        }

        #endregion

        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            bool usePaging = false, string continuationToken = null, int pageSize = 10,
            Dictionary<string, string> partitionKeys = null, bool enableCrossPartition = false)
        {
            var client = _client.Value;
            var container = client.GetContainer(_databaseId, _collectionId);
            PartitionKey? pk = new PartitionKey(ComposePartitionKeys(partitionKeys));

            if (_partitionPropertyDefined && partitionKeys == null) { enableCrossPartition = true; }

            if (enableCrossPartition) pk = null;

            var queryReqOpts = new QueryRequestOptions
            {
                MaxItemCount = usePaging ? pageSize : -1,
                PartitionKey = pk,
                PopulateIndexMetrics = false
            };

            predicate ??= p => true;

            orderBy ??= o => o.OrderBy(p => p.Id);

            var query = container.GetItemLinqQueryable<T>(
                    continuationToken: continuationToken != "" ? continuationToken : null,
                    requestOptions: queryReqOpts)
                .Where(predicate);

            var results = new List<T>();
            FeedResponse<T> response;
            using (var feedIterator = query.ToFeedIterator())
            {
                while (feedIterator.HasMoreResults)
                {
                    response = await feedIterator.ReadNextAsync();
                    results.AddRange(response);
                }
            }

            return results;
        }

        private async Task<T> GetByIdAsync(string id, string partitionKey = "")
        {
            try
            {
                var client = _client.Value;
                var container = client.GetContainer(_databaseId, _collectionId);

                var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

                return response;
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<T> UpdateAsync(string id, T item, string lastUpdatedBy = null, bool isOptimisticConcurrency = false)
        {
            var client = _client.Value;
            var container = client.GetContainer(this._databaseId, this._collectionId);
            var partitionKey = GeneratePartitionKey(item);

            var oldValue = await GetByIdAsync(id, partitionKey) ?? throw new ArgumentException("Error when update data, old data not found", "id");

            item.PartitionKey = oldValue?.PartitionKey;
            item.CreatedBy = oldValue?.CreatedBy;
            item.CreatedDateUtc = oldValue.CreatedDateUtc;

            if (!string.IsNullOrWhiteSpace(lastUpdatedBy)) item.LastUpdatedBy = lastUpdatedBy;
            item.LastUpdatedDateUtc = DateTime.UtcNow;

            ItemResponse<T> updatedItem;
            if (isOptimisticConcurrency)
            {
                var requestOptions = new ItemRequestOptions()
                {
                    IfMatchEtag = item.Etag
                };
                updatedItem = await container.ReplaceItemAsync<T>(item, id, new PartitionKey(partitionKey), requestOptions: requestOptions);

            }
            else
            {
                updatedItem = await container.ReplaceItemAsync<T>(item, id, new PartitionKey(partitionKey));
            }

            return updatedItem;
        }
    }
}
