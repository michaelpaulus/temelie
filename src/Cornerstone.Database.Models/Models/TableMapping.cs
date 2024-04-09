namespace Cornerstone.Database
{
    namespace Models
    {
        public class TableMapping : Mapping
        {
            private string _sourceTableName;
            public string SourceTableName
            {
                get
                {
                    return _sourceTableName;
                }
                set
                {
                    _sourceTableName = value;
                }
            }

            private string _targetTableName;
            public string TargetTableName
            {
                get
                {
                    return _targetTableName;
                }
                set
                {
                    _targetTableName = value;
                }
            }

            private string _sourceDatabase;
            public string SourceDatabase
            {
                get
                {
                    return _sourceDatabase;
                }
                set
                {
                    _sourceDatabase = value;
                }
            }

            private string _targetDatabase;
            public string TargetDatabase
            {
                get
                {
                    return _targetDatabase;
                }
                set
                {
                    _targetDatabase = value;
                }
            }

            private List<ColumnMappingModel> _columnMappings;
            public List<ColumnMappingModel> ColumnMappings
            {
                get
                {
                    if (this._columnMappings == null)
                    {
                        this._columnMappings = new List<ColumnMappingModel>();
                    }
                    return this._columnMappings;
                }
            }

            private string _selectCriteria;
            public string SelectCriteria
            {
                get
                {
                    return this._selectCriteria;
                }
                set
                {
                    this._selectCriteria = value;
                }
            }

            private bool _IncludeGO = true;
            public bool IncludeGO
            {
                get
                {
                    return _IncludeGO;
                }
                set
                {
                    _IncludeGO = value;
                }
            }

            public void Parse(System.Xml.XmlNode node)
            {
                this.SourceDatabase = this.GetAttributeValue(node.OwnerDocument.DocumentElement, "sourceDatabase");
                this.TargetDatabase = this.GetAttributeValue(node.OwnerDocument.DocumentElement, "targetDatabase");

                this.SourceTableName = this.GetAttributeValue(node, "sourceTableName");
                this.TargetTableName = this.GetAttributeValue(node, "targetTableName");

                foreach (System.Xml.XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case "columnMapping":
                            Models.ColumnMappingModel mapping = new Models.ColumnMappingModel();
                            mapping.Parse(childNode);
                            this.ColumnMappings.Add(mapping);
                            break;
                        case "selectCriteria":
                            this.SelectCriteria = childNode.InnerText;
                            break;
                    }
                }
            }

            public string GenerateIncludeNotExists(Models.TableModel sourceTable, Models.TableModel targetTable)
            {
                string strIncludeNotExists = string.Empty;

                var sourceKeyColumns = (
                    from i in sourceTable.Columns
                    where i.IsPrimaryKey
                    select i).ToList();
                var targetKeyColumns = (
                    from i in targetTable.Columns
                    where i.IsPrimaryKey
                    select i).ToList();

                System.Text.StringBuilder sbText = new System.Text.StringBuilder();

                foreach (var sourceKeyColumn in sourceKeyColumns)
                {
                    var mapping = (
                        from i in this.ColumnMappings
                        where i.SourceColumnName.Equals(sourceKeyColumn.ColumnName)
                        select i).FirstOrDefault();
                    if (mapping == null)
                    {
                        var targetKeyColumn = (
                            from i in targetKeyColumns
                            where i.ColumnName.Equals(sourceKeyColumn.ColumnName)
                            select i).FirstOrDefault();
                        if (targetKeyColumn != null)
                        {
                            mapping = new Models.ColumnMappingModel { SourceColumnName = sourceKeyColumn.ColumnName, TargetColumnName = targetKeyColumn.ColumnName };
                        }
                    }
                    if (mapping != null)
                    {
                        if (sbText.Length > 0)
                        {
                            sbText.Append(" AND ");
                        }
                        sbText.AppendFormat("t1.{0} = {1}.{2}", mapping.TargetColumnName, this.SourceTableName, mapping.SourceColumnName);
                    }
                }

                if (sbText.Length > 0)
                {
                    string strTargetTableName = this.TargetTableName;
                    if (!(string.IsNullOrEmpty(this.TargetDatabase)))
                    {
                        strTargetTableName = this.TargetDatabase + ".." + this.TargetTableName;
                    }
                    strIncludeNotExists = string.Format("NOT EXISTS (SELECT 1 FROM {0} t1 WHERE {1})", strTargetTableName, sbText.ToString());
                }

                return strIncludeNotExists;
            }

        }
    }

}
