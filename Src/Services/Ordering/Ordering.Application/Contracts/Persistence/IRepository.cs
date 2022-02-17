using Ordering.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ordering.Application.Contracts.Persistence
{
    public interface IRepository<T> where T : class, IAggregateRoot
	{
		Task<IReadOnlyList<T>> GetAllAsync();
		Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
		Task Add(T entity);
		void Delete(T entity);
		Task<bool> SaveChanges();
	}
}
