namespace SizManager.Models;

public class EmployeeSIZ
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Norm { get; set; } = string.Empty;

    public virtual Employee Employee { get; set; } = null!;
}
