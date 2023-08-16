using System.Collections;
using LootLocker;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LootLocker.Requests;

namespace Tests
{
    public class GuestLoginTest
    {
        private static LootLockerConfig.DebugLevel debugLevel;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            LLTestUtils.InitSDK();
            debugLevel = LootLockerConfig.current.currentDebugLevel;
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

        [UnityTest]
        public IEnumerator GuestUserCanLogIn()
        {
            LLTestUtils.InitSDK();

            string PlayerIdentifier = System.Guid.NewGuid().ToString();
            string ActualPlayerIdentifier = "";
            bool guestLogin = false;

            // When
            LootLockerSDKManager.StartGuestSession(PlayerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }
                ActualPlayerIdentifier = response.player_identifier;
                guestLogin = true;
            });

            // Wait for response
            yield return new WaitUntil(() => guestLogin);
            Assert.AreEqual(ActualPlayerIdentifier, PlayerIdentifier, "The expected player identifier was not set on the user");
        }
    }
}