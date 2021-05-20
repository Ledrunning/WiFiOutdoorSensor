using System;
using System.Reflection.Emit;
using System.Threading;
using MeteoraDesktop.Events;
using MeteoraDesktop.Service;
using MeteoraDesktop.View;

namespace MeteoraDesktop.Presenter
{
    public class MainFormPresenter
    {
        private const string BaseAddress = "http://192.168.0.101/";
        private readonly IMainForm view;
        private readonly ITelemetryService telemetryService;
        private CancellationTokenSource tokenSource;

        public MainFormPresenter(IMainForm view)
        {
            this.view = view;
            view.Presenter = this;
            telemetryService = new TelemetryService(BaseAddress);
            telemetryService.TelemetryEvent += OnTelemetryReceived;
            view.OnLoadForm += ViewOnLoadForm;
        }

        private void ViewOnLoadForm(object sender, EventArgs e)
        {
            StartReadData();
        }

        private void StartReadData()
        {
            tokenSource = new CancellationTokenSource();
            telemetryService.ReadDataAsync(tokenSource.Token);
        }

        private void StopReadData()
        {
            tokenSource?.Cancel();
        }

        private void OnTelemetryReceived(TelemetryEventArgs eventArgs)
        {
            view.Altitude = eventArgs.TelemetryDto.Altitude;
            view.BatteryLevel = eventArgs.TelemetryDto.BatteryLevel;
            view.Humidity = eventArgs.TelemetryDto.Humidity;
            view.Pressure = eventArgs.TelemetryDto.Pressure;
            view.Temperature = eventArgs.TelemetryDto.Temperature;
        }
    }
}