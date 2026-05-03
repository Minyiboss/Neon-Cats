using UnityEngine;
using UnityEngine.SceneManagement;

public class PrestigeManager : MonoBehaviour
{
    [SerializeField] SaveManager saveManager;

    public int PrestigePoints { get; private set; }

    void Awake()
    {
        if (saveManager == null)
            saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
            saveManager.EnsureLoaded();

        SyncFromSave();
    }

    void SyncFromSave()
    {
        if (saveManager == null || saveManager.CurrentSave == null)
        {
            PrestigePoints = 0;
            return;
        }

        PrestigePoints = Mathf.Max(0, saveManager.CurrentSave.prestigePoints);
    }

    bool EnsureSaveReady()
    {
        if (saveManager == null)
            saveManager = FindObjectOfType<SaveManager>();
        if (saveManager == null)
            return false;

        if (saveManager.CurrentSave == null)
            saveManager.SaveNow();

        return saveManager.CurrentSave != null;
    }

    public void addPrestige(float prestigeGain)
    {
        int gainedPoints = Mathf.Max(0, Mathf.FloorToInt(prestigeGain));
        if (gainedPoints <= 0)
            return;
        if (!EnsureSaveReady())
            return;

        if (!saveManager.ApplyPrestigeReset(gainedPoints))
            return;

        SyncFromSave();
        saveManager.SaveNow();

        SceneManager.LoadScene("GameScene");
    }

    public bool TrySpendPrestigePoints(int amount)
    {
        int spendAmount = Mathf.Max(0, amount);
        if (spendAmount <= 0)
            return true;
        if (!EnsureSaveReady())
            return false;
        if (PrestigePoints < spendAmount)
            return false;

        PrestigePoints -= spendAmount;
        saveManager.CurrentSave.prestigePoints = PrestigePoints;
        saveManager.SaveNow();
        return true;
    }
}
