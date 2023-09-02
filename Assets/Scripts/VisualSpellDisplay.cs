using UnityEngine.EventSystems;
using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class VisualSpellDisplay : SpellDisplay
{
    [Header("Visual Spell Display")]
    [SerializeField] private float delayBeforeSpawningToolTips = 0.5f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private CanvasGroup mainCV;
    [SerializeField] private GameObject currentOutOfCombatCooldown;
    [SerializeField] private TextMeshProUGUI currentOutOfCombatCooldownText;
    [SerializeField] private TextMeshProUGUI outOfCombatCooldownText;

    [Header("Spell Dependant")]
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image setColorOf;
    [SerializeField] private TextMeshProUGUI[] coloredTexts;
    private bool isForUpgrade;

    private IEnumerator SpawnToolTipsAfterDelay()
    {
        float t = 0;
        while (t < delayBeforeSpawningToolTips)
        {
            if (!isMouseOver)
            {
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        SpawnToolTipFunc();
    }

    protected override void SpawnToolTipFunc()
    {
        if (isForUpgrade)
        {
            Spell upgradedSpell = Spell.GetSpellOfType(Spell.Label);

            if (upgradedSpell.CanUpgrade)
            {
                upgradedSpell.Upgrade();
                spawnedToolTip = UIManager._Instance.SpawnComparisonToolTips(
                    new ToolTippableComparisonData[]
                        {
                        new ToolTippableComparisonData("Current: ", Spell),
                        new ToolTippableComparisonData("Upgraded: ", upgradedSpell)
                        },
                    transform);
            }
            else
            {
                base.SpawnToolTipFunc();
            }
        }
        else
        {
            base.SpawnToolTipFunc();
        }
    }

    public override void CallSpawnToolTip()
    {
        StartCoroutine(SpawnToolTipsAfterDelay());
    }

    public void SetAvailableState(int currentOOCCD)
    {
        // 
        if (currentOOCCD > 0)
        {
            currentOutOfCombatCooldownText.text = "Unavailable\nComplete " + currentOOCCD.ToString() + " Room" + (currentOOCCD > 1 ? "s" : "");
            currentOutOfCombatCooldown.SetActive(true);
            mainCV.interactable = false;
            mainCV.blocksRaycasts = false;
        }
        else
        {
            currentOutOfCombatCooldown.SetActive(false);
            mainCV.interactable = true;
            mainCV.blocksRaycasts = true;
        }
    }

    public void SetIsForUpgrade(bool b)
    {
        isForUpgrade = b;
    }

    public override void SetSpell(Spell spell)
    {
        base.SetSpell(spell);

        SetAvailableState(0);
        outOfCombatCooldownText.text = UIManager._Instance.HighlightKeywords("Out of Combat Cooldown: " + spell.OutOfCombatCooldown + " Room" + (spell.OutOfCombatCooldown > 1 ? "s" : ""));

        // Set Rarity Image Color
        rarityImage.color = UIManager._Instance.GetRarityColor(spell.Rarity);
        // Set Card Color
        SpellColorInfo colorInfo = UIManager._Instance.GetSpellColor(spell.Color);
        setColorOf.color = colorInfo.Color;
        foreach (TextMeshProUGUI text in coloredTexts)
        {
            text.color = colorInfo.TextColor;
        }

        // This must be done after Setting the Color of the rest of the Text
        // Debug.Log(spell.Name + " - Has Been Upgraded: " + spell.HasBeenUpgraded);
        if (spell.HasBeenUpgraded)
        {
            nameText.color = UIManager._Instance.GetEffectTextColor("UpgradedSpell");
        }

        switch (spell)
        {
            case ActiveSpell activeSpell:
                typeText.text = "Active";

                // Show Detail of Spells
                string[] tokens = spell.GetToolTipText().Split(',');
                string r = "";
                for (int i = 0; i < tokens.Length; i++)
                {
                    r += tokens[i];
                    if (i < tokens.Length - 1)
                    {
                        r += "\n";
                    }
                }
                text.text = r;

                break;
            case PassiveSpell passiveSpell:
                typeText.text = "Passive";
                text.text = spell.GetToolTipText();
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public CanvasGroup GetCanvasGroup()
    {
        return mainCV;
    }
}
