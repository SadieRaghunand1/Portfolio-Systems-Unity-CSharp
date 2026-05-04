using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InGameCamera : MonoBehaviour
{
    [Header("Camera Pictures")]
    [SerializeField] private RawImage liveFeed;
    [SerializeField] private RawImage lastImage;
    [SerializeField] private RenderTexture liveRender;
    public Camera gameCam;

    private Texture2D lastImageTx;

    [SerializeField] private CameraManager camManager;

    [SerializeField] private float showTime;
    private float showTimeLeft = -1f;

    float shotsTaken; //Temp maybe? currently used to name images in file names

    [SerializeField] private AviaryManager aviaryManager;
    [Header("Zoom")]
    [SerializeField] private int zoomMin = 0;
    [SerializeField] private int zoomMax;
    private int zoomAmount = 0;

    private List<string> currentBirdName;

    [Header("Recticle and scoring")]
    [SerializeField] private List<RawImage> recticle;
    [SerializeField] private GameObject reticlePrefab;
    [SerializeField] private Canvas camCanvas;
    [SerializeField] private float rOffset;
    public List<GameObject> subject;
    public List<int> thisBirdId;
    [SerializeField] private Camera cam;
    public GameObject testHeight;
    [SerializeField]Vector3[] _corners = new Vector3[4];
    [SerializeField] Vector2[] _corners2d = new Vector2[4];


    //Note - add zoom in/out and make sure zoom is counted into scoring (too zoomed in is bad, make sure if unzoomed the bird would show up but if zoomed and not in actual picture then no points/not actually in frame)

    private void Start()
    {
        currentBirdName = new List<string>();
    }

    private void Update()
    {
        Zoom();
        CheckTimeLeft();
        //FindSubjectShortDistance();
    }


    private void CheckTimeLeft()
    {
        if(showTimeLeft > 0)
        {
            showTimeLeft -= Time.deltaTime;
            
            //Stop showing image taken, continue showing live feed
            if(showTimeLeft <= 0)
            {
                liveFeed.gameObject.SetActive(true);
                //lastImage.gameObject.SetActive(false);
            }
        }
    } //END CheckTimeLeft()

    public void TakePicture() //Note, may need to break down this method
    {
        List<string> _birdName = new List<string>();
        shotsTaken++;

        //Copy texture of live feed on call to last image window
        //Apparently only GPU side, more efficient way of doing this, so later down the line look into if there is a way to get away with this only?
        //Graphics.CopyTexture(liveRender, lastImageTx);

        //...
        //CPU, more costly operation, but saves actual image on texture ont disk
        //Asynchronously accesses rendered info and applies, then runs everything else within request readback function
        AsyncGPUReadback.Request(liveRender, 0, (AsyncGPUReadbackRequest action) =>
        {
            //Create new texture for new images so they can be saved to a list in game
            lastImageTx = new Texture2D(liveRender.width, liveRender.height, liveRender.graphicsFormat, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

            //Get rendered image from GPU transer to CPU
            lastImageTx.SetPixelData(action.GetData<byte>(), 0);
            lastImageTx.Apply();

            //Add to correct lists
            camManager.currentHeldImages.Add(lastImageTx);
            for(int i = 0; i < thisBirdId.Count; i++)
            {
                aviaryManager.AddBirdToAviaryList(thisBirdId[i], lastImageTx); //OG first parameter was GetBirdData()
                _birdName.Add(CheckBirdType(thisBirdId[i])); //OG parameter was GetBirdData()
            }
           

            lastImage.texture = lastImageTx;


            int _topScore = 0;
            string _savePicAs = ""; //This is the bird photo is saved under, currently not set anywhere
             //Scoring
            for(int i = 0; i < subject.Count; i++)
            {
                //Score kept is highest between all bird scores
                int _thisScore = subject[i].gameObject.GetComponent<BirdDataHandler>().CheckPictureClarity();
                _topScore = (_thisScore > _topScore) ? _thisScore : _topScore;
                if(_thisScore > _topScore)
                {
                    _topScore = _thisScore;
                    _savePicAs = subject[i].GetComponent<BirdDataHandler>().GetName();

                }
            }

            //Set score as zero
            if(subject.Count == 0)
            {
                Debug.Log("Subject null");
                _topScore = camManager.CalculateScore(0, 300, 0);
            }


            //Set index for this bird image, save with higher score for bird
            SaveNumberOfBirdImages(_savePicAs);
            int _val = PlayerPrefs.GetInt(_savePicAs);

            //Check for existing top photo
            var _newPath = OrganizeTopPhoto(_savePicAs, _topScore);
            string _fileName;
            if(_newPath._pathName == null)
            {
                //Debug.Log("Save as normal path name");
                _fileName = $"{_val}_{_birdName}.jpeg";
            }
            else 
            {
                Debug.Log("Saving as top photo " + _newPath._pathName);
                 _fileName = _newPath._pathName;

                if(_newPath._oldPath != null)
                {
                    Debug.Log("Moving old top photo");
                    string _renamePath = Path.Combine(Application.persistentDataPath + "/" + _birdName, $"{_val}_{_birdName}.jpeg");
                    Debug.Log(_newPath._oldPath + "    " + _renamePath); //photo to move's current path, new path it will take on
                    File.Move(_newPath._oldPath, _renamePath); //Error could not find path occurs here on old path
                }
                
            }

            //Check for or create a directory for the bird captured (May be changed later)
            string _path = Application.persistentDataPath + "/" + _birdName;
            if(Directory.Exists(_path))
            {
               // Debug.Log("Directory exists for " + _birdName);
                
            }
            else
            {
                DirectoryInfo _directory = Directory.CreateDirectory(_path);
            }

           
            //Save image to gamefiles/computer
            _fileName = System.IO.Path.Combine(_path, _fileName);
            //Debug.Log(_fileName);
            System.IO.File.WriteAllBytes(_fileName, lastImageTx.EncodeToJPG());
            //Debug.Log(_fileName);


            //Time last image on screen
            showTimeLeft = showTime;

           

        }); //END Async Request

        

    } //END TakePicture()

    /// <summary>
    /// Gets the bird's info in picture and passes id
    /// </summary>
    public List<int> GetBirdData() // Double check this actually works and its not just the short distance function kicking in
    {
        currentBirdName.Clear();
        RaycastHit[] hit;
        List<int> _birdInts = new List<int>();
        //if (Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        hit = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), Mathf.Infinity);
        //{
            for (int i = 0; i < hit.Length; i++)
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit[i].distance, Color.yellow);
                Debug.Log("Ray drawn?");
                //Detect what bird is in the photo
                if (hit[i].collider.gameObject.GetComponent<BirdDataHandler>() != null)
                {
                    
                    Debug.Log("Ray hits bird");
                    BirdDataHandler _birdData = hit[i].collider.gameObject.GetComponent<BirdDataHandler>();
                    currentBirdName.Add(_birdData.birdName);

                    if (subject.Count == 0)
                    {
                        //Test for long distance, but don't want it to block short distance or scoring system
                        subject.Add(hit[i].collider.gameObject);
                        if(camManager.cameraOpen)
                        {
                            CreateNewReticle(subject[subject.Count - 1]);
                        }

                    }

                    Debug.Log("Did Hit " + _birdData.birdName);

                    _birdInts.Add(_birdData.birdID);
                }
               
            }
           
        //}
        if(hit.Length == 0 || _birdInts.Count == 0)
        {

            currentBirdName.Clear();
            currentBirdName.Add("None");
        }

        return _birdInts;


    }//END GetBirdData()

    string CheckBirdType(int _birdId)
    {
        Debug.Log("check bird type runs, photo added"); //Return string as a test for accessing directories
        if(_birdId == 0)
        {
            camManager.noBirdImages.Add(lastImageTx);
            return "No bird"; 
        }
        else if(_birdId == 1)
        {
            camManager.birdType1Images.Add(lastImageTx);
            return "Bird 1";
        }
        else if(_birdId == 2)
        {
            camManager.birdType2Images.Add(lastImageTx);
            return "Bird 2";
        }
        else
        {
            return "Fail";
        }
    }


    /// <summary>
    /// Camera zoom in and out w scroll wheel
    /// </summary>
    void Zoom()
    {
        if(Input.GetAxis("Mouse ScrollWheel") != 0 && camManager.cameraOpen)
        {
                //Zoom
                int _input = (int)(Input.GetAxis("Mouse ScrollWheel") * 10);
            if (_input + zoomAmount <= zoomMax && _input + zoomAmount >= zoomMin)
            {
                _input *= 10;

                zoomAmount += _input;

               

                if (_input > 0)
                {
                    gameCam.fieldOfView -= 5;
                }
                else if(_input < 0)
                {
                    gameCam.fieldOfView += 5;
                }

            }
            
        }


        
    } //END Zoom()


    void FindSubjectShortDistance()
    {
        if(subject.Count > 0 && camManager.cameraOpen)
        {
            //Reticle tracks first bird located
            if (Physics.Linecast(transform.position, subject[0].transform.position))
            {
                GameObject _firstPartOfBird = subject[0].GetComponent<BirdDataHandler>().birdParts[0];
                Vector2 _screenPosition = cam.WorldToScreenPoint(_firstPartOfBird.transform.position);

               
                GetComponent<RectTransform>().GetWorldCorners(_corners);
                 for(int i = 0; i < 4; i++)
                {
                    _corners2d[i] = cam.WorldToScreenPoint(_corners[i]);
                }
            }
        }
    } //END FindSubjectShortDistance()

    /// <summary>
    /// For use in names of images, save how many images exist of the bird
    /// </summary>
    void SaveNumberOfBirdImages(string _saveKey)
    {
        //PROBLEM: if player deletes images then the numbers won't line up
        //Possible solution: maybe don't save with number, just change the name of the currently selected as best image for journal?
        //Then find way to scroll through the folder essentially

        int _val = 0;
        if(PlayerPrefs.HasKey(_saveKey))
        {
            _val = PlayerPrefs.GetInt(_saveKey) + 1;
        }

        PlayerPrefs.SetInt(_saveKey, _val);
        PlayerPrefs.Save();
    } //END SaveNumberOfBirdImages

   /// <summary>
   /// Check if a high scored photo exist already
   /// </summary>
    (string _pathName, string _oldPath) OrganizeTopPhoto(string _folderName, int _score)
    {
        //The format for the top photo name is:
        // /[Score] TopPhoto.jpeg
        //If deliberately chosen as top photo by player, set score to -1
        string _path;

        for(int i = -1; i < 6; i++)
        {
            _path = Application.persistentDataPath + "/" + _folderName + "/" + i + " TopPhoto.jpeg";

            if (File.Exists(_path)) //Top photo exists already
            {
                if(i == -1) //The photo on top has been chosen by the player, leave alone
                {
                    return (null, null);
                }

                if(i <= _score) //If the top photo is of lower score, replace, if of same, replace with the newer one
                {
                    //THIS IS THE PROBLEM, if the scores are the same then it gets confused because the names are the same
                    _path = Application.persistentDataPath + "/" + _folderName + "/" + _score + " TopPhoto.jpeg";
                    string _oldPhotoPath = Application.persistentDataPath + "/" + _folderName + "/" + i + " TopPhoto.jpeg";
                    return (_path, _oldPhotoPath);
                }
            }
            else //The top photo does not exist at this score
            {
                if( i == 5) //The top photo does not exist at all
                {
                    _path = Application.persistentDataPath + "/" + _folderName + "/" + _score + " TopPhoto.jpeg";
                    return (_path, null);
                }
                continue;
            }

            //If following score would be higher than the score passed in
            if(i + 1 > _score)
            {
                return (null, null);
            }
        }

        return (null, null);
        
    } //END OrganizeTopPhoto()





    #region Getters/Setters/Helpers

    public void AddBirdSubject(GameObject _obj, int _i)
    {
        subject.Add(_obj);
        thisBirdId.Add(_i);
    }

    public void ResetAfterScoring(GameObject _bird, int _id)
    {
        subject.Remove(_bird);
        thisBirdId.Remove(_id);
    }

    public int GetNumPicSubjects()
    {
        return subject.Count;
    }

    public List<RawImage> GetReticles()
    {
        return recticle;
    }

    public List<GameObject> GetSubjects()
    {
        return subject;
    }

    //Reticle helpers

    /// <summary>
    /// Create a new reticle for additional birds in scene, considering object pooling but in case there is too many birds in pic currently instantiation
    /// </summary>
    /// <param name="_birdToFollow">Bird this reticle will follow</param>
    public void CreateNewReticle(GameObject _birdToFollow)
    {
        //Debug.Log("Creating new reticle for " + _birdToFollow.name);
        GameObject _thisRet = Instantiate(reticlePrefab, camCanvas.gameObject.transform);
        _thisRet.GetComponent<Recticle>().SetBirdInFocus(_birdToFollow);
        recticle.Add(_thisRet.GetComponent<RawImage>());
    }

    public void MatchReticlesToBirds()
    {
        if(recticle.Count != subject.Count)
        {
            for (int i = 1; i < subject.Count; i++)
            {
                CreateNewReticle(subject[i]);
            }
        }
       
    }

    public void DeleteAllReticles()
    {
        //Leave one reticle instantiated
        for(int i = 0; i < recticle.Count; i++)
        {
            recticle[i].GetComponent<Recticle>().DeleteThisReticle();
        }
    }

    #endregion



} //END INGAMECAMERA.cs
