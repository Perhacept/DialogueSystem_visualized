using Subtegral.DialogueSystem.DataContainers;
using Subtegral.DialogueSystem.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    DialogueGraphView _graphView;
    string _fileName = "New Narrative";
    //private DialogueContainer _dialogueContainer;

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow() {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMniMap();
        GenerateBlackBoard();
    }

    private void GenerateBlackBoard()
    {
        var blackboard = new Blackboard(_graphView);
        blackboard.Add(new BlackboardSection { title = "Exposed Variables" });
        blackboard.addItemRequested = _blackboard =>
        {
            _graphView.AddPropertyToBlackBoard(ExposedProperty.CreateInstance(), false);
        };
        blackboard.editTextRequested = (_blackboard, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField)element).text;
            if (_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one.",
                    "OK");
                return;
            }

            var targetIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
            _graphView.ExposedProperties[targetIndex].PropertyName = newValue;
            ((BlackboardField)element).text = newValue;
        };
        blackboard.SetPosition(new Rect(10, 30, 200, 300));
        _graphView.Add(blackboard);
        _graphView.Blackboard = blackboard;
    }

    private void GenerateMniMap()
    {
        var miniMap = new MiniMap { anchored = true };
        var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
        miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
        _graphView.Add(miniMap);
    }

    void ConstructGraphView() {
        _graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    void GenerateToolbar() {
        var _toolBar = new Toolbar();

        var filenameTextField = new TextField("File name: ");
        filenameTextField.SetValueWithoutNotify(_fileName);
        filenameTextField.MarkDirtyRepaint();
        filenameTextField.RegisterValueChangedCallback(evt => { _fileName = evt.newValue; });
        _toolBar.Add(filenameTextField);

        _toolBar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });
        _toolBar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Data" });

        var nodeCreateButton = new Button( () =>
        {
            _graphView.CreateNewDialogueNode(new Vector2(0,0), "Dialogue Node", "Dialogue Node",0);

         });
        nodeCreateButton.text = "Create Node";
        _toolBar.Add(nodeCreateButton);

        rootVisualElement.Add(_toolBar);
    }

    void RequestDataOperation(bool save) {
        if (!string.IsNullOrEmpty(_fileName))
        {
            GraphSaveUtility saveUtility = GraphSaveUtility.GetInstance(_graphView);
            if (save)
                saveUtility.SaveGraph(_fileName);
            else
                saveUtility.LoadGraph(_fileName);
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid File name", "Please Enter a valid filename", "OK");
        }
    }
}
