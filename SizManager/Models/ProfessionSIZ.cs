namespace SizManager.Models;

public class ProfessionSIZ
{
    public int Id { get; set; }
    public int ProfessionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Norm { get; set; } = string.Empty;

    public virtual Profession Profession { get; set; } = null!;
}
