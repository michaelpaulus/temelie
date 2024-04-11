namespace Cornerstone.Database
{
    namespace Models
    {
        public class CheckConstraintModel : DatabaseObjectModel
        {

            public string CheckConstraintName { get; set; }
            public string TableName { get; set; }
            public string SchemaName { get; set; }
            public string CheckConstraintDefinition { get; set; }

        }
    }

}
