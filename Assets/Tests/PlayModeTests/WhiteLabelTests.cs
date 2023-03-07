using System.Collections;
using System.Collections.Generic;
using LootLocker;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LootLocker.Requests;

namespace Tests
{
    public class WhiteLabelLoginTests
    {
        private static string WL_UNVERIFIED_USER_EMAIL = "erik+unityci@lootlocker.io";
        private static string WL_UNVERIFIED_USER_PASSWORD = "12345678";
        private static int WL_UNVERIFIED_USER_ID = 6249;
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
        [Ignore("We need to implement admin api removal of the user after tests to be able to run this continuously")]
        public IEnumerator WhiteLabelSignUpSucceeds()
        {
            // Given
            int actualId = -1;
            string actualCreatedAt = null;
            int actualStatusCode = -1;
            int expectedStatusCode = 200;

            // When
            LootLockerSDKManager.WhiteLabelSignUp(WL_UNVERIFIED_USER_EMAIL, WL_UNVERIFIED_USER_PASSWORD, (response) =>
            {
                actualStatusCode = response.statusCode;
                if (response.success)
                {
                    actualId = response.ID;
                    actualCreatedAt = response.CreatedAt;
                }
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return actualStatusCode >= 0;
            });

            // Then
            Assert.AreEqual(expectedStatusCode, actualStatusCode, "White Label SignUp failed, status code not 200");
            Assert.IsTrue(actualId >= 0, "New ID is unexpected, expected positive integer but got " + actualId);
            Assert.IsTrue(!string.IsNullOrEmpty(actualCreatedAt), "CreatedAt timestamp is null or empty");
        }

        [UnityTest]
        public IEnumerator WhiteLabelLoginAndSessionStartFromSavedStateSucceeds()
        {
            // Given
            int actualLoginPlayerId = -1;
            int actualSessionPlayerId = -1;
            string actualLoginVerifiedAt = null;
            string actualLoginSessionToken = null;
            string actualStartSessionSessionToken = null;
            int actualLoginStatusCode = -1;
            int actualStartSessionStatusCode = -1;
            int expectedStatusCode = 200;

            // When
            LootLockerSDKManager.WhiteLabelLogin(WL_UNVERIFIED_USER_EMAIL, WL_UNVERIFIED_USER_PASSWORD, false, loginResponse =>
            {
                actualLoginStatusCode = loginResponse.statusCode;
                if (loginResponse.success)
                {
                    actualLoginSessionToken = loginResponse.SessionToken;
                    actualLoginPlayerId = loginResponse.ID;
                    actualLoginVerifiedAt = loginResponse.VerifiedAt;
                }
                else
                {
                    return;
                }
                
                LootLockerSDKManager.StartWhiteLabelSession((startSessionResponse) =>
                {
                    actualStartSessionStatusCode = startSessionResponse.statusCode;
                    if (startSessionResponse.success)
                    {
                        actualSessionPlayerId = startSessionResponse.player_id;
                        actualStartSessionSessionToken = startSessionResponse.session_token;
                    }
                });
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return actualLoginStatusCode >= 0 && (actualLoginStatusCode != 200 || actualStartSessionStatusCode >= 0);
            });

            // Then
            Assert.AreEqual(expectedStatusCode, actualLoginStatusCode, "Login failed, status code not 200");
            Assert.IsTrue(actualLoginPlayerId >= 0, "Logged in player id is unexpected, expected positive integer but got " + actualLoginPlayerId);
            Assert.IsTrue(!string.IsNullOrEmpty(actualLoginSessionToken), "Login session token is null or empty");
            Assert.IsTrue(string.IsNullOrEmpty(actualLoginVerifiedAt), "Verified at timestamp is set despite user being unverified");

            Assert.AreEqual(expectedStatusCode, actualStartSessionStatusCode, "Start session failed, status code not 200");
            Assert.IsTrue(actualSessionPlayerId >= 0, "Session player id is unexpected, expected positive integer but got " + actualSessionPlayerId);
            Assert.IsTrue(!string.IsNullOrEmpty(actualStartSessionSessionToken), "Session token is null or empty");
        }

