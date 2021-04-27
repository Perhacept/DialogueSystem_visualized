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
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private Transform buttonContainer;
    public string playerName = "Perh";
    public GameObject qipao;

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
        _playerInputController.DialoguePlay.Enable();
        GameManager.instance._playerState = GameManager.PlayerState.inDialogue;
        GameManager.instance.playerIns.GetComponent<Animator>().SetBool("onGround", true) ;//todo
        qipao.SetActive(true);

        var narrativeData = dialogue.NodeLinks.First(); //Entrypoint node
        //ProceedToNarrative(narrativeData.TargetNodeGuid);
        StartCoroutine(ProceedTxtNButtons(narrativeData.TargetNodeGuid));
    }
    protected virtual void CloseAdialogue() {
        _playerInputController.DialoguePlay.Disable();
        GameManager.instance._playerState = GameManager.PlayerState.idling;
        qipao.SetActive(false);
    }

    IEnumerator ProceedTxtNButtons(string narrativeDataGUID) {
        //if (!dialogue.DialogueNodeData.Exists(x => x.NodeGUID == narrativeDataGUID)) {
           //Debug.Log("done");
           // CloseAdialogue();
           // yield return null;
       // }
        //else 
            {
            string textkey = getTxtStr(narrativeDataGUID);

            mylocalizedstring = new LocalizedString { TableReference = "DialogueTable", TableEntryReference = textkey };
            //mylocalizedstring.Arguments.Add(this.gameObject);
            yield return mylocalizedstring.GetLocalizedString();
            //if (mylocalizedstring.GetLocalizedString().IsDone)
            //{
            dialogueText_onUI.text = mylocalizedstring.GetLocalizedString().Result;
            //}       
            var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGUID == narrativeDataGUID);//前节点是narrativeDataGUID的data 基础的链接结构//

            var buttons = buttonContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                Destroy(buttons[i].gameObject);//warning: Destroy是异步的
            }
            yield return null;//至关重要//todo
            //for (int i=0;i<buttonTextKeys.Count();i++) {}
            foreach (var choice in choices)
            {
                var button = Instantiate(choicePrefab, buttonContainer);
                mylocalizedButtonstring = new LocalizedString { TableReference = "DialogueButtonTable", TableEntryReference = choice.PortName };
                yield return mylocalizedButtonstring.GetLocalizedString();
                button.GetComponentInChildren<TMP_Text>().text = mylocalizedButtonstring.GetLocalizedString().Result;
                button.GetComponent<Button>().onClick.AddListener(() => {
                    //if (choice.TargetNodeGuid != "")
                    {
                        StartCoroutine(ProceedTxtNButtons(choice.TargetNodeGuid));//这里可以增加特殊条件
                    }
                    //else {
                        //CloseAdialogue();
                    //}
                });//事件监听
            }
            if (buttonContainer.childCount != 0)
            {
                //Debug.Log(buttonContainer.childCount);
                buttonContainer.GetChild(buttonContainer.childCount - 1).GetComponent<Button>().Select();
            }
            else {
                //Debug.Log("tr2");
                //yield return null;
                CloseAdialogue();
            }
            yield return null;
        }
    }
}
