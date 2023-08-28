using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public enum IntentType
{
    SingleAttack,
    Ward,
    GainAffliction,
    ApplyAffliction,
    MultiAttack,
    Heal
}

public class IntentDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject[] attackIntentObjs;
    [SerializeField] private GameObject[] wardIntentObjs;
    [SerializeField] private TextMeshProUGUI attackIntentText;
    [SerializeField] private TextMeshProUGUI wardIntentText;
    [SerializeField] private CanvasGroup intentDisplayCV;

    private List<EnemyIntent> addedIntents = new List<EnemyIntent>();
    private GameObject spawnedToolTip;

    private EnemyAction currentEnemyAction;

    private void Update()
    {
        if (addedIntents.Count > 0)
        {
            // There are intents to display
            intentDisplayCV.alpha = 1;

            // Set intent Attack text
            if (currentEnemyAction.HasIntentType(IntentType.SingleAttack))
            {
                SetActiveState(attackIntentObjs, true);

                EnemySingleAttackIntent enemyAttackIntent = (EnemySingleAttackIntent)currentEnemyAction.GetIntentOfType(IntentType.SingleAttack);

                attackIntentText.text = CombatManager._Instance.CalculateDamage(enemyAttackIntent.DamageAmount,
                    Target.Enemy, Target.Character, DamageType.Default, DamageSource.EnemyAttack, false).ToString();
            }
            else if (currentEnemyAction.HasIntentType(IntentType.MultiAttack))
            {
                SetActiveState(attackIntentObjs, true);

                EnemyMultiAttackIntent enemyAttackIntent = (EnemyMultiAttackIntent)currentEnemyAction.GetIntentOfType(IntentType.MultiAttack);

                attackIntentText.text = CombatManager._Instance.CalculateDamage(enemyAttackIntent.DamageAmount,
                    Target.Enemy, Target.Character, DamageType.Default, DamageSource.EnemyAttack, false) + "x" + enemyAttackIntent.NumAttacks;
            }
            else
            {
                SetActiveState(attackIntentObjs, false);
            }

            // Set intent Ward Text
            if (currentEnemyAction.HasIntentType(IntentType.Ward))
            {
                SetActiveState(wardIntentObjs, true);

                wardIntentText.text = CombatManager._Instance.CalculateWard(((EnemyWardIntent)currentEnemyAction.GetIntentOfType(IntentType.Ward)).WardAmount, Target.Enemy).ToString();
            }
            else
            {
                SetActiveState(wardIntentObjs, false);
            }
        }
        else
        {
            intentDisplayCV.alpha = 0;
        }
    }

    private void SetActiveState(GameObject[] objects, bool active)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(active);
        }
    }

    public void SetEnemyAction(EnemyAction enemyAction)
    {
        currentEnemyAction = enemyAction;

        foreach (EnemyIntent intent in currentEnemyAction.GetEnemyIntents())
        {
            addedIntents.Add(intent);
        }
    }

    public void ClearIntents()
    {
        addedIntents.Clear();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (addedIntents.Count <= 0) return;

        if (CombatManager._Instance.AllowGameSpaceToolTips)
        {
            spawnedToolTip = UIManager._Instance.SpawnEqualListingToolTips(
                currentEnemyAction.GetEnemyIntents().Cast<ToolTippable>().ToList(), transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(spawnedToolTip);
    }
}
