using UnityEngine;

/// <summary>
/// Permanent passive income/sec; reapplied after prestige reset. Empty purchase effect = next prestige before it applies unless you add logic there.
/// </summary>
public class PrestigeStartingIncome : PrestigeUpgrade
{
    public int startingIncomeIncrease = 500;

    protected override void ApplyPurchaseEffect()
    {
        // Optional: gameManager.incomePerSecond += startingIncomeIncrease;
        gameManager.currency += startingIncomeIncrease;
    }

    protected override void ApplySavedBonusesForLevel(int level)
    {
        if (gameManager == null)
            return;
        gameManager.currency += level * startingIncomeIncrease;
    }
}
