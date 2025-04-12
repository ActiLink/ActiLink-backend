using ActiLink.Model;
using ActiLink.Repositories;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ActiLink.UnitTests.EventTests
{

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

        public T Current => _inner.Current;
    }

    internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var result = Execute<TResult>(expression);
            return new ValueTask<TResult>(result);
        }

        public TResult GetResult<TResult>(object value)
        {
            return (TResult)value!;
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var resultType = typeof(TResult);

            // Check if TResult is Task<T>
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var elementType = resultType.GetGenericArguments()[0];

                // Create a method to create a properly typed Task
                var method = typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(elementType);

                // Handle single objects
                var result = _inner.Execute(expression);

                // If result is null, return Task.FromResult(null) for the correct type
                if (result == null)
                {
                    object?[] parameters = new object?[] { null };
                    return (TResult)method.Invoke(null, parameters)!;
                }

                // If result is not null, ensure it has the correct type before wrapping
                if (elementType.IsAssignableFrom(result.GetType()))
                {
                    object[] parameters = new object[] { result };
                    return (TResult)method.Invoke(null, parameters)!;
                }
                else
                {
                    // Return null for the correct type if types don't match
                    object?[] parameters = new object?[] { null };
                    return (TResult)method.Invoke(null, parameters)!;
                }
            }

            // For other cases, execute synchronously
            return Execute<TResult>(expression);
        }
    }
}
