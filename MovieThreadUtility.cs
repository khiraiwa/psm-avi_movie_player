using System;
using System.Collections.Concurrent;

namespace Avi_Movie_Player
{
    public delegate void MovieMethod();
    public class MovieThreadUtility
    {
        private static ConcurrentQueue<MovieMethod> methodQueue = new ConcurrentQueue<MovieMethod>();

        public static void InvokeLator(MovieMethod method) {
            methodQueue.Enqueue(method);
        }

        public static void Run()
        {
            while (true) {
                if (methodQueue.IsEmpty) {
                    break;
                }
                MovieMethod method;
                methodQueue.TryDequeue(out method);
                method();
            }
        }
    }
}

