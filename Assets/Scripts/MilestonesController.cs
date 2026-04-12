using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Eight milestones: PlayerPrefs mask, row Image colors, optional bottom toast when something new unlocks.
/// Place the toast UI on the Canvas, anchored to the bottom (always-active parent), then assign references here.
/// </summary>
public class MilestonesController : MonoBehaviour
{
    const int MilestoneCount = 8;
    const string PlayerPrefsMaskKey = "NeonPaws_MilestoneMask_v2";

    static readonly string[] kToastTitles =
    {
        "Clicker 1",
        "Clicker 2",
        "Passive Purr",
        "Hired Help",
        "Purr Duplication",
        "Purr Points 1",
        "Purr Points 2",
        "Purr Points 3"
    };

    static int AllMilestoneBits => (1 << MilestoneCount) - 1;

    [SerializeField] GameManager gameManager;
    [SerializeField] Image[] milestoneBoxImages;
    [SerializeField] Color unlockedBoxColor = new Color(0.8f, 0.86f, 1f, 1f);
    [SerializeField] Color lockedBoxColor = new Color(0.72f, 0.74f, 0.78f, 1f);

    [Tooltip("Deletes saved milestone unlocks from PlayerPrefs when the scene loads. Use while testing; turn off for a real playthrough / build.")]
    [SerializeField] bool clearMilestoneSaveOnAwake;

    [Header("Bottom toast (optional)")]
    [Tooltip("Small panel at bottom of screen; inactive in scene until a milestone unlocks.")]
    [SerializeField] GameObject milestoneToastRoot;
    [SerializeField] TMP_Text milestoneToastText;
    [Tooltip("Optional fade on the same object as the root (or child).")]
    [SerializeField] CanvasGroup milestoneToastCanvasGroup;
    [SerializeField] float toastHoldSeconds = 2.2f;
    [SerializeField] float toastFadeInSeconds = 0.12f;
    [SerializeField] float toastFadeOutSeconds = 0.2f;

    int _savedMask;
    readonly Queue<int> _toastQueue = new Queue<int>();
    bool _toastRoutineRunning;
    bool _warnedToastHierarchy;

    void Awake()
    {
        // Ensure the game manager is assigned
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        // Clear the milestone save if the user wants to test
        if (clearMilestoneSaveOnAwake)
        {
            PlayerPrefs.DeleteKey(PlayerPrefsMaskKey);
            PlayerPrefs.Save();
        }

        // Get the saved milestone mask
        _savedMask = PlayerPrefs.GetInt(PlayerPrefsMaskKey, 0) & AllMilestoneBits;

        // Disable the toast root and canvas group if they are assigned
        if (milestoneToastRoot != null)
            milestoneToastRoot.SetActive(false);
        if (milestoneToastCanvasGroup != null)
            milestoneToastCanvasGroup.alpha = 0f;
    }

    void Update()
    {
        // Ensure the game manager is assigned
        if (gameManager == null)
            return;

        // Compute the earned mask
        int earned = ComputeEarnedMask();
        int merged = (_savedMask | earned) & AllMilestoneBits;
        int newlyUnlocked = merged & ~_savedMask;

        // If the merged mask is different from the saved mask, update the saved mask
        if (merged != _savedMask)
        {
            _savedMask = merged;
            PlayerPrefs.SetInt(PlayerPrefsMaskKey, _savedMask);
            PlayerPrefs.Save();
        }

        // If there are newly unlocked bits, enqueue the toasts
        if (newlyUnlocked != 0)
            EnqueueToastsForNewBits(newlyUnlocked);

        // If the milestone box images are assigned, update the box colors
        if (milestoneBoxImages == null)
            return;

        for (int i = 0; i < milestoneBoxImages.Length && i < MilestoneCount; i++)
        {
            Image img = milestoneBoxImages[i];
            if (img == null)
                continue;
            img.color = (_savedMask & (1 << i)) != 0 ? unlockedBoxColor : lockedBoxColor;
        }
    }

