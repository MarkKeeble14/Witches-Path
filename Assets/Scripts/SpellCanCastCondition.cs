using UnityEngine;

public abstract class SpellCanCastCondition
{
    protected Spell forSpell;

    public SpellCanCastCondition(Spell forSpell)
    {
        this.forSpell = forSpell;
    }

    public bool EvaluateCondition()
    {
        bool result = Evaluate();
        if (result)
        {
            OnEvaluationSuccess();
        }
        else
        {
            OnEvaluationFailure();
        }
        return result;
    }

    public abstract string GetEvaluationString();

    protected abstract bool Evaluate();

    protected virtual void OnEvaluationSuccess()
    {
        //
    }

    protected virtual void OnEvaluationFailure()
    {
        //
    }
}

public class HasManaSpellCanCastCondition : SpellCanCastCondition
{
    public HasManaSpellCanCastCondition(Spell forSpell) : base(forSpell)
    {
    }

    protected override bool Evaluate()
    {
        return GameManager._Instance.GetCurrentPlayerMana() >= forSpell.ManaCost
            || CombatManager._Instance.NumFreeSpells > 0;
    }

    protected override void OnEvaluationFailure()
    {
        base.OnEvaluationFailure();
        GameManager._Instance.PopManaText();
    }

    public override string GetEvaluationString()
    {
        return "";
    }
}

public class NotOnCooldownSpellCanCastCondition : SpellCanCastCondition
{
    private ReusableSpell Spell;

    public NotOnCooldownSpellCanCastCondition(Spell forSpell) : base(forSpell)
    {
        Spell = (ReusableSpell)forSpell;
    }

    protected override bool Evaluate()
    {
        return !Spell.OnCooldown || CombatManager._Instance.NumFreeSpells > 0;
    }

    public override string GetEvaluationString()
    {
        return "";
    }
}

public class MaximumSpellsInDrawPileSpellCanCastCondition : SpellCanCastCondition
{
    private int maxSpells;

    public MaximumSpellsInDrawPileSpellCanCastCondition(Spell forSpell, int maxSpells) : base(forSpell)
    {
        this.maxSpells = maxSpells;
    }

    protected override bool Evaluate()
    {
        return CombatManager._Instance.NumSpellsInDraw < maxSpells;
    }

    public override string GetEvaluationString()
    {
        return "Can only be Cast if there are less than " + maxSpells + " Spells in your Draw Pile.";
    }
}