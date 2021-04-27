using Subtegral.DialogueSystem.DataContainers;
using Subtegral.DialogueSystem.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    
    public readonly Vector2 _defaultNodeSize = new Vector2(150,200);

    public List<ExposedProperty> ExposedProperties { get; private set; } = new List<ExposedProperty>();
    public Blackboard Blackboard = new Blackboard();
    private NodeSearchWindow _searchWindow;

    public DialogueGraphView(DialogueGraph editorWindow) {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphUSS"));

        SetupZoom(ContentZoomer.DefaultMinScale,ContentZoomer.DefaultMaxScale);//zoom     

        //
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new FreehandSelector());

        var grid = new GridBackground();
        Insert(0,grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryPointNode());

        AddSearchWindow(editorWindow);
    }

    private void AddSearchWindow(DialogueGraph editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Configure(editorWindow, this);
        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    public void ClearBlackBoardAndExposedProperties()
    {
        ExposedProperties.Clear();
        Blackboard.Clear();
    }

    DialogueNode GenerateEntryPointNode() {
        var node = new DialogueNode {
            title = "Start",
            GUID = Guid.NewGuid().ToString(),
            DialogueTextCHS = "entryPoint",
            DialogueTextEN = "entryPoint",
            EntryPoint = true,
            customEventID = 0
        };//addhere114514

        var generatedport = GeneratePort(node,Direction.Output);
        generatedport.portName = "Next";
        node.outputContainer.Add(generatedport);

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100,200,100,150));
        return node;
    }

    Port GeneratePort(DialogueNode node,Direction portDir,Port.Capacity capacity= Port.Capacity.Single) {
        return node.InstantiatePort(Orientation.Horizontal,portDir,capacity,typeof(float));//可选：typeof(float)
    }

    public void CreateNewDialogueNode(Vector2 position, string CHStxt, string ENtxt, int _customEventID)
    {
        AddElement(CreateDialogueNode(position,CHStxt,ENtxt,_customEventID));
    }

    public DialogueNode CreateDialogueNode(Vector2 position, string CHStxt,string ENtxt,int _customEventID) {
        //addhere114514 多加参数 修改函数重载
        var dialogueNode = new DialogueNode()
        {
            title = ENtxt,
            DialogueTextCHS = CHStxt,
            DialogueTextEN = ENtxt,
            GUID = Guid.NewGuid().ToString(),
            customEventID = _customEventID,
        };//addhere114514

        //TODO more Loacalization TextField
        var textField = new TextField("CHS");
        textField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueTextCHS = evt.newValue;
            //dialogueNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(CHStxt);//
        dialogueNode.mainContainer.Add(textField);

        var textField2 = new TextField("EN");
        textField2.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueTextEN = evt.newValue;
            //dialogueNode.title = evt.newValue;
        });
        textField2.SetValueWithoutNotify(ENtxt);//
        dialogueNode.mainContainer.Add(textField2);

        //addhere114514 //customEventID
        var textField3 = new TextField("customEventID");
        textField3.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.customEventID = int.Parse(evt.newValue);
        });
        textField3.SetValueWithoutNotify(_customEventID.ToString());//addhere
        dialogueNode.mainContainer.Add(textField3);

        // var button = new Button(() => { AddChoicePort(dialogueNode); })
        {
            //    text = "Add Choice"
        };
        //  dialogueNode.titleButtonContainer.Add(button);

        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        var button = new Button(() => AddChoicePort(dialogueNode));
        button.text = "new choice";
        dialogueNode.titleContainer.Add(button);


        //tempDialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        //To-Do: implement screen center instantiation positioning



        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position,
            _defaultNodeSize));

        return dialogueNode;

    }

    /*
    public DialogueNode CreateDialogueNode(string nodeName, Vector2 position)
    {
        var dialogueNode = new DialogueNode()
        {
            title = nodeName,
            DialogueTextCHS = nodeName,
            DialogueTextEN = nodeName,
            GUID = Guid.NewGuid().ToString(),
            customEventID = 0,
        };//addhere114514
        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        var button = new Button(() => AddChoicePort(dialogueNode));
        button.text = "new choice";
        dialogueNode.titleContainer.Add(button);


        //tempDialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        //To-Do: implement screen center instantiation positioning

        //TODO more Loacalization TextField
        var textField = new TextField("CHS");
        textField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueTextCHS = evt.newValue;
            dialogueNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField);

        var textField2 = new TextField("EN");
        textField2.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueTextEN = evt.newValue;
            dialogueNode.title = evt.newValue;
        });
        textField2.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField2);

        //addhere114514 //customEventID
        var textField3 = new TextField("customEventID");
        textField3.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.customEventID = int.Parse(evt.newValue);
        });
        textField3.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField3);

        // var button = new Button(() => { AddChoicePort(dialogueNode); })
        {
            //    text = "Add Choice"
        };
        //  dialogueNode.titleButtonContainer.Add(button);

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position,
            _defaultNodeSize));

        return dialogueNode;
    }*/

    /*public DialogueNode CreateDialogueNode(string _nodeName)
    {
        var dialogueNode = new DialogueNode
        {
            title = _nodeName,
            DialogueTextCHS = _nodeName,
            DialogueTextEN = _nodeName,
            GUID = Guid.NewGuid().ToString()
        };
        var inputPort = GeneratePort(dialogueNode,Direction.Input,Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        var button = new Button(()=>AddChoicePort(dialogueNode));
        button.text = "new choice";
        dialogueNode.titleContainer.Add(button);


        //TODO more Loacalization TextField
        var textField = new TextField("CHS");
        textField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueTextCHS = evt.newValue;
            dialogueNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField);

        var textField2 = new TextField("EN");
        textField2.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueTextEN = evt.newValue;
            dialogueNode.title = evt.newValue;
        });
        textField2.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField2);

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(Vector2.zero,_defaultNodeSize));

        return dialogueNode;
    }*/

    public void AddPropertyToBlackBoard(ExposedProperty property, bool loadMode = false)
    {
        var localPropertyName = property.PropertyName;
        var localPropertyValue = property.PropertyValue;
        if (!loadMode)
        {
            while (ExposedProperties.Any(x => x.PropertyName == localPropertyName))
                localPropertyName = $"{localPropertyName}(1)";
        }

        var item = ExposedProperty.CreateInstance();
        item.PropertyName = localPropertyName;
        item.PropertyValue = localPropertyValue;
        ExposedProperties.Add(item);

        var container = new VisualElement();
        var field = new BlackboardField { text = localPropertyName, typeText = "string" };
        container.Add(field);

        var propertyValueTextField = new TextField("Value:")
        {
            value = localPropertyValue
        };
        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var index = ExposedProperties.FindIndex(x => x.PropertyName == item.PropertyName);
            ExposedProperties[index].PropertyValue = evt.newValue;
        });
        var sa = new BlackboardRow(field, propertyValueTextField);
        container.Add(sa);
        Blackboard.Add(container);
    }

    public Group CreateCommentBlock(Rect rect, CommentBlockData commentBlockData = null)
    {
        if (commentBlockData == null)
            commentBlockData = new CommentBlockData();
        var group = new Group
        {
            autoUpdateGeometry = true,
            title = commentBlockData.Title
        };
        AddElement(group);
        group.SetPosition(rect);
        return group;
    }

    public override List<Port> GetCompatiblePorts(Port _startPort,NodeAdapter _nodeAdapter) {
        var compatiblePorts = new List<Port>();
        ports.ForEach(
            (port) => {

                
                if (_startPort != port && _startPort.node != port.node) {
                    //_startPort.node != port.node避免循环
                    compatiblePorts.Add(port);
                }

            }          
            );
        return compatiblePorts;
    }

    /*
    public void AddChoicePort(DialogueNode nodeCache, string overriddenPortName = "")
    {
        var generatedPort = GeneratePort(nodeCache, Direction.Output);
        var portLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(portLabel);

        var outputPortCount = nodeCache.outputContainer.Query("connector").ToList().Count;
        
        var choicePortName = string.IsNullOrEmpty(overriddenPortName)
            ? $"Choice {outputPortCount + 1}"
            : overriddenPortName;
        
        var textField = new TextField()
        {
            name = string.Empty,
            value = choicePortName
        };
        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);
        var deleteButton = new Button(() => RemovePort(nodeCache, generatedPort))
        {
            text = "X"
        };

        generatedPort.contentContainer.Add(deleteButton);
        generatedPort.portName = choicePortName;
        nodeCache.outputContainer.Add(generatedPort);
        nodeCache.RefreshPorts();
        nodeCache.RefreshExpandedState();
    }

    private void RemovePort(Node node, Port socket)
    {
        var targetEdge = edges.ToList()
            .Where(x => x.output.portName == socket.portName && x.output.node == socket.node);
        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

        node.outputContainer.Remove(socket);
        node.RefreshPorts();
        node.RefreshExpandedState();
    }*/

    public void AddChoicePort(DialogueNode nodeCache, string overriddenPortName = "")
    {
        var generatedPort = GeneratePort(nodeCache, Direction.Output);
        var portLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(portLabel);

        var outputPortCount = nodeCache.outputContainer.Query("connector").ToList().Count();
        var outputPortName = string.IsNullOrEmpty(overriddenPortName)
            ? $"Option {outputPortCount + 1}"
            : overriddenPortName;


        var textField = new TextField()
        {
            name = string.Empty,
            value = outputPortName
        };
        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);
        var deleteButton = new Button(() => RemovePort(nodeCache, generatedPort))
        {
            text = "X"
        };
        generatedPort.contentContainer.Add(deleteButton);
        generatedPort.portName = outputPortName;
        nodeCache.outputContainer.Add(generatedPort);
        nodeCache.RefreshPorts();
        nodeCache.RefreshExpandedState();
    }

    private void RemovePort(Node node, Port socket)
    {
        var targetEdge = edges.ToList()
            .Where(x => x.output.portName == socket.portName && x.output.node == socket.node);
        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

        node.outputContainer.Remove(socket);
        node.RefreshPorts();
        node.RefreshExpandedState();
    }


}

