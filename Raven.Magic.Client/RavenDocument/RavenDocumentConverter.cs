namespace Raven.Magic.Client.RavenDocument
{
    using System;
    using Imports.Newtonsoft.Json;

    public class RavenDocumentConverter : JsonConverter
    {
        public override bool CanRead
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var proxy = value as IRavenDocument;
            if (proxy != null)
                writer.WriteValue(proxy.Id);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Should never read proxy items out of the database.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetInterface(typeof (IRavenDocument).FullName) != null;
        }
    }
}