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
    [SerializeField] private TextMeshProUGUI castTypeText;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private GameObject cooldownContainer;
    [SerializeField] private Image cooldownDisplay;
    [SerializeField] private TextMeshProUGUI keyCodeText;
    [SerializeField] private Button button;

    [Header("Spell Dependant")]
    [SerializeField] private Image rarityImage;
    private bool isForUpgrade;

    private KeyCode keyCode;

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

        if (!isMouseOver)
        {
            yield break;
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
                upgradedSpell.Upgrade(Sign.Positive);
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

    public void SetIsForUpgrade(bool b)
    {
        isForUpgrade = b;
    }

    public override void SetSpellDisplayState(SpellDisplayState displayState)
    {
        if (currentSpellDisplayState == SpellDisplayState.InHand && displayState != SpellDisplayState.InHand)
        {
            RemoveOnClick(TryCast);
        }

        base.SetSpellDisplayState(displayState);

        if (displayState == SpellDisplayState.InHand)
        {
            AddOnClick(TryCast);
        }
    }

    public void SetKeyBinding(KeyCode keyCode)
    {
        this.keyCode = keyCode;
        keyCodeText.text = keyCode.ToString().Substring(keyCode.ToString().Length - 1, 1);
        keyCodeText.gameObject.SetActive(true);
    }

    public override void SetSpell(Spell spell)
    {
        base.SetSpell(spell);

        // Set Rarity Image Color
        rarityImage.color = UIManager._Instance.GetRarityColor(spell.Rarity);

        switch (spell)
        {
            case ActiveSpell activeSpell:
                castTypeText.text = "Active";

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
                castTypeText.text = "Passive";
                text.text = spell.GetToolTipText();
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    protected override void Update()
    {
        base.Update();

        //
        manaCostText.text = Spell.ManaCost.ToString();

        switch (currentSpellDisplayState)
        {
            case SpellDisplayState.InHand:
                InHandUpdate();
                break;
            case SpellDisplayState.ChoosingDiscard:
                InHandUpdate();
                break;
            case SpellDisplayState.ChoosingExhaust:
                InHandUpdate();
                break;
            default:
                NotInhandUpdate();
                break;
        }
    }

    public void TryCast()
    {
        if ((currentSpellDisplayState == SpellDisplayState.ChoosingExhaust || currentSpellDisplayState == SpellDisplayState.ChoosingDiscard))
        {
            CombatManager._Instance.ClickedSpellForAlterHandSequence(Spell);
        }
        else if (currentSpellDisplayState == SpellDisplayState.InHand)
        {
            // Only allow for spell casts while in combat
            if (CombatManager._Instance.CanCastSpells)
            {
                if (!Spell.CanCast) return;

                // Tick Cooldowns
                CombatManager._Instance.TickHandCooldowns(Spell);

                if (Spell.Type == SpellCastType.Active)
                {
                    // Debug.Log("Adding: " + spellToCast + " to Queue");
                    CombatManager._Instance.AddSpellToCastQueue((ActiveSpell)Spell);
                }
                else if (Spell.Type == SpellCastType.Passive)
                {
                    Debug.Log(Spell + " Passive");
                    Spell.Cast();
                }
            }
        }
    }

    private void InHandUpdate()
    {
        // Activate On KeyBindPressed
        if (Input.GetKeyDown(keyCode))
        {
            TryCast();
        }

        if (Spell.Type == SpellCastType.Active)
        {
            ActiveSpell activeSpell = (ActiveSpell)Spell;
            if (activeSpell.CurrentCooldown > 0)
            {
                cooldownContainer.SetActive(true);
                cooldownDisplay.fillAmount = ((float)activeSpell.CurrentCooldown / activeSpell.MaxCooldown);
                progressBar.fillAmount = 1 - ((float)activeSpell.CurrentCooldown / activeSpell.MaxCooldown);
                cooldownText.text = "Cooldown:\n" + Utils.RoundTo(activeSpell.CurrentCooldown, 0).ToString();
            }
            else
            {
                cooldownContainer.SetActive(false);
                progressBar.fillAmount = 1;
            }
        }

        manaCostText.text = Spell.ManaCost.ToString();
    }

    private void NotInhandUpdate()
    {
        // 
    }

    public CanvasGroup GetCanvasGroup()
    {
        return mainCV;
    }
}
