using System;
using System.Collections.Concurrent;

namespace Avi_Movie_Player
{
    public delegate void StateMethod(StateEventArgs e);
    public delegate void ErrorMethod(ErrorEventArgs e);
    public class MovieThreadUtil
    {
        private static ConcurrentQueue<StateMethod> stateMethodQueue = new ConcurrentQueue<StateMethod>();
        private static ConcurrentQueue<StateEventArgs> stateArgsQueue = new ConcurrentQueue<StateEventArgs>();
        private static ConcurrentQueue<ErrorMethod> errorMethodQueue = new ConcurrentQueue<ErrorMethod>();
        private static ConcurrentQueue<ErrorEventArgs> errorArgsQueue = new ConcurrentQueue<ErrorEventArgs>();

        public static void InvokeLator(StateMethod method, StateEventArgs e) {
            stateMethodQueue.Enqueue(method);
            stateArgsQueue.Enqueue(e);
        }

        public static void InvokeLator(ErrorMethod method, ErrorEventArgs e) {
            errorMethodQueue.Enqueue(method);
            errorArgsQueue.Enqueue(e);
        }

        public static void Run()
        {
            while (true) {
                if (stateMethodQueue.IsEmpty) {
                    break;
                }
                StateMethod stateMethod;
                stateMethodQueue.TryDequeue(out stateMethod);
                StateEventArgs stateArgs;
                stateArgsQueue.TryDequeue(out stateArgs);
                stateMethod(stateArgs);
            }
            while (true) {
                if (errorMethodQueue.IsEmpty) {
                    break;
                }
                ErrorMethod errorMethod;
                errorMethodQueue.TryDequeue(out errorMethod);
                ErrorEventArgs errorArgs;
                errorArgsQueue.TryDequeue(out errorArgs);
                errorMethod(errorArgs);
            }
        }
    }
}

