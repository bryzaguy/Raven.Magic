namespace Raven.Magic.Client.MagicDocumentSession
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Abstractions.Data;
    using Json.Linq;
    using Raven.Client;
    using Raven.Client.Linq;

    public class MagicRavenQueryProvider<T> : IRavenQueryProvider 
    {
        private readonly IRavenQueryProvider _provider;
        public readonly LoadWithIncludeHelper LoadHelper;
        public readonly QueryIncludesParser Parser;

        public MagicRavenQueryProvider(IRavenQueryProvider provider, LoadWithIncludeHelper loadHelper, QueryIncludesParser parser)
        {
            _provider = provider;
            LoadHelper = loadHelper;
            Parser = parser;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var inspector = _provider.CreateQuery(expression);
            
            typeof(RavenQueryInspector<T>)
                .GetField("provider", BindingFlags.Instance|BindingFlags.NonPublic)
                .SetValue(inspector, this);

            return inspector;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var inspector = _provider.CreateQuery<TElement>(expression);
            
            typeof(RavenQueryInspector<TElement>)
                .GetField("provider", BindingFlags.Instance|BindingFlags.NonPublic)
                .SetValue(inspector, For<TElement>());

            return inspector;
        }

        public object Execute(Expression expression)
        {
            return _provider.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _provider.Execute<TResult>(expression);
        }

        public void AfterQueryExecuted(Action<QueryResult> afterQueryExecuted)
        {
            _provider.AfterQueryExecuted(afterQueryExecuted);
        }

        public void Customize(Action<IDocumentQueryCustomization> action)
        {
            action(Parser);
            _provider.Customize(action);
        }

        public void TransformWith(string transformerName)
        {
            _provider.TransformWith(transformerName);
        }

        public IRavenQueryProvider For<S>()
        {
            return new MagicRavenQueryProvider<S>(_provider.For<S>(), LoadHelper, Parser);
        }

        public IAsyncDocumentQuery<T> ToAsyncLuceneQuery<T>(Expression expression)
        {
            return _provider.ToAsyncLuceneQuery<T>(expression);
        }

        public IDocumentQuery<TResult> ToLuceneQuery<TResult>(Expression expression)
        {
            return _provider.ToLuceneQuery<TResult>(expression);
        }

        public Lazy<IEnumerable<T>> Lazily<T>(Expression expression, Action<IEnumerable<T>> onEval)
        {
            return _provider.Lazily(expression, onEval);
        }

        public void MoveAfterQueryExecuted<T>(IAsyncDocumentQuery<T> documentQuery)
        {
            _provider.MoveAfterQueryExecuted(documentQuery);
        }

        public void AddQueryInput(string input, RavenJToken foo)
        {
            _provider.AddQueryInput(input, foo);
        }

        public string IndexName
        {
            get { return _provider.IndexName; }
        }

        public IDocumentQueryGenerator QueryGenerator
        {
            get { return _provider.QueryGenerator; }
        }

        public Action<IDocumentQueryCustomization> CustomizeQuery
        {
            get { return _provider.CustomizeQuery; }
        }

        public HashSet<string> FieldsToFetch
        {
            get { return _provider.FieldsToFetch; }
        }

        public string ResultTransformer
        {
            get { return _provider.ResultTransformer; }
        }

        public Dictionary<string, RavenJToken> QueryInputs
        {
            get { return _provider.QueryInputs; }
        }

        public object LoadWithIncludes(object entity)
        {
            return LoadHelper.Load(entity, Parser.Includes.ToArray());
        }
    }
}