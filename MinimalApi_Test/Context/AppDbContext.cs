using Microsoft.EntityFrameworkCore;
using MinimalApi_Test.Entities.Base;
using MinimalApi_Test.Entities.User;
using System.Linq.Expressions;
using MinimalApi_Test.Configurations.User;

namespace MinimalApi_Test.Context
{
    public class AppDbContext:DbContext
    {
        #region Constractor

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        #endregion

        #region User

        public DbSet<User> Users { get; set; }

        #endregion

        #region OnModelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Entity Configuration

            modelBuilder.ApplyConfiguration(new UserConfiguration());

            #endregion

            #region Query Filter

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(BaseEntity.IsDelete));
                    var falseConstant = Expression.Constant(false);
                    var lambda = Expression.Lambda(Expression.Equal(property, falseConstant), parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            } 

            #endregion
        }

        #endregion
    }
}
