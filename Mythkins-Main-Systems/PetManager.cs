using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PetManager : MonoBehaviour
{
    public bool original;
    [SerializeField] private GameManager gameManager;

    [Header("Pet prefabs")]
    [SerializeField] private GameObject griffinPrefab;
    [SerializeField] private GameObject krakenPrefab;
    [SerializeField] private GameObject cerberusPrefab;

    //DOn't know if this will be kept between now and later,
    //For now singleton to hold affection scores
    //Code here is very prototype-esque
    //This goes for all things with tags right now: in future do not id by tag, id by script (when base pet and such have been expanded into children)

    //Need to refactor to allow for multiple instances of one kind of pet
    [Header("Affection")]
    public float griffinAffection;
    public float krakenAffection;
    public float cerberusAffection;
    public int neededAffectionForEnd = 150;

    [Header("Names")]
    string[] allNames = { 
        "Trudy",
        "Janine",
        "Rebecca",
        "Wilbur",
        "Kelley",
        "Teodoro",
        "Alicia",
        "Carlton",
        "Liza",
        "Kareem",
        "Danielle",
        "Carol",
        "Jarrett",
        "Lindsey",
        "Elliot",
        "Alma",
        "Roman",
        "Shayne",
        "Fanny",
        "Dee",
        "Gonzalo",
        "Winifred",
        "Elisha",
        "Nikki",
        "Shelia",
        "Lino",
        "Virginia",
        "Kerry",
        "Emerson",
        "Earnest"
    };
    List<string> unclaimedNames;

    [Header("Other")]
    public int totalPetsEver; //For now, manually set to 3
    public List<string> currentPetNames; //All currently housed pets' names
    public List<float> currentAffectionScores; //All currently housed pets' aff scores
    [SerializeField] private SaveManager saveManager;
    LoadManager loadManager;
    [SerializeField] SanctuaryManager sanctManager;
    private static System.Random random = new System.Random();

    [Header("Minigame stuff")]
    public bool backFromMinigame;
    public int nameIdxThisGame; //WHich index in current name/aff score corresponds w/this pet's minigame
    string enterPetMinigame; //What pet tag was interacted with, open minigame for
    public UnityEvent LoadAffectionScores = new UnityEvent();
    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
       // ReadAllNames();

    }

    public void InitOnLoad(List<BasePet> _allPets = null)
    {

        //Check for existing game manager
        if (FindObjectsByType<PetManager>(FindObjectsSortMode.None).Length > 1)
        {
            //Debug.Log(FindObjectsByType<PetManager>(FindObjectsSortMode.None).Length);
            if(!original)
                Destroy(gameObject);
        }
        original = true;

        if(_allPets != null)
        {
            StartCoroutine(WaitToUpdateScore(_allPets));
        }

        loadManager = FindAnyObjectByType<LoadManager>();

        /*if(saveManager.CheckForPets() == false)
        {
            MakeNewPet("Griffin");
            MakeNewPet("Kraken");
            MakeNewPet("Cerberus");
        }*/
        
        //LoadAffectionScores.Invoke();
    }

    public void ChangeAffectionScore(int _change, bool _race = false)
    {
        if(!_race)
        {
            Debug.Log("Call from cghange affection score " + enterPetMinigame);
            switch (enterPetMinigame)
            {
                case "Griffin":
                    griffinAffection += ((_change + (sanctManager.sanctuaryScore / 5)) * gameManager.GetScoreMult());
                    Debug.Log("Griffin affection" + griffinAffection);
                    break;
                case "Kraken":
                    krakenAffection += (_change + ((sanctManager.sanctuaryScore / 5)) * gameManager.GetScoreMult());
                    Debug.Log("Kraken affection" + krakenAffection);
                    break;
                case "Cerberus":
                    cerberusAffection += ((_change + (sanctManager.sanctuaryScore / 5)) * gameManager.GetScoreMult());
                    Debug.Log("Cerb  affection" + cerberusAffection);
                    break;


            }
        }
        //Else this is the racing minigame
        else
        {
            if(PlayerPrefs.HasKey("selectedCreature"))
            {
                Debug.Log("Selected creature is " + PlayerPrefs.GetInt("selectedCreature"));
                switch(PlayerPrefs.GetInt("selectedCreature"))
                {
                    case 0: 
                        cerberusAffection += _change + (sanctManager.sanctuaryScore / 5);
                        currentAffectionScores[2] = cerberusAffection;
                        FindAnyObjectByType<SaveManager>().ManuallySaveScoreRaceGame(2);
                        Debug.Log("Cerb  affection" + cerberusAffection);
                        break;
                    case 1:
                        griffinAffection += _change + (sanctManager.sanctuaryScore / 5);
                        currentAffectionScores[0] = griffinAffection;
                        FindAnyObjectByType<SaveManager>().ManuallySaveScoreRaceGame(0);
                        Debug.Log("Griffin affection" + griffinAffection);
                        break;
                    case 2:
                        krakenAffection += _change + (sanctManager.sanctuaryScore / 5);
                        currentAffectionScores[1] = krakenAffection;
                        FindAnyObjectByType<SaveManager>().ManuallySaveScoreRaceGame(1);
                        Debug.Log("Kraken affection" + krakenAffection);
                        break;
                }

                FindAnyObjectByType<SaveManager>().SaveAll();
            }
        }
        
    }


    /// <summary>
    /// Handle creation of entirely new pets, so first three and any new ones once the main story is done
    /// </summary>
    public GameObject MakeNewPet(string _type)
    {
        GameObject _pet = CreatePetOfType(_type);

        if(_pet == null)
        {
            return null;
        }

        BasePet _bp = _pet.GetComponent<BasePet>();
        loadManager.InitializeAllPets(_bp);
        //Temporary name generation
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        //string _randName = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
        string _randName = NamePet();
        //Temporary position generation
        _bp.AssignAllPetData(_bp.type, _randName, 0, new Vector3(0, 1, 0), totalPetsEver);
        _bp.OnCreation();
        MainUIManager _ui = FindAnyObjectByType<MainUIManager>();
        _ui.CreateInteractablePetUI(_pet);
        saveManager.ChangeNumPets(1);
        string _pass = _randName + " (" + _bp.type.ToString() + ")";
        AddToPetNames(_pass, 0);
        Debug.Log("New pet created of type - " + _type);


        //Set UI
        _ui.SetUIOnPetCreation(_pass + ": 0");

        return _pet;
    }


    //Call on interact button click?
    public void GetWhichPet(string _tag)
    {
        enterPetMinigame = _tag;
    }

    public int ReturnTotalPets()
    {
        return totalPetsEver;
    }

    public void SetTotalPets(int _total)
    {
        totalPetsEver = _total;
    }

    protected IEnumerator WaitToUpdateScore(List<BasePet> _allPets)
    {
        //Debug.Log("Call wait to update score");
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < _allPets.Count; i++)
        {
            //Debug.Log("This pet is " + _allPets[i] + " - " + _allPets.Count);
            _allPets[i].Callback_LoadScore(this.gameObject.GetComponent<PetManager>());
        }
    }

    

    #region Helper
    /// <summary>
    /// Instantiate prefab of a type of pet
    /// </summary>
    public GameObject CreatePetOfType(string _type)
    {
        GameObject _createdPet = null;

        //Check which section of hub player has entered
        string _thisScene = SceneManager.GetActiveScene().name;


        switch (_type)
        {
            case ("Griffin"):
                if (_thisScene != "Lvl_Meadow")
                    break;
                //Debug.Log("In meadow, should instance griffin");
                _createdPet = Instantiate(griffinPrefab);
                break;
            case ("Kraken"):
                if (_thisScene != "Lvl_Oasis")
                    break;
                //Debug.Log("In oasis, should instance kraken");
                _createdPet = Instantiate(krakenPrefab);
                break;
            case ("Cerberus"):
                if (_thisScene != "Lvl_Volcano")
                    break;
                //Debug.Log("In volcano, should instance cerb");
                _createdPet = Instantiate(cerberusPrefab);
                break;
            default:
                return null;
        }
        Debug.Log("Created pet " + _createdPet); //Need to adjust this function to not be reliant on the scene name
        //Alternatively generate all data, not pet object?
        return _createdPet;
    }

    public void AddToPetNames(string _newName, float _aff)
    {
       // Debug.Log("Add name " + _newName);
        for (int i = 0; i < currentPetNames.Count; i++)
        {
            if(_newName == currentPetNames[i])
            {
                //Debug.Log("Same name");
                return;
            }
        }
        Debug.Log("RRARARRARAA" + _newName + _aff);
        currentPetNames.Add(_newName);
        currentAffectionScores.Add(_aff);
    }

    //Return the line for affection score
    public string ReturnUILine(int index)
    {
        return currentPetNames[index] + ": " + currentAffectionScores[index];
    }

    //overload
    public string ReturnUILine(string a, float b)
    {
        return a + ": " + b;
    }

    public void ChangeAffectionScoreInArray(string _thisPet, BasePet.Type _t)
    {
       // _thisPet = _thisPet + " (" + _t.ToString() + ")";
        
        for (int i = 0; i < currentPetNames.Count; i++)
        {
            //Debug.LogError(currentPetNames[i] + ", " + _thisPet);
            if (currentPetNames[i] == _thisPet)
            {
                nameIdxThisGame = i;
                
                return;
            }
        }
    }


    #endregion

    #region Naming

    public void ReadAllNames()
    {
        //= Resources.Load("AllPossiblePetNames");
        unclaimedNames = new List<string>();
        for(int i = 0; i < allNames.Length; i++)
        {
            unclaimedNames.Add(allNames[i]);
        }
    }

    public string NamePet()
    {
        int _idx = UnityEngine.Random.Range(0, unclaimedNames.Count);
        string _newName = unclaimedNames[_idx];
        unclaimedNames.RemoveAt(_idx);
        ReplenishNames();
        return _newName;
    }

    void ReplenishNames()
    {
        if (unclaimedNames.Count == 0)
        {
            for (int i = 0; i < allNames.Length; i++)
            {
                unclaimedNames.Add(allNames[i]);
            }
        }
    }
    

    #endregion
    
    #region End Condition

    public bool EndConditionCheck()
    {
        return (griffinAffection >= neededAffectionForEnd) && (cerberusAffection >= neededAffectionForEnd) && (krakenAffection >= neededAffectionForEnd);
    }

    #endregion

}
