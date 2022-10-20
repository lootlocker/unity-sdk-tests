using System.Diagnostics;
using System.Threading;

namespace Tests
{
    public class LLTestUtils
    {
        public static void WaitForDebugger()
        {
            while (!Debugger.IsAttached)
            {

            }

            Thread.Sleep(2000); // Give the debugger a chance to properly attach
        }
    }
}