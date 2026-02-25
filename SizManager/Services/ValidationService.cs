using SizManager.Models;

namespace SizManager.Services;

public class ValidationService
{
    public List<string> ValidateEmployee(Employee employee)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(employee.ProfessionName))
            errors.Add("Профессия (должность) обязательна для заполнения");

        return errors;
    }
}
