using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus.Design.Application.Helpers
{
    internal static class ExtentionHelpers
    {
        /// <summary>
        /// Enqueues element to the queue, regardless of the order (used when the order is not important)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="values"></param>
        public static void Enqueue<T>(this Queue<T> q, IEnumerable<T> values)
        {
            foreach (var value in values)
                q.Enqueue(value);
        }

        /// <summary>
        /// Pushes element to the stack, regardless of the order (used when the order is not important)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="values"></param>
        public static void Push<T>(this Stack<T> s, IEnumerable<T> values)
        {
            foreach (var value in values)
                s.Push(value);
        }
    }
}
