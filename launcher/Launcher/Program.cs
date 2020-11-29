using System.Diagnostics;
using System.Threading;

namespace Launcher
{
    class Program
    {
        private static string IncomingQueueName { get; } = "inzynierka_launcher";
        private static string AppQueueName { get; } = "inzynierka_app";
        private static string PythonQueueName { get; } = "inzynierka_python";
        private static RabbitMQCommunicationService Communicator { get; set; }
        private static AutoResetEvent AppClose = new AutoResetEvent(false);

        static void InitializeCommunicator()
        {
            Communicator = RabbitMQCommunicationService.Instance;
            Communicator.Initialize();
            Communicator.DeclareIncomingQueue(IncomingQueueName);
            Communicator.MessageReceived += MessageReceiver;
        }

        static void Main(string[] args)
        {
            InitializeCommunicator();

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

            AppClose.WaitOne();
        }

        private static void MessageReceiver(object sender, MessageReceivedEventArgs e)
        {
            if (e.QueueName != IncomingQueueName)
                return;

            if (e.Message == "{\"name\": \"SetupFinishedIndication\", \"sender\": \"inzynierka_python\", \"receiver\": \"inzynierka_launcher\", \"contents\": []}")
            {
                Process appProcess = new Process();
                appProcess.StartInfo.FileName = "Inzynierka.exe";
                appProcess.StartInfo.Arguments = "";
                appProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

                // Comment this line, if you want to launch piceon yourself
                // from Visual studio
                appProcess.Start();

                AppClose.Set();
            }

            return;
        }
    }
}
