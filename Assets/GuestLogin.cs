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
        login("Start");
    }

    public void login(string source) {
        if(mIsLoggedIn || mIsRequestInProgress) {
            Debug.Log("Not in a state to log in, skipping login request from " + source);
            return;
        }
        Debug.Log("Starting log in request from " + source);
        
        mIsRequestDone = false;
        mIsRequestInProgress = true;
        initSDK();

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
                Debug.Log("Error: " + response.Error);
                mIsRequestDone = true;
                mIsRequestInProgress = false;
            }
        });
    }

    private void initSDK() {
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
            
            Debug.Log("Making log in request with apiKey: " + mApiKey + " and domainkey: " + mDomainKey);
            LootLockerSDKManager.Init(mApiKey, "0.0.0.1", LootLocker.LootLockerConfig.platformType.Android, true, mDomainKey);
            LootLocker.LootLockerConfig.current.currentDebugLevel = LootLocker.LootLockerConfig.DebugLevel.All;
        }
    }

    public bool isDone() {
        return mIsRequestDone;
    }

    public bool isLoggedIn() {
        return mIsLoggedIn;
    }

    public int getPlayerId() {
        return mPlayerId;
    }
}