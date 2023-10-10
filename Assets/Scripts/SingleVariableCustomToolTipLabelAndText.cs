using UnityEngine;

public abstract class SingleVariableCustomToolTipLabelAndText : CustomToolTipLabelAndText
{
    [SerializeField] protected string prefix;
    [SerializeField] protected string suffix;
    protected abstract float Variable { get; }

    public override string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(prefix + Variable.ToString() + suffix);
    }
}

