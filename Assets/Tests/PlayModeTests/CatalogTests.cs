using System;
using System.Collections;
using System.Collections.Generic;
using LootLocker;
using LootLocker.LootLockerEnums;
using LootLocker.Requests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Economy
{
    public class CatalogTests
    {
        private static LootLockerConfig.DebugLevel originalDebugLevel;
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            LLTestUtils.InitSDK();
            originalDebugLevel = LootLockerConfig.current.currentDebugLevel;
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
                    LootLockerConfig.current.currentDebugLevel = originalDebugLevel;
                    cleanupComplete = true;
                });
            });
            yield return new WaitUntil(() => cleanupComplete);
        }

        [UnityTest]
        public IEnumerator TemplateTest()
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
            LootLockerResponse actualResponse = null;

            // When
            LootLockerSDKManager.Ping((response) => { actualResponse = response; });

            // Wait for response
            yield return new WaitUntil(() => actualResponse != null);

            // Then
            Assert.IsTrue(actualResponse.success, "X request failed");
        }

        [UnityTest]
        public IEnumerator CanListCatalogs()
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
            LootLockerListCatalogsResponse expectedResponse = new LootLockerListCatalogsResponse()
            {
                success = true,
                catalogs = new LootLockerCatalog[2]
            };
            expectedResponse.catalogs[0] = new LootLockerCatalog()
            {
                key = "store",
                name = "Store",
                id = "01H8CQAYSXXW5RN5VMJFH02PDR"
            };
            expectedResponse.catalogs[1] = new LootLockerCatalog()
            {
                key = "second_catalog",
                name = "Second Catalog",
                id = "01HBH1V2BEKT42RN5GGGYDJ4X1"
            };
            LootLockerListCatalogsResponse actualResponse = null;

            // When
            LootLockerSDKManager.ListCatalogs((response) => { actualResponse = response; });

            // Wait for response
            yield return new WaitUntil(() => actualResponse != null);

            // Then
            Assert.IsTrue(actualResponse.success, "List catalogs request failed");
            Assert.AreEqual(expectedResponse.catalogs.Length, actualResponse.catalogs.Length, "Actual catalogs is not of the right length");
            int matches = 0;
            foreach (var actualCatalog in actualResponse.catalogs)
            {
                foreach (var expectedCatalog in expectedResponse.catalogs)
                {
                    if (actualCatalog.key.Equals(expectedCatalog.key, StringComparison.OrdinalIgnoreCase))
                    {
                        matches++;
                        Assert.AreEqual(expectedCatalog.name, actualCatalog.name, "Name did not match for catalog with key: " + actualCatalog.key);
                        Assert.AreEqual(expectedCatalog.id, actualCatalog.id, "Id did not match for catalog with key: " + actualCatalog.key);
                    }
                }
            }
            Assert.AreEqual(expectedResponse.catalogs.Length, matches, "Not all expected catalogs were in the response");
        }

        [UnityTest]
        public IEnumerator CanListCatalogPrices()
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
            LootLockerListCatalogPricesResponse expectedResponse = new LootLockerListCatalogPricesResponse()
            {
                success = true,
                catalog = new LootLockerCatalog()
                    {
                        key = "store",
                        name = "Store",
                        id = "01H8CQAYSXXW5RN5VMJFH02PDR"
                    },
                entries = new LootLockerCatalogEntry[2],
                asset_details = new Dictionary<string, LootLockerAssetDetails>(),
                currency_details = null,//new Dictionary<string, LootLockerCurrencyDetails>(),
                progression_points_details = new Dictionary<string, LootLockerProgressionPointDetails>(),
                progression_resets_details = null// new Dictionary<string, LootLockerProgressionResetDetails>()
            };
            expectedResponse.entries[0] = new LootLockerCatalogEntry()
            {
                entity_id = "01H8CQBPKCYENYQYFAT1FJ2BH7",
                entity_kind = LootLockerCatalogEntryEntityKind.asset,
                entity_name = "Sword",
                prices = new LootLockerCatalogEntryPrice[2],
                grouping_key = "01H8CQCV0MGRFPAWESYVFYQPWY",
                purchasable = true
            };
            expectedResponse.entries[0].prices[0] = new LootLockerCatalogEntryPrice()
            {
                amount = 20,
                display_amount = "20 GLD",
                currency_code = "gld",
                currency_name = "Gold",
                currency_id = "01H8C1ZENBMTFMM17X10SGHFVS",
                price_id = "01H8CQCV0MGRFPAWESYRYXW1ZX"
            };
            expectedResponse.entries[0].prices[1] = new LootLockerCatalogEntryPrice()
            {
                amount = 123,
                display_amount = "123 SLV",
                currency_code = "slv",
                currency_name = "Silver",
                currency_id = "01H8GK39VTD56Y8DWHM1K3K80Z",
                price_id = "01H8GK6SHGEV6SQX4ZYY5S6KC1"
            };
            expectedResponse.entries[1] = new LootLockerCatalogEntry()
            {
                entity_id = "01H8ERP31RWZ5FWE8S02N6ZJP8",
                entity_kind = LootLockerCatalogEntryEntityKind.progression_points,
                entity_name = "Basic",
                prices = new LootLockerCatalogEntryPrice[1],
                grouping_key = "01H8ERRN6V6STDKEH8DAZGXVM2",
                purchasable = true
            };
            expectedResponse.entries[1].prices[0] = new LootLockerCatalogEntryPrice()
            {
                amount = 20,
                display_amount = "20 GLD",
                currency_code = "gld",
                currency_name = "Gold",
                currency_id = "01H8C1ZENBMTFMM17X10SGHFVS",
                price_id = "01H8ERRN6PPM78M0SEPS648ER9"
            };
            expectedResponse.asset_details.Add("01H8CQCV0MGRFPAWESYVFYQPWY", new LootLockerAssetDetails()
            {
                grouping_key = "01H8CQCV0MGRFPAWESYVFYQPWY",
                id = "01H8CQBPKCYENYQYFAT1FJ2BH7",
                legacy_id = 298788,
                name = "Sword",
                rental_option_id = null,
                thumbnail = null,
                variation_id = null
            });
            expectedResponse.progression_points_details.Add("01H8ERRN6V6STDKEH8DAZGXVM2",
                new LootLockerProgressionPointDetails()
                {
                    grouping_key = "01H8ERRN6V6STDKEH8DAZGXVM2",
                    key = "bas",
                    name = "Basic",
                    amount = 50,
                    id = "01H8ERP31RWZ5FWE8S02N6ZJP8"
                });
            LootLockerListCatalogPricesResponse actualResponse = null;

            // When
            LootLockerSDKManager.ListCatalogItems("store", 2, null, (response) => { actualResponse = response; });

            // Wait for response
            yield return new WaitUntil(() => actualResponse != null);

            // Then
            Assert.IsTrue(actualResponse.success, "List catalog prices request failed");
            Assert.AreEqual(expectedResponse.entries.Length, actualResponse.entries.Length, "Actual entries are not of the right length");
            Assert.AreEqual(expectedResponse.catalog.key, actualResponse.catalog.key, "The keys of the catalogs doesn't match");
            int matches = 0;
            foreach (var actualCatalogEntry in actualResponse.entries)
            {
                foreach (var expectedCatalogEntry in expectedResponse.entries)
                {
                    if (actualCatalogEntry.grouping_key.Equals(expectedCatalogEntry.grouping_key, StringComparison.OrdinalIgnoreCase))
                    {
                        matches++;
                        Assert.AreEqual(expectedCatalogEntry.entity_name, actualCatalogEntry.entity_name, "Entity name did not match for catalog with key: " + actualCatalogEntry.grouping_key);
                        Assert.AreEqual(expectedCatalogEntry.entity_id, actualCatalogEntry.entity_id, "Entity id did not match for catalog with key: " + actualCatalogEntry.grouping_key);
                        Assert.AreEqual(expectedCatalogEntry.purchasable, actualCatalogEntry.purchasable, "Purchasable did not match for catalog with key: " + actualCatalogEntry.grouping_key);
                        Assert.AreEqual(expectedCatalogEntry.entity_kind, actualCatalogEntry.entity_kind, "Entity kind did not match for catalog with key: " + actualCatalogEntry.grouping_key);
                        Assert.AreEqual(expectedCatalogEntry.prices.Length, actualCatalogEntry.prices.Length, "Entity prices length did not match for catalog with key: " + actualCatalogEntry.grouping_key);

                        int priceMatches = 0;
                        foreach (var actualPrice in actualCatalogEntry.prices)
                        {
                            foreach (var expectedPrice in expectedCatalogEntry.prices)
                            {
                                if (actualPrice.price_id == expectedPrice.price_id)
                                {
                                    priceMatches++;
                                    Assert.AreEqual(expectedPrice.amount, actualPrice.amount, "Amount did not match for price with id: " + actualPrice.price_id);
                                    Assert.AreEqual(expectedPrice.currency_id, actualPrice.currency_id, "Currency Id did not match for price with id: " + actualPrice.price_id);
                                    Assert.AreEqual(expectedPrice.currency_code, actualPrice.currency_code, "Currency Code did not match for price with id: " + actualPrice.price_id);
                                    Assert.AreEqual(expectedPrice.currency_name, actualPrice.currency_name, "Currency name did not match for price with id: " + actualPrice.price_id);
                                    Assert.AreEqual(expectedPrice.display_amount, actualPrice.display_amount, "Display amount did not match for price with id: " + actualPrice.price_id);
                                }
                            }
                        }
                        Assert.AreEqual(expectedCatalogEntry.prices.Length, priceMatches, "Not all prices found a match");

                        Assert.AreEqual(expectedResponse.asset_details != null, actualResponse.asset_details != null, "Asset Details is null for one side of the matcher");
                        if(expectedResponse.asset_details != null && actualResponse.asset_details != null) {
                            bool actualAssetDetailsFound = actualResponse.asset_details.TryGetValue(actualCatalogEntry.grouping_key, out var actualAssetDetails);
                            bool expectedAssetDetailsFound = expectedResponse.asset_details.TryGetValue(actualCatalogEntry.grouping_key, out var expectedAssetDetails);
                            Assert.AreEqual(expectedAssetDetailsFound, actualAssetDetailsFound, "Asset Details found for only one side of the check");
                            if (actualAssetDetails != null && expectedAssetDetails != null)
                            {
                                Assert.AreEqual(expectedAssetDetails.id, actualAssetDetails.id, "Id of the asset detail did not match");
                                Assert.AreEqual(expectedAssetDetails.legacy_id, actualAssetDetails.legacy_id, "legacy_id of the asset detail did not match");
                                Assert.AreEqual(expectedAssetDetails.name, actualAssetDetails.name, "name of the asset detail did not match");
                                Assert.AreEqual(expectedAssetDetails.rental_option_id, actualAssetDetails.rental_option_id, "rental_option_id of the asset detail did not match");
                                Assert.AreEqual(expectedAssetDetails.variation_id, actualAssetDetails.variation_id, "variation_id of the asset detail did not match");
                                Assert.AreEqual(expectedAssetDetails.thumbnail, actualAssetDetails.thumbnail, "thumbnail of the asset detail did not match");
                            }
                        }

                        Assert.AreEqual(expectedResponse.progression_points_details != null, actualResponse.progression_points_details != null, "Progression Points Details is null for one side of the matcher");
                        if (expectedResponse.progression_points_details != null && actualResponse.progression_points_details != null)
                        {
                            bool actualProgressionPointsDetailsFound = actualResponse.progression_points_details.TryGetValue(actualCatalogEntry.grouping_key, out var actualProgressionPointDetails);
                            bool expectedProgressionPointsDetailsFound = expectedResponse.progression_points_details.TryGetValue(actualCatalogEntry.grouping_key, out var expectedProgressionPointDetails);
                            Assert.AreEqual(expectedProgressionPointsDetailsFound, actualProgressionPointsDetailsFound, "ProgressionPoint Details found for only one side of the check");
                            if (actualProgressionPointDetails != null && expectedProgressionPointDetails != null)
                            {
                                Assert.AreEqual(expectedProgressionPointDetails.id, actualProgressionPointDetails.id, "Id of the progression point detail did not match");
                                Assert.AreEqual(expectedProgressionPointDetails.amount, actualProgressionPointDetails.amount, "amount of the progression point detail did not match");
                                Assert.AreEqual(expectedProgressionPointDetails.key, actualProgressionPointDetails.key, "key of the progression point detail did not match");
                                Assert.AreEqual(expectedProgressionPointDetails.name, actualProgressionPointDetails.name, "name of the progression point detail did not match");
                            }
                        }

                        Assert.AreEqual(expectedResponse.progression_resets_details != null, actualResponse.progression_resets_details != null, "Progression Reset Details is null for one side of the matcher");
                        if (expectedResponse.progression_resets_details != null && actualResponse.progression_resets_details != null)
                        {
                            bool actualProgressionPointResetDetailsFound = actualResponse.progression_resets_details.TryGetValue(actualCatalogEntry.grouping_key, out var actualProgressionResetDetails);
                            bool expectedProgressionPointResetDetailsFound = expectedResponse.progression_resets_details.TryGetValue(actualCatalogEntry.grouping_key, out var expectedProgressionResetDetails);
                            Assert.AreEqual(expectedProgressionPointResetDetailsFound, actualProgressionPointResetDetailsFound, "Progression Reset Details found for only one side of the check");
                            if (actualProgressionResetDetails != null && expectedProgressionResetDetails != null)
                            {
                                Assert.AreEqual(expectedProgressionResetDetails.id, actualProgressionResetDetails.id, "Id of the progression reset detail did not match");
                                Assert.AreEqual(expectedProgressionResetDetails.key, actualProgressionResetDetails.key, "key of the progression reset detail did not match");
                                Assert.AreEqual(expectedProgressionResetDetails.name, actualProgressionResetDetails.name, "name of the progression reset detail did not match");
                            }
                        }

                        Assert.AreEqual(expectedResponse.currency_details != null, actualResponse.currency_details != null, "Currency Details is null for one side of the matcher");
                        if (expectedResponse.currency_details != null && actualResponse.currency_details != null)
                        {
                            bool actualCurrencyDetailsFound = actualResponse.currency_details.TryGetValue(actualCatalogEntry.grouping_key, out var actualCurrencyDetails);
                            bool expectedCurrencyDetailsFound = expectedResponse.currency_details.TryGetValue(actualCatalogEntry.grouping_key, out var expectedCurrencyDetails);
                            Assert.AreEqual(expectedCurrencyDetailsFound, actualCurrencyDetailsFound, "Currency Details found for only one side of the check");
                            if (actualCurrencyDetails != null && expectedCurrencyDetails != null)
                            {
                                Assert.AreEqual(expectedCurrencyDetails.id, actualCurrencyDetails.id, "Id of the currency detail did not match");
                                Assert.AreEqual(expectedCurrencyDetails.amount, actualCurrencyDetails.amount, "amount of the currency detail did not match");
                                Assert.AreEqual(expectedCurrencyDetails.code, actualCurrencyDetails.code, "code of the currency detail did not match");
                                Assert.AreEqual(expectedCurrencyDetails.name, actualCurrencyDetails.name, "name of the currency detail did not match");
                            }
                        }
                    }
                }
            }

            Assert.AreEqual(expectedResponse.entries.Length, matches, "Not all expected entries were in the response");
        }
    }
}
