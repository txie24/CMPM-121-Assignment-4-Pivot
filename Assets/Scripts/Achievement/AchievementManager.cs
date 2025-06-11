using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    public AchievementPopup popupPrefab;
    public Transform popupParent;

    private bool monsterHunterUnlocked = false;
    private bool suckAtAimingUnlocked = false;
    private bool stopSpammingUnlocked = false;
    private bool harryPotterUnlocked = false;

    private int missCount = 0;

    void Awake()
    {
        Instance = this;
        SpellCaster.OnSpellCast += CheckEnemyKill;
    }

    void Update()
    {
        // Press F9 to print achievement debug info
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[Achievement Debug]");
            Debug.Log($"  Monster Hunter: {(monsterHunterUnlocked ? "✔" : "❌")}");
            Debug.Log($"  Suck At Aiming: {(suckAtAimingUnlocked ? "✔" : "❌")} ({missCount}/20)");
            Debug.Log($"  Stop Spamming: {(stopSpammingUnlocked ? "✔" : "❌")} ({missCount}/30)");
            Debug.Log($"  Harry Potter: {(harryPotterUnlocked ? "✔" : "❌")}");
        }
    }

    public void CheckEnemyKill()
    {
        if (!monsterHunterUnlocked && GameManager.Instance.totalEnemiesKilled >= 20)
        {
            monsterHunterUnlocked = true;
            Debug.Log("[Achievement] Unlocked: Monster Hunter");
            Show("Monster Hunter", "Kill 20 enemies");
        }
    }

    public void RecordMiss()
    {
        missCount++;
        if (!suckAtAimingUnlocked && missCount == 20)
        {
            suckAtAimingUnlocked = true;
            Debug.Log("[Achievement] Unlocked: Wow! You Really Suck At Aiming");
            Show("Wow! You Really Suck At Aiming", "Miss 20 times");
        }
        if (!stopSpammingUnlocked && missCount == 30)
        {
            stopSpammingUnlocked = true;
            Debug.Log("[Achievement] Unlocked: Stop Spamming");
            Show("Stop Spamming", "Miss 30 times");
        }
    }

    public void CheckHarryPotter()
    {
        var spellCount = GameManager.Instance.player.GetComponent<SpellCaster>().spells.FindAll(s => s != null).Count;
        var relicCount = RewardScreenManager.Instance.GetOwnedRelics().Count;

        if (!harryPotterUnlocked && spellCount == 1 && relicCount == 3 && GameManager.Instance.playerWon)
        {
            harryPotterUnlocked = true;
            Debug.Log("[Achievement] Unlocked: Harry Potter");
            Show("Harry Potter", "Use only one spell and 3 relics to complete the game");
        }
    }

    private void Show(string title, string description)
    {
        var popup = Instantiate(popupPrefab.gameObject, popupParent);  // ← clone the hidden popup
        popup.SetActive(true); // show it

        var comp = popup.GetComponent<AchievementPopup>();
        comp.Show(title, description);
    }

}
