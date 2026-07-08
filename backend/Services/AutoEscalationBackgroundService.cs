using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ComplaintManagementSystem.Interfaces;

namespace ComplaintManagementSystem.Services;

public class AutoEscalationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoEscalationBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(1); // Run every hour

    public AutoEscalationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AutoEscalationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auto Escalation Background Service is starting.");

        // Wait a small delay after startup to let the app finish initializing
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Running automatic SLA breach check & escalation...");
                using (var scope = _serviceProvider.CreateScope())
                {
                    var escalationService = scope.ServiceProvider.GetRequiredService<IEscalationService>();
                    await escalationService.AutoEscalateComplaintsAsync();
                }
                _logger.LogInformation("Automatic SLA breach check completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during automatic SLA breach check.");
            }

            try
            {
                await Task.Delay(_period, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Auto Escalation Background Service is stopping.");
    }
}
