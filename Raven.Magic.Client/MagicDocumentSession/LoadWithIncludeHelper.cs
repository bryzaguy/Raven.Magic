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
            var id = _session.GetId(entity);
            if (id != null)
            {
                var result = _session.Load<object>(id);
                includes.ToList().ForEach(a => Load(result, a.Split('.'), false));
                return result;
            }
            return entity;
        }

        private void Load(object entity, string[] properties, bool isCollection)
        {
            if (entity == null || !properties.Any())
                return;

            if (!isCollection && properties[0].StartsWith(","))
            {
                foreach (object item in entity as IEnumerable)
                {
                    Load(item, properties, true);
                }
            }
            else
            {
                var propertyInfo = entity.GetType().GetProperty(isCollection ? properties[0].Remove(0, 1) : properties[0]);

                if (properties.Count() == 1)
                {
                    Load(entity, propertyInfo);
                }
                else
                {
                    Load(propertyInfo.GetValue(entity), properties.Skip(1).ToArray(), false);
                }
            }
        }

        private void Load(object entity, PropertyInfo propertyInfo)
        {
            object property = propertyInfo.GetValue(entity);
            
            if (propertyInfo.PropertyType.IsArray && property is IEnumerable)
            {
                dynamic array = property;
                for (var i = 0; i < array.Length; i++)
                {
                    var id = RavenDocumentExtentions.Id(array[i]);

                    if (id != null)
                    {
                        var result = _session.Load<object>(id);
                        if (result != null)
                            array[i] = _session.Load<object>(id);
                    }
                }
            }
            else if (property is IEnumerable)
            {
                MethodInfo listMethod = typeof (DocumentCollectionExtentions).GetMethods().Single(a => a.Name == "List" && a.GetParameters().Count() == 2);
                propertyInfo.SetValue(entity, listMethod.MakeGenericMethod(propertyInfo.PropertyType.GetGenericArguments()).Invoke(null, new[] {_session, property}));
            }
            else if (property.Id() != null)
            {
                var result = _session.Load<object>(property.Id());
                if (result != null)
                    propertyInfo.SetValue(entity, result);
            }
        }
    }
}