namespace Raven.Magic.Client.DocumentCollections
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Internal;
    using Raven.Client;

    public class DocumentSet<T> : DocumentCollection<T>, ISet<T> where T : class
    {
        private readonly ISet<string> _keys = new HashSet<string>();

        public DocumentSet(IDocumentSession session) : base(session)
        {
        }

        public DocumentSet()
        {
        }

        public DocumentSet(ISet<string> keys)
        {
            _keys = keys;
        }

        protected override ICollection<string> Keys
        {
            get { return _keys; }
        }

        bool ISet<T>.Add(T item)
        {
            StoreIfNew(item);
            return _keys.Add(GetId(item));
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _keys.UnionWith(other.ToArray().ConvertAll(GetId));
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _keys.IntersectWith(other.ToArray().ConvertAll(GetId));
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _keys.ExceptWith(other.ToArray().ConvertAll(GetId));
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _keys.SymmetricExceptWith(other.ToArray().ConvertAll(GetId));
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _keys.IsSubsetOf(other.ToArray().ConvertAll(GetId));
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _keys.IsSupersetOf(other.ToArray().ConvertAll(GetId));
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _keys.IsProperSupersetOf(other.ToArray().ConvertAll(GetId));
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _keys.IsProperSubsetOf(other.ToArray().ConvertAll(GetId));
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _keys.Overlaps(other.ToArray().ConvertAll(GetId));
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _keys.SetEquals(other.ToArray().ConvertAll(GetId));
        }
    }

}
