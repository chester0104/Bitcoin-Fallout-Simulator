using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeMenuBehavior : MonoBehaviour
{
    [System.Serializable]
    public class Upgrade
    {
        public string upgradeName;
        public Sprite icon;
        public UnityEngine.Events.UnityEvent onUpgrade;
    }
    
    public GameObject upgradeMenuPanel;
    public Button slot1;
    public Button slot2;
    public Button slot3;

    public Upgrade[] allUpgrades;

    void Start()
    {
        upgradeMenuPanel.SetActive(false);
    }

    public void UpgradePlayer()
    {
        AssignUpgrades();
        ShowUpgradeMenu();
    }

    public void ShowUpgradeMenu()
    {
        upgradeMenuPanel.SetActive(true);
        AssignUpgrades();
        Time.timeScale = 0f;
        PauseMenuBehavior.isGamePaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideUpgradeMenu()
    {
        upgradeMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        PauseMenuBehavior.isGamePaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void AssignUpgrades()
    {
        Upgrade[] shuffled = (Upgrade[])allUpgrades.Clone();
        Shuffle(shuffled);

        if (shuffled.Length > 0)
            SetSlot(slot1, shuffled[0]);
        if (shuffled.Length > 1)
            SetSlot(slot2, shuffled[1]);
        if (shuffled.Length > 2)
            SetSlot(slot3, shuffled[2]);
    }

    void SetSlot(Button slot, Upgrade upgrade)
    {
        TMP_Text slotText = slot.GetComponentInChildren<TMP_Text>();
        if (slotText != null)
        {
            slotText.text = upgrade.upgradeName;
        }

        if (upgrade.icon != null)
            slot.image.sprite = upgrade.icon;

        slot.onClick.RemoveAllListeners();
        slot.onClick.AddListener(() =>
        {
            upgrade.onUpgrade.Invoke();
            HideUpgradeMenu();
        });
    }

    void Shuffle(Upgrade[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randIndex = Random.Range(i, array.Length);
            (array[i], array[randIndex]) = (array[randIndex], array[i]);
        }
    }
}