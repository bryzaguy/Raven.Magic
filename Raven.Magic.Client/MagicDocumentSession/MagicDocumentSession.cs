namespace Raven.Magic.Client.MagicDocumentSession
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Abstractions.Data;
    using DocumentCollections;
    using Imports.Newtonsoft.Json;
    using Raven.Client;
    using Raven.Client.Document;
    using Raven.Client.Indexes;
    using Raven.Client.Linq;
    using RavenDocument;

    public class MagicDocumentSession : IDocumentSession, IMagicDocumentSession
    {
        private readonly IDocumentSession _session;

        private readonly HashSet<object> _loadedObjects = new HashSet<object>();

        public MagicDocumentSession(IDocumentSession session)
        {
            _session = SetupDocumentStore(session);
        }

        public static IDocumentSession SetupDocumentStore(IDocumentSession session)
        {
            var conventions = session.Advanced.DocumentStore.Conventions;
            conventions.CustomizeJsonSerializer = CustomizeJsonSerializer(conventions.CustomizeJsonSerializer);
            return session;
        }

        private static Action<JsonSerializer> CustomizeJsonSerializer(Action<JsonSerializer> customization)
        {
            return json =>
            {
                if (customization != null)
                    customization(json);

                if (json.Converters.All(a => a.GetType() != typeof(RavenDocumentConverter)))
                {
                    json.Converters.Add(new RavenDocumentConverter());
                    json.Converters.Add(new DocumentCollectionConverter());
                    json.Converters.Add(new RavenDocumentProxyConverter());
                }
            };
        }

        private T AddObjectToLoadedObjects<T>(T value)
        {
            _loadedObjects.Add(value);
            return value;
        }

        private T[] AddObjectsToLoadedObjects<T>(T[] values)
        {
            _loadedObjects.UnionWith(values.Cast<object>());
            return values;
        }

        private static dynamic UpdatedTarget(object entity)
        {
            var target = (entity as dynamic).__target;
            var result = RavenDocumentExtentions.MapTo(entity, target.GetType(), target);
            return result;
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        public void Delete<T>(T entity)
        {
            _session.Delete(entity);
        }

        public T Load<T>(string id)
        {
            return AddObjectToLoadedObjects(_session.Load<T>(id).Id(id));
        }

        public T[] Load<T>(params string[] ids)
        {
            return AddObjectsToLoadedObjects(_session.LoadIds(_session.Load<T>(ids)));
        }

        public T[] Load<T>(IEnumerable<string> ids)
        {
            return AddObjectsToLoadedObjects(_session.LoadIds(_session.Load<T>(ids)));
        }

        public T Load<T>(ValueType id)
        {
            return AddObjectToLoadedObjects(_session.LoadId(_session.Load<T>(id)));
        }

        public T[] Load<T>(params ValueType[] ids)
        {
            return AddObjectsToLoadedObjects(_session.LoadIds(_session.Load<T>(ids)));
        }

        public T[] Load<T>(IEnumerable<ValueType> ids)
        {
            return AddObjectToLoadedObjects(_session.LoadIds(_session.Load<T>(ids)));
        }

        public IRavenQueryable<T> Query<T>(string indexName, bool isMapReduce = false)
        {
            return new MagicRavenQueryInspector<T>(_session.Query<T>(indexName, isMapReduce), new LoadWithIncludeHelper(this));
        }

        public IRavenQueryable<T> Query<T>()
        {
            return new MagicRavenQueryInspector<T>(_session.Query<T>(), new LoadWithIncludeHelper(this));
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            return new MagicRavenQueryInspector<T>(_session.Query<T, TIndexCreator>(), new LoadWithIncludeHelper(this));
        }

        public ILoaderWithInclude<object> Include(string path)
        {
            return new MagicLoadWithInclude<object>(new LoadWithIncludeHelper(this), _session.Include(path)).Include(path);
        }

        public ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return new MagicLoadWithInclude<T>(new LoadWithIncludeHelper(this), _session.Include(path)).Include(path);
        }

        public ILoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, object>> path)
        {
            return new MagicLoadWithInclude<T>(new LoadWithIncludeHelper(this), _session.Include(path)).Include(path);
        }

        public TResult Load<TTransformer, TResult>(string id) where TTransformer : AbstractTransformerCreationTask, new()
        {
            return _session.Load<TTransformer, TResult>(id);
        }

        public TResult Load<TTransformer, TResult>(string id, Action<ILoadConfiguration> configure) where TTransformer : AbstractTransformerCreationTask, new()
        {
            return _session.Load<TTransformer, TResult>(id, configure);
        }

        public TResult[] Load<TTransformer, TResult>(params string[] ids) where TTransformer : AbstractTransformerCreationTask, new()
        {
            return _session.Load<TTransformer, TResult>(ids);
        }

        public TResult[] Load<TTransformer, TResult>(IEnumerable<string> ids, Action<ILoadConfiguration> configure) where TTransformer : AbstractTransformerCreationTask, new()
        {
            return _session.Load<TTransformer, TResult>(ids, configure);
        }

        public void SaveChanges()
        {
            foreach (var entity in _loadedObjects)
            {
                if (entity is IRavenDocument)
                {
                    UpdatedTarget(entity);
                }
            }

            _session.SaveChanges();
        }

        public void Store(object entity, Etag etag)
        {
            if (entity is IRavenDocument)
            {
                _session.Store(UpdatedTarget(entity), etag, (entity as IRavenDocument).Id);
            }
            else
                _session.Store(entity, etag);
        }

        public void Store(object entity, Etag etag, string id)
        {
            if (entity is IRavenDocument)
            {
                _session.Store(UpdatedTarget(entity), etag, id);
            }
            else
                _session.Store(entity, etag, id);
        }

        public void Store(dynamic entity)
        {
            if (entity is IRavenDocument)
            {
                _session.Store(UpdatedTarget(entity), (entity as IRavenDocument).Id);
            }
            else
                _session.Store(entity);
        }

        public void Store(dynamic entity, string id)
        {
            if (entity is IRavenDocument)
            {
                _session.Store(UpdatedTarget(entity), id);
            }
            else
                _session.Store(entity, id);
        }

        public ISyncAdvancedSessionOperation Advanced
        {
            get { return _session.Advanced; }
        }
    }

    public interface IMagicDocumentSession
    {
    }
}