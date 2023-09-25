using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeteoraDesktop.Presenter;

namespace MeteoraDesktop.View
{
    public interface IMainForm
    {
        MainFormPresenter Presenter { set; }

        string Temperature { get; set; }

        string Humidity { get; set; }

        string Altitude { get; set; }

        string Pressure { get; set; }

        string BatteryLevel { get; set; }

        Task<List<string>> GetData();

        event EventHandler OnLoadForm;
    }
}