﻿using Microsoft.EntityFrameworkCore;
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
    public class RepositoryBase<T> : IRepository<T> where T : Entity
    {
        protected readonly OrderContext _orderContext;

        public RepositoryBase(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _orderContext.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _orderContext.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<bool> AddAsync(T entity)
        {
            _orderContext.Set<T>().Add(entity);
            int rowsAffected = await _orderContext.SaveChanges();

            return rowsAffected > 0;
        }

        public async Task UpdateAsync(T entity)
        {
            _orderContext.Entry(entity).State = EntityState.Modified;
            await _orderContext.SaveChanges();
        }

        public async Task DeleteAsync(T entity)
        {
            _orderContext.Set<T>().Remove(entity);
            await _orderContext.SaveChanges();
        }
    }
}
