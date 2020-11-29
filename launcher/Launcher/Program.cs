using System.Diagnostics;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Process pythonControllerProcess = new Process();
            pythonControllerProcess.StartInfo.FileName = "pythonw";

            // Here paste the absolute path to your PythonScripts/controller.pyw file.
            // In the final release, the Launcher.exe file will be placed in a folder
            // whose relative position to PythonScripts/controller.pyw will be always the same.
            pythonControllerProcess.StartInfo.EnvironmentVariables.Add("PYTHONPATH", @"D:\Dane\MichalKuzemczak\Projects\Inzynierka_main\repo\python_controller\src");
            pythonControllerProcess.StartInfo.Arguments = @"-m python_controller";
            pythonControllerProcess.StartInfo.UseShellExecute = false;
            pythonControllerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            pythonControllerProcess.Start();
        }
    }
}
