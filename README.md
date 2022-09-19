<h1 align="center">LootLocker Unity SDK Test Suite</h1>

<h1 align="center">
  <a href="https://www.lootlocker.io/"><img src="https://s3.eu-west-1.amazonaws.com/cdn.lootlocker.io/public/lootLocker_wide_dark_whiteBG.png" alt="LootLocker"></a>
</h1>

<p align="center">
  <a href="#about">About</a> •
  <a href="#Usage">Usage</a> •
  <a href="#support">Support</a> •
  <a href="https://github.com/LootLocker/unity-sdk">SDK</a>
</p>

---

## About

This repo contains and us limited to the tests for https://github.com/LootLocker/unity-sdk.

LootLocker is a game backend-as-a-service that unlocks cross-platform systems so you can build, ship, and run your best games.

Full documentation is available here https://docs.lootlocker.io/getting-started/unity-tutorials


## Usage
### Run the tests locally

You can run the test suite locally to test the SDK code if you wish. You need to have installed and familiarized yourself with Git and Unity.
- Clone this repo
- Open the project in Unity
- Add and configure the LootLocker SDK package (see documentation in https://github.com/LootLocker/unity-sdk). You can add the git repository if you won't need to change it, or you can clone that repo and add the package locally if you mean to do SDK development.
- Run the tests (Unity guide can be found here: https://docs.unity3d.com/550/Documentation/Manual/testing-editortestsrunner.html)
  - Using the editor
    - Open the Test Runner (Windows > General > Test Runner)
    - Press "Run All", or select the tests you wish to run and run those specifically.
  - Using command line
    - You can run the tests headless using the batch mode. See the unity documentation. You need to also supply the LootLocker api key `-apiKey <apiKey>` and domain key `-domainKey <domainKey>`.


## Support

If you have any issues or just wanna chat you can reach us on our [Discord Server](https://discord.lootlocker.io/)