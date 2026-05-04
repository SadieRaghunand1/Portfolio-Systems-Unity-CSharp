using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool original; //Track which game manager is original manager

    public float currency;

    public List<BasePet> allPets;

    [Header("Days and time")]
    public int daysPassed;
    public int timeInDayLeft = 12;
    public bool shouldChangeDayNextLoad = true;
    private int thisTimeCost; //Set every time player enters a minigame, holds for use with action

    [Header("UI")]
    [SerializeField] TextMeshProUGUI[] affectionScoreUI;
    [SerializeField] TextMeshProUGUI sanctuaryScoreUI;
    public bool leftSideControls = true; //if true, cjoystick is on left side of screen, if false, joystick is on right side of screen
    [SerializeField] Toggle joystickCheck;


    [Header("Current minigame")]
    [SerializeField] private string mainGameSceneName;
    private Vector3 lastKnownPlayerPos;
    private GameObject playerObj;

    [Header("Other")]
    [SerializeField] private SanctuaryManager sanctuaryManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private PetManager petManager;

    [Header("Menu")]
    public GameObject mainMenuUI;
    public GameObject creditsMenuUI;
    public GameObject helpMenuUI;
    public GameObject optionsMenu;
    public float audioVolume = 1.0f;
    public Slider volumeSlider;

    [Header("Dialogue")]
    public bool seenStartDialogue = false;
    public bool seenEndDialogue = false;

    //Scoring
    private float scoreMult = 1;


    #region Monobehaviors
    // Start is called before the first frame update
    void Awake()
    {
        InitOnLoad();
        joystickCheck.isOn = saveManager.LoadJoystickSide();
        leftSideControls = joystickCheck.isOn;
        DontDestroyOnLoad(gameObject);
        sanctuaryManager = FindAnyObjectByType<SanctuaryManager>();
    }

    private void Start()
    {
        leftSideControls = joystickCheck.isOn;
    }


    private void OnEnable()
    {
        MinigameManager.action_MinigameDone += ReturnToGameScene;
        MinigameManager.action_DecrementTimeOnDone += ChangeTimeOfDayLeft;
    }

    private void OnDisable()
    {
        MinigameManager.action_MinigameDone -= ReturnToGameScene;
        MinigameManager.action_DecrementTimeOnDone -= ChangeTimeOfDayLeft;
    }


    #endregion
    public void InitOnLoad(List<BasePet> _allPets = null)
    {
        //Check for existing game manager
        if (FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length > 1)
        {
            if (!original)
                Destroy(gameObject);
        }
        //Set as original
        original = true;

        //Assign all pets
        if (_allPets != null)
        {
            allPets.Clear();
            allPets = _allPets;
        }

        if (SceneManager.GetActiveScene().name != "Main_Hub" && !seenEndDialogue)
        {
            if (daysPassed == 30) // If end of game reached
            {
                LoadMainHub(); // Return to main hub for end dialogue.
            }
        }

        if (SceneManager.GetActiveScene().name == "Main_Hub")
        {
           
        }


        PlayerControls _p = FindAnyObjectByType<PlayerControls>();
        if (_p != null)
        {
            playerObj = _p.gameObject;
            if (playerObj.GetComponent<GriffinMinigameMovement>() != null)
            {
                //Account for the fact that the griffin movement also uses player controls
                playerObj = null;
            }
        }


        if (playerObj != null && lastKnownPlayerPos != Vector3.zero)
        {
            playerObj.transform.position = lastKnownPlayerPos;
        }

    }

    //Joystick side save
    public void ChangeJoystick()
    {
        saveManager.SaveJoystickSide(joystickCheck.isOn);
        leftSideControls = joystickCheck.isOn;
    }

    public void LoadMainHub()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }

    public void LoadGame()
    {

    }

    public void Credits()
    {
        mainMenuUI.SetActive(false);
        creditsMenuUI.SetActive(true);
    }

    public void HelpMenu()
    {
        mainMenuUI.SetActive(false);
        helpMenuUI.SetActive(true);
    }

    public void OptionsMenu()
    {
        mainMenuUI.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReturnFromHelp()
    {
        mainMenuUI.SetActive(true);
        helpMenuUI.SetActive(false);
    }

    public void Back()
    {
        mainMenuUI.SetActive(true);
        creditsMenuUI.SetActive(false);
    }

    public void ReturnFromOptions()
    {
        mainMenuUI.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void ChangeVolume()
    {
        audioVolume = volumeSlider.value;
    }


    #region Time progression
    /// <summary>
    /// Change the amount of time left in the day, can be used either to subtract/add based on events or subtract/add based on real timer
    /// </summary>
    /// <param name="_changeTime">Negative if taking away time, positive if adding time back for next day</param>
    public void ChangeTimeOfDayLeft(int _changeTime)
    {
        
        timeInDayLeft += _changeTime;
        sanctuaryManager.BreakThings(_changeTime);
        saveManager.SaveTime();
    }

    public void ChangeDay()
    {
        daysPassed++;
        timeInDayLeft = 12;
        //Debug.Log(daysPassed + "-" + timeInDayLeft);

        saveManager.SavePets();
        BasePet[] _loadedPets = FindObjectsByType<BasePet>(FindObjectsSortMode.None);
        for (int i = 0; i < _loadedPets.Length; i++)
        {
            for (int j = 0; j < petManager.currentPetNames.Count; j++)
            {
                string tmp = _loadedPets[i].petName + " (" + _loadedPets[i].type.ToString() + ")";
                if (tmp == petManager.currentPetNames[j])
                {
                    saveManager.SavePetData(j, _loadedPets[i]);
                }
                else
                {
                    //Debug.Log(_loadedPets[i].petName + "---" + petManager.currentPetNames[j] + " are not the same!");
                }
            }
        }

        saveManager.SaveTime();
        //sanctuaryManager.StartAllBreakCycles();
    }
    #endregion

    #region Getting and setting
    public void GetCurrency()
    {

    }

    public void SetCurrency(float _newCurrency)
    {
        currency = _newCurrency;
    }

    public void SetTime(int _days, int _time)
    {
        daysPassed = _days;
        timeInDayLeft = _time;  
    }
    

    public void SetNumpets(int _numPets)
    {
       // allPets = new List<BasePet>()_numPets
    }

    #endregion

    #region Scene loading
    private void ReturnToGameScene(string _sceneToLoad)
    {
        if(timeInDayLeft == 0)
        {
            ChangeDay();
        }

        StartCoroutine(WaitToLoadScene(_sceneToLoad));
    }

    public void PrepareForMinigame(int _tC)
    {
        //Randmoization of hours

        thisTimeCost = _tC;
        //Debug.Log("thisTimeCost: " + thisTimeCost);
        ChangeTimeOfDayLeft(-thisTimeCost);
        //saveManager.ChangeSaveTime(daysPassed, timeInDayLeft);
        if(playerObj != null)
            lastKnownPlayerPos = playerObj.transform.position;
        saveManager.SaveAll();
    }

    public IEnumerator WaitToLoadScene(string _sceneToLoad)
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(_sceneToLoad);
    }

    /// <summary>
    /// Enter and exit seperate4 sections of the hub
    /// </summary>
    public void GoToNewAreaHub(string _areaName)
    {
        lastKnownPlayerPos = Vector3.zero;
        saveManager.SaveAll();
        SceneManager.LoadScene(_areaName);
    }



    #endregion

    #region Access Variables

    public List<BasePet> ReturnAllPets()
    {
        return allPets;
    }

    public BasePet ReturnPet(int _index)
    {
        return allPets[_index];
    }

    public int ReturnNumPets()
    {
        return allPets.Count;
    }

    //Scoring getter/setter
    public float GetScoreMult()
    {
        return scoreMult;
    }

    public void SetScoreMult(float _mult)
    {
        scoreMult = _mult;
    }


    #endregion
}
