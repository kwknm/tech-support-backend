namespace TechSupport.Contracts.Responses;

public class PersonalReportResponse
{
    public required int TicketsCount { get; set; }
    public required int CompletedTickets { get; set; }
    public required int CancelledTickets { get; set; }
    public required int InProgressTickets { get; set; }
    public required double AvgResponseTimePersonal { get; set; }
    public required double AvgResolutionTimePersonal { get; set; }
    public required double AvgResponseTimeGeneral { get; set; }
    public required double AvgResolutionTimeGeneral { get; set; }
}