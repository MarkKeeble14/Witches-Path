using TMPro;
using UnityEngine;

public class CharacterBasicAttackDamageDisplay : SingleVariableCustomToolTipLabelAndText
{
    protected override float Variable => CombatManager._Instance.GetPlayerBasicAttackDamage();

    [SerializeField] private TextMeshProUGUI text;
    private void Update()
    {
        text.text = Variable.ToString();
    }
}
