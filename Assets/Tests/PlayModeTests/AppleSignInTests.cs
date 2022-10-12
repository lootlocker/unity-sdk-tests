using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LootLocker.Requests;

namespace Tests
{
    public class AppleSignInTests
    {
        private static string AUTHORIZATION_CODE = "<Needs to be added manually>";

        [UnityTest]
        public IEnumerator RefreshAppleSessionWithInvalidTokenFails()
        {
            // Given
            var gameObject = new GameObject();
            var sdkPortal = gameObject.AddComponent<SDKPortal>();
            int expectedResponseCode = 400;
            int actualResponseCode = -1;

            // When
            sdkPortal.RefreshAppleSession("invalid-token", (response) =>
            {
                actualResponseCode = response.statusCode;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return actualResponseCode >= 0;
            });

            // Then
            Assert.AreEqual(expectedResponseCode, actualResponseCode, "Response code wrong, expected " + expectedResponseCode + " but was " + actualResponseCode);

            // Cleanup
            sdkPortal.EndSession((response) => { });
        }

        [UnityTest]
        [Ignore("Needs manual creation of Authorization Code")]
        public IEnumerator RefreshAppleSessionWhenSignedInSucceeds()
        {
            // Given
            var gameObject = new GameObject();
            var sdkPortal = gameObject.AddComponent<SDKPortal>();

            int actualSignInStatusCode = -1;
            int expectedSignInStatusCode = 200;
            string refreshToken = null;

            sdkPortal.StartAppleSession(AUTHORIZATION_CODE, (response) =>
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
            sdkPortal.RefreshAppleSession(refreshToken, (response) =>
            {
                actualResponseCode = response.statusCode;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return actualResponseCode >= 0;
            });

            // Then
            Assert.AreEqual(expectedResponseCode, actualResponseCode, "Response code wrong, expected " + expectedResponseCode + " but was " + actualResponseCode);

            // Cleanup
            sdkPortal.EndSession((response) => { });
        }

        [UnityTest]
        [Ignore("Needs manual creation of Authorization Code")]
        public IEnumerator StartAppleSessionSucceedsAndProvidesRefreshToken()
        {
            // Given
            var gameObject = new GameObject();
            var sdkPortal = gameObject.AddComponent<SDKPortal>();

            int actualSignInStatusCode = -1;
            int expectedSignInStatusCode = 200;
            string refreshToken = null;

            // When
            sdkPortal.StartAppleSession(AUTHORIZATION_CODE, (response) =>
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

            // Cleanup
            sdkPortal.EndSession((response) => { });
        }

        [UnityTest]
        public IEnumerator StartAppleSessionWithInvalidAuthorizationCodeFails()
        {
            // Given
            var gameObject = new GameObject();
            var sdkPortal = gameObject.AddComponent<SDKPortal>();

            int actualSignInStatusCode = -1;
            int expectedSignInStatusCode = 400;

            // When
            sdkPortal.StartAppleSession("invalid_auth_code", (response) =>
            {
                actualSignInStatusCode = response.statusCode;
            });

            yield return new WaitUntil(() =>
            {
                return actualSignInStatusCode >= 0;
            });

            // Then
            Assert.AreEqual(expectedSignInStatusCode, actualSignInStatusCode, "Failed Sign In");

            // Cleanup
            sdkPortal.EndSession((response) => { });
        }
    }
}