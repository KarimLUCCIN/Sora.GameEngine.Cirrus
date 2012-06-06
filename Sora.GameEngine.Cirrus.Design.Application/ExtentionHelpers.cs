using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    internal static class ExtentionHelpers
    {
        public static void Enqueue<T>(this Queue<T> q, IEnumerable<T> values)
        {
            foreach (var value in values)
                q.Enqueue(value);
        }
    }
}
