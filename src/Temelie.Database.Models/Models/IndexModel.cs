namespace Temelie.Database
{
    namespace Models
    {
        public class IndexModel : DatabaseObjectModel
        {
       
            public string TableName { get; set; }
            public string SchemaName { get; set; }
            public string IndexName { get; set; }
            public string IndexType { get; set; }
            public string FilterDefinition { get; set; }
            public string PartitionSchemeName { get; set; }
            public string DataCompressionDesc { get; set; }
            public bool IsUnique { get; set; }
            public int FillFactor { get; set; }

            public bool IsPrimaryKey { get; set; }
            public int TotalBucketCount { get; set; }

            private IList<IndexColumnModel> _columns;
            public IList<IndexColumnModel> Columns
            {
                get
                {
                    if (this._columns == null)
                    {
                        this._columns = new List<IndexColumnModel>();
                    }
                    return this._columns;
                }
                set
                {
                    this._columns = value;
                }
            }

            private IList<IndexColumnModel> _includeColumns;
            public IList<IndexColumnModel> IncludeColumns
            {
                get
                {
                    if (this._includeColumns == null)
                    {
                        this._includeColumns = new List<IndexColumnModel>();
                    }
                    return this._includeColumns;
                }
            }

        }
    }

}
