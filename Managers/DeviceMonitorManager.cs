using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Models.responses;
using System.Runtime.CompilerServices;

namespace Managers
{
    public class DeviceMonitorManager : IDeviceMonitorManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DeviceMonitorManager(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async IAsyncEnumerable<AlertStreamResultResponse> MonitorDeviceStream(int deviceId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var activeStatusCache = new HashSet<string>();

            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var deviceManager = scope.ServiceProvider.GetRequiredService<IDeviceManager>();
                    var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

                    var deviceSerialNumber = await deviceRepository.GetDeviceSerialNumberAsync(deviceId);

                    if(string.IsNullOrEmpty(deviceSerialNumber))
                    {
                        yield break;
                    }

                    var deviceAlerts = await deviceManager.GetDeviceAlerts(deviceSerialNumber,deviceId);

                    var currentSnapshot = new HashSet<string>();

                    foreach(var alert in deviceAlerts)
                    {
                        currentSnapshot.Add(alert.SensorType);

                        if (activeStatusCache.Contains(alert.SensorType))
                        {
                            continue;
                        }

                        activeStatusCache.Add(alert.SensorType);

                        yield return new AlertStreamResultResponse
                        {
                            Status = "Alert",
                            Message = alert.AlertMessage,
                            SensorType = alert.SensorType
                        };
                    }

                    var resolvedAlerts = activeStatusCache.Where(x => !currentSnapshot.Contains(x)).ToList();

                    foreach (var resolvedSensor in resolvedAlerts)
                    {
                        activeStatusCache.Remove(resolvedSensor);
                        yield return new AlertStreamResultResponse
                        {
                            Status = "Good",
                            Message = $"Alert for sensor {resolvedSensor} has been resolved.",
                            SensorType = resolvedSensor
                        };
                    }
                }
                try
                {
                    await Task.Delay(60000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }     
        }
    }
}
