using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MeteoraDesktop.Events;

namespace MeteoraDesktop.Service
{
    public interface ITelemetryService
    {
        event TelemetryReceiveEventHandler TelemetryEvent;
        Task ReadDataAsync(CancellationToken token);
    }
}
