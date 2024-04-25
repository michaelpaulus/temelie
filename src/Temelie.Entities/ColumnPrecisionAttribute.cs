namespace Temelie.Entities;
public class ColumnPrecisionAttribute : Attribute
{
    private readonly int _precision;
    private readonly int _scale;

    public ColumnPrecisionAttribute(int precision, int scale)
    {
        _precision = precision;
        _scale = scale;
    }

}
