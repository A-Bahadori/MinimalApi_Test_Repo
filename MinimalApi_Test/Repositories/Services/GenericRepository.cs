using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MinimalApi_Test.Context;
using MinimalApi_Test.Entities.Base;
using MinimalApi_Test.Repositories.Interfaces;
using System.Linq.Expressions;

namespace MinimalApi_Test.Repositories.Services
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly AppDbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private IDbContextTransaction? _transaction;

        public GenericRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        #region Query Methods

        public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>>? specification = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool tracking = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IQueryable<TEntity> query = tracking ? _dbSet : _dbSet.AsNoTracking();

                if (specification != null)
                {
                    query = query.Where(specification);
                }

                foreach (var includeProperty in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()))
                {
                    query = query.Include(includeProperty);
                }

                if (orderBy != null)
                {
                    query = orderBy(query);
                }

                return await query.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error retrieving entities", ex);
            }
        }

        public virtual async Task<TEntity?> GetByIdAsync(
            object id,
            string includeProperties = "",
            bool tracking = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (tracking)
                {
                    var entity = await _dbSet.FindAsync(new[] { id }, cancellationToken);
                    if (entity == null) return null;

                    if (!string.IsNullOrWhiteSpace(includeProperties))
                    {
                        foreach (var includeProperty in includeProperties
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim()))
                        {
                            await _context.Entry(entity)
                                .Reference(includeProperty)
                                .LoadAsync(cancellationToken);
                        }
                    }

                    return entity;
                }
                else
                {
                    var keyProperty = _context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties[0];
                    if (keyProperty == null)
                        throw new RepositoryException("Entity does not have a primary key defined");

                    var parameter = Expression.Parameter(typeof(TEntity), "e");
                    var predicate = Expression.Lambda<Func<TEntity, bool>>(
                        Expression.Equal(
                            Expression.Property(parameter, keyProperty.Name),
                            Expression.Constant(id)
                        ),
                        parameter
                    );

                    return await GetAsync(predicate, null, includeProperties, false, cancellationToken)
                        .ContinueWith(t => t.Result.FirstOrDefault(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error retrieving entity with id {id}", ex);
            }
        }

        public async Task<IReadOnlyList<TEntity>> GetWithDeletedAsync(
            Expression<Func<TEntity, bool>>? specification = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool tracking = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet.IgnoreQueryFilters();

            if (specification != null)
            {
                query = query.Where(specification);
            }

            foreach (var includeProperty in includeProperties.Split
                         ([','], StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync(cancellationToken);
        }


        public virtual async Task<bool> ExistsAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error checking entity existence", ex);
            }
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (predicate == null)
                    return await _dbSet.CountAsync(cancellationToken);

                return await _dbSet.CountAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error counting entities", ex);
            }
        }

        public async Task<int> CountWithDeletedAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet.IgnoreQueryFilters();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync(cancellationToken);
        }

        #endregion

        #region Command Methods

        public virtual async Task<TEntity> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                var result = await _dbSet.AddAsync(entity, cancellationToken);
                return result.Entity;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error adding entity", ex);
            }
        }

        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                var entityList = entities.ToList();
                foreach (var entity in entityList)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                await _dbSet.AddRangeAsync(entityList, cancellationToken);
                return entityList;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error adding entities", ex);
            }
        }

        public virtual Task<bool> UpdateAsync(
            TEntity entity,
            bool updateModifiedAt = true,
            CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                if (updateModifiedAt)
                {
                    entity.ModifiedAt = DateTime.UtcNow;
                }

                _context.Entry(entity).State = EntityState.Modified;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error updating entity", ex);
            }
        }

        public virtual Task<bool> UpdateRangeAsync(
            IEnumerable<TEntity> entities,
            bool updateModifiedAt = true,
            CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                foreach (var entity in entities)
                {
                    if (updateModifiedAt)
                    {
                        entity.ModifiedAt = DateTime.UtcNow;
                    }
                    _context.Entry(entity).State = EntityState.Modified;
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error updating entities", ex);
            }
        }

        public virtual async Task<bool> RemoveAsync(
            TEntity entity,
            bool hardDelete = false,
            CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                if (hardDelete)
                {
                    _dbSet.Remove(entity);
                    return true;
                }

                entity.IsDelete = true;
                entity.DeletedAt = DateTime.UtcNow;
                return await UpdateAsync(entity, false, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error removing entity", ex);
            }
        }

        public virtual async Task<bool> RemoveByIdAsync(
            object id,
            bool hardDelete = false,
            CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, string.Empty, true, cancellationToken);
            if (entity == null)
                return false;

            return await RemoveAsync(entity, hardDelete, cancellationToken);
        }

        public virtual async Task<bool> RemoveRangeAsync(
            IEnumerable<TEntity> entities,
            bool hardDelete = false,
            CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            try
            {
                var entityList = entities.ToList();
                if (hardDelete)
                {
                    _dbSet.RemoveRange(entityList);
                    return true;
                }

                foreach (var entity in entityList)
                {
                    entity.IsDelete = true;
                    entity.DeletedAt = DateTime.UtcNow;
                }

                return await UpdateRangeAsync(entityList, false, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error removing entities", ex);
            }
        }

        #endregion

        #region Transaction Methods

        public virtual async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error saving changes", ex);
            }
        }

        public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    throw new InvalidOperationException("A transaction is already in progress");
                }

                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error beginning transaction", ex);
            }
        }

        public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No transaction is currently in progress");
                }

                await _transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error committing transaction", ex);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No transaction is currently in progress");
                }

                await _transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error rolling back transaction", ex);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        #endregion
    }

    public class RepositoryException : Exception
    {
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}