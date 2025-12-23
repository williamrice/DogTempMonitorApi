using Microsoft.EntityFrameworkCore;
using TempMonitor.Data;
using TempMonitor.Models;

namespace TempMonitor.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly ApplicationDbContext _context;

    public WeatherRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WeatherReading?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.WeatherReadings
            .Include(w => w.TemperatureReading)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WeatherReading?> GetByTemperatureReadingIdAsync(int temperatureReadingId, CancellationToken cancellationToken = default)
    {
        return await _context.WeatherReadings
            .FirstOrDefaultAsync(w => w.TemperatureReadingId == temperatureReadingId, cancellationToken);
    }

    public async Task<WeatherReading> AddAsync(WeatherReading weatherReading, CancellationToken cancellationToken = default)
    {
        await _context.WeatherReadings.AddAsync(weatherReading, cancellationToken);
        return weatherReading;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
