using System;
using System.Windows.Forms;

namespace Mandelbrot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (s, e) => HandleException(e.Exception, "unhandled thread exception");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => HandleException(e.ExceptionObject, "unhandled domain exception");

            Application.SetHighDpiMode(HighDpiMode.SystemAware); 
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                //throw new Exception("testing");
                Application.Run(new MainForm());                
            }

            catch (Exception exception)
            {
                HandleException(exception, "unhandled exception");
            }
        }

        private static void HandleException(object exception, string caption)
        {
            if (exception is Exception e)
            {
                MessageBox.Show(e.ToString(), $"ERROR: {caption}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(exception.ToString(), $"ERROR: {caption} of unknown type", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

            Environment.Exit(1);
        }
    }
}
