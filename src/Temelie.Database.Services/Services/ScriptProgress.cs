namespace Temelie.Database.Services;

public class ScriptProgress
{
    public int ProgressPercentage { get; set; }
    public string ProgressStatus { get; set; }
    public string ErrorMessage { get; set; }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(ErrorMessage))
        {
            return $"{ProgressPercentage}% {ProgressStatus}";
        }
        else
        {
            return $"{ProgressPercentage}% {ProgressStatus} {ErrorMessage}";
        }
    }

}
