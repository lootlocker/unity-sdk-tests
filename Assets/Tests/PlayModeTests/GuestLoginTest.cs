using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
        }
    }
}