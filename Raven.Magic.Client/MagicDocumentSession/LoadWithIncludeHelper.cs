namespace Raven.Magic.Client.MagicDocumentSession
{
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using DocumentCollections;
    using Raven.Client;
    using RavenDocument;

    public class LoadWithIncludeHelper
    {
        private readonly IDocumentSession _session;

        public LoadWithIncludeHelper(IDocumentSession session)
        {
            _session = session;
        }

        public object Load(object entity, params string[] includes)
        {
            includes.ToList().ForEach(a => Load(entity, a.Split('.'), 0));
            return _session.LoadId(entity);
        }

        private void Load(object entity, string[] properties, int index)
        {
            if (entity == null)
                return;

            if (properties[index].StartsWith(","))
            {
                properties[index] = properties[index].Remove(0, 1);
                foreach (object item in entity as IEnumerable)
                {
                    Load(item, properties, index);
                }
            }
            else
            {
                PropertyInfo propertyInfo = entity.GetType().GetProperty(properties[index]);

                if (properties.Count() == index + 1)
                {
                    Load(entity, propertyInfo);
                }
                else
                {
                    Load(propertyInfo.GetValue(entity), properties, ++index);
                }
            }
        }

        private void Load(object entity, PropertyInfo propertyInfo)
        {
            object property = propertyInfo.GetValue(entity);

            if (property is IEnumerable)
            {
                MethodInfo listMethod = typeof (DocumentCollectionExtentions).GetMethods().Single(a => a.Name == "List" && a.GetParameters().Count() == 2);
                propertyInfo.SetValue(entity, listMethod.MakeGenericMethod(propertyInfo.PropertyType.GetGenericArguments()).Invoke(null, new[] {_session, property}));
            }
            else if (property.Id() != null)
            {
                propertyInfo.SetValue(entity, _session.Load<object>(property.Id()));
            }
        }
    }
}