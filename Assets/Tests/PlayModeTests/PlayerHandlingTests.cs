using System.Collections;
using LootLocker;
using LootLocker.Requests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PlayerHandlingTests
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
            bool cleanupComplete = false;
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            LootLockerSDKManager.EndSession((response) =>
            {
                LootLockerConfig.current.currentDebugLevel = debugLevel;
                cleanupComplete = true;
            });
            yield return new WaitUntil(() => cleanupComplete);
        }

        [UnityTest]
        public IEnumerator PlayerCanBeCreatedAndDeleted()
        {
            // Given
            string PlayerIdentifier = System.Guid.NewGuid().ToString();
            bool completed = false;
            int expectedStatusCode = 204;
            LootLockerResponse actualResponse = new LootLockerResponse();

            // When
            LootLockerSDKManager.StartGuestSession(PlayerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                    completed = true;
                    return;
                }
                Assert.IsFalse(response.seen_before, "Player was not newly created");
                LootLockerSDKManager.DeletePlayer(deleteResponse =>
                {
                    actualResponse = deleteResponse;
                    completed = true;
                });
            });

            // Wait for response
            yield return new WaitUntil(() => completed);
            
            // Then
            Assert.IsTrue(actualResponse.success, "Delete player failed");
            Assert.AreEqual(expectedStatusCode, actualResponse.statusCode, "Delete player returned non 204 response code");
        }
    }
}
