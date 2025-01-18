using Ical.Net;

namespace ICalAnonymizer;

internal class CalendarService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigService _configService;

    public CalendarService(HttpClient httpClient, ConfigService configService)
    {
        _httpClient = httpClient;
        _configService = configService;
    }

    public async Task<Calendar> GetCalendar(string key, CancellationToken cancellationToken = default)
    {
        var resultCalendar = new Calendar();
        
        var calendars = await _configService.GetCalendars(key, cancellationToken);
        if (calendars == null) return resultCalendar;
        
        foreach (var calendar in calendars)
        {
            var file = await _httpClient.GetStringAsync(calendar.Value, cancellationToken);
            var value = Calendar.Load(file);
            foreach (var calendarEvent in value.Events)
            {
                calendarEvent.Summary = string.Empty;
                calendarEvent.Description = string.Empty;
            }
            
            resultCalendar.MergeWith(value);
        }

        return resultCalendar;
    }
}