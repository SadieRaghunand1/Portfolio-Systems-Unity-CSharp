using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public KeyCode key;
    public bool cameraOpen;
    public bool journalOpen;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera inGameCamera;
    [SerializeField] private Canvas cameraUI;
    [SerializeField] private Canvas nonCamUI;
    [SerializeField] private Recticle reticle;
    private InGameCamera gameCamScript;
    private PlayerController playerController;

    public List<Texture2D> currentHeldImages; //Save not images, but textures as images, currently held, cleared when deposited in base

    [Header("Different bird images kept bw sessions")] //Keep this way or can access images from app data? Save these to a json file?
    public List<List<Texture2D>> listOfAllTextures;
    public List<Texture2D> noBirdImages;
    public List<Texture2D> birdType1Images;
    public List<Texture2D> birdType2Images;

    [Header("File paths for images")]
    public List<List<string>> listOfAllPaths;
    public List<string> bird1Paths;
    public List<string> bird2Paths;

    [Header("Scoring")]
    public List<int> score; //Each picture held adds an element to the list, clears when the pictures are deposited, indexes must align with currentHeldImages
    [SerializeField] float min1; //_score less than will get 0, greater or equal will get 1 star
    [SerializeField] float min2; //_score greater or equal will get 2 stars
    [SerializeField] float min3; //_score greater or equal will get 3 stars
    [SerializeField] float min4; //_score greater or equal will get 4 stars
    [SerializeField] float min5; //_score greater or equal will get 5 stars


    [Header("Other")]
    [SerializeField] private JournalManager journalManager;
    [SerializeField] private Canvas deletionMenuObj;
    [SerializeField] private ImageDeletion imageDeletion;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        gameCamScript = FindFirstObjectByType<InGameCamera>();
        playerController = FindAnyObjectByType<PlayerController>();

        //Initialize path lists
        bird1Paths = new List<string>();
        bird1Paths.Add("");
        bird2Paths = new List<string>();
        bird2Paths.Add("");

        LoadExistingJournalImage("Bird 1", 0);
        LoadExistingJournalImage("Bird 2", 1);

        LoadNonJournalImages("Bird 1", 0);
        LoadNonJournalImages("Bird 2", 1);

        //Initialize master lists of images and paths
        listOfAllTextures = new List<List<Texture2D>>();
        listOfAllPaths = new List<List<string>>();

        //Initialize texture list w values
        listOfAllTextures.Add(birdType1Images);
        listOfAllTextures.Add(birdType2Images);

        //Initialize path list w values
        listOfAllPaths.Add(bird1Paths);
        listOfAllPaths.Add(bird2Paths);
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (journalOpen)
            {
                CloseJournal();
            }
            else if (!journalOpen)
            {
                OpenJournal();
            }
        }

        if(Input.GetKeyDown(key))
        {
            if (cameraOpen)
            {
                CloseCamera();
            }
            else if (!cameraOpen) 
            {
                OpenCamera();
            }
        }


        //Temporary input for deletion menu: D on keyboard
        if(Input.GetKeyDown(KeyCode.P))
        {
            if(deletionMenuObj.enabled)
            {
                deletionMenuObj.enabled = false;
                imageDeletion.enabled = false;
            }
            else if (!deletionMenuObj.enabled)
            {
                deletionMenuObj.enabled = true;
                imageDeletion.enabled = true;
            }
        }
       

    }

    void OpenCamera()
    {
        mainCamera.enabled = false;
        inGameCamera.enabled = true;
        cameraUI.enabled = true;
        nonCamUI.enabled = false;
        cameraOpen = true;
        playerController.playerCamera = inGameCamera;
        if(gameCamScript.subject != null && gameCamScript.subject.Count > 0)
        {
            
            reticle.SetBirdInFocus(gameCamScript.subject[0]);
        }

        gameCamScript.MatchReticlesToBirds();

    }

    void CloseCamera()
    {
        mainCamera.enabled = true;
        gameCamScript.DeleteAllReticles();
        inGameCamera.enabled = false;
        cameraUI.enabled = false;
        nonCamUI.enabled = true;
        cameraOpen = false;
        playerController.playerCamera = mainCamera;

    }


    void OpenJournal()
    {
        journalManager.enabled = true;
        journalManager.gameObject.GetComponent<Canvas>().enabled = true;
        journalOpen = true;
    }

    void CloseJournal() 
    {
        journalManager.enabled = false;
        journalManager.gameObject.GetComponent<Canvas>().enabled = false;
        journalOpen = false;
    }


    public int CalculateScore(int _parts, float _distance, float _angle)
    {
        float _score = 0;
        
        //_parts is 0-3, 0 being worst and 3 being best
        //_distance -> larger distance is worse, shorter distance is better
        //Subtract distance from 100, base score, then add (_parts * 10)
        //Extra points for how many birds are in the photo
        _score = (100 - _distance) + (_parts * 10) + _angle + ((gameCamScript.GetNumPicSubjects() - 1) * 1.5f);
       // Debug.Log("Score before stars = " + _score);
        //From there convert to 5 star system, so _score is a number 0-5

        int _stars;

        if (_score < min1) //if score is less than the minimum to get 1 star
        {
            
            _stars = 0;
        }
        else if(_score >= min1 && _score < min2) //if score is greater than or equal to the minimum to get 1 star and less than the minimum to get 2 stars
        {
            
            _stars = 1;
        }
        else if (_score >= min2 && _score < min3) //if score is greater than or equal to the minimum to get 2 stars and less than the minimum to get 3 stars
        {
            
            _stars = 2;
        }
        else if (_score >= min3 && _score < min4) //if score is greater than or equal to the minimum to get 3 stars and less than the minimum to get 4 stars
        {
            
            _stars = 3;
        }
        else if (_score >= min4 && _score < min5) //if score is greater than or equal to the minimum to get 4 stars and less than the minimum to get 5 stars
        {
            
            _stars = 4;
        }
        else if (_score >= min5) //if the score is greater or equal to the minimum to get 5 stars
        {
            
            
            _stars = 5;
        }
        else
        {
            _stars = 0;
            Debug.Log("No score assigned");
        }

        Debug.Log("Stars: " + _stars);
        //If no bird
       /* if (gameCamScript.subject == null)
        {
            _stars = 0;
        }*/

        //Debug.Log("Score after stars = " + _stars);

        score.Add(_stars);
        return _stars;

       
    }

    public void ClearHeldImages() //On picture deposit in hub
    {
        currentHeldImages.Clear();
        score.Clear();
    }
    




    /// <summary>
    /// Set journal image for a bird from the photos in previous sessions, call when there is a change in the top photo, and when game starts
    /// </summary>
    public void LoadExistingJournalImage(string _birdName, int _birdIndex)
    {
       

        //Set top photo as the first index for the bird image list
        string _path = null;

        //Find the path for the top photo
        for (int i = -1; i < 6; i++)
        {
            _path = Application.persistentDataPath + "/" + _birdName + "/" + i + " TopPhoto.jpeg";

            if (File.Exists(_path))
            {
                
                break;
            }
            else //The top photo does not exist at this score
            {
                _path = null;
                continue;
            }

        }

        if(_path != null)
        {
            //Make new texture
            Texture2D _tex = new Texture2D(2, 2);  //Default vals
            //TextAsset _imageBytes = new TextAsset(_path);
            byte[] _imageBytes = File.ReadAllBytes(_path);
            //Debug.Log(_imageBytes);
            bool _textSuccess;
            _textSuccess = ImageConversion.LoadImage(_tex, _imageBytes);
            //Debug.Log(_textSuccess);
            //Set new texture to beginning of list
            if(_birdIndex == 0)
            {
                birdType1Images[0] = _tex;
                bird1Paths[0] = _path;
            }
            else if(_birdIndex == 1)
            {
                birdType2Images[0] = _tex;
                bird2Paths[0] = _path;
            }
            
        }

      

    } //END LoadExistingJournalImage()

    public void LoadNonJournalImages(string _birdName, int _birdIndex)
    {
        //Add all other existing images of a bird that are not the top photos as the rest of the list

        
        string _path = null;

        //Find the path for the top photo
            //Temp - Set max num photos to 20, for optimization purposes later set a value keeping track of how many images are in each folder
         for(int j = 0; j < 20; j++) //
         {
             _path = Application.persistentDataPath + "/" + _birdName + "/" + j + "_" + _birdName + ".jpeg";
            
            if (File.Exists(_path))
            {
                
             //Make new texture
                Texture2D _tex = new Texture2D(2, 2);  //Default vals
                                                           //TextAsset _imageBytes = new TextAsset(_path);
                 byte[] _imageBytes = File.ReadAllBytes(_path);
                 //Debug.Log(_imageBytes);
                 bool _textSuccess;
                 _textSuccess = ImageConversion.LoadImage(_tex, _imageBytes);
                // Debug.Log(_textSuccess);
                 //Set new texture to beginning of list
                if (_birdIndex == 0)
                {
                    
                    birdType1Images.Add( _tex );
                     bird1Paths.Add( _path );
                    
                }
                else if (_birdIndex == 1)
                {
                    birdType2Images.Add( _tex );
                    bird2Paths.Add( _path );
                }
            }
            else //The photo does not exist with this name
            {
               // Debug.Log(_path + " does not exist");
                continue;
            }
            
         }

       
    }


    #region Helpers

    

    #endregion

}
