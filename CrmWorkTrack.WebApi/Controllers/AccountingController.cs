using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountingController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountingController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _context.AccountingTransactions
            .Include(x => x.Customer)
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                CustomerName = x.Customer.CompanyName,
                x.Type,
                x.Category,
                x.Title,
                x.Description,
                x.Amount,
                x.VatRate,
                x.DiscountRate,
                x.DiscountAmount,
                x.VatAmount,
                x.TotalAmount,
                x.TransactionDate,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var income = await _context.AccountingTransactions
            .Where(x => x.Type == "Income")
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

        var expense = await _context.AccountingTransactions
            .Where(x => x.Type == "Expense")
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

        var transactionCount = await _context.AccountingTransactions.CountAsync();

        return Ok(new
        {
            totalIncome = income,
            totalExpense = expense,
            netBalance = income - expense,
            transactionCount
        });
    }

    [HttpGet("by-customer/{customerId:int}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var transactions = await _context.AccountingTransactions
            .Where(x => x.CustomerId == customerId)
            .Include(x => x.Customer)
            .OrderByDescending(x => x.TransactionDate)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                CustomerName = x.Customer.CompanyName,
                x.Type,
                x.Category,
                x.Title,
                x.Description,
                x.Amount,
                x.VatRate,
                x.DiscountRate,
                x.DiscountAmount,
                x.VatAmount,
                x.TotalAmount,
                x.TransactionDate,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountingTransactionRequest request)
    {
        if (request.CustomerId <= 0)
            return BadRequest("Geçerli bir müşteri seçilmelidir.");

        if (request.Type != "Income" && request.Type != "Expense")
            return BadRequest("İşlem türü Income veya Expense olmalıdır.");

        if (string.IsNullOrWhiteSpace(request.Category))
            return BadRequest("Kategori boş olamaz.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Başlık boş olamaz.");

        if (request.Amount <= 0)
            return BadRequest("Tutar sıfırdan büyük olmalıdır.");

        if (request.VatRate < 0 || request.VatRate > 100)
            return BadRequest("KDV oranı 0 ile 100 arasında olmalıdır.");

        if (request.DiscountRate < 0 || request.DiscountRate > 100)
            return BadRequest("İndirim oranı 0 ile 100 arasında olmalıdır.");

        var customerExists = await _context.Customers
            .AnyAsync(x => x.Id == request.CustomerId && !x.IsDeleted);

        if (!customerExists)
            return BadRequest("Seçilen müşteri bulunamadı.");

        var discountAmount = Math.Round(request.Amount * request.DiscountRate / 100, 2);
        var amountAfterDiscount = request.Amount - discountAmount;
        var vatAmount = Math.Round(amountAfterDiscount * request.VatRate / 100, 2);
        var totalAmount = amountAfterDiscount + vatAmount;

        var transaction = new AccountingTransaction
        {
            CustomerId = request.CustomerId,
            Type = request.Type,
            Category = request.Category.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Amount = request.Amount,
            VatRate = request.VatRate,
            DiscountRate = request.DiscountRate,
            DiscountAmount = discountAmount,
            VatAmount = vatAmount,
            TotalAmount = totalAmount,
            TransactionDate = request.TransactionDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccountingTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Muhasebe kaydı başarıyla oluşturuldu.",
            transaction.Id
        });
    }
//PUT
[HttpPut("{id:int}")]
public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountingTransactionRequest request)
{
    var transaction = await _context.AccountingTransactions.FindAsync(id);

    if (transaction == null)
        return NotFound("Muhasebe kaydı bulunamadı.");

    if (request.CustomerId <= 0)
        return BadRequest("Geçerli bir müşteri seçilmelidir.");

    if (request.Type != "Income" && request.Type != "Expense")
        return BadRequest("İşlem türü Income veya Expense olmalıdır.");

    if (string.IsNullOrWhiteSpace(request.Category))
        return BadRequest("Kategori boş olamaz.");

    if (string.IsNullOrWhiteSpace(request.Title))
        return BadRequest("Başlık boş olamaz.");

    if (request.Amount <= 0)
        return BadRequest("Tutar sıfırdan büyük olmalıdır.");

    if (request.VatRate < 0 || request.VatRate > 100)
        return BadRequest("KDV oranı 0 ile 100 arasında olmalıdır.");

    if (request.DiscountRate < 0 || request.DiscountRate > 100)
        return BadRequest("İndirim oranı 0 ile 100 arasında olmalıdır.");

    var customerExists = await _context.Customers
        .AnyAsync(x => x.Id == request.CustomerId && !x.IsDeleted);

    if (!customerExists)
        return BadRequest("Seçilen müşteri bulunamadı.");

    var discountAmount = Math.Round(request.Amount * request.DiscountRate / 100, 2);
    var amountAfterDiscount = request.Amount - discountAmount;
    var vatAmount = Math.Round(amountAfterDiscount * request.VatRate / 100, 2);
    var totalAmount = amountAfterDiscount + vatAmount;

    transaction.CustomerId = request.CustomerId;
    transaction.Type = request.Type;
    transaction.Category = request.Category.Trim();
    transaction.Title = request.Title.Trim();
    transaction.Description = request.Description?.Trim();
    transaction.Amount = request.Amount;
    transaction.VatRate = request.VatRate;
    transaction.DiscountRate = request.DiscountRate;
    transaction.DiscountAmount = discountAmount;
    transaction.VatAmount = vatAmount;
    transaction.TotalAmount = totalAmount;
    transaction.TransactionDate = request.TransactionDate ?? transaction.TransactionDate;
    transaction.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Muhasebe kaydı başarıyla güncellendi.",
        transaction.Id
    });
} 
//Delete
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _context.AccountingTransactions.FindAsync(id);

        if (transaction == null)
            return NotFound("Muhasebe kaydı bulunamadı.");

        _context.AccountingTransactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return Ok("Muhasebe kaydı silindi.");
    }

    public class CreateAccountingTransactionRequest
    {
        public int CustomerId { get; set; }

        public string Type { get; set; } = "Income";

        public string Category { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public decimal VatRate { get; set; }

        public decimal DiscountRate { get; set; }

        public DateTime? TransactionDate { get; set; }
    }
  
public class UpdateAccountingTransactionRequest
{
    public int CustomerId { get; set; }

    public string Type { get; set; } = "Income";

    public string Category { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public decimal VatRate { get; set; }

    public decimal DiscountRate { get; set; }

    public DateTime? TransactionDate { get; set; }
    }
}
