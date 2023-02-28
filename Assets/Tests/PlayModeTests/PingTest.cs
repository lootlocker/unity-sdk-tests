using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using LootLocker;
using LootLocker.Requests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PingTest
    {
        private readonly string _playerIdentifier = System.Guid.NewGuid().ToString();
        private static LootLockerConfig.DebugLevel _debugLevel;
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            LLTestUtils.InitSDK();
            _debugLevel = LootLockerConfig.current.currentDebugLevel;
            // When
            bool guestLoginCompleted = false;
            LootLockerSDKManager.StartGuestSession(_playerIdentifier, response =>
            {
                guestLoginCompleted = true;
            });

            // Wait for response
            yield return new WaitUntil(() => guestLoginCompleted);
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            // Cleanup
            bool cleanupComplete = false;
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            LootLockerSDKManager.DeletePlayer(deleteResponse =>
            {
                LootLockerSDKManager.EndSession((response) =>
                {
                    LootLockerConfig.current.currentDebugLevel = _debugLevel;
                    cleanupComplete = true;
                });
            });
            yield return new WaitUntil(() => cleanupComplete);
        }

        [UnityTest]
        public IEnumerator BackendCanBePinged()
        {
            // Given
            LootLockerPingResponse actualPingResponse = null;

            // When
            bool completed = false;
            LootLockerSDKManager.Ping(response =>
            {
                actualPingResponse = response;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            // Then
            Assert.NotNull(actualPingResponse, "Request did not execute correctly");
            Assert.IsTrue(actualPingResponse.success, "Ping failed");
            Assert.AreEqual(200, actualPingResponse.statusCode, "Ping returned non 200 response code");
            Assert.IsFalse(string.IsNullOrEmpty(actualPingResponse.date), "Returned date is null or empty");
        }
    }
}
