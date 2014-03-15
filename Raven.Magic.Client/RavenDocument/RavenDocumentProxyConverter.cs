namespace Raven.Magic.Client.RavenDocument
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Castle.DynamicProxy;
    using Imports.Newtonsoft.Json;
    using Imports.Newtonsoft.Json.Linq;

    public class RavenDocumentProxyConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Only read existing data types...should save as proxy objects.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String) return ProxyWithId(objectType, reader.Value as string);
            if (reader.TokenType == JsonToken.Null) return null;
            JObject json = JObject.Load(reader);
            object result = Activator.CreateInstance(GetType(json, objectType));
            foreach (PropertyInfo propertyInfo in objectType.GetProperties())
            {
                JToken property = json[propertyInfo.Name];
                if (property != null)
                {
                    var propResult = serializer.Deserialize(property.CreateReader(), GetType(property, propertyInfo.PropertyType));
                    propertyInfo.SetValue(result, propResult);
                }

            }
            return result;
        }

        internal object ProxyWithId(Type type, string id)
        {
            var document = new RavenDocument { Id = id };
            var interfaces = new[] { typeof(IRavenDocument) };

            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(document);
            
            if (type.IsInterface) return new ProxyGenerator().CreateInterfaceProxyWithoutTarget(type, interfaces, options);
            return new ProxyGenerator().CreateClassProxy(type, interfaces, options);
        }

        private static Type GetType(JToken token, Type defaultType)
        {
            if (token.Type == JTokenType.Object && token["$type"] != null)
                return Type.GetType(token["$type"].ToString());
            return defaultType;
        }

        public override bool CanConvert(Type objectType)
        {
            bool canConvert = objectType != typeof(string)                          // Not a string
                && !objectType.IsArray                                              // Not an array
                && (objectType.IsClass || objectType.IsInterface)                   // Is a class or an interface.
                && objectType.GetInterface(typeof(IEnumerable).FullName) == null;   // Doesn't implement IEnumerable

            return canConvert;
        }
    }
}
