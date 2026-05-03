using UnityEngine;

/// <summary>
/// Prestige upgrade that adds flat click power per purchase and reapplies <c>level × bonus</c> after prestige reset.
/// </summary>
public class PrestigeClickPowerUpgrade : PrestigeUpgrade
{
    public int clickPowerIncrease = 1;

    protected override void ApplyPurchaseEffect()
    {
        if (gameManager == null)
            return;
        gameManager.currencyPerClick += clickPowerIncrease;
    }

    protected override void ApplySavedBonusesForLevel(int level)
    {
        if (gameManager == null)
            return;
        gameManager.currencyPerClick += level * clickPowerIncrease;
    }
}
