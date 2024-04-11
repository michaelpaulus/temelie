namespace Cornerstone.Database
{
    namespace Models
    {
        public class ForeignKeyModel : DatabaseObjectModel
        {
          
            public string ForeignKeyName { get; set; }
            public string TableName { get; set; }
            public string SchemaName { get; set; }
            public bool IsNotForReplication { get; set; }
            public string DeleteAction { get; set; }
            public string UpdateAction { get; set; }
            public string ReferencedTableName { get; set; }
            public string ReferencedSchemaName { get; set; }

            private IList<ForeignKeyDetailModel> _detail;
            public IList<ForeignKeyDetailModel> Detail
            {
                get
                {
                    if (this._detail == null)
                    {
                        this._detail = new List<ForeignKeyDetailModel>();
                    }
                    return this._detail;
                }
            }

        }
    }

}
