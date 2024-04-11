namespace Cornerstone.Database
{
    namespace Models
    {
        public class DefinitionModel : DatabaseObjectModel
        {

            public string DefinitionName { get; set; }
            public string SchemaName { get; set; }
            public string Definition { get; set; }
            public string XType { get; set; }

            public string Type
            {
                get
                {
                    switch (this.XType)
                    {
                        case "P":
                            return "PROCEDURE";
                        case "V":
                            return "VIEW";
                    }
                    return "FUNCTION";
                }
            }

            public TableModel View { get; set; }

        }
    }

}
