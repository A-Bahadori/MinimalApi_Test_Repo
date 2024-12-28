using MinimalApi_Test.Entities.Base;
using System.Linq.Expressions;


namespace MinimalApi_Test.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// </summary>
    /// <typeparam name="TEntity">Entity type that inherits from BaseEntity</typeparam>
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        #region Query Methods

        /// <summary>
        /// Retrieves entities based on specified criteria
        /// </summary>
        /// <param name="specification">Specification to filter entities</param>
        /// <param name="orderBy">Optional ordering function</param>
        /// <param name="includeProperties">Navigation properties to include, comma-separated</param>
        /// <param name="tracking">Enable or disable entity tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IReadOnlyList<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>>? specification = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool tracking = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="includeProperties">Navigation properties to include, comma-separated</param>
        /// <param name="tracking">Enable or disable entity tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<TEntity?> GetByIdAsync(
            object id,
            string includeProperties = "",
            bool tracking = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves entities based on specified criteria with option to ignore query filters
        /// </summary>
        Task<IReadOnlyList<TEntity>> GetWithDeletedAsync(
            Expression<Func<TEntity, bool>>? specification = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool tracking = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity matches the specified condition
        /// </summary>
        Task<bool> ExistsAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts entities matching the specified condition
        /// </summary>
        Task<int> CountAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts entities matching the specified condition with option to ignore query filters
        /// </summary>
        Task<int> CountWithDeletedAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Command Methods

        /// <summary>
        /// Adds a new entity
        /// </summary>
        Task<TEntity> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple entities
        /// </summary>
        Task<IEnumerable<TEntity>> AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        Task<bool> UpdateAsync(
            TEntity entity,
            bool updateModifiedAt = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates multiple entities
        /// </summary>
        Task<bool> UpdateRangeAsync(
            IEnumerable<TEntity> entities,
            bool updateModifiedAt = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity
        /// </summary>
        Task<bool> RemoveAsync(
            TEntity entity,
            bool hardDelete = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity by ID
        /// </summary>
        Task<bool> RemoveByIdAsync(
            object id,
            bool hardDelete = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes multiple entities
        /// </summary>
        Task<bool> RemoveRangeAsync(
            IEnumerable<TEntity> entities,
            bool hardDelete = false,
            CancellationToken cancellationToken = default);

        #endregion

        #region Transaction Methods

        /// <summary>
        /// Saves all changes made in this repository
        /// </summary>
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new transaction
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        #endregion
    }
}