namespace CrmWorkTrack.Domain.Entities;

public class AccountingTransaction
{
	public int Id { get; set; }

	public int CustomerId { get; set; }
	public Customer Customer { get; set; } = null!;

	public string Type { get; set; } = "Income";
	// Income | Expense

	public string Category { get; set; } = string.Empty;

	public string Title { get; set; } = string.Empty;

	public string? Description { get; set; }

	public decimal Amount { get; set; }

	public decimal VatRate { get; set; }

	public decimal DiscountRate { get; set; }

	public decimal DiscountAmount { get; set; }

	public decimal VatAmount { get; set; }

	public decimal TotalAmount { get; set; }

	public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public DateTime? UpdatedAt { get; set; }
}