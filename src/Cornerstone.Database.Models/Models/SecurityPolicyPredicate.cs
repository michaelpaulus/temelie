namespace Temelie.Database.Models;

public class SecurityPolicyPredicate
{
    public string PredicateType { get; set; }
    public string PredicateDefinition { get; set; }
    public string TargetSchema { get; set; }
    public string TargetName { get; set; }
    public string Operation { get; set; }
}