    void EnqueueToastsForNewBits(int newBits)
    {
        if (milestoneToastRoot == null || milestoneToastText == null)
            return;

        // Enqueue the toasts for the newly unlocked bits
        for (int i = 0; i < MilestoneCount; i++)
        {
            if ((newBits & (1 << i)) != 0)
                _toastQueue.Enqueue(i);
        }

        // If there are toasts in the queue and the routine is not running, start the routine
        if (_toastQueue.Count > 0 && !_toastRoutineRunning)
            StartCoroutine(RunToastQueueRoutine());
    }

    IEnumerator RunToastQueueRoutine()
    {
        // Set the routine running
        _toastRoutineRunning = true;
        while (_toastQueue.Count > 0)
        {
            // Dequeue the next toast
            int index = _toastQueue.Dequeue();
            string title = index >= 0 && index < kToastTitles.Length ? kToastTitles[index] : $"Milestone {index + 1}";
            // Show the toast
            yield return ShowOneToast(title);
        }

        // Set the routine not running
        _toastRoutineRunning = false;
    }

    IEnumerator ShowOneToast(string title)
    {
        // Ensure the toast root and text are assigned
        if (milestoneToastRoot == null || milestoneToastText == null)
            yield break;

        // Set the toast text
        milestoneToastText.text = "Unlocked: " + title;
        milestoneToastRoot.SetActive(true);

        // Ensure the toast root is active
        if (!milestoneToastRoot.activeInHierarchy)
        {
            // Warn if the toast root is not active
            if (!_warnedToastHierarchy)
            {
                _warnedToastHierarchy = true;
                Debug.LogWarning(
                    "Milestone toast cannot show (a parent is inactive). Put the toast under the Canvas, not under a tab panel that is turned off.",
                    milestoneToastRoot);
            }

            // Set the toast root inactive
            milestoneToastRoot.SetActive(false);
            yield break;
        }

        // If the toast canvas group is assigned, fade it in
        if (milestoneToastCanvasGroup != null)
        {
            float inDur = Mathf.Max(0.01f, toastFadeInSeconds);
            // Fade the toast canvas group in
            for (float e = 0f; e < inDur; e += Time.unscaledDeltaTime)
            {
                milestoneToastCanvasGroup.alpha = Mathf.Clamp01(e / inDur);
                yield return null;
            }

            // Set the toast canvas group alpha to 1
            milestoneToastCanvasGroup.alpha = 1f;
        }

        // Wait for the toast hold time
        yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, toastHoldSeconds));

        // If the toast canvas group is assigned, fade it out
        if (milestoneToastCanvasGroup != null)
        {
            float outDur = Mathf.Max(0.01f, toastFadeOutSeconds);
            // Fade the toast canvas group out
            for (float e = 0f; e < outDur; e += Time.unscaledDeltaTime)
            {
                milestoneToastCanvasGroup.alpha = 1f - Mathf.Clamp01(e / outDur);
                yield return null;
            }

            // Set the toast canvas group alpha to 0
            milestoneToastCanvasGroup.alpha = 0f;
        }

        // Set the toast root inactive
        milestoneToastRoot.SetActive(false);
    }

    int ComputeEarnedMask()
    {
        // Compute the earned mask based off of milestones
        int m = 0;
        if (gameManager.totalManualClicks >= 10)
            m |= 1 << 0;
        if (gameManager.totalManualClicks >= 100)
            m |= 1 << 1;
        if (gameManager.incomePerSecond > 10)
            m |= 1 << 2;
        if (gameManager.autoClickerCount >= 1)
            m |= 1 << 3;
        if (gameManager.incomeMultiplier >= 2f)
            m |= 1 << 4;
        if (gameManager.currency > 100f)
            m |= 1 << 5;
        if (gameManager.currency > 1000f)
            m |= 1 << 6;
        if (gameManager.currency > 50000f)
            m |= 1 << 7;
        return m & AllMilestoneBits;
    }
}
