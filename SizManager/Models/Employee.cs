namespace SizManager.Models;

public class Employee
{
    public int Id { get; set; }

    // Номер карточки
    public string? CardNumber { get; set; }

    // ФИО
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Gender { get; set; }

    // Служебные данные
    public string? PersonnelNumber { get; set; }
    public string? Department { get; set; }
    public int? ProfessionId { get; set; }
    public string ProfessionName { get; set; } = string.Empty;
    public DateTime? HireDate { get; set; }
    public DateTime? ChangeDate { get; set; }

    // Размеры
    public int? Height { get; set; }
    public string? ClothingSize { get; set; }
    public string? ShoeSize { get; set; }
    public string? HeadwearSize { get; set; }
    public string? RespiratorsSize { get; set; }
    public string? GlovesSize { get; set; }

    // Метаданные
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    public virtual Profession? Profession { get; set; }
    public virtual ICollection<EmployeeSIZ> SizList { get; set; } = new List<EmployeeSIZ>();

    public string FullName =>
        string.Join(" ", new[] { LastName, FirstName, MiddleName }.Where(s => !string.IsNullOrWhiteSpace(s)));
}
