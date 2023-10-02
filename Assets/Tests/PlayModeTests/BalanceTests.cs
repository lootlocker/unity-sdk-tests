using System.Collections;
using LootLocker;
using LootLocker.LootLockerEnums;
using LootLocker.Requests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Economy
{
    public class BalanceTests
    {
        private static LootLockerConfig.DebugLevel originalDebugLevel;
        private static readonly string GoldCurrencyId = "01H8C1ZENBMTFMM17X10SGHFVS";

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
        public IEnumerator CanGetWalletByPlayerHolderId()
        {
            // Prerequisites
            string playerIdentifier = System.Guid.NewGuid().ToString();
            string playerUlid = "";
            bool completed = false;

            LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }

                playerUlid = response.player_ulid;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(playerUlid, "Player ULID not returned");

            // Given
            LootLockerGetWalletResponse actualResponse = null;

            // When
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            LootLockerSDKManager.GetWalletByHolderId(playerUlid, LootLockerWalletHolderTypes.player, (response) =>
            {
                actualResponse = response;
            });

            // Wait for response
            yield return new WaitUntil(() => actualResponse != null);

            LootLockerConfig.current.currentDebugLevel = originalDebugLevel;
            // Then
            Assert.IsTrue(actualResponse.success, "Getting wallet by holder id request failed");
            Assert.IsNotEmpty(actualResponse.id, "Wallet Id was not returned");
        }

        [UnityTest]
        public IEnumerator CreditAndDebitBalancesSucceeds()
        {
            // Prerequisites
            string playerIdentifier = System.Guid.NewGuid().ToString();
            string playerUlid = "";
            bool completed = false;

            LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }
                playerUlid = response.player_ulid;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(playerUlid, "Player ULID not returned");

            completed = false;
            string walletId = "";
            LootLockerCreateWalletRequest request = new LootLockerCreateWalletRequest()
            {
                holder_id = playerUlid,
                holder_type = LootLockerWalletHolderTypes.player.ToString()
            };
            LootLockerServerRequest.CallAPI(LootLockerEndPoints.createWallet.endPoint, LootLockerEndPoints.createWallet.httpMethod, LootLockerJson.SerializeObject(request),
                response =>
                {
                    walletId = LootLockerJson.DeserializeObject<LootLockerCreateWalletResponse>(response.text).wallet_id;
                    completed = true;
                }
            );

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(walletId, "No wallet id returned");

            // Given #Credit
            LootLockerCreditWalletResponse actualCreditResponse = null;

            // When #Credit
            LootLockerSDKManager.CreditBalanceToWallet(walletId, GoldCurrencyId, "10000", response =>
            {
                actualCreditResponse = response;
            });

            // Wait for response #Credit
            yield return new WaitUntil(() => actualCreditResponse != null);

            // Then #Credit
            Assert.IsTrue(actualCreditResponse.success, "Credit Request failed");
            Assert.AreEqual("10000", actualCreditResponse.amount, "New amount in the balance did not match the expected balance");

            // Given #Debit
            LootLockerDebitWalletResponse actualDebitResponse = null;

            // When #Debit
            LootLockerSDKManager.DebitBalanceToWallet(walletId, GoldCurrencyId, "5000", response =>
            {
                actualDebitResponse = response;
            });

            // Wait for response #Debit
            yield return new WaitUntil(() => actualDebitResponse != null);

            // Then #Debit
            Assert.IsTrue(actualDebitResponse.success, "Debit Request failed");
            Assert.AreEqual("5000", actualDebitResponse.amount, "New amount in the balance did not match the expected balance");
        }

        [UnityTest]
        public IEnumerator ListBalancesInWalletListsBalances()
        {
            // Prerequisites
            string playerIdentifier = System.Guid.NewGuid().ToString();
            string playerUlid = "";
            bool completed = false;

            LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }
                playerUlid = response.player_ulid;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(playerUlid, "Player ULID not returned");

            completed = false;
            string walletId = "";
            LootLockerCreateWalletRequest request = new LootLockerCreateWalletRequest()
            {
                holder_id = playerUlid,
                holder_type = LootLockerWalletHolderTypes.player.ToString()
            };
            LootLockerServerRequest.CallAPI(LootLockerEndPoints.createWallet.endPoint, LootLockerEndPoints.createWallet.httpMethod, LootLockerJson.SerializeObject(request),
                response =>
                {
                    walletId = LootLockerJson.DeserializeObject<LootLockerCreateWalletResponse>(response.text).wallet_id;
                    completed = true;
                }
            );

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(walletId, "No wallet id returned");

            LootLockerCreditWalletResponse actualCreditResponse = null;
            string setBalance = "19923111888133277716623458188182773666177267364888189917826";

            LootLockerSDKManager.CreditBalanceToWallet(walletId, GoldCurrencyId, setBalance.ToString(), response =>
            {
                actualCreditResponse = response;
            });

            yield return new WaitUntil(() => actualCreditResponse != null);

            Assert.IsTrue(actualCreditResponse.success, "Required balance credit did not succeed");

            // Given
            LootLockerListBalancesForWalletResponse actualResponse = null;

            // When
            LootLockerSDKManager.ListBalancesInWallet(walletId, response =>
            {
                actualResponse = response;
            });

            // Wait for response
            yield return new WaitUntil(() => actualResponse != null);

            // Then
            Assert.IsTrue(actualResponse.success, "List Request failed");
            Assert.AreEqual(1, actualResponse.balances.Length, "Balance list did not return the expected amount of balances");
            Assert.AreEqual(setBalance.ToString(), actualResponse.balances[0].amount, "The wrong balance was returned");
        }

        [UnityTest]
        public IEnumerator GetWalletByIdReturnsWallet()
        {
            // Prerequisites
            string playerIdentifier = System.Guid.NewGuid().ToString();
            string playerUlid = "";
            bool completed = false;

            LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }
                playerUlid = response.player_ulid;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(playerUlid, "Player ULID not returned");

            completed = false;
            string walletId = "";
            LootLockerCreateWalletRequest request = new LootLockerCreateWalletRequest()
            {
                holder_id = playerUlid,
                holder_type = LootLockerWalletHolderTypes.player.ToString()
            };
            LootLockerServerRequest.CallAPI(LootLockerEndPoints.createWallet.endPoint, LootLockerEndPoints.createWallet.httpMethod, LootLockerJson.SerializeObject(request),
                response =>
                {
                    walletId = LootLockerJson.DeserializeObject<LootLockerCreateWalletResponse>(response.text).wallet_id;
                    completed = true;
                }
            );

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(walletId, "No wallet id returned");

            // Given
            LootLockerGetWalletResponse actualResponse = null;

            // When
            LootLockerSDKManager.GetWalletByWalletId(walletId, response => actualResponse = response);

            // Wait for response
            yield return new WaitUntil(() => actualResponse != null);

            // Then
            Assert.IsTrue(actualResponse.success, "Get Wallet By ID request failed");
            Assert.AreEqual(playerUlid, actualResponse.holder_id, "Holder ID did not match for the wallet");
            Assert.AreEqual(LootLockerWalletHolderTypes.player, actualResponse.type, "Holder type did not match for the wallet");
        }

        [UnityTest]
        public IEnumerator CanCreateAndGetWalletForCharacter()
        {
            // Prerequisites
            string playerIdentifier = System.Guid.NewGuid().ToString();
            string playerUlid = "";
            bool completed = false;

            LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }

                playerUlid = response.player_ulid;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            string characterUlid = "";
            completed = false;

            LootLockerSDKManager.CreateCharacter(2.ToString(), "Beast", true, response =>
            {
                characterUlid = response.GetCharacter("Beast").ulid;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);

            Assert.IsNotEmpty(characterUlid, "Required character Ulid not set");

            // Given #ByHolderID
            LootLockerGetWalletResponse actualResponse = null;

            // When #ByHolderID
            LootLockerConfig.current.currentDebugLevel = LootLockerConfig.DebugLevel.AllAsNormal;
            LootLockerSDKManager.GetWalletByHolderId(characterUlid, LootLockerWalletHolderTypes.character, (response) =>
            {
                actualResponse = response;
            });

            // Wait for response #ByHolderID
            yield return new WaitUntil(() => actualResponse != null);
            LootLockerConfig.current.currentDebugLevel = originalDebugLevel;

            // Then #ByHolderID
            Assert.IsTrue(actualResponse.success, "Getting wallet by holder id request failed");
            Assert.IsNotEmpty(actualResponse.id, "Wallet Id was not returned");
            Assert.AreEqual(LootLockerWalletHolderTypes.character, actualResponse.type, "Wallet was not for the expected holder type");

            // Given #ByWalletID
            string walletId = actualResponse.id;
            actualResponse = null;

            // When #ByWalletID
            LootLockerSDKManager.GetWalletByWalletId(walletId, response => actualResponse = response);

            // Wait for response #ByWalletID
            yield return new WaitUntil(() => actualResponse != null);

            // Then #ByWalletID
            Assert.IsTrue(actualResponse.success, "Get Wallet By ID request failed");
            Assert.AreEqual(characterUlid, actualResponse.holder_id, "Holder ID did not match for the wallet");
            Assert.AreEqual(LootLockerWalletHolderTypes.character, actualResponse.type, "Holder type did not match for the wallet");
        }

        [UnityTest]
        public IEnumerator CanMakePurchase()
        {
            // Prerequisites
            string playerIdentifier = System.Guid.NewGuid().ToString();
            string playerUlid = "";
            bool completed = false;

            LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
            {
                if (!response.success)
                {
                    Assert.Fail("Required Guest Login failed");
                }
                playerUlid = response.player_ulid;
                completed = true;
            });

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(playerUlid, "Player ULID not returned");

            completed = false;
            string walletId = "";
            LootLockerCreateWalletRequest request = new LootLockerCreateWalletRequest()
            {
                holder_id = playerUlid,
                holder_type = LootLockerWalletHolderTypes.player.ToString()
            };
            LootLockerServerRequest.CallAPI(LootLockerEndPoints.createWallet.endPoint, LootLockerEndPoints.createWallet.httpMethod, LootLockerJson.SerializeObject(request),
                response =>
                {
                    walletId = LootLockerJson.DeserializeObject<LootLockerCreateWalletResponse>(response.text).wallet_id;
                    completed = true;
                }
            );

            // Wait for response
            yield return new WaitUntil(() => completed);
            Assert.IsNotEmpty(walletId, "No wallet id returned");

            LootLockerCreditWalletResponse actualCreditResponse = null;
            LootLockerSDKManager.CreditBalanceToWallet(walletId, GoldCurrencyId, "10000", response =>
            {
                actualCreditResponse = response;
            });

            // Wait for response
            yield return new WaitUntil(() => actualCreditResponse != null);
            Assert.IsTrue(actualCreditResponse.success, "Required credit request failed");

            // Given
            LootLockerPurchaseCatalogItemResponse actualPurchaseResponse = null;
            LootLockerCatalogItemAndQuantityPair[] itemsToBePurchased = new LootLockerCatalogItemAndQuantityPair[2];
            itemsToBePurchased[0] = new LootLockerCatalogItemAndQuantityPair()
            {
                catalog_item_id = "01H8ERSV2ABH5VZV4TXG5CF4MC",
                quantity = 1
            };
            itemsToBePurchased[1] = new LootLockerCatalogItemAndQuantityPair()
            {
                catalog_item_id = "01HBH029ZC91B5TQNPTX31ZFGE",
                quantity = 2
            };

            // When
            LootLockerSDKManager.LootLockerPurchaseCatalogItems(walletId, itemsToBePurchased, response =>
            {
                actualPurchaseResponse = response;
            });

            // Wait for response
            yield return new WaitUntil(() => actualPurchaseResponse != null);

            // Then
            Assert.IsTrue(actualPurchaseResponse.success, "Purchase Request failed");
            //Assert.AreEqual("5000", actualDebitResponse.amount, "New amount in the balance did not match the expected balance");
        }
    }
}
