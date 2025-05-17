using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechSupport.Contracts.Responses;
using TechSupport.Database;
using TechSupport.Database.Entities;

namespace TechSupport.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("general"),
     Authorize(Roles = RolesEnum.Support, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetGeneralReport(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var ticketsQuery = _context.Tickets
            .Include(x => x.Chat)
            .ThenInclude(x => x.Messages).Include(ticket => ticket.IssueType)
            .AsNoTracking()
            .AsQueryable();

        if (startDate.HasValue && endDate.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t =>
                DateOnly.FromDateTime(t.CreatedAt.DateTime) >= DateOnly.FromDateTime(
                    startDate.Value.DateTime)
                && DateOnly.FromDateTime(t.CreatedAt.DateTime) <= DateOnly.FromDateTime(
                    endDate.Value.DateTime));
        }

        var tickets = await ticketsQuery.ToListAsync();

        var completedTickets = tickets.Count(x => x.Status == TicketStatus.Completed);
        var cancelledTickets = tickets.Count(x => x.Status == TicketStatus.Cancelled);
        var inProgressTickets = tickets.Count(x => x.Status == TicketStatus.InProgress);

        double avgResponseTime = 0;
        double avgResolutionTime = 0;

        try
        {
            var ticketsWithMessages = tickets.Where(x => x.Status != TicketStatus.Registered &&
                                                         x.Chat.Messages.Count > 0).Select(t =>
                t.Chat.Messages.First().Timestamp - t.CreatedAt).ToList();
            avgResponseTime = ticketsWithMessages.Average(x => x.TotalMilliseconds);
        }
        catch
        {
            // ignored
        }

        try
        {
            var closedTickets = tickets.Where(t => t.IsClosed).Select(t => t.ClosedAt - t.CreatedAt).ToList();
            avgResolutionTime = closedTickets.Average(x => x?.TotalMilliseconds ?? 0);
        }
        catch
        {
            // ignored
        }

        var newTicketsDict = new Dictionary<DateOnly, int>();
        foreach (var ticket in tickets)
        {
            var dateOnly = DateOnly.FromDateTime(ticket.CreatedAt.DateTime);
            if (!newTicketsDict.TryAdd(dateOnly, 1))
            {
                newTicketsDict[dateOnly]++;
            }
        }

        var newTicketsChartData = newTicketsDict.Select(x => new NameValue<DateOnly, int>(x.Key, x.Value)).ToList();
        
        if (startDate.HasValue && endDate.HasValue)
        {
            newTicketsChartData = FillMissingDates(newTicketsChartData, startDate.Value, endDate.Value);
        }
        else if (tickets.Count > 1)
        {
            var firstTicketCreatedAt = tickets.First().CreatedAt;
            var lastTicketCreatedAt = tickets.Last().CreatedAt;

            newTicketsChartData = FillMissingDates(newTicketsChartData, firstTicketCreatedAt, lastTicketCreatedAt);
        }

        var ticketsByStatus = new Dictionary<TicketStatus, int>();
        foreach (var ticket in tickets.Where(ticket => !ticketsByStatus.TryAdd(ticket.Status, 1)))
        {
            ticketsByStatus[ticket.Status]++;
        }

        var mappedTicketsByStatus = MapTicketsByStatus(ticketsByStatus);

        var ticketsByIssueType = new Dictionary<string, int>();
        foreach (var ticket in tickets.Where(ticket => !ticketsByIssueType.TryAdd(ticket.IssueType.Name, 1)))
        {
            ticketsByIssueType[ticket.IssueType.Name]++;
        }

        var mappedTicketsByIssueType = ticketsByIssueType
            .Select(x => new NameValue<string, int>(x.Key, x.Value))
            .OrderByDescending(x => x.Value);

        return Ok(new GeneralReportResponse
        {
            TicketsCount = tickets.Count,
            CompletedTickets = completedTickets,
            CancelledTickets = cancelledTickets,
            AvgResponseTime = avgResponseTime,
            AvgResolutionTime = avgResolutionTime,
            InProgressTickets = inProgressTickets,
            NewTicketsChartData = newTicketsChartData,
            TicketsByStatus = mappedTicketsByStatus,
            TicketsByIssueType = mappedTicketsByIssueType
        });
    }

    private static IOrderedEnumerable<NameValue<TicketStatus, int>> MapTicketsByStatus(
        Dictionary<TicketStatus, int> ticketsByStatus)
    {
        var allStatuses = Enum.GetValues(typeof(TicketStatus)).Cast<TicketStatus>();
        var mappedTicketsByStatus = ticketsByStatus
            .Select(x => new NameValue<TicketStatus, int>(x.Key, x.Value))
            .Concat(allStatuses.Except(ticketsByStatus.Keys)
                .Select(s => new NameValue<TicketStatus, int>(s, 0)))
            .OrderBy(c => c.Name);
        return mappedTicketsByStatus;
    }
    
    private static List<NameValue<DateOnly, int>> FillMissingDates(IList<NameValue<DateOnly, int>> existingData,
        DateTimeOffset startDate, DateTimeOffset endDate)
    {
        var startDateOnly = DateOnly.FromDateTime(startDate.Date);
        var endDateOnly = DateOnly.FromDateTime(endDate.Date);

        var existingDates = new HashSet<DateOnly>(existingData.Select(x => x.Name));

        var allDatesInRange = new List<DateOnly>();
        for (var date = startDateOnly; date <= endDateOnly; date = date.AddDays(1))
        {
            allDatesInRange.Add(date);
        }

        var result = allDatesInRange
            .Select(date => new NameValue<DateOnly, int>(date, existingDates.Contains(date)
                ? existingData.First(x => x.Name == date).Value
                : 0))
            .OrderBy(x => x.Name)
            .ToList();

        return result;
    }
}