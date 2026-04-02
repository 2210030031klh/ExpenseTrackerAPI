namespace ShashiControllerAPI.DTOs;

public class GetIncomeDto
{
    public Guid IncomeId { get; set; }
    public Guid UserId { get; set; }
    public string? Name{get; set;}
    public int Amount { get; set; }
    public string? Description { get; set; }
    public string Source { get; set; } = string.Empty;  // e.g. Salary, Freelance
    public DateOnly Date { get; set; }
    public DateOnly CreatedAt { get; set; }
}