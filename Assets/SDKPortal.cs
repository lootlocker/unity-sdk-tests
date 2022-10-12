using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;

public class SDKPortal : MonoBehaviour
{
    public void SubmitScore(string memberId, int score, string leaderBoardKey, string metadata, Action<LootLockerSubmitScoreResponse> onComplete) {
        LootLockerSDKManager.SubmitScore(memberId, score, leaderBoardKey, metadata, onComplete);
    }

    public void SubmitScore(string memberId, int score, int leaderBoardId, string metadata, Action<LootLockerSubmitScoreResponse> onComplete) {
        LootLockerSDKManager.SubmitScore(memberId, score, leaderBoardId, metadata, onComplete);
    }

    public void StartAppleSession(string authorizationCode, Action<LootLockerAppleSessionResponse> onComplete)
    {
        LootLockerSDKManager.StartAppleSession(authorizationCode, onComplete);
    }

    public void RefreshAppleSession(string refreshToken, Action<LootLockerAppleSessionResponse> onComplete)
    {
        LootLockerSDKManager.RefreshAppleSession(refreshToken, onComplete);
    }

    public void EndSession(Action<LootLockerSessionResponse> onComplete)
    {
        LootLockerSDKManager.EndSession(onComplete);
    }
}
