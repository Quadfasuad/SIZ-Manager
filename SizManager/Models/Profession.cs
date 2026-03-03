namespace SizManager.Models;

public class Profession
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<ProfessionSIZ> SizList { get; set; } = new List<ProfessionSIZ>();

    // Для числовой сортировки в DataGrid
    public int NumberAsInt => int.TryParse(Number, out var n) ? n : 0;

    public string DisplayName =>
        Name + (string.IsNullOrEmpty(Number) ? "" : $" (№{Number})");
}
