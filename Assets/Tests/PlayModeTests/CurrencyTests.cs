using System;
using System.Collections;
using LootLocker;
using LootLocker.Requests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Economy
{
    public class CurrencyTests
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
        public IEnumerator CurrenciesCanBeListed()
        {
            // Prerequisites
            string playerIdentifier = System.Guid.NewGuid().ToString();
            bool completed = false;

            LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            // Given
            LootLockerListCurrenciesResponse expectedResponse = new LootLockerListCurrenciesResponse
            {
                success = true,
                currencies = new LootLockerCurrency[2]
            };
            expectedResponse.currencies[0] = new LootLockerCurrency()
            {
                code = "GLD", name = "Gold", game_api_writes_enabled = true
            };
            expectedResponse.currencies[1] = new LootLockerCurrency()
            {
                code = "SLV", name = "Silver"
            };
            LootLockerListCurrenciesResponse actualResponse = null;

            // When
            LootLockerSDKManager.ListCurrencies((response) =>
            {
                actualResponse = response;
            });

            // Wait for response
            yield return new WaitUntil(() => actualResponse != null);

            // Then
            Assert.IsTrue(actualResponse.success, "List currencies request failed");
            Assert.AreEqual(expectedResponse.currencies.Length, actualResponse.currencies.Length, "Actual Currencies is not of the right length");
            int matches = 0;
            foreach (LootLockerCurrency actualCurrency in actualResponse.currencies)
            {
                foreach (var expectedCurrency in expectedResponse.currencies)
                {
                    if (actualCurrency.code.Equals(expectedCurrency.code, StringComparison.OrdinalIgnoreCase))
                    {
                        matches++;
                        Assert.AreEqual(expectedCurrency.name, actualCurrency.name, "Name did not match for currency with code: " + actualCurrency.code);
                        Assert.AreEqual(expectedCurrency.game_api_writes_enabled, actualCurrency.game_api_writes_enabled, "Enable Game API Writes did not match for currency with code: " + actualCurrency.code);
                    }
                }
            }
            Assert.AreEqual(expectedResponse.currencies.Length, matches, "Not all expected currencies were in the response");
        }

        [UnityTest]
        public IEnumerator CurrencyDenominationsCanBeFetchedByCode()
        {
            // Prerequisites
            string playerIdentifier = System.Guid.NewGuid().ToString();
            bool completed = false;

            LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            // Given
            LootLockerListDenominationsResponse expectedResponse = new LootLockerListDenominationsResponse()
            {
                success = true,
                denominations = new LootLockerDenomination[3]
            };
            expectedResponse.denominations[0] = new LootLockerDenomination()
            {
                name = "Coin",
                value = 1
            };
            expectedResponse.denominations[1] = new LootLockerDenomination()
            {
                name = "Mark",
                value = 10
            };
            expectedResponse.denominations[2] = new LootLockerDenomination()
            {
                name = "Nugget",
                value = 100
            };
            LootLockerListDenominationsResponse actualResponse = null;

            // When
            LootLockerSDKManager.GetCurrencyDenominationsByCode("GLD", (response) =>
            {
                actualResponse = response;
            });

            // Wait for response
            yield return new WaitUntil(() => actualResponse != null);

            // Then
            Assert.IsTrue(actualResponse.success, "Getting Currency denominations by code failed");
            Assert.AreEqual(expectedResponse.denominations.Length, actualResponse.denominations.Length, "Actual denominations is not of the right length");
            int matches = 0;
            foreach (LootLockerDenomination denomination in actualResponse.denominations)
            {
                var name = denomination.name;
                foreach (var t in expectedResponse.denominations)
                {
                    if (name.Equals(t.name, StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.AreEqual(t.value, denomination.value, "Value did not match for denomination " + denomination.name);
                        matches++;
                    }
                }
            }
            Assert.AreEqual(expectedResponse.denominations.Length, matches, "Not all expected denominations were in the response");
        }
    }
}
