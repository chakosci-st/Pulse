using log4net;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
namespace Pulse.Infrastructure.Repositories
{
    public abstract class BaseRepository<TEntity, TId> : IBaseRepository<TEntity, TId>
    {
        private readonly ILog _logger;
        protected readonly OracleDataAccessLayer _dataAccess;
        protected IDbTransaction Transaction => _dataAccess.GetTransaction();
        protected BaseRepository(OracleDataAccessLayer dataAccess, ILog logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _logger.Info($"Repository created. DataAccess instance hash: {dataAccess.GetHashCode()}");
        }

        public abstract Task<IEnumerable<TEntity>> GetListAsync();
        public abstract Task<TEntity> GetAsync(TId id);
        public abstract Task<TId> AddAsync(TEntity entity);
        public abstract Task<int> UpdateAsync(TEntity entity);
        public abstract Task<int> DeleteAsync(TId id);

        ////// Transaction management
        ////public void BeginTransaction()
        ////{
        ////    _dataAccess.BeginTransaction();
        ////}

        ////public void CommitTransaction()
        ////{
        ////    _dataAccess.BeginTransaction();
        ////}

        ////public void RollbackTransaction()
        ////{
        ////    _dataAccess.BeginTransaction();
        ////}

 
    }
}
