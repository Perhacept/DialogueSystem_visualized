using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Subtegral.DialogueSystem.DataContainers;
using UnityEngine.UIElements;

namespace Subtegral.DialogueSystem.Editor
{
    public class GraphSaveUtility
    {
        private List<Edge> Edges => _graphView.edges.ToList();
        private List<DialogueNode> Nodes => _graphView.nodes.ToList().Cast<DialogueNode>().ToList();

        private List<Group> CommentBlocks =>
            _graphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();

        private DialogueContainer _containerCache;
        private DialogueGraphView _graphView;


        public static GraphSaveUtility GetInstance(DialogueGraphView graphView)
        {
            return new GraphSaveUtility
            {
                _graphView = graphView
            };
        }
        public void SaveGraph(string fileName) {
             if (!Edges.Any()) {
                 //空不储存
                 return;
             }
             var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

            SaveExposedProperties(dialogueContainer);
            SaveCommentBlocks(dialogueContainer);

            var connectedPorts = Edges.Where(x=>x.input.node!=null).ToArray();
             for (int i = 0; i < connectedPorts.Count(); i++)
             {
                 var outputNode = connectedPorts[i].output.node as DialogueNode;
                 var inputNode = connectedPorts[i].input.node as DialogueNode;

                 dialogueContainer.NodeLinks.Add(new NodeLinkData {
                     BaseNodeGUID = outputNode.GUID,
                     PortName = connectedPorts[i].output.portName,
                     TargetNodeGuid = inputNode.GUID
                 });
             }

            foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
            {
                dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
                {
                    //addhere114514
                    NodeGUID = dialogueNode.GUID,
                    DialogueTextCHS = dialogueNode.DialogueTextCHS,
                    DialogueTextEN = dialogueNode.DialogueTextEN,
                    customEventID = dialogueNode.customEventID,
                    Position = dialogueNode.GetPosition().position
                });
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            UnityEngine.Object loadedAsset = AssetDatabase.LoadAssetAtPath($"Assets/Resources/{fileName}.asset", typeof(DialogueContainer));
            if (loadedAsset == null || !AssetDatabase.Contains(loadedAsset))
            {
                AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
            }
            else
            {
                DialogueContainer container = loadedAsset as DialogueContainer;
                container.NodeLinks = dialogueContainer.NodeLinks;
                container.DialogueNodeData = dialogueContainer.DialogueNodeData;
                container.ExposedProperties = dialogueContainer.ExposedProperties;
                container.CommentBlockData = dialogueContainer.CommentBlockData;
                EditorUtility.SetDirty(container);
            }
            //AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();


        }
        private void SaveExposedProperties(DialogueContainer dialogueContainer)
        {
            dialogueContainer.ExposedProperties.Clear();
            dialogueContainer.ExposedProperties.AddRange(_graphView.ExposedProperties);
        }
        private void SaveCommentBlocks(DialogueContainer dialogueContainer)
        {
            foreach (var block in CommentBlocks)
            {
                var nodes = block.containedElements.Where(x => x is DialogueNode).Cast<DialogueNode>().Select(x => x.GUID)
                    .ToList();

                dialogueContainer.CommentBlockData.Add(new CommentBlockData
                {
                    ChildNodes = nodes,
                    Title = block.title,
                    Position = block.GetPosition().position
                });
            }
        }

        public void LoadGraph(string fileName)
        {
            _containerCache = Resources.Load<DialogueContainer>(fileName);
            if (_containerCache == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target Narrative Data does not exist!", "OK");
                return;
            }

            
            ClearGraph();
            CreateNodes();//
            ConnectNodes();//
            AddExposedProperties();
            GenerateCommentBlocks();
        }
        private void AddExposedProperties()
        {
            _graphView.ClearBlackBoardAndExposedProperties();
            foreach (var exposedProperty in _containerCache.ExposedProperties)
            {
                _graphView.AddPropertyToBlackBoard(exposedProperty);
            }
        }
        private void GenerateCommentBlocks()
        {
            foreach (var commentBlock in CommentBlocks)
            {
                _graphView.RemoveElement(commentBlock);
            }

            foreach (var commentBlockData in _containerCache.CommentBlockData)
            {
                var block = _graphView.CreateCommentBlock(new Rect(commentBlockData.Position, _graphView._defaultNodeSize),
                     commentBlockData);
                block.AddElements(Nodes.Where(x => commentBlockData.ChildNodes.Contains(x.GUID)));
            }
        }

        private void ConnectNodes()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var k = i; //防止访问已修改closure
                var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGUID == Nodes[k].GUID).ToList();
                for (var j = 0; j < connections.Count(); j++)
                {
                    var targetNodeGUID = connections[j].TargetNodeGuid;
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                    LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                        _containerCache.DialogueNodeData.First(x => x.NodeGUID == targetNodeGUID).Position,
                        _graphView._defaultNodeSize));
                }
            }
        }

        private void LinkNodes(Port outputSocket, Port inputSocket)
        {
            var tempEdge = new Edge()
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            _graphView.Add(tempEdge);
        }

        private void CreateNodes()
        {
            foreach (var perNode in _containerCache.DialogueNodeData)
            {
                //addhere114514
                //Vector2 position, string CHStxt,string ENtxt,int _customEventID
                var tempNode = _graphView.CreateDialogueNode(Vector2.zero, perNode.DialogueTextCHS, perNode.DialogueTextEN, perNode.customEventID);//todo
                tempNode.GUID = perNode.NodeGUID;
                tempNode.customEventID = perNode.customEventID;
                tempNode.DialogueTextCHS = perNode.DialogueTextCHS;
                tempNode.DialogueTextEN = perNode.DialogueTextEN;
                _graphView.AddElement(tempNode);

                var nodePorts = _containerCache.NodeLinks.Where(x => x.BaseNodeGUID == perNode.NodeGUID).ToList();
                nodePorts.ForEach(x => _graphView.AddChoicePort(tempNode, x.PortName));
            }
        }

        private void ClearGraph()
        {
            Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGUID;
            foreach (var perNode in Nodes)
            {
                if (perNode.EntryPoint) continue;//
                Edges.Where(x => x.input.node == perNode).ToList()
                    .ForEach(edge => _graphView.RemoveElement(edge));
                _graphView.RemoveElement(perNode);
            }
        }
    }
}