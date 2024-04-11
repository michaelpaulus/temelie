namespace Cornerstone.Database.Models;

public class SecurityPolicyModel : DatabaseObjectModel
{
    public string PolicySchema { get; set; }
    public string PolicyName { get; set; }
    public IList<SecurityPolicyPredicate> Predicates { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsSchemaBound { get; set; }

}
