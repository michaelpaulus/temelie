namespace Temelie.Database.Models;

public class ColumnMappingModel : Mapping
{

    private string _sourceColumnName;
    public string SourceColumnName
    {
        get
        {
            return _sourceColumnName;
        }
        set
        {
            _sourceColumnName = value;
        }
    }

    private string _targetColumnName;
    public string TargetColumnName
    {
        get
        {
            return _targetColumnName;
        }
        set
        {
            _targetColumnName = value;
        }
    }

    public bool WrapInIsNull { get; set; }

    private string _columnMapping;
    public string ColumnMapping
    {
        get
        {
            return _columnMapping ?? "";
        }
        set
        {
            if (value == null)
            {
                value = "";
            }
            if (value.Equals("None"))
            {
                value = string.Empty;
            }
            this._columnMapping = value;
        }
    }

    public void Parse(System.Xml.XmlNode xmlNode)
    {
        this.SourceColumnName = this.GetAttributeValue(xmlNode, "sourceColumnName");
        this.TargetColumnName = this.GetAttributeValue(xmlNode, "targetColumnName");
        this.ColumnMapping = this.GetAttributeValue(xmlNode, "columnMapping");
    }

    public bool IsTargetColumnIdentity { get; set; }

    public string TargetColumnNameWithMapping
    {
        get
        {
            string strColumnName = this.TargetColumnName;

            if (strColumnName.Contains(" ") || strColumnName.Contains("-"))
            {
                strColumnName = "[" + strColumnName + "]";
            }

            return strColumnName;
        }
    }

    public string SourceColumnNameWithMapping
    {
        get
        {
            string strColumnMapping = this.ColumnMapping;

            if (string.IsNullOrEmpty(this.ColumnMapping))
            {
                strColumnMapping = "{0}";
            }

            string strColumnName = this.SourceColumnName;

            if (strColumnName.Contains(" ") || strColumnName.Contains("-"))
            {
                strColumnName = "[" + strColumnName + "]";
            }

            var value = string.Format(strColumnMapping, strColumnName);

            if (WrapInIsNull)
            {
                value = $"ISNULL({value}, '')";
            }

            return value;
        }
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(this.ColumnMapping))
        {
            return string.Format("{0} --> {1}", this.SourceColumnName, this.TargetColumnName);
        }
        else
        {
            return string.Format("{0} --> {1} ({2})", this.SourceColumnName, this.TargetColumnName, this.ColumnMapping);
        }
    }

}
