namespace Raven.Magic.Client.MagicDocumentSession
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Raven.Client.Document;
    using RavenDocument;

    public class MagicLoadWithInclude<T> : ILoaderWithInclude<T>
    {
        private readonly ILoaderWithInclude<T> _loader;
        private readonly List<Expression<Func<T, object>>> _paths = new List<Expression<Func<T, object>>>();

        public MagicLoadWithInclude(ILoaderWithInclude<T> loader, Expression<Func<T, object>> path)
        {
            _loader = loader;
            _paths.Add(path);
        }

        private static TResult[] LoadIds<TResult>(TResult[] items, IList<string> ids)
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = items[i].Id(ids[i]);
            }
            return items;
        }

        public ILoaderWithInclude<T> Include(string path)
        {
            _loader.Include(path);
            return this;
        }

        public ILoaderWithInclude<T> Include(Expression<Func<T, object>> path)
        {
            _loader.Include(path);
            _paths.Add(path);
            return this;
        }

        public ILoaderWithInclude<T> Include<TInclude>(Expression<Func<T, object>> path)
        {
            _loader.Include<TInclude>(path);
            _paths.Add(path);
            return this;
        }

        public T[] Load(params string[] ids)
        {
            return LoadIds(_loader.Load(ids), ids.ToList());
        }

        public T[] Load(IEnumerable<string> ids)
        {
            return LoadIds(_loader.Load(ids), ids.ToList());
        }

        public T Load(string id)
        {
            return _loader.Load(id).Id(id);
        }

        public T Load(ValueType id)
        {
            return _loader.Load(id);
        }

        public T[] Load(params ValueType[] ids)
        {
            return _loader.Load(ids);
        }

        public T[] Load(IEnumerable<ValueType> ids)
        {
            return _loader.Load(ids);
        }

        public TResult[] Load<TResult>(params string[] ids)
        {
            return LoadIds(_loader.Load<TResult>(ids), ids.ToList());
        }

        public TResult[] Load<TResult>(IEnumerable<string> ids)
        {
            return LoadIds(_loader.Load<TResult>(ids), ids.ToList());
        }

        public TResult Load<TResult>(string id)
        {
            return _loader.Load<TResult>(id).Id(id);
        }

        public TResult Load<TResult>(ValueType id)
        {
            return _loader.Load<TResult>(id);
        }

        public TResult[] Load<TResult>(params ValueType[] ids)
        {
            return _loader.Load<TResult>(ids);
        }

        public TResult[] Load<TResult>(IEnumerable<ValueType> ids)
        {
            return _loader.Load<TResult>(ids);
        }
    }
}