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
    [SerializeField] private SerializableDictionary<IntentType, List<GameObject>> intentTypeObjs = new SerializableDictionary<IntentType, List<GameObject>>();
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
                EnemySingleAttackIntent enemyAttackIntent = (EnemySingleAttackIntent)currentEnemyAction.GetIntentOfType(IntentType.SingleAttack);
                attackIntentText.text = CombatManager._Instance.CalculateDamage(enemyAttackIntent.DamageAmount,
                    Target.Enemy, Target.Character, DamageType.Default, DamageSource.EnemyAttack, false).ToString();
            }
            else if (currentEnemyAction.HasIntentType(IntentType.MultiAttack))
            {
                EnemyMultiAttackIntent enemyAttackIntent = (EnemyMultiAttackIntent)currentEnemyAction.GetIntentOfType(IntentType.MultiAttack);
                attackIntentText.text = CombatManager._Instance.CalculateDamage(enemyAttackIntent.DamageAmount,
                    Target.Enemy, Target.Character, DamageType.Default, DamageSource.EnemyAttack, false) + "x" + enemyAttackIntent.NumAttacks;
            }

            // Set intent Ward Text
            if (currentEnemyAction.HasIntentType(IntentType.Ward))
            {
                wardIntentText.text = CombatManager._Instance.CalculateWard(((EnemyWardIntent)currentEnemyAction.GetIntentOfType(IntentType.Ward)).WardAmount, Target.Enemy).ToString();
            }
        }
        else
        {
            intentDisplayCV.alpha = 0;
        }
    }

    private void SetActiveState(IEnumerable objects, bool active)
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

            SetActiveState(intentTypeObjs[intent.Type], true);
        }
    }

    public void ClearIntents()
    {
        while (addedIntents.Count > 0)
        {
            EnemyIntent cur = addedIntents[0];
            SetActiveState(intentTypeObjs[cur.Type], false);
            addedIntents.RemoveAt(0);
        }
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
