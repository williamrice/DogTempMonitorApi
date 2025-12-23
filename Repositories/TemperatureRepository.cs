using Microsoft.EntityFrameworkCore;
using TempMonitor.Data;
using TempMonitor.Models;

namespace TempMonitor.Repositories;

public class TemperatureRepository : ITemperatureRepository
{
    private readonly ApplicationDbContext _context;

    public TemperatureRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TemperatureReading?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TemperatureReadings
            .Include(t => t.WeatherReading)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TemperatureReading> AddAsync(TemperatureReading temperatureReading, CancellationToken cancellationToken = default)
    {
        await _context.TemperatureReadings.AddAsync(temperatureReading, cancellationToken);
        return temperatureReading;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
