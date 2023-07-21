using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameOccuranceMap
{
    private List<GameOccuranceMapNode> nodes = new List<GameOccuranceMapNode>();

    [SerializeField] private GameOccurance[] test;

    public void Load()
    {
        //
        Debug.Log("Test.Length = " + test.Length);
        for (int i = 0; i < test.Length; i++)
        {
            Debug.Log("Map Load: " + i);
            GameOccuranceMapNode node = new GameOccuranceMapNode(test[i]);

            if (i > 0)
            {
                Debug.Log("Map Load: i > 0; Adding Connection Nodes");
                node.PreviousNodes.Add(nodes[i - 1]);
                nodes[i - 1].NextNodes.Add(node);
            }

            nodes.Add(node);
            Debug.Log("Added Node: " + node);
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            Debug.Log("Node #" + i + ": " + nodes[i]);
        }
    }

    public List<GameOccuranceMapNode> GetAllowedNodes()
    {
        List<GameOccuranceMapNode> r = new List<GameOccuranceMapNode>();
        foreach (GameOccuranceMapNode node in nodes)
        {
            if (node.PreviousNodes.Count == 0)
            {
                r.Add(node);
            }
        }
        return r;
    }

    public List<GameOccuranceMapNode> GetAllowedNodes(GameOccuranceMapNode fromNode)
    {
        return fromNode.NextNodes;
    }

    public class GameOccuranceMapNode
    {
        public List<GameOccuranceMapNode> PreviousNodes { get; set; }
        public List<GameOccuranceMapNode> NextNodes { get; set; }

        private GameOccurance representingOccurance;

        public GameOccuranceMapNode(GameOccurance occurance)
        {
            PreviousNodes = new List<GameOccuranceMapNode>();
            NextNodes = new List<GameOccuranceMapNode>();
            SetOccurance(occurance);
        }

        private void SetOccurance(GameOccurance setTo)
        {
            GameOccurance newSetTo = GameManager.Instantiate(setTo);
            newSetTo.SetResolve(false);
            representingOccurance = newSetTo;
        }

        public GameOccurance GetOccurance()
        {
            return representingOccurance;
        }

        public override string ToString()
        {
            return base.ToString() + ", NumPrevNodes: " + PreviousNodes.Count + ", NumNextNodes: " + NextNodes.Count;
        }
    }
}
