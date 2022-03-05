//---------------------------------------------------------------------
// This is the class for indexing caching
// In real production environment this should be in physical file
//---------------------------------------------------------------------

using System.Collections.Generic;
using Zendesk.JsonSearch.Models;

namespace Zendesk.JsonSearch.Logic.CacheModel
{
    public class CachedIndexingCollection
    {
        public Dictionary<string, Dictionary<string, CachedIndexings>> IndexingCollection { get; set; }
    }

    public class CachedIndexings
    {
        public CachedIndexings(List<Entity> entities, string fieldName)
        {
            Indexings = new Dictionary<object, List<object>>();
            foreach (var entity in entities)
            {
                var fieldValue = entity.GetAttribute<object>(fieldName);
                if (fieldValue != null)
                {
                    if (fieldValue is List<string>)
                    {
                        foreach(var subValue in fieldValue as List<string>)
                        {
                            AppendIndexing(subValue, entity);
                        }
                    }
                    else
                    {
                        AppendIndexing(fieldValue, entity);
                    }
                }
            }
        }

        private void AppendIndexing(object value,Entity entity)
        {
            if (Indexings.ContainsKey(value))
            {
                Indexings[value].Add(entity._id);
            }
            else
            {
                Indexings.Add(value, new List<object> { entity._id });
            }
        }

        public Dictionary<object, List<object>> Indexings { get; set; }
    }
}
