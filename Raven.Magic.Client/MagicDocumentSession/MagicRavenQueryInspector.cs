namespace Raven.Magic.Client.MagicDocumentSession
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Json.Linq;
    using Raven.Client;
    using Raven.Client.Indexes;
    using Raven.Client.Linq;
    using Raven.Client.Spatial;

    public class MagicRavenQueryInspector<T> : IRavenQueryable<T>
    {
        private readonly IRavenQueryable<T> _inspector;
        public readonly LoadWithIncludeHelper LoadHelper;
        public readonly QueryIncludesParser Parser;

        public MagicRavenQueryInspector(IRavenQueryable<T> inspector, LoadWithIncludeHelper loadHelper) : this(inspector, loadHelper, new QueryIncludesParser())
        {
        }

        public MagicRavenQueryInspector(IRavenQueryable<T> inspector, LoadWithIncludeHelper loadHelper, QueryIncludesParser parser)
        {
            _inspector = inspector;
            LoadHelper = loadHelper;
            Parser = parser;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _inspector.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get { return _inspector.Expression; }
        }

        public Type ElementType
        {
            get { return _inspector.ElementType; }
        }

        public IQueryProvider Provider
        {
            get { return _inspector.Provider; }
        }

        public IRavenQueryable<T> Statistics(out RavenQueryStatistics stats)
        {
            _inspector.Statistics(out stats);
            return this;
        }

        public IRavenQueryable<T> Customize(Action<IDocumentQueryCustomization> action)
        {
            action(Parser);
            _inspector.Customize(action);
            return this;
        }

        public IRavenQueryable<TResult> TransformWith<TTransformer, TResult>() where TTransformer : AbstractTransformerCreationTask, new()
        {
            return new MagicRavenQueryInspector<TResult>(_inspector.TransformWith<TTransformer, TResult>(), LoadHelper, Parser);
        }

        public IRavenQueryable<T> AddQueryInput(string name, RavenJToken value)
        {
            _inspector.AddQueryInput(name, value);
            return this;
        }

        public IRavenQueryable<T> Spatial(Expression<Func<T, object>> path, Func<SpatialCriteriaFactory, SpatialCriteria> clause)
        {
            _inspector.Spatial(path, clause);
            return this;
        }

        public T LoadWithIncludes(T entity)
        {
            return (T)LoadHelper.Load(entity, Parser.Includes.ToArray());
        }
    }
}