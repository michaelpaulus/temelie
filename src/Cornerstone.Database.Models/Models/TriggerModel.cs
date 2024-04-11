using Cornerstone.Database.Extensions;

namespace Cornerstone.Database
{
    namespace Models
    {
        public class TriggerModel
        {

            public string TriggerName { get; set; }
            public string SchemaName { get; set; }
            public string Definition { get; set; }
            public string TableName { get; set; }

        }
    }

}
