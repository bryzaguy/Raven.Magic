namespace Raven.Magic.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Abstractions.Data;
    using Raven.Client;

    public static class Extensions
    {
        public static void Save<T>(this IDocumentStore store, T value) 
        {
            using (var session = store.OpenSession())
                session.Save(value);
        }
        
        public static void Save<T>(this IDocumentStore store, T value, string id) 
        {
            using (var session = store.OpenSession())
                session.Save(value, id);
        }
        
        public static void Save<T>(this IDocumentStore store, T value, Etag tag) 
        {
            using (var session = store.OpenSession())
                session.Save(value, tag);
        }

        public static void Save<T>(this IDocumentStore store, T value, Etag tag, string id) 
        {
            using (var session = store.OpenSession())
                session.Save(value, tag, id);
        }
        
        public static void SaveRange<T>(this IDocumentStore store, IEnumerable<T> values) 
        {
            using (var session = store.OpenSession())
                session.SaveRange(values);
        }

        public static void SaveRange<T>(this IDocumentStore store, IEnumerable<T> values, Func<T, string> getKey) 
        {
            using (var session = store.OpenSession())
                session.SaveRange(values, getKey);
        }

        public static void Save<T>(this IDocumentSession session, T value) 
        {
            session.Store(value);
            session.SaveChanges();
        }
        
        public static void Save<T>(this IDocumentSession session, T value, string id) 
        {
            session.Store(value, id);
            session.SaveChanges();
        }
        
        public static void Save<T>(this IDocumentSession session, T value, Etag tag) 
        {
            session.Store(value, tag);
            session.SaveChanges();
        }
        
        public static void Save<T>(this IDocumentSession session, T value, Etag tag, string id) 
        {
            session.Store(value, tag, id);
            session.SaveChanges();
        }
        
        public static void SaveRange<T>(this IDocumentSession session, IEnumerable<T> values) 
        {
            session.StoreRange(values);
            session.SaveChanges();
        }

        public static void SaveRange<T>(this IDocumentSession session, IEnumerable<T> values, Func<T, string> getKey) 
        {
            session.StoreRange(values, getKey);
            session.SaveChanges();
        }

        public static void StoreRange<T>(this IDocumentSession session, IEnumerable<T> values) 
        {
            foreach (var value in values)
            {
                session.Store(value);
            }
        }

        public static void StoreRange<T>(this IDocumentSession session, IEnumerable<T> values, Func<T, string> getKey) 
        {
            foreach (var value in values)
            {
                session.Store(value, getKey(value));
            }
        }
    }
    
    public static class AsyncExtensions
    {
        public static async Task SaveAsync<T>(this IDocumentStore store, T value) 
        {
            using (var session = store.OpenAsyncSession())
                await session.SaveAsync(value);
        }
        
        public static async Task SaveAsync<T>(this IDocumentStore store, T value, string id) 
        {
            using (var session = store.OpenAsyncSession())
                await session.SaveAsync(value, id);
        }
        
        public static async Task SaveAsync<T>(this IDocumentStore store, T value, Etag tag) 
        {
            using (var session = store.OpenAsyncSession())
                await session.SaveAsync(value, tag);
        }

        public static async Task SaveAsync<T>(this IDocumentStore store, T value, Etag tag, string id) 
        {
            using (var session = store.OpenAsyncSession())
                await session.SaveAsync(value, tag, id);
        }
        
        public static async Task SaveRangeAsync<T>(this IDocumentStore store, IEnumerable<T> values) 
        {
            using (var session = store.OpenAsyncSession())
                await session.SaveRangeAsync(values);
        }

        public static async Task SaveRangeAsync<T>(this IDocumentStore store, IEnumerable<T> values, Func<T, string> getKey) 
        {
            using (var session = store.OpenAsyncSession())
                await session.SaveRangeAsync(values, getKey);
        }

        public static async Task SaveAsync<T>(this IAsyncDocumentSession session, T value) 
        {
            await session.StoreAsync(value);
            await session.SaveChangesAsync();
        }
        
        public static async Task SaveAsync<T>(this IAsyncDocumentSession session, T value, string id) 
        {
            await session.StoreAsync(value, id);
            await session.SaveChangesAsync();
        }
        
        public static async Task SaveAsync<T>(this IAsyncDocumentSession session, T value, Etag tag) 
        {
            await session.StoreAsync(value, tag);
            await session.SaveChangesAsync();
        }
        
        public static async Task SaveAsync<T>(this IAsyncDocumentSession session, T value, Etag tag, string id) 
        {
            await session.StoreAsync(value, tag, id);
            await session.SaveChangesAsync();
        }
        
        public static async Task SaveRangeAsync<T>(this IAsyncDocumentSession session, IEnumerable<T> values) 
        {
            await session.StoreRangeAsync(values);
            await session.SaveChangesAsync();
        }

        public static async Task SaveRangeAsync<T>(this IAsyncDocumentSession session, IEnumerable<T> values, Func<T, string> getKey) 
        {
            await session.StoreRangeAsync(values, getKey);
            await session.SaveChangesAsync();
        }

        public static async Task StoreRangeAsync<T>(this IAsyncDocumentSession session, IEnumerable<T> values) 
        {
            foreach (var value in values)
            {
                await session.StoreAsync(value);
            }
        }

        public static async Task StoreRangeAsync<T>(this IAsyncDocumentSession session, IEnumerable<T> values, Func<T, string> getKey) 
        {
            foreach (var value in values)
            {
                await session.StoreAsync(value, getKey(value));
            }
        }
    }
}
