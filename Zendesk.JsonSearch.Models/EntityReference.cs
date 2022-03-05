//---------------------------------------------------------------------
// This class is used to save relationship between entities
// e.x. in ticket entity, assignee_id can be saved as this class instance.
// This can be used to easily reflect foreign key (relationships) in further development
//---------------------------------------------------------------------

namespace Zendesk.JsonSearch.Models
{
    public class EntityReference
    {
        public string _id { get; set; }
        public string entityName { get; set; }
        public string name { get; set; }
    }
}
