using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;

public class GuestLogin : MonoBehaviour
{
    private int     mPlayerId = 0;
    public string   mApiKey;
    public string   mDomainKey;
    private bool    mIsRequestDone = false;
    private bool    mIsRequestInProgress = false;
    private bool    mIsLoggedIn = false;
    
    void Start()
    {
        Login("Start");
    }

    public void Login(string source) {
        if(mIsLoggedIn || mIsRequestInProgress) {
            Debug.Log("Not in a state to log in, skipping login request from " + source);
            return;
        }
        Debug.Log("Starting log in request from " + source);
        
        mIsRequestDone = false;
        mIsRequestInProgress = true;
        InitSDK();

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if(response.success)
            {
                mPlayerId = response.player_id;
                Debug.Log("Logged in with player id: " + mPlayerId);
                mIsRequestDone = true;
                mIsLoggedIn = true;
                mIsRequestInProgress = false;
            }
            else
            {
                Debug.LogError("Error: " + response.Error);
                mIsRequestDone = true;
                mIsRequestInProgress = false;
            }
        });
    }

    private void InitSDK() {
        string[] args = System.Environment.GetCommandLineArgs ();
        for(int i = 0; i < args.Length; i++) {
            if(args[i] == "-apikey") {
                mApiKey = args[i+1];
            } else if (args[i] == "-domainkey") {
                mDomainKey = args[i+1];
            }
        }
        if((string.IsNullOrEmpty(mApiKey) || string.IsNullOrEmpty(mDomainKey))) {
            if(!LootLockerSDKManager.CheckInitialized(true)) {
                Debug.LogError("Can't run because no api key or domain key supplied");
                return;
            }
        } else {
            LootLockerSDKManager.Init(mApiKey, "0.0.0.1", mDomainKey);
            LootLocker.LootLockerConfig.current.currentDebugLevel = LootLocker.LootLockerConfig.DebugLevel.All;
        }
    }

    public bool IsDone() {
        return mIsRequestDone;
    }

    public bool IsLoggedIn() {
        return mIsLoggedIn;
    }

    public int GetPlayerId() {
        return mPlayerId;
    }
}