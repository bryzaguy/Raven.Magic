namespace Raven.Magic.Client.DocumentCollections
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Internal;
    using Raven.Client;
    using RavenDocument;

    public abstract class DocumentCollection<T> : ICollection<T> where T : class
    {
        protected IDocumentSession _session;

        protected DocumentCollection()
        {
        }

        protected DocumentCollection(IDocumentSession session)
        {
            Connect(session);
        }

        protected string GetId(T item)
        {
            return _session.GetId(item);
        }

        protected abstract ICollection<string> Keys { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Keys.Select(key => _session.Load<T>(key).Id(key)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(T item)
        {
            StoreIfNew(item);
            Keys.Add(GetId(item));
        }

        protected void StoreIfNew(T item)
        {
            if (_session != null && item.Id() == null)
                _session.Store(item);
        }

        public void Clear()
        {
            Keys.Clear();
        }

        public bool Contains(T item)
        {
            return Keys.Contains(_session.GetId(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Keys.CopyTo(array.ConvertAll(a => _session.GetId(a)), arrayIndex);
        }

        public bool Remove(T item)
        {
            return Keys.Remove(GetId(item));
        }

        public int Count
        {
            get { return Keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Connect(IDocumentSession session)
        {
            _session = session;
        }
    }
}