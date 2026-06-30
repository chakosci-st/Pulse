using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = Pulse.Core.Entities;
/// <summary>
/// A generic repository interface for managing entities.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the entity's ID (e.g., int, string).</typeparam>
namespace Pulse.Core.Interfaces
{
    public interface IBaseRepository<TEntity, TId> 
    {
        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>Entities.</returns>
        Task<IEnumerable<TEntity>> GetListAsync();

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <returns>Entity.</returns>
        Task<TEntity> GetAsync(TId id);




        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <returns>Primary Key.</returns>
        Task<TId> AddAsync(TEntity entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <returns>Number of rows affected.</returns>
        Task<int> UpdateAsync(TEntity entity);

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <returns>Number of rows affected.</returns>
        Task<int> DeleteAsync(TId id);

        ////// Transaction management
        ////void BeginTransaction();
        ////void CommitTransaction();
        ////void RollbackTransaction();
 
    }
}
