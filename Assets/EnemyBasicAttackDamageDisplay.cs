using System.Collections;

public class EnemyBasicAttackDamageDisplay : SingleVariableCustomToolTipLabelAndText
{
    protected override float Variable => CombatManager._Instance.CurrentEnemy.GetBasicAttackDamage();
}
