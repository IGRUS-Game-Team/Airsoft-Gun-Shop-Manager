/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaver.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using EasyBuildSystem.Features.Runtime.Buildings.Part;

using Debug = UnityEngine.Debug;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver
{
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-manager/building-saver")]
    public class BuildingSaver : MonoBehaviour
    {
        #region Fields

        public static BuildingSaver Instance;

        [SerializeField] bool m_LoadAsync;
        [SerializeField] float m_LoadAsyncLimit = 0.0167f;

        [SerializeField] bool m_UseAutoSaver;
        [SerializeField] float m_AutoSaverInterval = 60f;

        bool m_LoadBuildingAtStart = true;
        public bool LoadBuildingAtStart { get { return m_LoadBuildingAtStart; } set { m_LoadBuildingAtStart = value; } }

        bool m_SaveBuildingAtExit = true;
        public bool SaveBuildingAtExit { get { return m_SaveBuildingAtExit; } set { m_SaveBuildingAtExit = value; } }

        bool m_IsLoaded = false;
        public bool IsLoaded { get { return m_IsLoaded; } set { m_IsLoaded = value; } }

        float m_TimerAutoSave;

        bool m_IsSaving;
        bool m_IsLoading;

        [Serializable]
        public class SaveData
        {
            public List<BuildingPart.SaveSettings> Data = new List<BuildingPart.SaveSettings>();
        }

        public string CustomPath = string.Empty
            ;

        public string GetSavePath
        {
            get
            {
                return CustomPath == string.Empty ? Application.persistentDataPath + "/data_" + SceneManager.GetActiveScene().name.Replace(" ", "") + "_save.txt" : CustomPath;
            }
        }

        #region Events

        /// <summary>
        /// Event triggered when the saving process begins.
        /// </summary>
        [Serializable] public class StartSaveEvent : UnityEvent { }
        public StartSaveEvent OnStartSaveEvent = new StartSaveEvent();

        /// <summary>
        /// Event triggered when the saving process completes.
        /// </summary>
        [Serializable] public class EndingSaveEvent : UnityEvent<BuildingPart[]> { }
        public EndingSaveEvent OnEndingSaveEvent = new EndingSaveEvent();

        /// <summary>
        /// Event triggered when the loading process starts.
        /// </summary>
        [Serializable] public class StartLoadingEvent : UnityEvent { }
        public StartLoadingEvent OnStartLoadingEvent = new StartLoadingEvent();

        /// <summary>
        /// Event triggered when the loading process completes.
        /// </summary>
        [Serializable] public class EndingLoadingEvent : UnityEvent<BuildingPart[], long> { }
        public EndingLoadingEvent OnEndingLoadingEvent = new EndingLoadingEvent();

        #endregion

        #endregion

        #region Unity Methods

        public virtual void Awake()
        {
            Instance = this;
        }

        public virtual void Start()
        {
            if (m_UseAutoSaver)
            {
                m_TimerAutoSave = m_AutoSaverInterval;
            }

            if (m_LoadBuildingAtStart)
            {
                StartCoroutine(Load(GetSavePath));
            }
        }

        public virtual void Update()
        {
            if (m_UseAutoSaver)
            {
                if (m_TimerAutoSave <= 0)
                {
                    StartCoroutine(Save(GetSavePath));

                    m_TimerAutoSave = m_AutoSaverInterval;
                }
                else
                {
                    m_TimerAutoSave -= Time.deltaTime;
                }
            }
        }

        public virtual void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                StartCoroutine(Save(GetSavePath));
            }
        }

        public virtual void OnApplicationQuit()
        {
            if (m_SaveBuildingAtExit)
            {
                StartCoroutine(Save(GetSavePath));
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Forces the saving of Building Parts.
        /// </summary>
        /// <param name="path">The file path to save the Building Parts to (optional).</param>
        public void ForceSave(string path = null)
        {
            StartCoroutine(Save(path ?? GetSavePath));
        }

        /// <summary>
        /// Forces the loading of Building Parts.
        /// </summary>
        /// <param name="path">The file path to load the Building Parts from (optional).</param>
        public void ForceLoad(string path = null)
        {
            StartCoroutine(Load(path ?? GetSavePath));
        }

        /// <summary>
        /// Forces the saving of Building Parts.
        /// </summary>
        /// <param name="path">The file path to save the Building Parts to (optional).</param>
        public void ForceDelete(string path)
        {
            if (!File.Exists(path ?? GetSavePath))
            {
                return;
            }

            File.Delete(path ?? GetSavePath);
        }

        /// <summary>
        /// Saves all the Building Parts present in the current scene.
        /// </summary>
        /// <param name="path">The file path to save the Building Parts to.</param>
        IEnumerator Save(string path)
        {
            if (m_IsSaving)
            {
                yield break;
            }

            if (m_IsLoading)
            {
                yield break;
            }

            m_IsSaving = true;

            List<BuildingPart> savedBuildingParts = new List<BuildingPart>();

            OnStartSaveEvent.Invoke();

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (BuildingManager.Instance.RegisteredBuildingParts.Count > 0)
            {
                SaveData saveData = new SaveData
                {
                    Data = new List<BuildingPart.SaveSettings>()
                };

                BuildingPart[] registeredBuildingParts = BuildingManager.Instance.RegisteredBuildingParts.ToArray();

                for (int i = 0; i < registeredBuildingParts.Length; i++)
                {
                    if (registeredBuildingParts[i] != null)
                    {
                        if (registeredBuildingParts[i].State != BuildingPart.StateType.PREVIEW)
                        {
                            BuildingPart.SaveSettings saveSettings = registeredBuildingParts[i].GetSaveData();

                            if (saveSettings != null)
                            {
                                saveData.Data.Add(saveSettings);
                                savedBuildingParts.Add(registeredBuildingParts[i]);
                            }
                        }
                    }
                }

                File.AppendAllText(path, JsonUtility.ToJson(saveData));
            }

            if (savedBuildingParts.Count > 0)
            {
                Debug.Log("<b>Easy Build System</b> Saved " + savedBuildingParts.Count + " Building Parts.");
            }

            OnEndingSaveEvent.Invoke(savedBuildingParts.ToArray());

            m_IsSaving = false;
        }

        /// <summary>
        /// Loads all the Building Parts from a save file.
        /// </summary>
        /// <param name="path">The file path to load the Building Parts from.</param>
        IEnumerator Load(string path)
        {
            if (m_IsLoading)
            {
                yield break;
            }

            m_IsLoading = true;

            if (!File.Exists(path))
            {
                m_IsLoading = false;
                yield break;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            OnStartLoadingEvent.Invoke();

            SaveData saveData = null;

            try
            {
                string jsonData = File.ReadAllText(path);
                saveData = JsonUtility.FromJson<SaveData>(jsonData);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing data: " + ex.ToString());
            }

            if (saveData != null)
            {
                List<BuildingPart> loadedBuildingParts = new List<BuildingPart>();

                float startTime = Time.realtimeSinceStartup;

                for (int i = 0; i < saveData.Data.Count; i++)
                {
                    if (saveData.Data[i] != null)
                    {
                        BuildingPart buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(saveData.Data[i].Identifier);

                        if (buildingPart != null)
                        {
                            BuildingPart instancedBuildingPart =
                                BuildingManager.Instance.PlaceBuildingPart(buildingPart, saveData.Data[i].Position, saveData.Data[i].Rotation, saveData.Data[i].Scale);

                            instancedBuildingPart.Properties = saveData.Data[i].Properties;

                            loadedBuildingParts.Add(instancedBuildingPart);

                            if (m_LoadAsync)
                            {
                                if (Time.realtimeSinceStartup - startTime > 0.0167 / m_LoadAsyncLimit)
                                {
                                    yield return null;
                                    startTime = Time.realtimeSinceStartup;
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("<b>Easy Build System</b> The Building Part reference with the name: <b>" + saveData.Data[i].Name + "</b> does not exist in Building Manager.");
                        }
                    }
                }

                stopWatch.Stop();

                OnEndingLoadingEvent.Invoke(loadedBuildingParts.ToArray(), stopWatch.ElapsedMilliseconds);

                IsLoaded = true;

                if (loadedBuildingParts.Count > 0)
                {
                    Debug.Log("<b>Easy Build System</b> Loaded " + loadedBuildingParts.Count + " Building Parts in " + stopWatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds.");
                }
            }

            m_IsLoading = false;
        }

        #endregion
    }
}