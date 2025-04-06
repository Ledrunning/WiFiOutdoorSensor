using System;
using System.Threading;
using MeteoraDesktop.Events;
using MeteoraDesktop.Service;
using MeteoraDesktop.View;

namespace MeteoraDesktop.Presenter
{
    public class MainFormPresenter
    {
        private const string BaseAddress = "http://192.168.1.111/";
        private readonly ITelemetryService _telemetryService;
        private readonly IMainForm _view;
        private CancellationTokenSource _tokenSource;

        public MainFormPresenter(IMainForm view)
        {
            this._view = view;
            view.Presenter = this;

            _telemetryService = new TelemetryService(BaseAddress);
            _telemetryService.TelemetryEvent += OnTelemetryReceived;
            view.OnLoadForm += ViewOnLoadForm;
        }

        private void ViewOnLoadForm(object sender, EventArgs e)
        {
            StartReadData();
        }

        private void StartReadData()
        {
            _tokenSource = new CancellationTokenSource();
            _telemetryService.ReadDataAsync(_tokenSource.Token);
        }

        private void StopReadData()
        {
            _tokenSource?.Cancel();
        }

        private void OnTelemetryReceived(TelemetryEventArgs eventArgs)
        {
            _view.Altitude = eventArgs.TelemetryDto.Altitude;
            _view.BatteryLevel = eventArgs.TelemetryDto.BatteryLevel;
            _view.Humidity = eventArgs.TelemetryDto.Humidity;
            _view.Pressure = eventArgs.TelemetryDto.Pressure;
            _view.Temperature = eventArgs.TelemetryDto.Temperature;
        }
    }
}