        [UnityTest]
        public IEnumerator WhiteLabelLoginAndStartSessionSucceeds()
        {
            // Given
            int actualLoginPlayerId = -1;
            int actualSessionPlayerId = -1;
            string actualLoginVerifiedAt = null;
            string actualLoginSessionToken = null;
            string actualStartSessionSessionToken = null;
            int actualLoginStatusCode = -1;
            int actualStartSessionStatusCode = -1;
            int actualSummarizedStatusCode = -1;
            int expectedStatusCode = 200;

            // When
            LootLockerSDKManager.WhiteLabelLoginAndStartSession(WL_UNVERIFIED_USER_EMAIL, WL_UNVERIFIED_USER_PASSWORD, false, loginAndStartSessionResponse =>
            {
                if (loginAndStartSessionResponse == null) return;

                actualSummarizedStatusCode = loginAndStartSessionResponse.statusCode;

                if (loginAndStartSessionResponse.LoginResponse != null)
                {
                    actualLoginPlayerId = loginAndStartSessionResponse.LoginResponse.ID;
                    actualLoginVerifiedAt = loginAndStartSessionResponse.LoginResponse.VerifiedAt;
                    actualLoginSessionToken = loginAndStartSessionResponse.LoginResponse.SessionToken;
                    actualLoginStatusCode = loginAndStartSessionResponse.LoginResponse.statusCode;

                }

                if (loginAndStartSessionResponse.SessionResponse != null)
                {
                    actualStartSessionStatusCode = loginAndStartSessionResponse.SessionResponse.statusCode;
                    actualStartSessionSessionToken = loginAndStartSessionResponse.SessionResponse.session_token; 
                    actualSessionPlayerId = loginAndStartSessionResponse.SessionResponse.player_id;
                }
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return actualLoginStatusCode >= 0 && (actualLoginStatusCode != 200 || actualStartSessionStatusCode >= 0);
            });

            // Then
            Assert.AreEqual(actualStartSessionStatusCode > 0 ? actualStartSessionStatusCode : actualLoginStatusCode, actualSummarizedStatusCode, "Latest status code not reflected in meta structure");
            Assert.AreEqual(expectedStatusCode, actualLoginStatusCode, "Login failed, status code not 200");
            Assert.IsTrue(actualLoginPlayerId >= 0, "Logged in player id is unexpected, expected positive integer but got " + actualLoginPlayerId);
            Assert.IsTrue(!string.IsNullOrEmpty(actualLoginSessionToken), "Login session token is null or empty");
            Assert.IsTrue(string.IsNullOrEmpty(actualLoginVerifiedAt), "Verified at timestamp is set despite user being unverified");

            Assert.AreEqual(expectedStatusCode, actualStartSessionStatusCode, "Start session failed, status code not 200");
            Assert.IsTrue(actualSessionPlayerId >= 0, "Session player id is unexpected, expected positive integer but got " + actualSessionPlayerId);
            Assert.IsTrue(!string.IsNullOrEmpty(actualStartSessionSessionToken), "Session token is null or empty");
        }

        [UnityTest]
        public IEnumerator WhiteLabelCheckSessionFailsWhenNoSessionExists()
        {
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;

            // Given
            bool actualSessionState = false;
            bool responseReceived = false;

            // When
            LootLockerSDKManager.CheckWhiteLabelSession((response) =>
            {
                actualSessionState = response;
                responseReceived = true;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return responseReceived;
            });

            // Then
            Assert.IsFalse(actualSessionState, "Session Verified OK despite no session active");
        }

        [UnityTest]
        public IEnumerator WhiteLabelCheckSessionSucceeds()
        {
            // Prerequisite
            int startWhiteLabelSessionStatusCode = -1;
            int expectedStartWhiteLabelSessionStatusCode = 200;

            LootLockerSDKManager.WhiteLabelLogin(WL_UNVERIFIED_USER_EMAIL, WL_UNVERIFIED_USER_PASSWORD, false, loginResponse =>
            {
                if (!loginResponse.success)
                {
                    startWhiteLabelSessionStatusCode = loginResponse.statusCode;
                    return;
                }

                LootLockerSDKManager.StartWhiteLabelSession((startSessionResponse) =>
                {
                    startWhiteLabelSessionStatusCode = loginResponse.statusCode;
                });
            });

            yield return new WaitUntil(() =>
            {
                return startWhiteLabelSessionStatusCode >= 0;
            });

            Assert.AreEqual(expectedStartWhiteLabelSessionStatusCode, startWhiteLabelSessionStatusCode, "Prerequisite start session failed, status code not 200");

            // Given
            bool actualSessionState = false;
            bool responseReceived = false;

            // When
            LootLockerSDKManager.CheckWhiteLabelSession((response) =>
            {
                actualSessionState = response;
                responseReceived = true;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return responseReceived;
            });

            // Then
            Assert.IsTrue(actualSessionState, "Session Verification failed");
        }

