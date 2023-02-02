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
        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            // Cleanup
            bool cleanupComplete = false;
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            LootLockerSDKManager.EndSession((response) => {
                LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.All; 
                cleanupComplete = true;
            });
            yield return new WaitUntil(() => cleanupComplete);
        }

        [UnityTest]
        public IEnumerator GuestUserCanLogIn()
        {
            var guestLoginGO = new GameObject();
            var guestLogin = guestLoginGO.AddComponent<GuestLogin>();
            yield return new WaitUntil(() => guestLogin.IsDone());
            Assert.IsTrue(guestLogin.IsLoggedIn());
            Assert.IsTrue(guestLogin.GetPlayerId() != 0);
        }
    }
}