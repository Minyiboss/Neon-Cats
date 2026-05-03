using System;
using UnityEngine;

/// <summary>
/// Serializable snapshot of the player's long-term progression.
/// This does not perform any saving by itself; it is a data container
/// that other systems (e.g. a SaveManager) can serialize to PlayerPrefs or disk.
/// </summary>
[Serializable]
public class SaveData
{
    public int version = 0;

    // Core run economy
    public float currentRunCurrency;
    public float currency;
    public int currencyPerClick;
    public int totalManualClicks;
    public int incomePerSecond;
    public int autoClickerCount;
    public float incomeMultiplier;
    public float normalUpgradeCostMultiplier;

    // Upgrade levels by global upgrade index (all upgrade types share this list).
    public int[] upgradeLevels;
    // Prestige upgrade levels by global prestige-upgrade index.
    public int[] prestigeUpgradeLevels;

    // Milestones bitmask
    public int milestonesMask;

    // Prestige (to be wired later)
    public int prestigePoints;
    public int prestigeLevel;
    public int totalRuns;

    // Create a SaveData snapshot from current game state.
    public static SaveData FromGame(
        GameManager gameManager,
        int milestonesMask = 0,
        int prestigePoints = 0,
        int prestigeLevel = 0,
        int totalRuns = 0,
        int[] upgradeLevels = null,
        int[] prestigeUpgradeLevels = null)
    {
        if (gameManager == null)
        {
            Debug.LogError("SaveData.FromGame called with null GameManager.");
            return new SaveData();
        }

        return new SaveData
        {
            version = 0,
            currentRunCurrency = gameManager.currentRunCurrency,
            currency = gameManager.currency,
            currencyPerClick = gameManager.currencyPerClick,
            totalManualClicks = gameManager.totalManualClicks,
            incomePerSecond = gameManager.incomePerSecond,
            autoClickerCount = gameManager.autoClickerCount,
            incomeMultiplier = gameManager.incomeMultiplier,
            normalUpgradeCostMultiplier = gameManager.normalUpgradeCostMultiplier,
            upgradeLevels = upgradeLevels ?? Array.Empty<int>(),
            prestigeUpgradeLevels = prestigeUpgradeLevels ?? Array.Empty<int>(),
            milestonesMask = milestonesMask,
            prestigePoints = prestigePoints,
            prestigeLevel = prestigeLevel,
            totalRuns = totalRuns
        };
    }

    //Populate an existing GameManager instance from this save.
    public void ApplyToGame(GameManager gameManager)
    {
        if (gameManager == null)
        {
            Debug.LogError("SaveData.ApplyToGame called with null GameManager.");
            return;
        }

        gameManager.currentRunCurrency = currentRunCurrency;
        gameManager.currency = currency;
        gameManager.currencyPerClick = currencyPerClick;
        gameManager.totalManualClicks = totalManualClicks;
        gameManager.incomePerSecond = incomePerSecond;
        gameManager.autoClickerCount = autoClickerCount;
        gameManager.incomeMultiplier = incomeMultiplier;
        gameManager.normalUpgradeCostMultiplier = normalUpgradeCostMultiplier <= 0f ? 1f : normalUpgradeCostMultiplier;
    }
}

