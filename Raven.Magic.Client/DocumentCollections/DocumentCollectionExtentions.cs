namespace Raven.Magic.Client.DocumentCollections
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions.Extensions;
    using Raven.Client;
    using Raven.Client.Linq;

    public static class DocumentCollectionExtentions
    {
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