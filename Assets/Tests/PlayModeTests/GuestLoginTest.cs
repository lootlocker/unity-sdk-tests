using System.Collections;
using LootLocker;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LootLocker.Requests;
using UnityEditor;

namespace Tests
{
    public class GuestLoginTest
    {
        private static LootLockerConfig.DebugLevel debugLevel;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LLTestUtils.InitSDK();
            debugLevel = LootLockerConfig.current.currentDebugLevel;
        }

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            // Cleanup
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            bool cleanupComplete = false;
            LootLockerSDKManager.DeletePlayer(deletePlayerResponse =>
            {
                if (!deletePlayerResponse.success)
                {
                    Debug.LogError("Failed to delete player: " + deletePlayerResponse.text);
                    LootLockerSDKManager.EndSession(endSessionResponse =>
                    {
                        cleanupComplete = true;
                    });
                }
                else
                {
                    cleanupComplete = true;
                }
            });
            yield return new WaitUntil(() => cleanupComplete);
            LootLockerConfig.current.currentDebugLevel = debugLevel;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [UnityTest]
        public IEnumerator GuestUserCanLoginWithoutPlayerId()
        {
            // Given
            bool guestLoginSucceeded = false;
            bool requestCompleted = false;

            // When
            LootLockerSDKManager.StartGuestSession(response =>
            {
                guestLoginSucceeded = response.success;
                requestCompleted = true;
            });  

            // Wait for response
            yield return new WaitUntil(() => requestCompleted);

            // Then
            Assert.IsTrue(guestLoginSucceeded, "Guest login failed");
        }

        [UnityTest]
        public IEnumerator GuestUserCanLoginWithPlayerId()
        {
            // Given
            bool guestLoginSucceeded = false;
            bool requestCompleted = false;
            string expectedPlayerIdentifier = GUID.Generate().ToString();
            string actualPlayerIdentifier = "";

            // When
            LootLockerSDKManager.StartGuestSession(expectedPlayerIdentifier, response =>
            {
                guestLoginSucceeded = response.success;
                requestCompleted = true;
                actualPlayerIdentifier = response.player_identifier;
            });

            // Wait for response
            yield return new WaitUntil(() => requestCompleted);

            // Then
            Assert.IsTrue(guestLoginSucceeded, "Guest login failed");
            Assert.AreEqual(expectedPlayerIdentifier, actualPlayerIdentifier, "Player identifier was not as expected");
        }
    }
}