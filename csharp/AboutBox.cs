using System;
using System.Reflection;
using System.Windows.Forms;

namespace Mandelbrot
{
    sealed partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            Text = $"About {AssemblyTitle}";
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = $"Version {AssemblyVersion}";
            labelCopyright.Text = AssemblyCopyright;
            textBoxDescription.Text = Description;
        }

        private static string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        private static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static string AssemblyProduct
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attributes.Length == 0 ? "" : ((AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        private string AssemblyCopyright
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        private string Description => GetCpuIdDescription() + "\r\n" + GetAleaDescription();

        private string GetCpuIdDescription()
        {
            return "";                
                //_cpuid.Brand + "\r\n" + 
                //"AVX: " + (_cpuid.AVX ? "Enabled" : "Disabled") + "\r\n";
        }

        private string GetAleaDescription()
        {
            var cout = Console.Out;
            var writer = new System.IO.StringWriter();

            Console.SetOut(writer);

            try
            {
                //Alea.Device.Default.Print();
                return writer.ToString();
            }

            catch (Exception exception)
            {
                return "CUDA: " + exception.Message;
            }

            finally
            {
                Console.SetOut(cout);
            }
        }
    }
}
