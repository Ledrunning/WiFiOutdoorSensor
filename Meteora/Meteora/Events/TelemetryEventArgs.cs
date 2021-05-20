using System;
using MeteoraDesktop.Model;

namespace MeteoraDesktop.Events
{
    public delegate void TelemetryReceiveEventHandler(TelemetryEventArgs eventArgs);

    public class TelemetryEventArgs : EventArgs
    {
        public TelemetryEventArgs(TelemetryDto telemetryDto)
        {
            TelemetryDto = telemetryDto;
        }

        public TelemetryDto TelemetryDto { get; set; }
    }
}