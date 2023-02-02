using System.Diagnostics;
using System.Threading;
using LootLocker.Requests;

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

        public static bool initialized = false;
        public static bool InitSDK()
        {
            string apiKey = null;
            string domainKey = null;
            if (!initialized)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-apikey")
                    {
                        apiKey = args[i + 1];
                    }
                    else if (args[i] == "-domainkey")
                    {
                        domainKey = args[i + 1];
                    }
                }

                if ((string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(domainKey)))
                {
                    if (LootLockerSDKManager.CheckInitialized(true))
                    {
                        initialized = true;
                    }
                }
                else
                {
                    LootLockerSDKManager.Init(apiKey, "0.0.0.1", true, domainKey);
                    LootLocker.LootLockerConfig.current.currentDebugLevel = LootLocker.LootLockerConfig.DebugLevel.All;
                    initialized = true;
                }
            }

            return initialized;
        }
    }
}