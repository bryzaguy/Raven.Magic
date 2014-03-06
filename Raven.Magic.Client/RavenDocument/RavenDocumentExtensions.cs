namespace Raven.Magic.Client.RavenDocument
{
    using System;
    using System.Reflection;
    using Castle.Core.Internal;
    using Castle.DynamicProxy;
    using DocumentCollections;
    using Imports.Newtonsoft.Json;
    using Raven.Client;

    public static class RavenDocumentExtentions
    {
        public static IDocumentStore WithMagic(this IDocumentStore store, Action<JsonSerializer> customizeJsonSerializer = null)
        {
            store.Conventions.CustomizeJsonSerializer = json =>
            {
                json.Converters.Add(new RavenDocumentConverter());
                json.Converters.Add(new DocumentCollectionConverter());
                json.Converters.Add(new RavenDocumentProxyConverter());
                if (customizeJsonSerializer != null) customizeJsonSerializer.Invoke(json);
            };

            return store;
        }

        public static T Property<T>(this IDocumentSession session, T entity)
        {
            if (session.Advanced.GetDocumentId(entity) == null)
            {
                session.Store(entity);
            }

            return LoadId(session, entity);
        }

        public static T[] LoadIds<T>(this IDocumentSession session, T[] entities)
        {
            return entities.ConvertAll(session.LoadId);
        }

        public static T LoadId<T>(this IDocumentSession session, T entity)
        {
            return new RavenDocument { Id = session.Advanced.GetDocumentId(entity) }.AttachRavenIdToEntity(entity);
        }

        public static string GetId<T>(this IDocumentSession session, T entity)
        {
            return session.Advanced.GetDocumentId(entity) ?? entity.Id();
        }

        public static string Id<T>(this T entity)
        {
            return (entity is IRavenDocument) ? (entity as IRavenDocument).Id : null;
        }

        public static T Id<T>(this T entity, string id)
        {
            return new RavenDocument {Id = id}.AttachRavenIdToEntity(entity);
        }

        private static T AttachRavenIdToEntity<T>(this RavenDocument document, T entity)
        {
            if (entity is IRavenDocument) return entity;
            var proxy = new ProxyGenerator().CreateClassProxyWithTarget(entity.GetType(), new[] {typeof (IRavenDocument)}, entity, GetOptions(document));
            return entity.MapTo((T)proxy);
        }

        public static T MapTo<T>(this T entity, T proxy)
        {
            foreach (PropertyInfo propertyInfo in typeof (T).GetProperties())
            {
                if (propertyInfo.CanWrite)
                    propertyInfo.SetValue(proxy, propertyInfo.GetValue(entity));
            }
            return proxy;
        }

        private static ProxyGenerationOptions GetOptions(RavenDocument document)
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(document);
            return options;
        }

        internal static object ProxyWithId(this Type type, string id)
        {
            var document = new RavenDocument {Id = id};
            var interfaces = new[] {typeof (IRavenDocument)};
            if (type.IsInterface) return new ProxyGenerator().CreateInterfaceProxyWithoutTarget(type, interfaces, GetOptions(document));
            return new ProxyGenerator().CreateClassProxy(type, interfaces, GetOptions(document));
        }
    }
}