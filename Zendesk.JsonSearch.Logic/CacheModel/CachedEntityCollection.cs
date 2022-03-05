//---------------------------------------------------------------------
// This is the class for entity caching
//---------------------------------------------------------------------

using System.Collections.Generic;
using Zendesk.JsonSearch.Models;

namespace Zendesk.JsonSearch.Logic.CacheModel
{
    public class CachedEntityCollection
    {
        public Dictionary<string, CachedEntities> EntityCollection { get; set; }
    }

    public class CachedEntities
    {
        public CachedEntities(List<Entity> entities)
        {
            Entities = new Dictionary<object, Entity>();
            foreach(var entity in entities)
            {
                Entities.Add(entity._id, entity);
            }
        }
        public Dictionary<object, Entity> Entities { get; set; }
    }
}
