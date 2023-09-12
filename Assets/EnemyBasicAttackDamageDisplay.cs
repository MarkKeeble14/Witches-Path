using System.Collections;
using UnityEngine;
using TMPro;

public class EnemyBasicAttackDamageDisplay : SingleVariableCustomToolTipLabelAndText
{
    protected override float Variable => CombatManager._Instance.GetEnemyBasicAttackDamage();

    [SerializeField] private TextMeshProUGUI text;
    private void Update()
    {
        text.text = Variable.ToString();
    }
}
