using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LootLocker.Requests;

namespace Tests
{
    public class SubmitScoresWithGuestLogin
    {
        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            // Cleanup
            bool cleanupComplete = false;
            LootLockerSDKManager.EndSession((response) => { cleanupComplete = true; });
            yield return new WaitUntil(() =>
            {
                return cleanupComplete;
            });
            yield return null;
        }

        [UnityTest]
        [Ignore("There is a race condition somewhere in the request chain so responses are intermittently incorrect in the CI system. Needs looking into when we expand the leaderboard test suite")]
        public IEnumerator TestResponsesAreAsExpectedForAllLeaderboardsAndPlayers()
        {
            // Given
            int responsesExpected = 2 * players.Length * leaderboards.Length;
            var guestLoginGO = new GameObject();
            var guestLogin = guestLoginGO.AddComponent<GuestLogin>();

            // Wait for SDK Init
            yield return new WaitUntil(() => guestLogin.IsDone());
            Assert.IsTrue(guestLogin.IsLoggedIn());

            // When
            int responsesReceived = 0;
            foreach (var leaderboard in leaderboards) {
                foreach (var player in players) {
                    var expectedPlayer = leaderboard.isPlayerType ? players[0] : player;
                    int responsesExpectedLocal = responsesReceived+=1;
                    //By leaderboard id
                    LootLockerSDKManager.SubmitScore(player.memberId, player.score, leaderboard.id, player.metadata, (response) => {
                        // Then
                        AssertResponse(leaderboard, expectedPlayer, response);
                        responsesReceived++;
                    });
                    yield return new WaitUntil(() => {
                        return responsesReceived >= responsesExpectedLocal;
                    });
                    responsesExpectedLocal = responsesReceived += 1;
                    //By leaderboard key
                    LootLockerSDKManager.SubmitScore(player.memberId, player.score, leaderboard.key, player.metadata, (response) => {
                        // Then
                        AssertResponse(leaderboard, expectedPlayer, response);
                        responsesReceived++;
                    });
                    yield return new WaitUntil(() => {
                        return responsesReceived >= responsesExpectedLocal;
                    });
                }
            }
            yield return new WaitUntil(() => {
                return responsesReceived >= responsesExpected;
            });
        }

        private void AssertResponse(LeaderboardId expectedLeaderboard, LeaderBoardPlayer expectedPlayer, LootLockerSubmitScoreResponse response) {
            string combo = "board " + expectedLeaderboard.name + " and player " + expectedPlayer.memberId;
            Assert.AreEqual(200, response.statusCode, "Response code wrong for " + combo + " expected " + 200 + " but was " + response.statusCode);
            Assert.AreEqual(expectedPlayer.score, response.score, "Wrong score returned for " + combo + " expected " + expectedPlayer.score + " but was " + response.score);
            Assert.IsTrue(!string.IsNullOrEmpty(response.member_id), "No member id returned for " + combo);
            if(!expectedLeaderboard.isPlayerType) {
                Assert.AreEqual(expectedPlayer.rank, response.rank, "Wrong rank returned for " + combo + " expected " + expectedPlayer.rank + " but was " + response.rank);
            }

            if (expectedLeaderboard.hasMetadata) {
                Assert.AreEqual(expectedPlayer.metadata, response.metadata, "Wrong metadata returned for " + combo + " expected " + expectedPlayer.metadata + " but was " + response.metadata);
            } else {
                Assert.IsTrue(string.IsNullOrEmpty(response.metadata), "Metadata returned for non metadata leaderboard " + combo);
            }
        }

        // TEST DATA
        private LeaderBoardPlayer[] players = new LeaderBoardPlayer[] {
            new LeaderBoardPlayer("p1",     1337,   1, "MetadataForPlayer1"),
            new LeaderBoardPlayer("p2",     1336,   2, "MetadataForPlayer2"),
            new LeaderBoardPlayer("p3",     1335,   3, "MetadataForPlayer3"),
            new LeaderBoardPlayer("p4",     1334,   4, "MetadataForPlayer4"),
            new LeaderBoardPlayer("p5",     1333,   5, "MetadataForPlayer5"),
            new LeaderBoardPlayer("p6",     1332,   7, "MetadataForPlayer6"),
            new LeaderBoardPlayer("p7",     1332,   6, "MetadataForPlayer7"),
            new LeaderBoardPlayer("p8",     900,    8, "MetadataForPlayer8"),
            new LeaderBoardPlayer("p9",     24,     9, "MetadataForPlayer9"),
            new LeaderBoardPlayer("p10",    13,     10, "MetadataForPlayer10"),
            new LeaderBoardPlayer("p11",    1,      11, "MetadataForPlayer11"),
        };

        private LeaderboardId[] leaderboards = new LeaderboardId[] {
            new LeaderboardId("PlayerLeaderboardWithMetadata",          "player_leaderboard_metadata",  7227,   true,   true),
            new LeaderboardId("PlayerLeaderboardWithoutMetadata",       "player_leaderboard",           7228,   false,  true),
            new LeaderboardId("Generic Leaderboard With Metadata",      "generic_leaderboard_metadata", 7229,   true,   false),
            new LeaderboardId("Generic Leaderboard Without Metadata",   "generic_leaderboard",          7230,   false,  false),
        };


        private struct LeaderBoardPlayer {
            public LeaderBoardPlayer(string _memberId, int _score, int _rank, string _metadata) {
                memberId = _memberId;
                score = _score;
                rank = _rank;
                metadata = _metadata;
            }
            public string memberId;
            public int score;
            public int rank;
            public string metadata;
        }

        private struct LeaderboardId {
            public LeaderboardId(string _name, string _leaderboardKey, int _leaderboardId, bool _hasMetadata, bool _isPlayerType) {
                name = _name;
                key = _leaderboardKey;
                id = _leaderboardId;
                hasMetadata = _hasMetadata;
                isPlayerType = _isPlayerType;
            }
            public string name;
            public string key;
            public int id;
            public bool hasMetadata;
            public bool isPlayerType;
        }
    }
}