using System;
using System.Windows.Forms;
using FakeHostLocalLab.UI;

namespace FakeHostLocalLab;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
            MessageBox.Show(e.Exception.ToString(), "LIHT - Unhandled Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            MessageBox.Show(e.ExceptionObject?.ToString(), "LIHT - Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        ApplicationConfiguration.Initialize();

        try
        {
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "LIHT - Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
