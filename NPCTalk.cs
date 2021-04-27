using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Subtegral.DialogueSystem.DataContainers;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class NPCTalk : MonoBehaviour, kejiaohu
{
    [SerializeField] private DialogueContainer dialogue;
    [SerializeField] private TextMeshProUGUI dialogueText_onUI;
    public LocalizedString mylocalizedstring;
    LocalizedString mylocalizedButtonstring;
    [SerializeField] private Button choicePrefab;
    [SerializeField] private Transform buttonContainer;
    public string playerName = "Perh";

    PlayerInputActions _playerInputController;

    private void OnEnable()
    {
        
        //mylocalizedstring.Arguments[0] = this.gameObject;
        //mylocalizedstring.SetReference("UIText","");
        //_playerInputController.RealGamePlay.Enable();
    }

    private void OnDisable()
    {
        //_playerInputController.RealGamePlay.Disable();
    }
    void Awake()
    {
        //input
        _playerInputController = new PlayerInputActions();
        _playerInputController.DialoguePlay.skip.started += ctr => {  };

        _playerInputController.DialoguePlay.Disable();
    }

    public void jiaohu()
    {
        Debug.Log("triggered");
        OpenAdialogue();
    }

    protected virtual void OpenAdialogue()
    {
        _playerInputController.RealGamePlay.Enable();

        GameManager.instance._playerState = GameManager.PlayerState.inDialogue;

        var narrativeData = dialogue.NodeLinks.First(); //Entrypoint node
        ProceedToNarrative(narrativeData.TargetNodeGuid);
    }
    protected virtual void CloseAdialogue() {
        _playerInputController.RealGamePlay.Disable();
        GameManager.instance._playerState = GameManager.PlayerState.idling;
    }

    private void ProceedToNarrative(string narrativeDataGUID)
    {
        //get txt
        string textkey = getTxtStr(narrativeDataGUID);

        mylocalizedstring = new LocalizedString { TableReference = "DialogueTable", TableEntryReference = textkey };
        Debug.Log(textkey);
        //mylocalizedstring.SetReference("DialogueTable", textkey);
        //= new LocalizedString { TableReference = "My String Table Collection", TableEntryReference = "My Text 1" };
        //mylocalizedstring.Arguments.Add(this.gameObject);

        var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGUID == narrativeDataGUID);//前节点是narrativeDataGUID的data 基础的链接结构
        //dialogueText_onUI.text = mylocalizedstring.GetLocalizedString().Result;
        dialogueText_onUI.text = mylocalizedstring.GetLocalizedString().Result;
        
        //dialogueText_onUI.text = ProcessProperties(text);

        //refresh Button
        //des
        var buttons = buttonContainer.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Destroy(buttons[i].gameObject);
        }
        //ini
        foreach (var choice in choices)
        {
            var button = Instantiate(choicePrefab, buttonContainer);
            //button.GetComponentInChildren<Text>().text = ProcessProperties(choice.PortName);
            //mylocalizedButtonstring.StringReference.SetReference("DialogueButtonTable", choice.PortName);
            mylocalizedButtonstring = new LocalizedString { TableReference = "DialogueButtonTable", TableEntryReference = choice.PortName };

            StartCoroutine(modifyLocaleStr(mylocalizedButtonstring.GetLocalizedString(),button));
            //if (!mylocalizedButtonstring.GetLocalizedString().IsDone)
                //await mylocalizedButtonstring.GetLocalizedString();


            /*

            Debug.Log(mylocalizedButtonstring.CurrentLoadingOperation.ToString());
            //button.GetComponentInChildren<Text>().text = mylocalizedButtonstring.GetLocalizedString().Result;
            button.GetComponentInChildren<Text>().text = mylocalizedButtonstring.GetLocalizedString().Result;
            button.onClick.AddListener(() => ProceedToNarrative(choice.TargetNodeGuid));//事件监听

            */
        }
        buttons[buttons.Length - 1].Select();
        //buttons[0]
        //started buttonToSet.Select();
    }
    private string ProcessProperties(string text)
    {        
        foreach (var exposedProperty in dialogue.ExposedProperties)
        {
            text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);           
        }
        //or string.Replace(propertyName,runtimeValue);
        //但因为本地化 用不到
        return text;
    }

    string getTxtStr(string _narrativeDataGUID) {
        /*if (GameManager.instance._currentLanguage == GameManager.WhichLanguage.Chinese)
        {
            return dialogue.DialogueNodeData.Find(x => x.NodeGUID == _narrativeDataGUID).DialogueTextCHS;
        }
        else if (GameManager.instance._currentLanguage == GameManager.WhichLanguage.English)
        {
            return dialogue.DialogueNodeData.Find(x => x.NodeGUID == _narrativeDataGUID).DialogueTextEN;
        }
        else
        {
            return dialogue.DialogueNodeData.Find(x => x.NodeGUID == _narrativeDataGUID).DialogueTextEN;
        }*/
        return dialogue.DialogueNodeData.Find(x => x.NodeGUID == _narrativeDataGUID).DialogueTextEN;
    }

    private IEnumerator modifyLocaleStr(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<string> temp,Button _buttons)
    {
        yield return temp;

        if (temp.IsDone)
        {
            Debug.Log(temp.Result);
        }
    }



}
