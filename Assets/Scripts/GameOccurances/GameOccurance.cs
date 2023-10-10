using System;
using System.Collections;
using UnityEngine;

public abstract class GameOccurance : ScriptableObject
{
    public abstract MapNodeType Type { get; }
    public string Label => Utils.CapitalizeFirstLetters(Type.ToString(), new char[] { '_' });

    private bool resolved;

    public void SetResolve(bool val)
    {
        resolved = val;
    }

    public IEnumerator RunOccurance()
    {
        Debug.Log(name + ": Beginning to Run");
        yield return GameManager._Instance.StartCoroutine(OnInit());

        GameOccuranceUIManager._Instance.StartGameOccurance(Type);

        yield return GameManager._Instance.StartCoroutine(OnStart());

        Debug.Log(name + ": Waiting to Resolve");

        yield return new WaitUntil(() => resolved);
        Debug.Log(name + ": Has Been Resolved");

        yield return GameManager._Instance.StartCoroutine(OnResolve());
        GameOccuranceUIManager._Instance.CloseGameOccurance();
        Debug.Log(name + ": Has Ended");
    }

    protected abstract IEnumerator OnStart();

    protected virtual IEnumerator OnInit()
    {
        yield return null;
    }

    protected abstract IEnumerator OnResolve();
}
