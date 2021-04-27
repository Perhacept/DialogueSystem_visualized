using System;
using UnityEngine;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public class DialogueNodeData
    {
        public string NodeGUID;
        public string DialogueTextCHS;
        public string DialogueTextEN;
        public Vector2 Position;
        public int customEventID;
        //search: addhere114514
        //this, DialogueNode, GraphSaveUtility, DialogueGraphView, NodeSearchWindow
    }
}