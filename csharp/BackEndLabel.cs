
namespace Mandelbrot
{
    internal static class BackEndLabel
    {
        public static string Get(BackEnd backEnd)
        {
            switch (backEnd)
            {
                case BackEnd.Managed:
                    return "C#";

                case BackEnd.Sse2:
                    return "SSE2";

                case BackEnd.Avx:
                    return "AVX";

                case BackEnd.Cuda:
                    return "CUDA";

                case BackEnd.OpenCl:
                    return "OpenCL";

                default:
                    return "Unknown";
            }
        }
    }
}
