using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerXP : MonoBehaviour
{
    public UpgradeMenuBehavior upgradeMenuBehavior;
    public Slider XPSlider;
    public TMP_Text XPLevel;
    public static int maxXP = 100;
    public static int currentXP = 0;
    public static int currentLevel = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void TakeXP(int XPAmount)
    {
        int totalXP = currentXP + XPAmount;

        // handle multiple level-ups if XP exceeds maxXP
        while (totalXP >= maxXP)
        {
            if (upgradeMenuBehavior)
                upgradeMenuBehavior.UpgradePlayer();
                
            currentLevel++;
            totalXP -= maxXP;
        }

        currentXP = totalXP;

        UpdateXPLevel();
        UpdateXPSlider();
    }

    void UpdateXPSlider()
    {
        if (XPSlider)
            XPSlider.value = currentXP;
    }

    void UpdateXPLevel()
    {
        if (XPLevel)
            XPLevel.text = currentLevel.ToString();
    }
}
