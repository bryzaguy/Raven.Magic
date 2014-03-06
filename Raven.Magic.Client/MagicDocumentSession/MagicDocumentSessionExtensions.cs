namespace Raven.Magic.Client.MagicDocumentSession
{
    using Raven.Client;

    public static class MagicDocumentSessionExtensions
    {
        public static IDocumentSession OpenMagicSession(this IDocumentStore store)
        {
            return new MagicDocumentSession(store.OpenSession());
        }
    }
}