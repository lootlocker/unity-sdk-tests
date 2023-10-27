using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using LootLocker;
using LootLocker.LootLockerEnums;
using LootLocker.Requests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Tests
{
    public class RemoteSessionTests
    {
        EndPointClass _claimSessionUrl = new EndPointClass("v3/client/remote/claim", LootLockerHTTPMethod.POST);
        EndPointClass _oauthSignInUrl = new EndPointClass("v3/client/oauth/token", LootLockerHTTPMethod.POST);
        EndPointClass _verifySessionUrl = new EndPointClass("v3/client/remote/verify", LootLockerHTTPMethod.POST);
        EndPointClass _authorizeSessionUrl = new EndPointClass("v3/client/remote/authorize", LootLockerHTTPMethod.POST);
        private static string WL_USER_EMAIL = "erik+unityci@lootlocker.io";
        private static string WL_USER_PASSWORD = "12345678";
        private static LootLockerConfig.DebugLevel _debugLevel;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            LLTestUtils.InitSDK();
            _debugLevel = LootLockerConfig.current.currentDebugLevel;
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
                LootLockerConfig.current.currentDebugLevel = _debugLevel;
                cleanupComplete = true;
            });
            yield return new WaitUntil(() => cleanupComplete);
        }

        [UnityTest]
        public IEnumerator RemoteSessionStartCanBeCancelled()
        {
            // Given
            string leaseCode = "";
            string leaseNonce = "";
            bool leaseCompleted = false;
            var leaseCallback = new Action<LootLockerLeaseRemoteSessionResponse>(leaseResponse =>
            {
                leaseCode = leaseResponse.code;
                leaseNonce = leaseResponse.nonce;
                leaseCompleted = true;
            });

            LootLockerRemoteSessionLeaseStatus latestStatus = LootLockerRemoteSessionLeaseStatus.Failed;
            int updateCallCount = 0;
            var updateCallback = new Action<LootLockerRemoteSessionStatusPollingResponse>((updateResponse) =>
            {
                latestStatus = updateResponse.lease_status;
                updateCallCount++;
            });

            LootLockerStartRemoteSessionResponse finalResponse = null;
            var remoteSessionProcessCompleted = new Action<LootLockerStartRemoteSessionResponse>(finishedProcessResponse =>
            {
                finalResponse = finishedProcessResponse;
            });

            // When Start Remote Session
            Guid processGuid = LootLockerSDKManager.StartRemoteSession(leaseCallback, updateCallback, remoteSessionProcessCompleted);

            // Assert lease response
            yield return new WaitUntil(() => leaseCompleted);
            Assert.IsNotEmpty(leaseCode, "Lease code was not returned");
            Assert.IsNotEmpty(leaseNonce, "Lease nonce was not returned");

            // Assert Updates are called
            yield return new WaitForSeconds(2.5f); // Give time for a couple of update callbacks
            Assert.IsTrue(updateCallCount >= 2, "Update was not called as expected");
            Assert.AreEqual(LootLockerRemoteSessionLeaseStatus.Created, latestStatus, "Latest status is not as expected");

            // When Cancel Remote Session
            LootLockerSDKManager.CancelRemoteSessionProcess(processGuid);

            // Wait for process finished response
            yield return new WaitUntil(() => finalResponse != null);
            updateCallCount = 0;

            // Then
            Assert.IsFalse(finalResponse.success, "Response status was not as expected");
            Assert.AreEqual(LootLockerRemoteSessionLeaseStatus.Cancelled, finalResponse.lease_status, "Final response was not 'cancelled'");

            // Wait for additional update calls
            yield return new WaitForSeconds(2.5f);
            Assert.AreEqual(0, updateCallCount, "Update was still called after remote session call was cancelled");
        }

        [UnityTest]
        public IEnumerator RemoteSessionStartCanTimeout()
        {
            // Given
            string leaseCode = "";
            string leaseNonce = "";
            bool leaseCompleted = false;
            var leaseCallback = new Action<LootLockerLeaseRemoteSessionResponse>(leaseResponse =>
            {
                leaseCode = leaseResponse.code;
                leaseNonce = leaseResponse.nonce;
                leaseCompleted = true;
            });

            LootLockerRemoteSessionLeaseStatus latestStatus = LootLockerRemoteSessionLeaseStatus.Failed;
            int updateCallCount = 0;
            var updateCallback = new Action<LootLockerRemoteSessionStatusPollingResponse>((updateResponse) =>
            {
                latestStatus = updateResponse.lease_status;
                updateCallCount++;
            });

            LootLockerStartRemoteSessionResponse finalResponse = null;
            var remoteSessionProcessCompleted = new Action<LootLockerStartRemoteSessionResponse>(finishedProcessResponse =>
            {
                finalResponse = finishedProcessResponse;
            });

            // When Start Remote Session
            Guid processGuid = LootLockerSDKManager.StartRemoteSession(leaseCallback, updateCallback, remoteSessionProcessCompleted, timeOutAfterMinutes: 0.1f);

            // Assert lease response
            yield return new WaitUntil(() => leaseCompleted);
            Assert.IsNotEmpty(leaseCode, "Lease code was not returned");
            Assert.IsNotEmpty(leaseNonce, "Lease nonce was not returned");

            // Assert Updates are called
            yield return new WaitForSeconds(2.5f); // Give time for a couple of update callbacks
            Assert.IsTrue(updateCallCount >= 2, "Update was not called as expected");
            Assert.AreEqual(LootLockerRemoteSessionLeaseStatus.Created, latestStatus, "Latest status is not as expected");

            // When enough time passes for timeout to hit
            yield return new WaitForSeconds(5.0f);

            // Wait for process finished response
            yield return new WaitUntil(() => finalResponse != null);
            updateCallCount = 0;

            // Then
            Assert.IsFalse(finalResponse.success, "Response status was not as expected");
            Assert.AreEqual(LootLockerRemoteSessionLeaseStatus.Timed_out, finalResponse.lease_status, "Final response was not 'timed out'");

            // Wait for additional update calls
            yield return new WaitForSeconds(2.5f);
            Assert.AreEqual(0, updateCallCount, "Update was still called after remote session call was timed out");
        }

        public class LootLockerClaimSession
        {
            public string code { get; set; }
            public string nonce { get; set; }
        }

        public class LootLockerClaimSessionResponse : LootLockerResponse
        {
            public string code { get; set; }
            public string nonce { get; set; }
        }

        public class LootLockerVerifySessionResponse : LootLockerResponse
        {
            public string player_id { get; set; }
            public LootLockerClaimSession lease { get; set; }
        }
        public class LootLockerAuthorizeSessionResponse : LootLockerVerifySessionResponse
        {
        }

        public class LootLockerOauthResponse : LootLockerResponse
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
        }

        [UnityTest]
        public IEnumerator RemoteSessionStartCanBeCompleted()
        {
            // Given
            string leaseCode = "";
            string leaseNonce = "";
            bool leaseCompleted = false;
            var leaseCallback = new Action<LootLockerLeaseRemoteSessionResponse>(leaseResponse =>
            {
                leaseCode = leaseResponse.code;
                leaseNonce = leaseResponse.nonce;
                leaseCompleted = true;
            });

            LootLockerRemoteSessionLeaseStatus latestStatus = LootLockerRemoteSessionLeaseStatus.Failed;
            int updateCallCount = 0;
            var updateCallback = new Action<LootLockerRemoteSessionStatusPollingResponse>((updateResponse) =>
            {
                latestStatus = updateResponse.lease_status;
                updateCallCount++;
            });

            LootLockerStartRemoteSessionResponse finalResponse = null;
            var remoteSessionProcessCompleted = new Action<LootLockerStartRemoteSessionResponse>(finishedProcessResponse =>
            {
                finalResponse = finishedProcessResponse;
            });

            // When Start Remote Session
            Guid processGuid = LootLockerSDKManager.StartRemoteSession(leaseCallback, updateCallback, remoteSessionProcessCompleted);

            // Then assert lease response
            yield return new WaitUntil(() => leaseCompleted);
            Assert.IsNotEmpty(leaseCode, "Lease code was not returned");
            Assert.IsNotEmpty(leaseNonce, "Lease nonce was not returned");

            // Then assert Updates are called
            yield return new WaitForSeconds(2.5f); // Give time for a couple of update callbacks
            Assert.IsTrue(updateCallCount >= 2, "Update was not called as expected");
            Assert.AreEqual(LootLockerRemoteSessionLeaseStatus.Created, latestStatus, "Latest status is not as expected");

            // When other client claims session
            LootLockerClaimSessionResponse lootLockerClaimResponse = null;
            LootLockerServerRequest.CallAPI(_claimSessionUrl.endPoint, _claimSessionUrl.httpMethod,
                "{ \"code\": \"" + leaseCode + "\" }",
                (rawClaimSessionResponse) =>
                {
                    lootLockerClaimResponse = LootLockerResponse.Deserialize<LootLockerClaimSessionResponse>(rawClaimSessionResponse);
                }, false, LootLockerCallerRole.Base);
            yield return new WaitUntil(() => lootLockerClaimResponse != null);
            
            // Then assert claim was a success
            Assert.IsTrue(lootLockerClaimResponse.success, "Claim request was not a success");

            // Then assert that polling reflects claim
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(LootLockerRemoteSessionLeaseStatus.Claimed, latestStatus, "Latest status was not claimed even though we've claimed the session");

            // When other client signs in using oauth
            string grantTypeURL = "https://api.lootlocker.io/v3/client/oauth/grant-type/credentials";
            LootLockerOauthResponse lootLockerOauthResponse = null;
            LootLockerServerRequest.CallAPI(_oauthSignInUrl.endPoint, _oauthSignInUrl.httpMethod,
                "{ \"grant_type\": \"" + grantTypeURL + "\", \"api_key\": \"" + LootLockerConfig.current.apiKey + "\", \"email\": \"" + WL_USER_EMAIL + "\", \"password\": \"" + WL_USER_PASSWORD + "\" }",
                (rawOauthResponse) =>
                {
                    lootLockerOauthResponse = LootLockerResponse.Deserialize<LootLockerOauthResponse>(rawOauthResponse);
                }, false, LootLockerCallerRole.Base);
            yield return new WaitUntil(() => lootLockerOauthResponse != null);

            // Then assert oauth was a success
            Assert.IsTrue(lootLockerOauthResponse.success, "Oauth request was not a success");

            // When other client verifies
            LootLockerVerifySessionResponse lootLockerVerifySessionResponse = null;
            var authHeader =
                new Dictionary<string, string> { { "Authorization", "Bearer " + lootLockerOauthResponse.access_token } };
            LootLockerServerRequest.CallAPI(_verifySessionUrl.endPoint, _verifySessionUrl.httpMethod,
                "{ \"code\": \"" + leaseCode + "\", \"nonce\": \""+ leaseNonce +"\" }",
                (rawVerifyResponse) =>
                {
                    lootLockerVerifySessionResponse = LootLockerResponse.Deserialize<LootLockerVerifySessionResponse>(rawVerifyResponse);
                }, false, LootLockerCallerRole.Base, authHeader);
            yield return new WaitUntil(() => lootLockerVerifySessionResponse != null);

            // Then assert verification was a success
            Assert.IsTrue(lootLockerVerifySessionResponse.success, "Verify request was not a success");

            // Then assert that polling reflects verification
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(LootLockerRemoteSessionLeaseStatus.Verified, latestStatus, "Latest status was not verified even though we've verified the session");

            // When other client verifies
            LootLockerAuthorizeSessionResponse lootLockerAuthorizeSessionResponse = null;
            LootLockerServerRequest.CallAPI(_authorizeSessionUrl.endPoint, _authorizeSessionUrl.httpMethod,
                "{ \"code\": \"" + leaseCode + "\", \"nonce\": \"" + lootLockerClaimResponse.nonce + "\" }",
                (rawAuthorizeResponse) =>
                {
                    lootLockerAuthorizeSessionResponse = LootLockerResponse.Deserialize<LootLockerAuthorizeSessionResponse>(rawAuthorizeResponse);
                }, false, LootLockerCallerRole.Base, authHeader);
            yield return new WaitUntil(() => lootLockerAuthorizeSessionResponse != null);

            // Then assert verification was a success
            Assert.IsTrue(lootLockerAuthorizeSessionResponse.success, "Authorize request was not a success");

            // Wait for process finished response
            yield return new WaitUntil(() => finalResponse != null);
            updateCallCount = 0;

            // Then assert remote session finishes
            Assert.IsTrue(finalResponse.success, "Remote session start failed");
            Assert.AreEqual(LootLockerRemoteSessionLeaseStatus.Authorized, finalResponse.lease_status, "Final response was not 'Authorized'");

            // Then assert we have a session and can call LL methods
            LootLockerGetPlayerInfoResponse playerInfoResponse = null;
            LootLockerSDKManager.GetPlayerInfo((lootLockerGetPlayerInfoResponse =>
            {
                playerInfoResponse = lootLockerGetPlayerInfoResponse;
            }));
            yield return new WaitUntil(() => playerInfoResponse != null);

            Assert.IsTrue(playerInfoResponse.success, "Could not execute LL method after remote session was started");
            Assert.AreEqual(lootLockerAuthorizeSessionResponse.player_id, finalResponse.player_ulid, "Player id differs between authorize and remote session");

            // Wait for additional update calls
            yield return new WaitForSeconds(2.5f);
            Assert.AreEqual(0, updateCallCount, "Update was still called after remote session call was cancelled");
        }
    }
}
