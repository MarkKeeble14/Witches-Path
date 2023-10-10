using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultipleVariableCustomToolTipLabelAndText : CustomToolTipLabelAndText
{
    [SerializeField] protected string[] strings;
    private List<Func<float>> variables = new List<Func<float>>();

    private void Start()
    {
        AddVariables();
    }

    protected void AddVariable(Func<float> v)
    {
        variables.Add(v);
    }

    protected abstract void AddVariables();

    public override string GetToolTipText()
    {
        string result = "";
        for (int i = 0; i < variables.Count; i++)
        {
            if (i < strings.Length)
            {
                string s = strings[i];
                result += s;
            }
            if (i < variables.Count)
            {
                float v = variables[i]();
                result += v;
            }
        }

        return UIManager._Instance.HighlightKeywords(result);
    }
}

