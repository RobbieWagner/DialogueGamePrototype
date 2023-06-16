using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class DialogueManager : MonoBehaviour
{
     //DialogueSentences class stores a list of sentences and possible choices
    [System.Serializable]
    public class DialogueSentences
    {
        public List<Sentence> sentences;
    }

    //Sentence class holds properties for the person speaking, text, and choices
    [System.Serializable]
    public class Sentence
    {
        public int textID;
        public string text;
        public Choice[] choices;
        public string speaker;
        public string textColor;
    }

    //Holds information for each dialogue choice option and saves any necessary data
    [System.Serializable]
    public class Choice
    {
        public string choiceText;
        public int nextTextID;
        public string saveName;
        public string saveDataString;
        public int saveDataInt;
        public bool saveDataBool;
    }

    [SerializeField] protected SaveDataManager saveDataManager;
    [SerializeField] protected List<Sentence> sentences;
    private Sentence sentence;
    private int curChoiceIndex;
    [SerializeField] TextAsset exampleDialogue;

    public TextMeshProUGUI dialogueText;

    public Canvas dialogueCanvas;

    [SerializeField] private AudioSource talkingNoise;
    [SerializeField] private AudioSource menuNavNoise;

    protected int currentSentence;

    [SerializeField] private Image blinkIcon;
    [SerializeField] private GameObject[] choicePanelsInactive;
    [SerializeField] private TextMeshProUGUI[] choiceTextsInactive;
    [SerializeField] private GameObject[] choicePanelsActive;
    [SerializeField] private TextMeshProUGUI[] choiceTextsActive;


    private bool typingSentence;
    private bool skipSentence;

    protected bool canMoveOn;
    private bool waitingForPlayerToContinue;

    [HideInInspector]
    public bool dialogueRunning;

    protected virtual void Start()
    {
        currentSentence = -1;

        if(blinkIcon != null)blinkIcon.enabled = false;
        dialogueCanvas.enabled = false;

        canMoveOn = false;
        waitingForPlayerToContinue = false;

        curChoiceIndex = 0;

        skipSentence = false;
        typingSentence = false;

        DeactivateChoicePanels();
    }

    protected virtual void Update() 
    {
        if(dialogueRunning && canMoveOn && waitingForPlayerToContinue)
        {
            if((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
            {
                waitingForPlayerToContinue = false;
                talkingNoise.Play();
                if(sentence.choices == null)
                {
                    currentSentence++;
                    DisplayNextSentence(currentSentence);
                }
                else
                {
                    Choice choice = sentence.choices[curChoiceIndex];
                    if(choice.saveName != null) 
                    {
                        saveDataManager.SaveNewData
                        (new SaveData(choice.saveName, choice.saveDataInt, choice.saveDataBool, choice.saveDataString));
                        //Debug.Log(choice.saveDataBool);
                    }
                    else DisplayNextSentence(currentSentence);
                }
            } 
            else if(sentence.choices != null && sentence.choices.Length > 1)
            {
                if(Input.GetKeyDown(KeyCode.DownArrow) && curChoiceIndex > 0)
                {
                    curChoiceIndex--;
                    menuNavNoise.Play();

                    foreach(GameObject choicePanel in choicePanelsActive) choicePanel.SetActive(false);
                    choicePanelsActive[curChoiceIndex].SetActive(true);
                }
                else if(Input.GetKeyDown(KeyCode.UpArrow) && curChoiceIndex < sentence.choices.Length - 1)
                {
                    curChoiceIndex++;
                    menuNavNoise.Play();

                    foreach(GameObject choicePanel in choicePanelsActive) choicePanel.SetActive(false);
                    choicePanelsActive[curChoiceIndex].SetActive(true);
                }
            }
        }

        if(typingSentence && Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            skipSentence = true;
        }
    }

    //Sets up a dialogue to be run
    public virtual IEnumerator StartDialogue(TextAsset dialogueFile, Sprite displayImage = null)
    {
        if(!dialogueRunning)
        {
            dialogueRunning = true;

            currentSentence = 0;
            
            sentences = JsonUtility.FromJson<DialogueSentences>(dialogueFile.text).sentences;

            DisplayNextSentence(currentSentence);
            dialogueCanvas.enabled = true;
            while(dialogueRunning) yield return null;
            EndDialogue();
            StopCoroutine(StartDialogue(dialogueFile, displayImage));
        }
    }

    //Prepares a sentence to be displayed to the player
    public virtual void DisplayNextSentence(int textID)
    {
        if(textID >= sentences.Count)
        {
            EndDialogue();
        }
        else
        {
            currentSentence = textID;
            sentence = sentences[textID];
            
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence));
        }
    }

    protected virtual void EndDialogue()
    {
        dialogueRunning = false;
        dialogueCanvas.enabled = false;
        EventSystem.current.SetSelectedGameObject(null);
        currentSentence = -1;

        DeactivateChoicePanels();
    }

    //Types a sentence out into the text box
    protected IEnumerator TypeSentence(Sentence sentence, float timeBetweenCharacters = .025f, bool pauseAfter = false)
    {
        yield return null;
        curChoiceIndex = 0;
        dialogueText.text = "";
        string textToDisplay = sentence.text;
        textToDisplay = InsertPlayerName(textToDisplay);
        if(sentence.textColor == null || sentence.textColor.ToLower().Equals("white")) dialogueText.color = Color.white;
        else if(sentence.textColor.ToLower().Equals("red")) dialogueText.color = Color.red;
        else if(sentence.textColor.ToLower().Equals("blue")) dialogueText.color = Color.blue; 
        else if(sentence.textColor.ToLower().Equals("green")) dialogueText.color = Color.green; 
        typingSentence = true;

        for(int i = 0; i < textToDisplay.Length; i++)
        {
            dialogueText.text += textToDisplay[i];
            yield return new WaitForSeconds(timeBetweenCharacters);
            if(skipSentence) i = textToDisplay.Length;
        }

        dialogueText.text = textToDisplay;
        skipSentence = false;
        typingSentence = false;

        if(pauseAfter) yield return new WaitForSeconds(1f);
        
        canMoveOn = true;

        if(blinkIcon != null && (sentence.choices == null || sentence.choices.Length < 2)) StartCoroutine(BlinkIcon());
        else if(sentence.choices != null && sentence.choices.Length >= 2)
        {
            for(int i = 0; i < sentence.choices.Length; i++)
            {
                choiceTextsInactive[i].text = sentence.choices[i].choiceText;
                choiceTextsActive[i].text = sentence.choices[i].choiceText;
                choicePanelsInactive[i].SetActive(true);
            }
            choicePanelsActive[curChoiceIndex].SetActive(true);
            waitingForPlayerToContinue = true;
        }

        StopCoroutine(TypeSentence(sentence));
    }

    //Blinks an icon to show the player that they can move to the next sentence
    IEnumerator BlinkIcon()
    {
        waitingForPlayerToContinue = true;
        while(waitingForPlayerToContinue) 
        {
            blinkIcon.enabled = true;
            yield return new WaitForSeconds(.5f);
            blinkIcon.enabled = false;
            yield return new WaitForSeconds(.3f);
        }
        waitingForPlayerToContinue = false;

        StopCoroutine(BlinkIcon());
    }

    private void DeactivateChoicePanels()
    {
        for(int i = 0; i < choicePanelsActive.Length; i++)
        {
            choicePanelsInactive[i].SetActive(false);
            choicePanelsActive[i].SetActive(false);
            choiceTextsInactive[i].text = "";
            choiceTextsActive[i].text = "";
        }
    }

    protected string InsertPlayerName(string textIn)
    {
        string textOut = textIn;
        if(textIn.Contains("***"))
        {
            SaveData playerNameSaveData = saveDataManager.LoadSaveData("playerName");
            string playerName = "Therppo";

            if(playerNameSaveData != null && playerNameSaveData.savedString != null) playerName = playerNameSaveData.savedString;

            textOut = textIn.Replace("***", playerName);
        }
        
        return textOut;
    }
}