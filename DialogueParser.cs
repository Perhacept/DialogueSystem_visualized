﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Subtegral.DialogueSystem.DataContainers;

namespace Subtegral.DialogueSystem.Runtime
{
    public class DialogueParser : MonoBehaviour
    {
        [SerializeField] private DialogueContainer dialogue;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button choicePrefab;
        [SerializeField] private Transform buttonContainer;

        private void Start()
        {
            var narrativeData = dialogue.NodeLinks.First(); //Entrypoint node
            ProceedToNarrative(narrativeData.TargetNodeGuid);
        }

        private void ProceedToNarrative(string narrativeDataGUID)
        {
            //get txt
            string text;
            if (GameManager.instance._currentLanguage == GameManager.WhichLanguage.Chinese)
            {
                text = dialogue.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID).DialogueTextCHS;
            }
            else if (GameManager.instance._currentLanguage == GameManager.WhichLanguage.English)
            {
                text = dialogue.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID).DialogueTextEN;
            }
            else {
                text = dialogue.DialogueNodeData.Find(x => x.NodeGUID == narrativeDataGUID).DialogueTextEN;
            }

            var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGUID == narrativeDataGUID);
            dialogueText.text = ProcessProperties(text);
            var buttons = buttonContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                Destroy(buttons[i].gameObject);
            }

            foreach (var choice in choices)
            {
                var button = Instantiate(choicePrefab, buttonContainer);
                button.GetComponentInChildren<Text>().text = ProcessProperties(choice.PortName);
                button.onClick.AddListener(() => ProceedToNarrative(choice.TargetNodeGuid));
            }
        }

        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in dialogue.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }
    }
}