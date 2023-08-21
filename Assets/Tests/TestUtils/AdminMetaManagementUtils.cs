using System.Collections.Generic;
using LootLocker;
using LootLocker.Requests;
using LootLocker.LootLockerEnums;
using System;

namespace TestUtils
{
    public class LLMetaTestUtils
    {
        static bool initialized = false;
        private static Dictionary<string, string> CommandLineArgs = new Dictionary<string, string>();
        private static EndPointClass AdminAuth = new EndPointClass("v1/session", LootLockerHTTPMethod.POST);
        private static string GameName = System.Guid.NewGuid().ToString();

        public static bool InitMetaTestUtils()
        {
            return InitMetaTestUtils("", "", "");
        }

        public static bool InitMetaTestUtils(string testUrl, string adminUserName, string adminPassword)
        {
            if (!initialized)
            {
                if(string.IsNullOrEmpty(testUrl))
                {
                    testUrl = GetStringValueFromCommandLineArgs("testUrl");
                }
                if (string.IsNullOrEmpty(adminUserName))
                {
                    adminUserName = GetStringValueFromCommandLineArgs("adminUsername");
                }
                if (string.IsNullOrEmpty(adminPassword))
                {
                    adminPassword = GetStringValueFromCommandLineArgs("adminPassword");
                }

                LootLockerConfig config = LootLockerConfig.Get();
                if(config != null && string.IsNullOrEmpty(testUrl))
                {
                    config.OverrideURLCore(testUrl);
                }

                LootLockerAdminSessionRequest request = new LootLockerAdminSessionRequest();
                request.email = adminUserName;
                request.password = adminPassword;
                LootLockerServerRequest.CallAPI(AdminAuth.endPoint, AdminAuth.httpMethod, LootLockerJson.SerializeObject(request), onComplete: (serverResponse) =>
                {
                    Action<LootLockerAdminAuthResponse> deserializedOnComplete = (response) => {
                        LootLockerConfig.current.adminToken = response.auth_token;
                    };
                    LootLockerResponse.Deserialize(deserializedOnComplete, serverResponse);
                }, useAuthToken: true, callerRole: LootLockerCallerRole.Admin);
            }

            return initialized;
        }

        private static string GetStringValueFromCommandLineArgs(string key)
        {
            if (CommandLineArgs.Count <= 0)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    CommandLineArgs.Add(args[i].Substring(1).ToLower(), args[i+1]);
                }
            }
            return CommandLineArgs[key.ToLower()];
        }

        [Serializable]
        public class LootLockerAdminSessionRequest : LootLockerGetRequest
        {
            public string email;
            public string password;
        }
        public class LootLockerAdminAuthResponse : LootLockerResponse
        {
            public string auth_token;
        }
    }
}
