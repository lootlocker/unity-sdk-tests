using System.IO;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Tests
{
    public class AddAndRunSamplesTests
    {
        static ListRequest listInstalledPackagesRequest;
        static bool listRequestCompleted;

        string lootLockerPackageVersion = "";
        string importedSamplesPath = "";
        string[] sampleScenes;
        string previouslyActiveScenePath;

        [UnitySetUp]
        public IEnumerator UnitySetup()
        {
            Debug.Log("Setup Started");
            listInstalledPackagesRequest = Client.List();
            EditorApplication.update += ListRequestProgress;

            yield return new WaitUntil(() => listRequestCompleted);
            foreach (var package in listInstalledPackagesRequest.Result)
            {
                if (package.name.Equals("com.lootlocker.lootlockersdk"))
                {
                    lootLockerPackageVersion = package.version;
                    foreach (var sample in Sample.FindByPackage(package.name, package.version))
                    {
                        if (sample.displayName == "LootLockerExamples")
                        {
                            sample.Import(Sample.ImportOptions.HideImportWindow | Sample.ImportOptions.OverridePreviousImports);
                            if (sample.isImported)
                            {
                                importedSamplesPath = sample.importPath.Substring(sample.importPath.IndexOf("Assets/")); //Relative path
                            }
                        }
                    }
                }
            }

            sampleScenes = Directory.GetFiles(importedSamplesPath + "/Scenes", "*.unity");

            Debug.Log("Setup Ended");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            Debug.Log("Teardown started");
            if (!string.IsNullOrEmpty(previouslyActiveScenePath) && !EditorSceneManager.GetActiveScene().path.Equals(previouslyActiveScenePath))
            {
                EditorSceneManager.SetActiveScene(EditorSceneManager.OpenScene(previouslyActiveScenePath));
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(previouslyActiveScenePath);
                int sceneCount = EditorSceneManager.sceneCount;
                for (int i = 0; i < sceneCount; i++)
                {
                    Scene s = EditorSceneManager.GetSceneAt(i);
                    if(!s.path.Equals(previouslyActiveScenePath))
                    { 
                        EditorSceneManager.CloseScene(s, true);
                    }

                }
            }

            AssetDatabase.DeleteAsset("Assets/Samples");
            Debug.Log("Teardown ended");
            yield return null;
        }

        [UnityTest]
        public IEnumerator AddLootLockerSDKSamplesBuildAllScenesSucceeds()
        {
            Debug.Log("Test started at least");
            previouslyActiveScenePath = EditorSceneManager.GetSceneAt(0).path;
            Debug.Log("Previously Active Scene: " + previouslyActiveScenePath);
            LLTestUtils.InitSDK();
            Debug.LogError("BAILANDO FAILS");

            /*Assert.IsNotEmpty(lootLockerPackageVersion, "LootLocker Package not found");
            Assert.IsNotEmpty(importedSamplesPath, "Samples not imported");
            foreach (var sampleScene in sampleScenes)
            {
                string activeSceneBeforeTestPath = EditorSceneManager.GetActiveScene().path;
                Assert.IsNotEmpty(sampleScene, "Sample Scene is invalid");

                var scene = EditorSceneManager.OpenScene(sampleScene);
                Assert.IsTrue(scene.isLoaded, "Scene wasn't loaded: " + scene.path);
                EditorSceneManager.SetActiveScene(scene);
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                Assert.IsTrue(true, "No errors were logged when opening sample");
            }
            var buildReport = BuildPipeline.BuildPlayer(new BuildPlayerOptions { options = BuildOptions.None, locationPathName = "Build", targetGroup = BuildTargetGroup.Standalone, target = BuildTarget.StandaloneWindows });

            Debug.Log("Build completed, Summary: Build time - " + (buildReport.summary.buildEndedAt - buildReport.summary.buildStartedAt).Seconds + "s. Result - " + buildReport.summary.result + ". Errors - " + buildReport.summary.totalErrors + ". Warnings - " + buildReport.summary.totalWarnings);

            Assert.AreEqual(UnityEditor.Build.Reporting.BuildResult.Succeeded, buildReport.summary.result, "Compilation succeeded");
            */

            yield return null;
        }

        static void ListRequestProgress()
        {
            if (listInstalledPackagesRequest.IsCompleted)
            {
                EditorApplication.update -= ListRequestProgress;
                listRequestCompleted = true;
            }
        }
    }
}