        [UnityTest]
        public IEnumerator WhiteLabelCheckSessionWithEmailAndTokenSucceeds()
        {
            // Prerequisite
            int startWhiteLabelSessionStatusCode = -1;
            int expectedStartWhiteLabelSessionStatusCode = 200;
            string whitelabelSessionToken = null;

            LootLockerSDKManager.WhiteLabelLogin(WL_UNVERIFIED_USER_EMAIL, WL_UNVERIFIED_USER_PASSWORD, false, loginResponse =>
            {
                if (!loginResponse.success)
                {
                    startWhiteLabelSessionStatusCode = loginResponse.statusCode;
                    return;
                }
                whitelabelSessionToken = loginResponse.SessionToken;

                LootLockerSDKManager.StartWhiteLabelSession((startSessionResponse) =>
                {
                    startWhiteLabelSessionStatusCode = startSessionResponse.statusCode;
                });
            });

            yield return new WaitUntil(() =>
            {
                return startWhiteLabelSessionStatusCode >= 0;
            });

            Assert.AreEqual(expectedStartWhiteLabelSessionStatusCode, startWhiteLabelSessionStatusCode, "Prerequisite start session failed, status code not 200");

            // Given
            bool actualSessionState = false;
            bool responseReceived = false;

            // When
            LootLockerSDKManager.CheckWhiteLabelSession(WL_UNVERIFIED_USER_EMAIL, whitelabelSessionToken, (response) =>
            {
                actualSessionState = response;
                responseReceived = true;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return responseReceived;
            });

            // Then
            Assert.IsTrue(actualSessionState, "Session Verification failed");
        }

        [UnityTest]
        public IEnumerator WhiteLabelCheckSessionWithWrongEmailFails()
        {
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            // Prerequisite
            int startWhiteLabelSessionStatusCode = -1;
            int expectedStartWhiteLabelSessionStatusCode = 200;
            string whitelabelSessionToken = null;

            LootLockerSDKManager.WhiteLabelLogin(WL_UNVERIFIED_USER_EMAIL, WL_UNVERIFIED_USER_PASSWORD, false, loginResponse =>
            {
                if (!loginResponse.success)
                {
                    startWhiteLabelSessionStatusCode = loginResponse.statusCode;
                    return;
                }
                whitelabelSessionToken = loginResponse.SessionToken;

                LootLockerSDKManager.StartWhiteLabelSession((startSessionResponse) =>
                {
                    startWhiteLabelSessionStatusCode = startSessionResponse.statusCode;
                });
            });

            yield return new WaitUntil(() =>
            {
                return startWhiteLabelSessionStatusCode >= 0;
            });

            Assert.AreEqual(expectedStartWhiteLabelSessionStatusCode, startWhiteLabelSessionStatusCode, "Prerequisite start session failed, status code not 200");

            // Given
            bool actualSessionState = false;
            bool responseReceived = false;

            // When
            LootLockerSDKManager.CheckWhiteLabelSession("otheremail@boost.com", whitelabelSessionToken, (response) =>
            {
                actualSessionState = response;
                responseReceived = true;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return responseReceived;
            });

            // Then
            Assert.IsFalse(actualSessionState, "Session Verification failed");
        }

        [UnityTest]
        public IEnumerator RequestPasswordResetSucceeds()
        {
            // Given
            int expectedPasswordResetStatusCode = 204;
            int passwordResetStatusCode = -1;
            bool responseReceived = false;

            // When
            LootLockerSDKManager.WhiteLabelRequestPassword(WL_UNVERIFIED_USER_EMAIL, (response) =>
            {
                passwordResetStatusCode = response.statusCode;
                responseReceived = true;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return responseReceived;
            });

            // Then
            Assert.AreEqual(expectedPasswordResetStatusCode, passwordResetStatusCode, "Password reset failed, status code not 200");
        }

        [UnityTest]
        [Ignore("Rate limitations causes this test to fail")]
        public IEnumerator RequestEmailVerificationSucceeds()
        {
            // Given
            int expectedEmailVerificationStatusCode = 204;
            int emailVerificationResetStatusCode = -1;
            bool responseReceived = false;

            // When
            LootLockerSDKManager.WhiteLabelRequestVerification(WL_UNVERIFIED_USER_ID, (response) =>
            {
                emailVerificationResetStatusCode = response.statusCode;
                responseReceived = true;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return responseReceived;
            });

            // Then
            Assert.AreEqual(expectedEmailVerificationStatusCode, emailVerificationResetStatusCode, "Email Verification request failed, status code not 200");
        }

        [UnityTest]
        [Ignore("Rate limitations causes this test to fail")]
        public IEnumerator RequestEmailVerificationByEmailSucceeds()
        {
            // Given
            int expectedEmailVerificationStatusCode = 204;
            int emailVerificationResetStatusCode = -1;
            bool responseReceived = false;

            // When
            LootLockerSDKManager.WhiteLabelRequestVerification(WL_UNVERIFIED_USER_EMAIL, (response) =>
            {
                emailVerificationResetStatusCode = response.statusCode;
                responseReceived = true;
            });

            // Wait for response
            yield return new WaitUntil(() =>
            {
                return responseReceived;
            });

            // Then
            Assert.AreEqual(expectedEmailVerificationStatusCode, emailVerificationResetStatusCode, "Email Verification request failed, status code not 200");
        }
    }
}
