using System.Collections;
using LootLocker;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LootLocker.Requests;

namespace Tests
{
    public class GoogleSignInTests
    {
        private static string ID_TOKEN = "<Needs to be added manually>";
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
        public IEnumerator RefreshGoogleSessionWithInvalidTokenFails()
        {
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            // Given
            int expectedResponseCode = 401;
            int actualResponseCode = -1;

            // When
            LootLockerSDKManager.RefreshGoogleSession("invalid-token", (response) =>
            {
                actualResponseCode = response.statusCode;
            });

            // Wait for response
            yield return new WaitUntil(() => actualResponseCode >= 0);

            // Then
            Assert.AreEqual(expectedResponseCode, actualResponseCode, "Response code wrong, expected " + expectedResponseCode + " but was " + actualResponseCode);
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.All;
        }

        [UnityTest]
        [Ignore("Needs manual creation of Authorization Code")]
        public IEnumerator RefreshGoogleSessionWhenSignedInSucceeds()
        {
            // Given
            int actualSignInStatusCode = -1;
            int expectedSignInStatusCode = 200;
            string refreshToken = null;

            LootLockerSDKManager.StartGoogleSession(ID_TOKEN, (response) =>
            {
                actualSignInStatusCode = response.statusCode;
                refreshToken = response.refresh_token;
            });

            yield return new WaitUntil(() =>
            {
                return actualSignInStatusCode >= 0;
            });

            Assert.AreEqual(expectedSignInStatusCode, actualSignInStatusCode, "Failed Sign In");

            int expectedResponseCode = 200;
            int actualResponseCode = -1;

            // When
            LootLockerSDKManager.RefreshGoogleSession((response) =>
            {
                actualResponseCode = response.statusCode;
            });

            // Wait for response
            yield return new WaitUntil(() => actualResponseCode >= 0);

            // Then
            Assert.AreEqual(expectedResponseCode, actualResponseCode, "Response code wrong, expected " + expectedResponseCode + " but was " + actualResponseCode);
        }

        [UnityTest]
        [Ignore("Needs manual creation of Authorization Code")]
        public IEnumerator StartGoogleSessionSucceedsAndProvidesRefreshToken()
        {
            // Given
            int actualSignInStatusCode = -1;
            int expectedSignInStatusCode = 200;
            string refreshToken = null;

            // When
            LootLockerSDKManager.StartGoogleSession(ID_TOKEN, (response) =>
            {
                actualSignInStatusCode = response.statusCode;
                refreshToken = response.refresh_token;
            });

            yield return new WaitUntil(() =>
            {
                return actualSignInStatusCode >= 0;
            });

            // Then
            Assert.AreEqual(expectedSignInStatusCode, actualSignInStatusCode, "Failed Sign In");
            Assert.IsTrue(!string.IsNullOrEmpty(refreshToken), "No Refresh Token returned");
            Debug.Log("Refresh Token: " + refreshToken);
        }

        [UnityTest]
        public IEnumerator StartGoogleSessionWithInvalidAuthorizationCodeFails()
        {
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            // Given
            int actualSignInStatusCode = -1;
            int expectedSignInStatusCode = 400;

            // When
            LootLockerSDKManager.StartGoogleSession("invalid_id_token", (response) =>
            {
                actualSignInStatusCode = response.statusCode;
            });

            yield return new WaitUntil(() => actualSignInStatusCode >= 0);

            // Then
            Assert.AreEqual(expectedSignInStatusCode, actualSignInStatusCode, "Failed Sign In");
        }
    }
}