namespace CrmWorkTrack.Api.DTOs;

public class ReturnRequestCreateDto
{
    public int CustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
}