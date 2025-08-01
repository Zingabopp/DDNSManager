
namespace DDNSManager.Lib;
public class ProblemDetails
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public string? Type { get; set; }

    public override string ToString()
    {
        if (Title is not null)
        {
            if (Detail is not null)
            {
                return $"{Title}: {Detail}";
            }
            else
            {
                return Title;
            }
        }
        return Detail ?? string.Empty;
    }
}
