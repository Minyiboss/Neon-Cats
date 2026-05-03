using System;
using UnityEngine;

/// <summary>
/// Handles saving/loading persistent game state to PlayerPrefs (as JSON).
/// This is the single source of truth for long-term fields like milestones and prestige.
/// </summary>
public class SaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] PrestigeManager prestigeManager;

    [Header("Save settings")]
    [Tooltip("Key used in PlayerPrefs for the JSON save payload.")]
    [SerializeField] string saveKey = "NeonPaws_Save_v1";
    public string SaveKey => saveKey;

    [Tooltip("If enabled, loads immediately on Awake (recommended).")]
    [SerializeField] bool loadOnAwake = true;

    [Tooltip("Autosave interval in seconds. Set to 0 to disable autosave.")]
    [SerializeField] float autosaveIntervalSeconds = 15f;

    [Tooltip("If enabled, clears saved state on Awake (testing only).")]
    [SerializeField] bool clearSaveOnAwake;

    public SaveData CurrentSave { get; private set; }
    public bool IsLoaded { get; private set; }
    public bool LoadOnAwakeEnabled => loadOnAwake;

    float _nextAutosaveTime;

    void EnsureUpgradeLevelCapacity(int minSize)
    {
        if (CurrentSave == null)
            return;
        if (minSize < 1)
            minSize = 1;
        if (CurrentSave.upgradeLevels == null)
            CurrentSave.upgradeLevels = new int[minSize];
        else if (CurrentSave.upgradeLevels.Length < minSize)
            Array.Resize(ref CurrentSave.upgradeLevels, minSize);
    }

    void EnsurePrestigeUpgradeLevelCapacity(int minSize)
    {
        if (CurrentSave == null)
            return;
        if (minSize < 1)
            minSize = 1;
        if (CurrentSave.prestigeUpgradeLevels == null)
            CurrentSave.prestigeUpgradeLevels = new int[minSize];
        else if (CurrentSave.prestigeUpgradeLevels.Length < minSize)
            Array.Resize(ref CurrentSave.prestigeUpgradeLevels, minSize);
    }

    public int GetUpgradeLevel(int upgradeLevelIndex)
    {
        if (CurrentSave == null)
            return 0;
        if (upgradeLevelIndex < 0)
            upgradeLevelIndex = 0;
        EnsureUpgradeLevelCapacity(upgradeLevelIndex + 1);
        return Mathf.Max(0, CurrentSave.upgradeLevels[upgradeLevelIndex]);
    }

    public void SetUpgradeLevel(int upgradeLevelIndex, int levelValue)
    {
        if (CurrentSave == null)
            return;
        if (upgradeLevelIndex < 0)
            upgradeLevelIndex = 0;
        EnsureUpgradeLevelCapacity(upgradeLevelIndex + 1);
        CurrentSave.upgradeLevels[upgradeLevelIndex] = Mathf.Max(0, levelValue);
    }

    public int GetPrestigeUpgradeLevel(int prestigeUpgradeLevelIndex)
    {
        if (CurrentSave == null)
            return 0;
        if (prestigeUpgradeLevelIndex < 0)
            prestigeUpgradeLevelIndex = 0;
        EnsurePrestigeUpgradeLevelCapacity(prestigeUpgradeLevelIndex + 1);
        return Mathf.Max(0, CurrentSave.prestigeUpgradeLevels[prestigeUpgradeLevelIndex]);
    }

    public void SetPrestigeUpgradeLevel(int prestigeUpgradeLevelIndex, int levelValue)
    {
        if (CurrentSave == null)
            return;
        if (prestigeUpgradeLevelIndex < 0)
            prestigeUpgradeLevelIndex = 0;
        EnsurePrestigeUpgradeLevelCapacity(prestigeUpgradeLevelIndex + 1);
        CurrentSave.prestigeUpgradeLevels[prestigeUpgradeLevelIndex] = Mathf.Max(0, levelValue);
    }

    public bool ApplyPrestigeReset(int prestigeGain)
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
            return false;

        int gainedPoints = Mathf.Max(0, prestigeGain);
        if (CurrentSave == null)
        {
            CurrentSave = SaveData.FromGame(gameManager, milestonesMask: 0, prestigePoints: 0, prestigeLevel: 0, totalRuns: 0);
            CurrentSave.version = 0;
        }

        int keptMilestonesMask = CurrentSave.milestonesMask;
        int newPrestigePoints = Mathf.Max(0, CurrentSave.prestigePoints + gainedPoints);
        int keptTotalRuns = Mathf.Max(0, CurrentSave.totalRuns);
        int upgradeSlots = CurrentSave.upgradeLevels != null ? CurrentSave.upgradeLevels.Length : 1;
        int[] keptPrestigeUpgradeLevels = CurrentSave.prestigeUpgradeLevels ?? Array.Empty<int>();

        // Reset run values.
        gameManager.currentRunCurrency = 0f;
        gameManager.currency = 0f;
        gameManager.currencyPerClick = 1;
        gameManager.totalManualClicks = 0;
        gameManager.incomePerSecond = 0;
        gameManager.autoClickerCount = 0;
        gameManager.incomeMultiplier = 1f;
        gameManager.normalUpgradeCostMultiplier = 1f;

        ApplyPrestigeUpgradeBonusesFromSave();

        // Keep milestones and add prestige gain; reset everything else per requested rules.
        CurrentSave = SaveData.FromGame(
            gameManager,
            milestonesMask: keptMilestonesMask,
            prestigePoints: newPrestigePoints,
            prestigeLevel: 0,
            totalRuns: keptTotalRuns + 1,
            upgradeLevels: new int[Mathf.Max(1, upgradeSlots)],
            prestigeUpgradeLevels: keptPrestigeUpgradeLevels);
        CurrentSave.version = 0;
        IsLoaded = true;
        return true;
    }

    /// <summary>
    /// Lets each <see cref="PrestigeUpgrade"/> read its own index from save and reapply bonuses (after run baseline reset).
    /// </summary>
    public void ApplyPrestigeUpgradeBonusesFromSave()
    {
        foreach (var pu in FindObjectsOfType<PrestigeUpgrade>(true))
        {
            if (pu != null)
                pu.ApplySavedBonusesFromSave();
        }
    }

    void Awake()
    {
        // Ensure the game manager is assigned
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        if (prestigeManager == null)
            prestigeManager = FindObjectOfType<PrestigeManager>();

        if (clearSaveOnAwake){
            DeleteSave();
            IsLoaded = false;
        }else if (loadOnAwake)
            Load();
        else
            IsLoaded = false;
    }

    void Update()
    {
        // If the autosave interval is greater than 0 and the next autosave time is greater than the current time, save
        if (autosaveIntervalSeconds > 0f && Time.unscaledTime >= _nextAutosaveTime)
        {
            Save();
            _nextAutosaveTime = Time.unscaledTime + autosaveIntervalSeconds;
        }
    }

    public void EnsureLoaded()
    {
        if (IsLoaded || !loadOnAwake)
            return;

        Load();
    }

    public void Load()
    {
        if (gameManager == null)
        {
            Debug.LogError("SaveManager could not find GameManager.");
            return;
        }

        IsLoaded = false;

        // Get the save from the player prefs
        string json = PlayerPrefs.GetString(saveKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            // No existing save: initialize from current game state.
            CurrentSave = SaveData.FromGame(gameManager, milestonesMask: 0, prestigePoints: 0, prestigeLevel: 0, totalRuns: 0);
            CurrentSave.version = 0;
            IsLoaded = true;
            _nextAutosaveTime = Time.unscaledTime + autosaveIntervalSeconds;
            EnsureUpgradeLevelCapacity(1);
            return;
        }

        try
        {
            // Parse the save from the json
            CurrentSave = JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load save JSON: " + e.Message);
            CurrentSave = SaveData.FromGame(gameManager, milestonesMask: 0, prestigePoints: 0, prestigeLevel: 0, totalRuns: 0);
        }

        if (CurrentSave == null)
        {
            CurrentSave = SaveData.FromGame(gameManager, milestonesMask: 0, prestigePoints: 0, prestigeLevel: 0, totalRuns: 0);
        }

        // Apply economy values to the live GameManager.
        CurrentSave.ApplyToGame(gameManager);

        IsLoaded = true;
        // Set the next autosave time
        _nextAutosaveTime = Time.unscaledTime + autosaveIntervalSeconds;
        EnsureUpgradeLevelCapacity(1);
    }

    public void Save()
    {
        if (gameManager == null)
            return;
        if (prestigeManager == null)
            prestigeManager = FindObjectOfType<PrestigeManager>();
        if (CurrentSave == null)
        {
            CurrentSave = SaveData.FromGame(gameManager, milestonesMask: 0, prestigePoints: 0, prestigeLevel: 0, totalRuns: 0);
            CurrentSave.version = 0;
        }

        EnsureUpgradeLevelCapacity(1);

        int prestigePointsToSave = CurrentSave.prestigePoints;
        if (prestigeManager != null)
            prestigePointsToSave = Mathf.Max(0, prestigeManager.PrestigePoints);

        // Refresh run economy values from the current GameManager,
        // while keeping sticky milestones + prestige fields from the existing save object.
        var save = SaveData.FromGame(
            gameManager,
            milestonesMask: CurrentSave.milestonesMask,
            prestigePoints: prestigePointsToSave,
            prestigeLevel: CurrentSave.prestigeLevel,
            totalRuns: CurrentSave.totalRuns,
            upgradeLevels: CurrentSave.upgradeLevels,
            prestigeUpgradeLevels: CurrentSave.prestigeUpgradeLevels);

        save.version = CurrentSave.version;
        CurrentSave = save;

        string json = JsonUtility.ToJson(CurrentSave);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    public void SaveNow()
    {
        Save();
    }

    public void DeleteSave()
    {
        Debug.Log("Deleting save: " + saveKey);
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
    }
}

