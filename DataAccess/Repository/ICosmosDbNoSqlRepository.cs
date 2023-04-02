using DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public interface ICosmosDbNoSqlRepository<T> : IDisposable where T : BaseModel
    {
        public Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            bool usePaging = false, string continuationToken = null, int pageSize = 10,
            Dictionary<string, string> partitionKeys = null, bool enableCrossPartition = false);

        public Task<T> CreateAsync(T item, string createdBy = null);

        public Task<T> UpdateAsync(string id, T item, string lastUpdatedBy = null, bool isOptimisticConcurrency = false);
    }
}
