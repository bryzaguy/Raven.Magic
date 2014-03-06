namespace Raven.Magic.Client.RavenDocument
{
    public interface IRavenDocument
    {
        string Id { get; set; }
    }

    public class RavenDocument : IRavenDocument
    {
        public string Id { get; set; }
    }
}