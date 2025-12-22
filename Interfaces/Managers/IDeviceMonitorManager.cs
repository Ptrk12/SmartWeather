using Models.responses;
using System.Runtime.CompilerServices;

namespace Interfaces.Managers
{
    public interface IDeviceMonitorManager
    {
        IAsyncEnumerable<AlertStreamResultResponse> MonitorDeviceStream(int deviceId, [EnumeratorCancellation] CancellationToken cancellationToken);
    }
}
