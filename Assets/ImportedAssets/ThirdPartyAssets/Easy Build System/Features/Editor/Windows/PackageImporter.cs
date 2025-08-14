/// <summary>
/// Project : Easy Build System
/// Class : PackageImporter.cs
/// Namespace : EasyBuildSystem.Features.Editor.Window
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using UnityEngine;
using UnityEngine.Rendering;

using EasyBuildSystem.Features.Editor.Extensions;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace EasyBuildSystem.Features.Editor.Window
{
    public class PackageImporter : EditorWindow
    {
        const string VERSION = "6.8.2";

        static List<BuildTargetGroup> m_Targets;
        static readonly List<string> m_Integrations = new List<string>();

        Vector2 m_ScrollPos;

        static List<PackageInfo> m_InstalledPackages = new List<PackageInfo>();

        [Serializable]
        class RequiredPackage
        {
            public string Name;
            public string DisplayName;

            public RequiredPackage(string name, string displayName)
            {
                Name = name;
                DisplayName = displayName;
            }
        }

        static EditorWindow window;
        public static void Init()
        {
            window = GetWindow(typeof(PackageImporter), false, "Package Importer", true);
            window.titleContent.image = EditorGUIUtility.IconContent("d__Popup").image;
            window.autoRepaintOnSceneChange = true;

            float width = 800;
            float height = 510;

            float x = (Screen.currentResolution.width - (width * 2.15f)) / 2;
            float y = (Screen.currentResolution.height - (height * 2f)) / 2;

            window.minSize = new Vector2(width, height);
            window.position = new Rect(x, y, width, height);

            window.Show(true);

            m_InstalledPackages = GetInstalledPackages();

            EditorApplication.UnlockReloadAssemblies();
        }

        GUIContent[] m_Tabs = new GUIContent[]
        {
            new GUIContent("Support Packages"),
            new GUIContent("Integration Packages")
        };

        int m_TabIndex;

        void OnGUI()
        {
            if (m_InstalledPackages == null || m_InstalledPackages.Count == 0)
            {
                m_InstalledPackages = GetInstalledPackages();
            }

            EditorGUIUtilityExtension.BeginVertical(false);

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Package Importer",
                "Here, you can import the support packages and integrations you want to use with Easy Build System.\n" +
                "For each support or integration package listed here, you will find a complete installation guide in the documentation.", true);

            GUILayout.Space(-3f);

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.Label(new GUIContent("Package Name : <color=#00ff44>Easy Build System</color>" + "   |   "
                + "Package Version : " + "<color=#00ff44>" + VERSION + "</color>" + "   |   "
                + "Render Pipeline : " + "<color=#00ff44>" + GetRenderPipeline() + "</color>"), new GUIStyle(GUI.skin.label) { richText = true });

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            m_TabIndex = EditorGUIUtilityExtension.Toolbar(m_TabIndex, m_Tabs);

            EditorGUIUtilityExtension.BeginVertical();

            if (m_TabIndex == 0)
            {
                m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                GUILayout.Space(3f);

                bool xr = false;

#if EBS_XR
                xr = true;
#endif

                AddSupportPackage("XR Interaction Toolkit Support", "High-level, component-based, interaction system for creating VR and AR experiences.\n" +
                    "Enables the use of the XR Interaction Toolkit package with Easy Build System.",
                    new List<RequiredPackage>() { new RequiredPackage("com.unity.xr.interaction.toolkit", "XR Interaction Toolkit") }, () =>
                    {
                        EditorApplication.update += ProcessDownload;
                        CurrentDownload = InstallSupports(true);
                    }, xr);

                AddSupportPackage("High Definition Render Pipeline Support", "Enables the replacement of all materials to be compatible with the High Definition Render Pipeline.",
                    new List<RequiredPackage>() { new RequiredPackage("com.unity.render-pipelines.high-definition", "High Definition RP") }, () =>
                    {
                        ProjectIntegrity.UpgradeToHDRP();
                    });

                AddSupportPackage("Universal Render Pipeline Support", "Enables the replacement of all materials to be compatible with the Universal Render Pipeline.",
                    new List<RequiredPackage>() { new RequiredPackage("com.unity.render-pipelines.universal", "Universal RP") }, () =>
                    {
                        ProjectIntegrity.UpgradeToURP();
                    });

                bool assembly = false;

#if EBS_ASSEMBLY
                assembly = true;
#endif

                AddSupportPackage("(Experimental) Assembly Support", "Enables the import of assemblies for Easy Build System.\n" +
                    "Reduce compilation times and the complexity of dependencies between scripts.",
                    new List<RequiredPackage>() { }, () =>
                    {
                        string manifestPath = AssetDatabase.GetAssetPath(Resources.Load("Packages/Supports/manifest"));
                        AssetDatabase.ImportPackage(manifestPath.Replace("manifest.prefab", "Assembly Support.unitypackage"), true);

                        AssetDatabase.importPackageCompleted += (string packageName) =>
                        {
                            EnableIntegration("EBS_ASSEMBLY", () =>
                            {
                                Debug.Log("<b>Easy Build System</b> Assemblies has been successfully installed!");
                            });
                        };
                    }, assembly);

                GUILayout.EndVertical();

                GUILayout.Space(1f);

                GUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
            }
            else if (m_TabIndex == 1)
            {
                m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

                EditorGUIUtilityExtension.HelpBox("Each integrations listed here have been developed on specific versions.\n" +
                    "It is possible that the integration you wish to use has been updated, which could cause errors. Please contact support if needed.", MessageType.Warning);

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                GUILayout.Space(3f);

                AddIntegrationPackage("Rewired", "Guavaman Enterprises",
                    "https://assetstore.unity.com/packages/tools/utilities/rewired-21676", "EBS_REWIRED", "Rewired Integration", "1.1.49.1", null, null);

                AddIntegrationPackage("PlayMaker", "Hutong Games LLC",
                    "https://assetstore.unity.com/packages/tools/visual-scripting/playmaker-368", "EBS_PLAYMAKER", "PlayMaker Integration", "1.9.7", null, null);

                AddIntegrationPackage("Game Creator 2", "Catsoft Works",
                    "https://assetstore.unity.com/packages/tools/game-toolkits/game-creator-2-203069", "EBS_GAMECREATORV2", "Game Creator 2 Integration", "2.17.51", null, null);

                AddIntegrationPackage("Game Creator 2 - Inventory", "Catsoft Works",
                    "https://assetstore.unity.com/packages/tools/game-toolkits/game-creator-2-203069", "EBS_GAMECREATORV2_INVENTORY", "Game Creator 2 Inventory Integration", "2.8.19", null, null);

                AddIntegrationPackage("RPGBuilder", "Blink",
                    "https://assetstore.unity.com/packages/tools/game-toolkits/rpg-builder-177657", "EBS_RPG_BUILDER", "RPGBuilder Integration", "2.2", null, null);

                AddIntegrationPackage("uSurvival", "Vis2k",
                    "https://assetstore.unity.com/packages/templates/systems/usurvival-multiplayer-survival-95015", "EBS_USURVIVAL", "uSurvival Integration", "1.86", null, null);

                AddIntegrationPackage("Mirror", "Vis2k",
                    "https://assetstore.unity.com/packages/tools/network/mirror-129321", "EBS_MIRROR", "Mirror Integration", "96.0.1", null, null);

                AddIntegrationPackage("PUN V2", "Exit Games",
                    "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922", "EBS_PUNV2", "PUN V2 Integration", "2.49", null, null);

                AddIntegrationPackage("FishNet", "FirstGearGames",
                    "https://assetstore.unity.com/packages/tools/network/fish-net-networking-evolved-207815", "EBS_FISHNET", "FishNet Integration", "4.6.2R", null, null);

                GUILayout.EndVertical();

                GUILayout.Space(1f);

                GUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
            }

            EditorGUIUtilityExtension.EndVertical();

            EditorGUIUtilityExtension.EndVertical();

            Repaint();
        }

        static IEnumerator CurrentDownload;
        public static void ProcessDownload()
        {
            if (CurrentDownload != null)
            {
                CurrentDownload.MoveNext();
            }
        }

        void AddSupportPackage(string name, string description,
            List<RequiredPackage> dependencies, Action onEnable, bool isInstalled = false)
        {
            EditorGUIUtilityExtension.BeginHorizontal();

            GUILayout.BeginVertical();

            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();

            GUILayout.Label(name, EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            bool result = true;

            if (!isInstalled)
            {
                for (int i = 0; i < dependencies.Count; i++)
                {
                    if (!HasPackage(dependencies[i].Name))
                    {
                        result = false;
                    }
                }
            }

            GUI.enabled = result && !isInstalled;

            if (isInstalled)
            {
                GUI.color = Color.green;
                GUILayout.Button("Installed!", GUILayout.Width(120));
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button("Install Support...", GUILayout.Width(120)))
                {
                    if (onEnable != null)
                    {
                        onEnable.Invoke();
                    }
                }
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.Label(description, EditorStyles.label);

            GUILayout.Space(3f);

            GUILayout.BeginVertical();

            for (int i = 0; i < dependencies.Count; i++)
            {
                if (HasPackage(dependencies[i].Name))
                {
                    GUI.color = Color.green;
                    GUILayout.Label("[Installed] Package Name : " + dependencies[i].DisplayName, EditorStyles.miniLabel);
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("[Required] Package Name : " + dependencies[i].DisplayName, EditorStyles.miniLabel);
                    GUI.color = Color.white;
                }
            }

            GUILayout.Space(5f);

            GUILayout.EndVertical();

            GUILayout.Space(5f);

            GUILayout.EndVertical();

            EditorGUIUtilityExtension.EndHorizontal();
        }

        void AddIntegrationPackage(string name, string author, string link, string defName, string packageName, string version, Action onEnable, Action onDisable)
        {
            EditorGUIUtilityExtension.BeginHorizontal();

            GUILayout.BeginVertical();

            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();

            GUILayout.Label(name, EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (!HasSymbol(defName))
            {
                GUI.enabled = !EditorApplication.isCompiling;
                if (GUILayout.Button("Install Integration...", GUILayout.Width(140)))
                {
                    EnableIntegration(defName, () =>
                    {
                        EditorPrefs.SetString("EBS_LAST_INTEGRATION", packageName);
                    });
                }
                GUI.enabled = true;
            }
            else
            {
                GUI.enabled = !EditorApplication.isCompiling;
                if (GUILayout.Button("Disable Integration...", GUILayout.Width(140)))
                {
                    DisableIntegration(defName, onDisable);
                    EditorPrefs.SetString("EBS_LAST_INTEGRATION", string.Empty);
                }
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();

            GUILayout.Label("Author : " + author + " | Version : " + version, EditorStyles.label);

            GUILayout.Space(3f);

            GUILayout.BeginVertical();

            EditorGUIUtilityExtension.LinkLabel(link, link);

            GUILayout.Space(5f);

            GUILayout.EndVertical();

            GUILayout.Space(5f);

            GUILayout.EndVertical();

            EditorGUIUtilityExtension.EndHorizontal();
        }

        static bool m_ImportedPackageXRSupport;
        static bool m_CancelImport;
        public static IEnumerator InstallSupports(bool installXR)
        {
            string manifestPath = "";

            EditorApplication.LockReloadAssemblies();

            EditorPrefs.SetString("EBS_LAST_SCENE", EditorSceneManager.GetActiveScene().path);
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

            if (installXR)
            {
                manifestPath = AssetDatabase.GetAssetPath(Resources.Load("Packages/Supports/manifest"));
                AssetDatabase.ImportPackage(manifestPath.Replace("manifest.prefab", "XR Interaction Toolkit Support.unitypackage"), true);

                AssetDatabase.importPackageCompleted += (string packageName) => { m_ImportedPackageXRSupport = true; };
                AssetDatabase.importPackageCancelled += (string packageName) => { m_CancelImport = true; };

                while (!m_ImportedPackageXRSupport)
                {
                    if (m_CancelImport)
                    {
                        LoadLastScene();
                        m_CancelImport = false;
                        yield break;
                    }

                    yield return new WaitForEndOfFrame();
                }

                EnableIntegration("EBS_XR", () =>
                {
                    Debug.Log("<b>Easy Build System</b> XR Interaction Toolkit support has been successfully installed!");
                });
            }

            EditorApplication.UnlockReloadAssemblies();

            yield break;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptsReloaded()
        {
            EditorApplication.delayCall += () =>
            {
                m_InstalledPackages = GetInstalledPackages();

                if (EditorPrefs.GetString("EBS_LAST_SCENE") != string.Empty)
                {
                    LoadLastScene();
                }

                if (EditorPrefs.GetString("EBS_LAST_INTEGRATION") != string.Empty)
                {
                    string relativePath = GetRelativePath(EditorPrefs.GetString("EBS_LAST_INTEGRATION"));
                    AssetDatabase.ImportPackage(relativePath, true);
                    EditorPrefs.SetString("EBS_LAST_INTEGRATION", string.Empty);
                }
            };
        }

        string GetRenderPipeline()
        {
            if (GraphicsSettings.currentRenderPipeline)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                {
                    return "High Definition";
                }
                else
                {
                    return "Universal";
                }
            }
            else
            {
                return "Built-In";
            }
        }

        static List<PackageInfo> GetInstalledPackages()
        {
            List<PackageInfo> installedPackages = new List<PackageInfo>();

            ListRequest request = Client.List(true);

            while (!request.IsCompleted) { }

            if (request.Status == StatusCode.Success)
            {
                foreach (var package in request.Result)
                {
                    installedPackages.Add(package);
                }
            }
            else
            {
                Debug.LogError("Failed to get package list!");
            }

            return installedPackages;
        }

        bool HasPackage(string packageName)
        {
            foreach (PackageInfo package in m_InstalledPackages)
            {
                if (package.name.ToLower() == packageName.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        static string GetRelativePath(string packageName)
        {
            string[] allPaths = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);

            for (int i = 0; i < allPaths.Length; i++)
            {
                if (allPaths[i].Contains("Easy Build System"))
                {
                    if (allPaths[i].Contains(packageName + ".unitypackage"))
                    {
                        return allPaths[i];
                    }
                }
            }

            return string.Empty;
        }

        static bool HasSymbol(string name)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains(name);
        }

        static void LoadLastScene()
        {
            if (Application.isPlaying)
            {
                EditorPrefs.SetString("EBS_LAST_SCENE", string.Empty);
                return;
            }

            if (EditorPrefs.GetString("EBS_LAST_SCENE") != string.Empty)
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(EditorPrefs.GetString("EBS_LAST_SCENE"));
            }

            EditorPrefs.SetString("EBS_LAST_SCENE", string.Empty);
        }

        public static void DisableIntegration(string name, Action onDisable)
        {
            if (HasSymbol(name) == false)
            {
                return;
            }

            if (onDisable != null)
            {
                onDisable.Invoke();
            }

            m_Targets = new List<BuildTargetGroup>
            {
                BuildTargetGroup.iOS,

                BuildTargetGroup.WebGL,

                BuildTargetGroup.Standalone,

                BuildTargetGroup.Android
            };

            foreach (BuildTargetGroup target in m_Targets)
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                string[] splitArray = symbols.Split(';');

                List<string> array = new List<string>(splitArray);

                array.Remove(name);

                if (target != BuildTargetGroup.Unknown)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", array.ToArray()));
                }
            }
        }

        public static void EnableIntegration(string name, Action onEnable)
        {
            if (HasSymbol(name))
            {
                return;
            }

            m_Targets = new List<BuildTargetGroup>
            {
                BuildTargetGroup.iOS,
                BuildTargetGroup.WebGL,
                BuildTargetGroup.Standalone,
                BuildTargetGroup.Android,
            };

            foreach (BuildTargetGroup target in m_Targets)
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                string[] splitArray = symbols.Split(';');

                List<string> array = new List<string>(splitArray)
                {
                    name
                };

                if (target != BuildTargetGroup.Unknown)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", array.ToArray()));
                }
            }

            if (onEnable != null)
            {
                m_Integrations.Add(onEnable.Method.Name);
                onEnable.Invoke();
            }
        }
    }
}