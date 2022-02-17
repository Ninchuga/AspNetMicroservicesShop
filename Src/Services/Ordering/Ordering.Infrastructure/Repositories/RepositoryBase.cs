using Microsoft.EntityFrameworkCore;
using Ordering.Application.Contracts.Persistence;
using Ordering.Domain.Common;
using Ordering.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Repositories
{
    public class RepositoryBase<T> : IRepository<T> where T : class, IAggregateRoot
    {
        protected readonly OrderContext _orderContext;

        public RepositoryBase(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync() =>
            await _orderContext.Set<T>().ToListAsync();
        
        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate) =>
            await _orderContext.Set<T>().Where(predicate).ToListAsync();
        
        public async Task Add(T entity) =>
            await _orderContext.Set<T>().AddAsync(entity);

        public void Delete(T entity) =>
            _orderContext.Set<T>().Remove(entity);

        public async Task<bool> SaveChanges()
        {
            int insertedEntitiesNumber = await _orderContext.SaveChanges();

            return SuccessfullySavedBy(insertedEntitiesNumber);
        }

        private bool SuccessfullySavedBy(int insertedEntitiesNumber) => insertedEntitiesNumber > 0;
    }
}
