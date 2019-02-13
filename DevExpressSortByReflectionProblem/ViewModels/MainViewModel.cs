using DevExpress.Xpf.Data;
using DevExpressSortByReflectionProblem.Services;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DevExpressSortByReflectionProblem
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService dataService;

        private InfiniteAsyncSource _bookInfiniteAsyncSource;
        public InfiniteAsyncSource BookInfiniteAsyncSource
        {
            get => _bookInfiniteAsyncSource;
            set => Set(ref _bookInfiniteAsyncSource, value);
        }

        public MainViewModel(IDataService dataService)
        {
            this.dataService = dataService;

            var infiniteSource = new InfiniteAsyncSource()
            {
                ElementType = typeof(BookQueryEntity)
            };

            infiniteSource.FetchRows += (s, e) =>
            {
                e.Result = Task.Run<FetchRowsResult>(() =>
                {
                    var query = dataService.GetBooksQuery();

                    // the property that is used for the SortBy is the property "Lastname"
                    // "Lastname" not defined in "BookQueryEntity" but in it´s base class
                    // "AuthorQueryEntity".
                    // SortBy doese not work with inherited properties.
                    // The bug is your method "Devexpress.Xpf.Data.ExpressionHelper.GetProperty<T>(string propertyName)"
                    // see: DevExpressSortByReflectionProblem.IQueryableExtensions.GetProperty<T>(string propertyName)

                    //query = query.SortBy(e.SortOrder, e.SortOrder.First());
                    query = query.RZLSortBy<BookQueryEntity, string>(e.SortOrder.First());

                    return query
                        .Skip(e.Skip)
                        .Take(300)
                        .ToArray();
                });
            };

            BookInfiniteAsyncSource = infiniteSource;
        }
    }

    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> RZLSortBy<T, TProperty>(this IQueryable<T> query, SortDefinition sortDescription)
        {
            var keySelector = MakeMemberExpression<T, TProperty>(sortDescription.PropertyName);

            if (sortDescription.Direction != 0)
                return query.OrderByDescending(keySelector);
            return query.OrderBy(keySelector);
        }

        private static Expression<Func<T, TResult>> MakeMemberExpression<T, TResult>(string propertyName)
            => MakeMemberExpression<T, TResult>(GetProperty<T>(propertyName));
        public static Expression<Func<T, TResult>> MakeMemberExpression<T, TResult>(PropertyInfo property)
        {
            var parameterExpression = Expression.Parameter(typeof(T), "x");
            Expression expression = Expression.Property(parameterExpression, property);

            if (typeof(TResult) != property.PropertyType)
                expression = Expression.Convert(expression, typeof(TResult));

            return Expression.Lambda<Func<T, TResult>>(expression, parameterExpression);
        }

        /// <summary>
        /// Devexpress.Xpf.Data.ExpressionHelper.GetProperty()
        /// is just calling "return typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);"
        /// But if the searched property is declared in the base class the returned PropertyInfo object a different than
        /// when called on the declaring type.
        /// So the right way that method should be implemented is like here.
        /// </summary>
        private static PropertyInfo GetProperty<T>(string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo.DeclaringType != propertyInfo.ReflectedType)
                return propertyInfo.DeclaringType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            return propertyInfo;
        }

        private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
        private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
        private static readonly FieldInfo QueryModelGeneratorField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryModelGenerator");
        private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
        private static readonly PropertyInfo DatabaseDependenciesField = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");
        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(query.Provider);
            var modelGenerator = (QueryModelGenerator)QueryModelGeneratorField.GetValue(queryCompiler);
            var queryModel = modelGenerator.ParseQuery(query.Expression);
            var database = (IDatabase)DataBaseField.GetValue(queryCompiler);
            var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesField.GetValue(database);
            var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);
            var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
            modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
            var sql = modelVisitor.Queries.First().ToString();

            return sql;
        }
    }
}
