using UnityEngine;

/// <summary>
/// Prestige upgrade that adds flat click power per purchase and reapplies <c>level × bonus</c> after prestige reset.
/// </summary>
public class PrestigePointsUpgrade : PrestigeUpgrade
{
    public int pointsIncreaseModifier = 1;
    [SerializeField] prestigeButton targetPrestigeButton;

    prestigeButton ResolvePrestigeButton() => targetPrestigeButton;

    protected override void ApplyPurchaseEffect()
    {
        prestigeButton prestigeButton = ResolvePrestigeButton();
        if (prestigeButton == null)
            return;
        prestigeButton.prestigePointsModifier += pointsIncreaseModifier;
    }

    protected override void ApplySavedBonusesForLevel(int level)
    {
        prestigeButton prestigeButton = ResolvePrestigeButton();
        if (prestigeButton == null)
            return;
        prestigeButton.prestigePointsModifier += level * pointsIncreaseModifier;
    }
}
