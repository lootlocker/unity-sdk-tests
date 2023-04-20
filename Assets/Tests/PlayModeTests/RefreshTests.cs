using System.Collections;
using System.Collections.Generic;
using LootLocker;
using LootLocker.Requests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class RefreshTests
    {
        private static string WL_USER_EMAIL = "erik+unityci@lootlocker.io";
        private static string WL_USER_PASSWORD = "12345678";
        private static LootLockerConfig.DebugLevel _debugLevel;
        private static bool _autoRefresh;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            LLTestUtils.InitSDK();
            _debugLevel = LootLockerConfig.current.currentDebugLevel;
            _autoRefresh = LootLockerConfig.current.allowTokenRefresh;
            // When
            bool wlLoginCompleted = false;
            LootLockerSDKManager.WhiteLabelLoginAndStartSession(WL_USER_EMAIL, WL_USER_PASSWORD, true, response =>
            {
                wlLoginCompleted = true;
            });

            // Wait for response
            yield return new WaitUntil(() => wlLoginCompleted);
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            // Cleanup
            bool cleanupComplete = false;
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            LootLockerSDKManager.EndSession((response) =>
            {
                LootLockerConfig.current.currentDebugLevel = _debugLevel;
                LootLockerConfig.current.allowTokenRefresh = _autoRefresh;
                cleanupComplete = true;
            });
            LootLockerSDKManager.ClearLocalSession();
            yield return new WaitUntil(() => cleanupComplete);
        }

        [UnityTest]
        public IEnumerator SDKAutomaticallyRefreshesExpiredSessionWhenEnabled()
        {
            // Given
            const string invalidToken = "ThisIsANonExistentToken";
            LootLockerConfig.current.token = invalidToken;
            LootLockerConfig.current.allowTokenRefresh = true;
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
            Assert.AreNotEqual(invalidToken, LootLockerConfig.current.token, "Token was not refreshed");
        }

        [UnityTest]
        public IEnumerator SDKDoesNotAutomaticallyRefreshesExpiredSessionWhenDisabled()
        {
            // Given
            const string invalidToken = "ThisIsANonExistentToken";
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            LootLockerConfig.current.token = invalidToken;
            LootLockerConfig.current.allowTokenRefresh = false;
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
            Assert.IsFalse(actualPingResponse.success, "Ping failed");
            Assert.AreNotEqual(200, actualPingResponse.statusCode, "Ping returned non 200 response code");
            Assert.AreEqual(invalidToken, LootLockerConfig.current.token, "Token was not refreshed");
        }
    }
}
