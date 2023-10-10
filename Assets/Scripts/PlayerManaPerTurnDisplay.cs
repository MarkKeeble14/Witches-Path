public class PlayerManaPerTurnDisplay : MultipleVariableCustomToolTipLabelAndText
{
    protected override void AddVariables()
    {
        AddVariable(() => GameManager._Instance.GetManaPerTurn());
        AddVariable(() => GameManager._Instance.GetMaxPlayerMana());
    }
}
