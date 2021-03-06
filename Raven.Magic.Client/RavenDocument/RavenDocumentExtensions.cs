﻿namespace Raven.Magic.Client.RavenDocument
{
    using System;
    using Castle.Core.Internal;
    using Castle.DynamicProxy;
    using Raven.Client;

    public static class RavenDocumentExtentions
    {
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
            if (entity is IRavenDocument)
                return (entity as IRavenDocument).Id;

            var property = typeof (T).GetProperty("Id", typeof (string));
            if (property != null)
                return property.GetValue(entity) as string;

            return null;
        }

        public static T Id<T>(this T entity, string id)
        {
            var property = typeof (T).GetProperty("Id", typeof (string));
            if (property != null)
            {
                if (property.CanWrite && (string) property.GetValue(entity) != id)
                    property.SetValue(entity, id);

                return entity;
            }

            return new RavenDocument {Id = id}.AttachRavenIdToEntity(entity);
        }

        private static T AttachRavenIdToEntity<T>(this RavenDocument document, T entity)
        {
            if (entity is IRavenDocument || Equals(entity, null)) return entity;
            var proxy = Generator.CreateClassProxyWithTarget(entity.GetType(), new[] {typeof (IRavenDocument)}, entity, GetOptions(document));
            return (T)entity.MapTo(proxy);
        }

        public static readonly ProxyGenerator Generator = new ProxyGenerator();

        public static object MapTo(this object source, object target)
        {
            return source.MapTo(source.GetType(), target);
        }

        public static object MapTo(this object source, Type targetType, object target)
        {
            foreach (var propertyInfo in targetType.GetProperties())
            {
                if (propertyInfo.CanWrite && !Equals(propertyInfo.GetValue(source),(propertyInfo.GetValue(target))))
                    propertyInfo.SetValue(target, propertyInfo.GetValue(source));
            }

            foreach (var fieldInfo in targetType.GetFields())
            {
                if (!Equals(fieldInfo.GetValue(source), fieldInfo.GetValue(target)))
                    fieldInfo.SetValue(target, fieldInfo.GetValue(source));
            }

            return target;
        }

        public static ProxyGenerationOptions GetOptions(RavenDocument document)
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(document);
            return options;
        }
    }
}