using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdDataHandler : MonoBehaviour
{
    BirdData data;
    AviaryManager aviaryManager;
    CameraManager cameraManager;
    [SerializeField] private BaseBirdAI thisAI;
    [SerializeField] Recticle rect;
    private Recticle ogRect; //First in pool, prevent null errors

    [Header("Components")]
    public string birdName;
    public int birdID;
    public int habitatID;
    public bool inAviary;

    [Header("Scoring")]
    public GameObject[] birdParts;
    [SerializeField] private GameObject gameCam;
    [SerializeField] private GameObject middleOfScreen;
    public int partsCounted;
    private InGameCamera inGameCamera;
    private Camera cam;
    public float distance;
    public float anglePts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitValues();
        //rect = FindAnyObjectByType<Recticle>(FindObjectsInactive.Exclude);
        ogRect = FindAnyObjectByType<Recticle>(FindObjectsInactive.Include);
    }


    private void OnTriggerEnter(Collider other)
    {
        //Instead of using trigger enter, could use a screen-sized collider on bird, combine trigger and raycast if player is very far away to get bird
        SetAsSubject(other);
    }

    

    private void OnTriggerExit(Collider other)
    {
        ResetSubject(other);
    }

    void InitValues()
    {
        aviaryManager = FindFirstObjectByType<AviaryManager>();
        gameCam = FindFirstObjectByType<InGameCamera>().gameObject;
        inGameCamera = FindFirstObjectByType<InGameCamera>();
        cam = gameCam.GetComponent<Camera>();
        cameraManager = FindFirstObjectByType<CameraManager>();
        middleOfScreen = FindFirstObjectByType<ScreenPointMiddleID>().gameObject;
    }

    /// <summary>
    /// Set data for this bird, may not need if input on the prefab
    /// </summary>
    public void InitBirdData(BirdData _data)
    {
        data = _data;
        birdName = data.birdName;
        birdID = data.birdID;
        habitatID = data.habitatID;
        inAviary = data.inAviary;
    }

    /// <summary>
    /// Set bird as photo subject for close range birds
    /// </summary>
    void SetAsSubject(Collider _other)
    {
        //Return if the triggering object is not the camera
        if(_other.gameObject.GetComponent<InGameCamera>() == null)
        {
            return;
        }

        //Add to subject list
        _other.gameObject.GetComponent<InGameCamera>().AddBirdSubject(this.gameObject, birdID);

        //Reticle management if camera is open
        if (cameraManager.cameraOpen)
        {
            //Recticle _rect = GameObject.FindAnyObjectByType(typeof(Recticle), true);
            if (inGameCamera.GetSubjects().Count > 1 && rect == null)
            {
                inGameCamera.CreateNewReticle(this.gameObject);
                Debug.Log("Should create new reticle " + rect);
            }
            else if (inGameCamera.GetSubjects().Count <= 1 && rect == null)
            {
                Debug.Log("Set original reticle on " + birdName);
                rect = ogRect;
            }
            //Debug
            else if(inGameCamera.GetSubjects().Count > 1 && rect != null)
            {
                Debug.Log("Multiple subjects but rect is not null");
            }
            rect.SetBirdInFocus(this.gameObject);
        }

        //Set player nearby, if statement if for debugging early in development when not all birds have AI
        if (thisAI != null)
            thisAI.SetPlayerNearby(true);
    } //END SetAsSubject()

    /// <summary>
    /// Undo set as subject
    /// </summary>
    void ResetSubject(Collider _other)
    {
        
        if (_other.gameObject.GetComponent<InGameCamera>() != null)
        {
            //Debug.Log("Removing and resetting " + birdName);
            _other.gameObject.GetComponent<InGameCamera>().ResetAfterScoring(this.gameObject, birdID);
            rect.DeleteThisReticle();
            rect = null;
        }

        //Set player nearby, if statement if for debugging early in development when not all birds have AI
        if(thisAI != null)
            thisAI.SetPlayerNearby(false);
        
    } //END ResetSubject()

    /// <summary>
    /// See how many parts of the bird are in the camera's view or if the bird is partially hidden
    /// </summary>
    public int CheckPictureClarity()
    {
        #region angles
        //Uses angle
        //float _differenceAngles = Vector3.Angle(cam.WorldToScreenPoint(birdParts[1].transform.position) - cam.WorldToScreenPoint(middleOfScreen.transform.position), transform.forward);

        //Uses position
        Vector3 _differenceAngles = cam.WorldToScreenPoint(birdParts[1].transform.position) - cam.WorldToScreenPoint(middleOfScreen.transform.position); 
        Debug.Log("Angle is " + _differenceAngles);
        //Basically good score bw 150 - 250 on x, bw -30 - (30) on y  not great score is less or above that
         if(_differenceAngles.x >= 150 && _differenceAngles.x <= 250)
        {
            anglePts = 50;
        }
         else
        {
            anglePts = 0;
        }

        if (_differenceAngles.y >= -30 && _differenceAngles.y <= 30)
        {
            anglePts += 30;
        }
        else
        {
            anglePts += 0;
        }

        #endregion region angles

        #region parts
        //Below is commented out parts system, still want to implement this bc it kind of is what makes the thing above and beyond a responsive camera system, but not including for now
        partsCounted = 0;

        LayerMask _layerMask = LayerMask.GetMask("Bird");

        for (int i = 0; i < birdParts.Length; i++)
        {
           if(Physics.Linecast(birdParts[i].transform.position, gameCam.transform.position, out RaycastHit _hit, _layerMask, QueryTriggerInteraction.Ignore))
            //if(Physics.Linecast(birdParts[i].transform.position, gameCam.transform.position)) //Exclusively registers as true
            {
                //Debug.Log("Hit = " + _hit); //Never reaching this? Skips and labels as false for all this was working yesterday what happened aaaaaa
                //Debug.Log("Is the hit even registering");
                //Debug.DrawLine(birdParts[i].transform.position, gameCam.transform.position, Color.blue, 5f);

                partsCounted++;
                
            }
            else
            {
                if(i == 2 && partsCounted == 0)
                {
                    //Check if right in front of bird, works a bit better but still needs refining bc now says true a little too often
                    inGameCamera.testHeight.transform.position = new Vector3(inGameCamera.testHeight.transform.position.x, birdParts[i].transform.position.y, inGameCamera.testHeight.transform.position.z);
                    RaycastHit hit;
                    if (Physics.Raycast(inGameCamera.testHeight.transform.position, inGameCamera.testHeight.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))

                    {
                        Debug.Log("in weird raycast");
                        if (hit.collider.gameObject.layer == 6)
                        {
                            Debug.Log("Weird raycast success");
                            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                            
                            partsCounted = 3;
                            Debug.Log("In front of");
                            break;
                        }
                    }
                }

                continue;
                
            }
        }
        #endregion

        return GetPlayerDistance();

    } //END CheckPictureClarity()

    int GetPlayerDistance()
    {

        //distance formula: d = Math.Sqrt((Mathf.Pow(x2 - x1)) + (Mathf.Pow(y2 - y1)) + (Mathf.Pow(z2 - z1)))
        Vector3 _gameCamPos = inGameCamera.transform.position;
        Vector3 _thisPos = birdParts[0].gameObject.transform.position;
        distance = Mathf.Sqrt((Mathf.Pow(_gameCamPos.x - _thisPos.x, 2)) + (Mathf.Pow(_gameCamPos.y - _thisPos.y, 2)) + (Mathf.Pow(_gameCamPos.z - _thisPos.z, 2)));

        Debug.Log(distance);


        if(inGameCamera.subject != null)
        {
            return cameraManager.CalculateScore(partsCounted, distance, anglePts);
        }

        return 0;
       
        

    } //END GetPlayerDistance()


    #region Getters/Setters

    public string GetName()
    {
        return birdName;
    }

    public Recticle GetReticle()
    {
        return rect;
    }

    public void SetReticle(Recticle _rect)
    {
        rect = _rect;
    }

    #endregion

} //END BirdDataHandler.cs
