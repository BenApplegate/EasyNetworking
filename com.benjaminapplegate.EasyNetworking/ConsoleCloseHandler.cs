using System;
using System.Runtime.InteropServices;

namespace PrivateMessagingClient
{
    public class ConsoleCloseHandler
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        
        public delegate void OnCloseConsole();

        public static OnCloseConsole closeMethod = null;

        public static void RegisterHandler()
        {
            SetConsoleCtrlHandler(Handler, true);
        }
        
        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        
        private static bool Handler(CtrlType signal)
        {
            switch (signal)
            {
                default:
                    closeMethod?.Invoke();
                    return true;
            }
        }
        
    }
}