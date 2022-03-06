//---------------------------------------------------------------------
// This is the base class for all entites
// Every record in any entity json file (users.json,tickets.json or any new ones) will be initised as instance of this class
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zendesk.JsonSearch.Models.Helpers;

namespace Zendesk.JsonSearch.Models
{
    public class Entity
    {
        public Entity()
        {
            Attributes = new List<KeyValuePair<string, object>>();
        }

        public Entity(string logicalName)
        {
            entityName = logicalName;
            Attributes = new List<KeyValuePair<string, object>>();
        }

        public Entity(string id, string logicalName)
        {
            _id = id;
            entityName = logicalName;
            Attributes = new List<KeyValuePair<string, object>>();
            SetAttribute(nameof(_id), id);
        }

  
        public string _id { get; set; }

        /// <summary>
        /// name of record. It can be different field name (e.x. name in User but subject in Ticket) defined in metadata
        /// </summary>
        public string name { get; set; }

     
        public string entityName { get; set; }

       
        public List<KeyValuePair<string, object>> Attributes { get; set; }

    
        public T GetAttribute<T>(string name)
        {
            if (!Attributes.Any(a => a.Key == name)) return default(T);
            return (T)Attributes.First(att => att.Key == name).Value;
        }

    
        public void SetAttribute(string name, object value)
        {
            if (name == nameof(_id)) _id = (string)value;
            if (Attributes.Any(a => a.Key == name))
            {
                Attributes.RemoveAll(a => a.Key == name);
            }
            Attributes.Add(new KeyValuePair<string, object>(name, value));
        }

        public virtual string ToConsoleString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var pair in Attributes)
            {
                stringBuilder.Append($"{pair.Key,-20}{pair.Value.ConvertToString()}");
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        public static string ToConsoleString(List<Entity> entities)
        {
            var stringBuilder = new StringBuilder();
            foreach (var entity in entities)
            {
                stringBuilder.Append(entity.ToConsoleString());
                stringBuilder.Append("------------------------------------------");
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

   
        public EntityReference ToEntityReference()
        {
            return new EntityReference
            {
                _id = _id,
                name = name,
                entityName = entityName
            };
        }

        /// <summary>
        /// This is to convert generic Entity to any specific entity type. (e.x. User/Ticket in early bound folder)
        /// Not used in this project. For extension and demo only.
        /// </summary>
        /// <typeparam name="T">target entity type class</typeparam>
        /// <returns>instance of target entity type</returns>
        public T ToEntity<T>() where T : Entity
        {
            if (typeof(T) == typeof(Entity))
            {
                Entity target = new Entity();
                this.ShallowCopyTo(target);
                return target as T;
            }
            if (string.IsNullOrWhiteSpace(entityName))
                throw new NotSupportedException("Entity name must be set before calling ToEntity()");
            T instance = (T)Activator.CreateInstance(typeof(T));
            this.ShallowCopyTo((Entity)instance);
            return instance;
        }

        internal void ShallowCopyTo(Entity target)
        {
            if (target == null || target == this)
                return;
            target._id = _id;
            target.name = name;
            target.entityName = entityName;
            target.Attributes = Attributes;
        }
    }
}
