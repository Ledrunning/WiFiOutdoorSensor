using System;
using System.Windows.Forms;
using MeteoraDesktop.Presenter;
using MeteoraDesktop.View;

namespace MeteoraDesktop
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainForm = new MainForm();
            var mainPresenter = new MainFormPresenter(mainForm);

            Application.Run(mainForm);
        }
    }
}
