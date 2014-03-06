namespace Raven.Magic.Client.DocumentCollections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Internal;
    using Imports.Newtonsoft.Json;
    using Imports.Newtonsoft.Json.Linq;
    using RavenDocument;

    public class DocumentCollectionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, (value as IEnumerable).Cast<IRavenDocument>().ToArray().ConvertAll(a => a.Id));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Activator.CreateInstance(objectType, JArray.Load(reader).ToObject<IEnumerable<string>>());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.BaseType != null && objectType.BaseType.IsGenericType && objectType.BaseType.GetGenericTypeDefinition() == typeof (DocumentCollection<>);
        }
    }
}