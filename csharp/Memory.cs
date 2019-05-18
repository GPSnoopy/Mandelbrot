using System;

namespace Mandelbrot
{
    internal static class Memory
    {
        public static unsafe void Copy(void* dst, void* src, long count)
        {
            if (dst == null) throw new ArgumentException("dst cannot be null");
            if (src == null) throw new ArgumentException("src cannot be null");
            if (count < 0) throw new ArgumentException("count cannot be negative");

            var d = (byte*)dst;
            var s = (byte*)src;
            var aligned = (count / 16) * 16;
            var i = 0;

            for (; i < aligned; i += 16)
            {
                *(long*)(d + i) = *(long*)(s + i);
                *(long*)(d + i + 8) = *(long*)(s + i + 8);
            }

            for (; i < count; i++)
            {
                d[i] = s[i];
            }
        }
    }
}
