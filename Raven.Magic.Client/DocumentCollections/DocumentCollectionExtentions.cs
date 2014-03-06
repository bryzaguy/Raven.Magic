namespace Raven.Magic.Client.DocumentCollections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Abstractions.Extensions;
    using Castle.Core.Internal;
    using Raven.Client;
    using Raven.Client.Linq;
    using RavenDocument;

    public static class DocumentCollectionExtentions
    {
        public static T LoadWithIncludes<T>(this IDocumentSession session, string id, params Expression<Func<T, object>>[] includes)
        {
            var entity = session.Load<T>(id);
            return session.LoadWithIncludes(entity, includes);
        }

        public static T LoadWithIncludes<T>(this IDocumentSession session, T entity, params Expression<Func<T, object>>[] includes)
        {
            return session.LoadWithIncludes(entity, includes.ConvertAll(a => a.ToPropertyPath()));
        }

        private static T LoadWithIncludes<T>(this IDocumentSession session, T entity, params string[] includes)
        {
            includes.ToList().ForEach(a => LoadWithIncludes(session, entity, a.Split('.'), 0));
            return entity;
        }

        private static void LoadWithIncludes(IDocumentSession session, object entity, string[] properties, int index)
        {
            if (entity == null)
                return;

            if (properties[index].StartsWith(","))
            {
                properties[index] = properties[index].Remove(0, 1);
                foreach (object item in entity as IEnumerable)
                {
                    LoadWithIncludes(session, item, properties, index);
                }
            }
            else
            {
                PropertyInfo propertyInfo = entity.GetType().GetProperty(properties[index]);

                if (properties.Count() == index + 1)
                {
                    session.LoadWithIncludes(entity, propertyInfo);
                }
                else
                {
                    LoadWithIncludes(session, propertyInfo.GetValue(entity), properties, ++index);
                }
            }
        }

        private static void LoadWithIncludes(this IDocumentSession session, object entity, PropertyInfo propertyInfo)
        {
            object property = propertyInfo.GetValue(entity);

            if (property is IEnumerable)
            {
                MethodInfo listMethod = typeof (DocumentCollectionExtentions).GetMethods().Single(a => a.Name == "List" && a.GetParameters().Count() == 2);
                propertyInfo.SetValue(entity, listMethod.MakeGenericMethod(propertyInfo.PropertyType.GetGenericArguments()).Invoke(null, new[] {session, property}));
            }
            else if (property.Id() != null)
            {
                propertyInfo.SetValue(entity, session.Load<object>(property.Id()));
            }
        }

        public static DocumentList<T> ToList<T>(this IRavenQueryable<T> query, IDocumentSession session) where T : class
        {
            return List(session, query.ToList());
        }

        public static DocumentList<T> List<T>(this IDocumentSession session) where T : class
        {
            return new DocumentList<T>(session);
        }

        public static DocumentList<T> List<T>(this IDocumentSession session, IEnumerable<T> items) where T : class
        {
            DocumentList<T> list = List<T>(session);
            if (items != null) list.AddRange(items);
            return list;
        }
    }
}