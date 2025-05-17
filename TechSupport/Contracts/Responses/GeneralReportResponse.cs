using TechSupport.Database.Entities;

namespace TechSupport.Contracts.Responses;

public record NameValue<T, TK>(T Name, TK Value);

public class GeneralReportResponse
{
    public required int TicketsCount { get; set; }
    public required int CompletedTickets { get; set; }
    public required int CancelledTickets { get; set; }
    public required int InProgressTickets { get; set; }
    public required double AvgResponseTime { get; set; }
    public required double AvgResolutionTime { get; set; }

    public required IEnumerable<NameValue<DateOnly, int>> NewTicketsChartData { get; set; }
    public required IEnumerable<NameValue<TicketStatus, int>> TicketsByStatus { get; set; }  
    public required IEnumerable<NameValue<string, int>> TicketsByIssueType { get; set; }  
}