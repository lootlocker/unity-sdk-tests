﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LootLocker.Requests;

namespace Tests
{
    public class GuestLoginTest
    {
        [UnityTest]
        public IEnumerator GuestUserCanLogIn()
        {
            var guestLoginGO = new GameObject();
            var guestLogin = guestLoginGO.AddComponent<GuestLogin>();
            yield return new WaitUntil(() => guestLogin.IsDone());
            Assert.IsTrue(guestLogin.IsLoggedIn());
            Assert.IsTrue(guestLogin.GetPlayerId() != 0);

            // Cleanup
            bool cleanupComplete = false;
            LootLockerSDKManager.EndSession((response) => { cleanupComplete = true; });
            yield return new WaitUntil(() =>
            {
                return cleanupComplete;
            });
        }
    }
}