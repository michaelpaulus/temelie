namespace Temelie.Database.Models.ChangeTracking;

public partial record TrackedTable
{

    public virtual string SchemaName { get; set; } = "";

    public virtual string TableName { get; set; } = "";

    public virtual string ColumnsJSON { get; set; }

    public virtual string PkColumnsJSON { get; set; }

    public virtual long? CurrentVersionId { get; set; }

}
