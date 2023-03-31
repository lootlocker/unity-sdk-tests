using System;
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
    public class PlayerFilesTests
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
            LootLockerSDKManager.DeletePlayer(deleteResponse =>
            {
                LootLockerSDKManager.EndSession((response) =>
                {
                    LootLockerConfig.current.currentDebugLevel = debugLevel;
                    cleanupComplete = true;
                });
            });
            yield return new WaitUntil(() => cleanupComplete);
        }

        [UnityTest]
        public IEnumerator PlayerFileCanBeCreatedWithPathUpdatedAndThenDeleted()
        {
            // Given
            string path = Application.temporaryCachePath + "/PlayerFileCanBeCreatedWithPathUpdatedAndThenDeleted-creation.txt";
            string content = "First added line";
            TextWriter writer = new StreamWriter(path);
            writer.WriteLine(content);
            writer.Close();

            string PlayerIdentifier = System.Guid.NewGuid().ToString();
            bool completed = false;
            int expectedStatusCode = 200;
            LootLockerPlayerFile actualResponse = new LootLockerPlayerFile();
            bool setToPublic = true;

            // When
            LootLockerSDKManager.StartGuestSession(PlayerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                    completed = true;
                    return;
                }
                LootLockerSDKManager.UploadPlayerFile(path, "test", setToPublic, fileResponse =>
                {
                    actualResponse = fileResponse;
                    completed = true;
                });
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            // Then
            Assert.IsTrue(actualResponse.success, "File upload failed");
            Assert.AreEqual(expectedStatusCode, actualResponse.statusCode, "Delete player returned non 200 response code");
            Assert.AreEqual(setToPublic, actualResponse.is_public, "File does not have the same public setting");

            // Then Given - GET CONTENT
            completed = false;
            string actualUploadedContent = null;
            bool actualPublicState = false;

            // When
            LootLockerSDKManager.GetPlayerFile(actualResponse.id, fileResponse =>
            {
                if (fileResponse.success)
                {
                    WebClient client = new WebClient();

                    Stream data = client.OpenRead(@fileResponse.url);
                    StreamReader reader = new StreamReader(data);
                    actualUploadedContent = reader.ReadToEnd();
                    actualPublicState = fileResponse.is_public;
                    completed = true;
                }
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            // Then
            Assert.IsNotNull(actualUploadedContent, "File could not be fetched");
            Assert.AreEqual(content.Trim(), actualUploadedContent.Trim(), "Content did not match in the downloaded file");
            Assert.AreEqual(setToPublic, actualPublicState, "File does not have the same public setting");

            // Then Given - UPDATE FILE
            completed = false;
            LootLockerPlayerFile actualUpdateResponse = null;
            string updatedFilePath = Application.temporaryCachePath + "/PlayerFileCanBeCreatedWithPathUpdatedAndThenDeleted-update.txt";
            string updatedFileContent = "Second added line";
            writer = new StreamWriter(updatedFilePath);
            writer.WriteLine(updatedFileContent);
            writer.Close();

            // When
            LootLockerSDKManager.UpdatePlayerFile(actualResponse.id, updatedFilePath, updateFileResponse =>
            {
                actualUpdateResponse = updateFileResponse;
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            // Then
            Assert.IsNotNull(actualUpdateResponse);
            Assert.IsTrue(actualUpdateResponse.success, "Failed to update file");
            Assert.AreEqual(actualResponse.id, actualUpdateResponse.id, "Id of the file changed");
            Assert.AreNotEqual(actualResponse.revision_id, actualUpdateResponse.revision_id, "Revision Id did not change");
            Assert.AreEqual(setToPublic, actualUpdateResponse.is_public, "File does not have the same public setting");

            // Then Given - GET UPDATED CONTENT
            completed = false;
            string actualUpdatedContent = null;

            // When
            LootLockerSDKManager.GetPlayerFile(actualUpdateResponse.id, fileResponse =>
            {
                if (fileResponse.success)
                {
                    WebClient client = new WebClient();

                    Stream data = client.OpenRead(@fileResponse.url);
                    StreamReader reader = new StreamReader(data);
                    actualUpdatedContent = reader.ReadToEnd();
                    actualPublicState = fileResponse.is_public;
                    completed = true;
                }
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            // Then
            Assert.IsNotNull(actualUpdatedContent, "File could not be fetched");
            Assert.AreNotEqual(content.Trim(), actualUpdatedContent.Trim(), "Content did not update in the downloaded file");
            Assert.AreEqual(updatedFileContent.Trim(), actualUpdatedContent.Trim(), "Content did not match in the downloaded file");
            Assert.AreEqual(setToPublic, actualPublicState, "File does not have the same public setting");

            // Then Given - DELETE FILE
            completed = false;
            LootLockerResponse deleteResponse = null;

            // When
            LootLockerSDKManager.DeletePlayerFile(actualResponse.id, response =>
            {
                deleteResponse = response;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            // Then
            Assert.IsTrue(deleteResponse.success, "File could not be deleted");
        }
    }
}
