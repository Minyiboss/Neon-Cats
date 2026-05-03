using UnityEngine;

/// <summary>
/// Prestige upgrade that adds flat click power per purchase and reapplies <c>level × bonus</c> after prestige reset.
/// </summary>
public class PrestigePassiveUpgrade : PrestigeUpgrade
{
    public int passiveIncomeIncrease = 5;

    protected override void ApplyPurchaseEffect()
    {
        if (gameManager == null)
            return;
        gameManager.incomePerSecond += passiveIncomeIncrease;
    }

    protected override void ApplySavedBonusesForLevel(int level)
    {
        if (gameManager == null)
            return;
        gameManager.incomePerSecond += level * passiveIncomeIncrease;
    }
}
