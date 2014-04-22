namespace Raven.Magic.Client.DocumentCollections
{
    using System.Collections.Generic;
    using Raven.Client;
    using RavenDocument;

    public class DocumentList<T> : DocumentCollection<T>, IList<T> where T : class
    {
        private readonly IList<string> _keys = new List<string>();

        public DocumentList(IDocumentSession session) : base(session)
        {
        }

        public DocumentList(IList<string> keys)
        {
            _keys = keys;
        }

        public DocumentList()
        {
        }

        public override ICollection<string> Keys
        {
            get { return _keys; }
        }

        public int IndexOf(T item)
        {
            return _keys.IndexOf(GetId(item));
        }

        public void Insert(int index, T item)
        {
            StoreIfNew(item);
            _keys.Insert(index, GetId(item));
        }

        public void RemoveAt(int index)
        {
            _keys.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return _session.Load<T>(_keys[index]).Id(_keys[index]); }
            set
            {
                StoreIfNew(value);
                _keys[index] = GetId(value);
            }
        }
    }
}