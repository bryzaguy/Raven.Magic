namespace Raven.Magic.Client.DocumentCollections
{
    using System;
    using System.Collections.Generic;
    using Imports.Newtonsoft.Json;
    using Imports.Newtonsoft.Json.Linq;

    public class DocumentCollectionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IDocumentCollection)
            {
                serializer.Serialize(writer, (value as IDocumentCollection).Keys);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Activator.CreateInstance(objectType, JArray.Load(reader).ToObject<IEnumerable<string>>());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetInterface(typeof (IDocumentCollection).FullName) != null;
        }
    }
}