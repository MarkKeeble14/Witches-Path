public class CharacterBasicAttackDamageDisplay : SingleVariableCustomToolTipLabelAndText
{
    protected override float Variable => CombatManager._Instance.CurrentEnemy.GetBasicAttackDamage();
}
