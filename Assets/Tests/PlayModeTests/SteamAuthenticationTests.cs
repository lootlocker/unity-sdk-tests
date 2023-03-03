using System.Collections;
using LootLocker;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LootLocker.Requests;
using Steamworks;
using System;
using System.Text;

namespace Tests
{
    public class SteamAuthenticationTests
    {
        private static LootLockerConfig.DebugLevel debugLevel;
        private static GameObject SteamManagerGO;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("One Time Setup");
            LLTestUtils.InitSDK();
            debugLevel = LootLockerConfig.current.currentDebugLevel;

            // Setup Steam Manager
            SteamManagerGO = new GameObject("SteamManager");
            SteamManagerGO.AddComponent<SteamManager>();
            if (!SteamManager.Initialized)
            {
                Debug.LogError("SteamManager Failed to initialize");
            }
            //SignInWithSteam();
        }

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            //yield return new WaitUntil(() => m_IsSteamInitialized);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            // Cleanup
            LootLockerConfig.current.currentDebugLevel = debugLevel;
            bool cleanupComplete = false;
            LootLockerSDKManager.EndSession((response) =>
            {
                cleanupComplete = true;
            });
            yield return new WaitUntil(() => cleanupComplete);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [UnityTest]
        [Ignore("No steam account set up without Steam Guard, so CI env can't run the test as a signed in client is needed")]
        public IEnumerator SteamLoginSucceeds()
        {
            // Given
            bool requestCompleted = false;
            LootLockerSessionResponse sessionResponse = null;

            // When
            var ticket = new byte[1024];
            SteamUser.GetAuthSessionTicket(ticket, 1024, out uint ticketSize);
            Array.Resize(ref ticket, (int)ticketSize);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ticketSize; i++)
            {
                sb.AppendFormat("{0:x2}", ticket[i]);
            }

            var steamSessionTicket = sb.ToString();

            LootLockerSDKManager.VerifySteamID(steamSessionTicket, verifySteamIdResponse =>
            {
                if (verifySteamIdResponse.success)
                {
                    LootLockerSDKManager.StartSteamSession(SteamUser.GetSteamID().ToString(),
                        startSteamSessionResponse =>
                        {
                            sessionResponse = startSteamSessionResponse;
                            requestCompleted = true;
                        });
                }
                else
                {
                    requestCompleted = true;
                    Assert.Fail("Failed to verify steam id");
                }
            });

            // Wait for response
            yield return new WaitUntil(() => requestCompleted);

            // Then
            Assert.IsNotNull(sessionResponse);
            Assert.IsTrue(sessionResponse.success, "Failed to start steam session");
            Assert.AreNotEqual(0, sessionResponse.player_id, "Player ID not set");
        }
    }
}