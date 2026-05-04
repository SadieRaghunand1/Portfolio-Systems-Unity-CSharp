using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public GameManager gameManager;
    public PetManager petManager;
    public SanctuaryManager sanctuaryManager;

    float currency; //"Currency"
    int daysPassed; //"Days"
    int timeLeftInDay; //"TimeLeft"
    [SerializeField] int numPets; //"NumPets"
    int totalPets; //"TotalPets"
    float sanctuaryScore; //"SanctuaryScore"
    [SerializeField] bool inStory; //"InStory", save at int where 0 - false, 1 - true
    //Also - "LeftStick", 0 - false, 1 - true
    //"Seen dialogue" - to be set to true/1 after seeing intro dialogue for first time, set false/0 on new game

    //, pop off if pet is rehabilitated, push in as pet is added.length should be mapped to numpets
    //Used to find the text files containing the data for each currently held pet
    //Files for old pets should be deleted
    //Start with first ever pet at 0, increase as pets are added.  Do not roll back to old numbers,
    //So if there are three pets (0, 1, 2), and 0 is rehabilitated, pet 1 is still 1, not 0
    //The precise names will be kept in a seperate text file of data file paths
    //Update any time a pet is added or removed
    [SerializeField] List<string> petFileNames = new List<string>(); //Saved with "Pet#" format, individual names of pet files
    string allPetFileNames;  //File containing all pet file names


    //Within pet files, order of data should be:
    //Line 1 - Type
    //Line 2 - Name
    //Line 3 - Affection score
    //Line 4 - Last known position
    //Line 5 - Number pet ever

    [SerializeField] private GameObject griffinData;
    [SerializeField] private GameObject krakenData;
    [SerializeField] private GameObject cerberusData;

    public bool openGame = true; //Whether loading in from the menu or just returning to a scene

    //For uncomplicated variables, use player prefs
    //More complicated, use txt files


    private static System.Random random = new System.Random();

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        allPetFileNames = Application.persistentDataPath + "/AllPetFiles.txt";

        
    }

    #region Save

    //Currency
    private void SaveCurrency()
    {
        PlayerPrefs.SetFloat("Currency", currency);
        PlayerPrefs.Save();
    }


    //Time progression
    public void SaveTime()
    {
        daysPassed = gameManager.daysPassed;
        timeLeftInDay = gameManager.timeInDayLeft;
        PlayerPrefs.SetInt("Days", daysPassed);
        PlayerPrefs.Save();
        PlayerPrefs.SetInt("TimeLeft", timeLeftInDay);
        PlayerPrefs.Save();
    }

    //Sanctuary score
    public void SaveSanctuaryScore()
    {
        sanctuaryScore = sanctuaryManager.ReturnSanctuaryScore();
        PlayerPrefs.SetFloat("SanctuaryScore", sanctuaryScore);
        PlayerPrefs.Save();
    }

    private void SaveStory()
    {
        int _val = (inStory) ? 1 : 0;
        PlayerPrefs.SetInt("InStory", _val);
        PlayerPrefs.Save();
    }

    private void SaveDialogue()
    {
        PlayerPrefs.SetInt("SeenDialogue", 1);
        PlayerPrefs.Save();
    }

    //Which side joystick is on
    public void SaveJoystickSide(bool val)
    {
        int i = 1;
        if(val)
        {
            i = 1;
        }
        else if (!val)
        {
            i = 0;
        }

        PlayerPrefs.SetInt("LeftStick", i);
        PlayerPrefs.Save();
        Debug.Log("Save joystick as " + PlayerPrefs.GetInt("LeftStick"));
    }

    //Pets, all pets
    public void SavePets()
    {
        Debug.Log("RUN SAVE ALL PETS");

        //numPets = gameManager.ReturnNumPets();
       // Debug.Log("Call from gm, num pets is " + numPets);
        PlayerPrefs.SetInt("NumPets", numPets);
        PlayerPrefs.Save();

        totalPets = petManager.ReturnTotalPets();
        PlayerPrefs.SetInt("TotalPets", totalPets);
        PlayerPrefs.Save();
        
        if(!File.Exists(allPetFileNames))
        {

            File.CreateText(allPetFileNames).Dispose();

        }


        //Open file
        string[] allLines = File.ReadAllLines(allPetFileNames);
        //Debug.Log("Length of all lines = " + allLines.Length);
        
        petFileNames.Clear();

        //Push all file names for pet data to list
        for (int i = 0; i < allLines.Length; i++)
        {
            //Add each file name to list
            //Debug.Log("Run loop " + i);
            petFileNames.Add(allLines[i]); //This here is the problem!!!!
            

            //Debug.Log("Saving pets " + allLines[i]);

        }

        if (allLines.Length < numPets) //account for extra pets since last checked
        {
            for (int i = numPets; i > allLines.Length; i--)
            {
                //Debug.Log("Saving " + i);
                //Save any new pets file names
                Debug.Log("Adding a previously unsaved pet " + i);
                petFileNames.Add(Application.persistentDataPath + "/Pet" + i + ".txt");
                
                
            }
        }
        

        File.WriteAllText(allPetFileNames, string.Empty);
        File.WriteAllLines(allPetFileNames, petFileNames);
        //petFileNames.Clear();
        

    }

    //Pets, data for each pet
    //Save to text file
    public void SavePetData(int index, BasePet _pet)
    {
       // Debug.Log("Index is " + index);
        if (!File.Exists(petFileNames[index]))
        {
            //Create file
            //Add to list of file names
            //Ohsnap this overrides other pets if they aren't all in the same scene
            ////Chat what if we just checked if the name was the same to determine if this pet has been added before
            Debug.Log("Creating " + petFileNames[index]);
            File.CreateText(petFileNames[index]).Dispose();
            
        }
        else
        {
            
                Debug.Log("File" + petFileNames[index] + " exists");
            
        }

        //Check here
        string[] _data = _pet.ReturnPetDataForSave();
        File.WriteAllLines(petFileNames[index], _data); //Sharing data error
    }


    public void ManuallySaveScoreRaceGame(int index)
    {
        // Debug.Log("Index is " + index);
        if (!File.Exists(petFileNames[index]))
        {
            //Create file
            //Add to list of file names
            //Ohsnap this overrides other pets if they aren't all in the same scene
            ////Chat what if we just checked if the name was the same to determine if this pet has been added before
            Debug.Log("Creating " + petFileNames[index]);
            File.CreateText(petFileNames[index]).Dispose();

        }
        else
        {

            Debug.Log("File" + petFileNames[index] + " exists");

        }

        //Check here
        string[] _data = File.ReadAllLines(petFileNames[index]);
        _data[2] = petManager.currentAffectionScores[index].ToString();
        File.WriteAllLines(petFileNames[index], _data); //Sharing data error
    }

    /// <summary>
    /// Called on save button, saves whole game
    /// </summary>
    public void SaveAll()
    {
        SaveCurrency();
        SaveTime();
        SaveSanctuaryScore();
        SaveStory();
        SaveDialogue();
        SavePets();
        BasePet[] _loadedPets = FindObjectsByType<BasePet>(FindObjectsSortMode.None);
        for(int i = 0; i < _loadedPets.Length; i++)
        {
            for(int j = 0; j < petManager.currentPetNames.Count; j++)
            {
                string tmp = _loadedPets[i].petName;
                if (tmp == petManager.currentPetNames[j])
                {
                    SavePetData(j, _loadedPets[i]);
                }
                else
                {
                    Debug.Log(tmp + "---" + petManager.currentPetNames[j] + " are not the same!");
                }
            }
        }

        /*for(int i = 0; i < petManager.currentPetNames.Count; i++)
        {
            if(gameManager.ReturnAllPets()[i] != null)
                SavePetData(i, gameManager.ReturnAllPets()[i]);
        }*/
    }

    #endregion

    #region Load
    //Currency
    private void LoadCurrency()
    {
        if(PlayerPrefs.HasKey("Currency"))
        {
            currency = PlayerPrefs.GetFloat("Currency");
            gameManager.SetCurrency(currency);
        }
    } 
    

    //Time progression
    private void LoadTime()
    {
        if(PlayerPrefs.HasKey("Days") && PlayerPrefs.HasKey("TimeLeft"))
        {
            if (PlayerPrefs.GetInt("Days") == 0 && PlayerPrefs.GetInt("TimeLeft") == 0) //Check for invalid times
            {
                daysPassed = 0;
                timeLeftInDay = 12;
                SaveTime();
                gameManager.SetTime(daysPassed, timeLeftInDay);
            }

            daysPassed = PlayerPrefs.GetInt("Days");
            timeLeftInDay = PlayerPrefs.GetInt("TimeLeft");

            gameManager.SetTime(daysPassed, timeLeftInDay);
        }
        else
        {
            daysPassed = 0;
            timeLeftInDay = 12;
            SaveTime();
            gameManager.SetTime(daysPassed, timeLeftInDay);
        }

        
    }


    //Sanctuary score
    private void LoadSanctuaryScore()
    {
        if(PlayerPrefs.HasKey("SanctuaryScore"))
        {
            sanctuaryScore = PlayerPrefs.GetFloat("SanctuaryScore");
            sanctuaryManager.SetSanctuaryScoreLoad(sanctuaryScore);
        }
    }

    //Whether the player has finishd the story or not
    private void LoadStory()
    {
        if(PlayerPrefs.HasKey("InStory"))
        {
            int i = PlayerPrefs.GetInt("InStory");
            inStory = (i == 0) ? false : true;
        }
    }


    //Side joystick
    public bool LoadJoystickSide()
    {
        if(PlayerPrefs.HasKey("LeftStick"))
        {
            int _val = PlayerPrefs.GetInt("LeftStick");

            if(_val == 1)
            {
                Debug.Log(PlayerPrefs.GetInt("LeftStick"));
                return true;
            }
            else
            {
                Debug.Log(PlayerPrefs.GetInt("LeftStick"));
                return false;
            }
        }
        else
        {
            Debug.Log("No player pref yet, load " + PlayerPrefs.GetInt("LeftStick"));
            PlayerPrefs.SetInt("LeftStick", 1);
            PlayerPrefs.Save();
            return true;
        }
    }

    //Pet data
    private void LoadAllPets()
    {
        Debug.Log("RUN LOAD ALL PETS " + SceneManager.GetActiveScene().name);
        //Number of pets currently held
        if(PlayerPrefs.HasKey("NumPets"))
        {
            numPets = PlayerPrefs.GetInt("NumPets");
            //game manager thing idk
        }

        //Number of pets total
        if(PlayerPrefs.HasKey("TotalPets"))
        {
            totalPets = PlayerPrefs.GetInt("TotalPets");
            petManager.SetTotalPets(totalPets);
        }

       //Handle LoadManager in hub scene's list of all pets
       LoadManager _lM = FindAnyObjectByType<LoadManager>();
       
        //For the file names, iterate through and load in data for each pet, add to pet managers and game manager when necessary
        if(File.Exists(allPetFileNames))
        {
            string[] _petFiles = File.ReadAllLines(allPetFileNames);

            for(int i = 0; i < _petFiles.Length; i++)
            {
                //Get the string corresponding to this pet's save file
                string[] _petDataRead = File.ReadAllLines(_petFiles[i]); //Cannot access files, not created ig
                //Load in pet
                GameObject _thisPet = petManager.CreatePetOfType(_petDataRead[0]);
                
                //Pet is in save file, but is not located in this habitat
                if(_thisPet == null)
                {
                    //Pet exists just not in this environment, conduct UI things
                    //string _passName = _petDataRead[1] + " (" + _petDataRead[0].ToString() + ")";
                    string _passName = _petDataRead[1];
                    float _passAff;
                    //Int32.TryParse(_petDataRead[2], out _passAff);
                    _passAff = Single.Parse(_petDataRead[2]);
                    petManager.AddToPetNames(_passName, _passAff);
                    string _c = petManager.ReturnUILine(_passName, _passAff);
                    FindAnyObjectByType<MainUIManager>().UpdateScoreUI(_passName, i);
                    //Skip rest, pet doesn't belong here
                    continue;
                }

                //Finish loop if pet does belong in this habitat
                int _tmpAff;
                Int32.TryParse(_petDataRead[2], out _tmpAff);
                //string _pass = _petDataRead[1] + " (" + _petDataRead[0].ToString() + ")";
                string _pass = _petDataRead[1];
                petManager.AddToPetNames(_pass, _tmpAff);
                BasePet _tp = _thisPet.GetComponent<BasePet>(); //Null error
                if(_lM != null)
                {
                    //Add to load manager
                    _lM.InitializeAllPets(_tp);
                }
                HandleIndividualPetData(_tp, _petDataRead);
                Debug.Log("Previously loaded in pet - " + _tp.petName);
                //Create individualized interaction UI
                MainUIManager _ui = FindAnyObjectByType<MainUIManager>();
                if(_ui != null)
                {
                    _ui.CreateInteractablePetUI(_thisPet);
                }

            }

            //No pets generated for this region
            if(gameManager.ReturnAllPets().Count == 0)
            {
                LoadStoryPets(_lM);
            }
            
        }
        //No pets, new game
        else
        {
            //Here goes nothing
            InitializeFirstTimePetData();
        }

    } 

    public void LoadAll()
    {
        LoadCurrency();
        LoadTime();
        LoadSanctuaryScore();
        LoadStory();
        LoadAllPets();
        openGame = false;
    }

    #endregion

    #region Clear data

    public void NewGame()
    {
        //Currency
        if(PlayerPrefs.HasKey("Currency"))
        {
            PlayerPrefs.SetInt("Currency", 0);
            PlayerPrefs.Save();
        }
        //Time
        if (PlayerPrefs.HasKey("Days"))
        {
            PlayerPrefs.SetInt("Days", 1);
            PlayerPrefs.Save();
        }
        if (PlayerPrefs.HasKey("TimeLeft"))
        {
            PlayerPrefs.SetInt("TimeLeft", 12);
            PlayerPrefs.Save();
        }
        //Sanctuary
        if (PlayerPrefs.HasKey("SanctuaryScore"))
        {
            PlayerPrefs.SetInt("SanctuaryScore", 0);
            PlayerPrefs.Save();
        }
        //Story
        if(PlayerPrefs.HasKey("InStory"))
        {
            PlayerPrefs.SetInt("InStory", 1);
            inStory = true;
            PlayerPrefs.Save();
        }
        //Dialogue
        if(PlayerPrefs.HasKey("SeenDialogue"))
        {
            PlayerPrefs.SetInt("SeenDialogue", 0);
            PlayerPrefs.Save();
        }

        //Pets
        if (PlayerPrefs.HasKey("NumPets"))
        {
            PlayerPrefs.SetInt("NumPets", 0);
            PlayerPrefs.Save();
        }

        if (PlayerPrefs.HasKey("TotalPets"))
        {
            PlayerPrefs.SetInt("TotalPets", 0);
            PlayerPrefs.Save();
        }

        if (File.Exists(allPetFileNames))
        {
            string[] _petFiles = File.ReadAllLines(allPetFileNames);

            for (int i = 0; i < _petFiles.Length; i++)
            {
                if (File.Exists(_petFiles[i]))
                {
                    File.Delete(_petFiles[i]);
                }

            }

            File.Delete(allPetFileNames);
        }

        gameManager.LoadMainHub();

    }



    #endregion

    #region Helper methods

    //Assign data to pet
    private void HandleIndividualPetData(BasePet _pet, string[] _petData)
    {

        BasePet.Type _petType = BasePet.Type.Griffin; //Default to griffin to make the error go away
        string _petName = _petData[1];
        float _petAffection;
        //Int32.TryParse(_petData[2], out _petAffection);
        _petAffection = Single.Parse(_petData[2]);
        Vector3 _petPos = Vector3.zero; //Default to zero
        int _petNumEver = 0;
        Int32.TryParse(_petData[4], out _petNumEver);

        //properly assign pet type
        switch (_petData[0])
        {
            case "Griffin":
                _petType = BasePet.Type.Griffin;
                if (!petManager.backFromMinigame)
                {
                    petManager.griffinAffection = _petAffection; //Temp
                    petManager.backFromMinigame = false;
                }
                else
                {
                    petManager.currentAffectionScores[petManager.nameIdxThisGame] = petManager.griffinAffection;
                    _petAffection = petManager.griffinAffection;
                    petManager.backFromMinigame = false;
                }
                    
                break;
            case "Kraken":
                _petType = BasePet.Type.Kraken;
                if (!petManager.backFromMinigame)
                {
                    petManager.krakenAffection = _petAffection; //Temp?
                    petManager.backFromMinigame= false;
                }
                else
                {
                    petManager.currentAffectionScores[petManager.nameIdxThisGame] = petManager.krakenAffection;
                    _petAffection = petManager.krakenAffection;
                    petManager.backFromMinigame = false;
                }
                    
                break;
            case "Cerberus":
                _petType= BasePet.Type.Cerberus;
                if (!petManager.backFromMinigame)
                {
                    petManager.cerberusAffection = _petAffection; //Temp?
                    petManager.backFromMinigame = false;
                }   
                else
                {
                    
                    petManager.currentAffectionScores[petManager.nameIdxThisGame] = petManager.cerberusAffection;
                    _petAffection = petManager.cerberusAffection;
                    petManager.backFromMinigame = false;
                }
                    
                break;
        }

        //Properly assign position
        if (_petData[3].StartsWith("(") && _petData[3].EndsWith(")"))
        {
            _petData[3] = _petData[3].Substring(1, _petData[3].Length - 2);
        }
 
        string[] sArray = _petData[3].Split(',');
        _petPos = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));


        //Apply all the data
        _pet.AssignAllPetData(_petType, _petName, _petAffection, _petPos, _petNumEver);
    }

    public void ChangeSaveTime(int _d, int _t)
    {
        daysPassed = _d;
        timeLeftInDay = _t;
    }

    /// <summary>
    /// Mainly a debugging function
    /// </summary>
    public bool CheckForPets()
    {
        if (!File.Exists(allPetFileNames))
        {
            return false;
        }

        string[] paths = File.ReadAllLines(allPetFileNames);
        if (paths.Length < 3 && inStory)
        {
            return false;
        }

        return true;
    }

    public void ChangeNumPets(int _val) //Increase when adding a pet, decrease when a pet leaves sanctuary
    {
        numPets += _val; 
    }

    public void InitializeFirstTimePetData()
    {
        //Load info for pets at begining of new game without initializing actual pet objects in scene permanetly


        WritePetData(1, "Griffin");
        WritePetData(2, "Kraken");
        WritePetData(3, "Cerberus");
    }

    //This might replace above function
    /// <summary>
    /// Create file with data for a pet without actually creating the pet in the scene
    /// </summary>
    /// <param name="_petTotal">The number in total this pet will be</param>
    /// <param name="_petType">The type of pet it is, in string form</param>
    void WritePetData(int _petTotal, string _petType)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        //string _randName = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
        string _randName = petManager.NamePet();
        _randName += " (" + _petType + ")";
        string _fileName = Application.persistentDataPath + "/Pet" + _petTotal + ".txt";
        File.CreateText(_fileName).Dispose();
        petFileNames.Add(_fileName);
        string[] _fileText =
        {
            _petType,
            _randName,
            0.ToString(),
            Vector3.zero.ToString(),
            _petTotal.ToString()
        };

        petManager.AddToPetNames(_randName, 0);
        File.WriteAllLines(allPetFileNames, petFileNames);
        File.WriteAllLines(_fileName, _fileText);

        if(petManager.totalPetsEver != _petTotal)
        {
            petManager.totalPetsEver = _petTotal;
            numPets++;
            totalPets = _petTotal;
        }
    }


    public bool ReturnStory()
    {
        return inStory;
    }

    void LoadStoryPets(LoadManager _lm)
    {
        if(inStory)
            _lm.CreateNewPets();
    }

    public int GetNumPets()
    {
        return numPets;
    }

    #endregion


}
