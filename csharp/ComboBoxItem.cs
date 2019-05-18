
namespace Mandelbrot
{
    internal sealed class ComboBoxItem
    {
        public ComboBoxItem(string text, object value)
        {
            Text = text;
            Value = value;
        }

        public readonly string Text;
        public readonly object Value;

        public override string ToString()
        {
            return Text;
        }
    }
}
