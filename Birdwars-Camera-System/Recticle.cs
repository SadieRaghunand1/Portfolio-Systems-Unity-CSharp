using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Recticle : MonoBehaviour
{

    [SerializeField] private RectTransform reticle;
    [SerializeField] private RawImage reticleImg;
    [SerializeField] private GameObject birdInFocus;
    [SerializeField] private Camera cam;
    Vector2 pos;
    [SerializeField] private RawImage plane; //Plane w render text on it
    private InGameCamera inGameCam;

    [Header("Main reticle")]
    [Tooltip("Should be checked on reticle that is not instantiated, first in list")]
    [SerializeField] private bool isOriginal; 

    //Camera screen bounds
    private float minX = 0;
    private float maxX = 1050;
    private float minY = 0;
    private float maxY = 400;

    private void Start()
    {
        InitValues();
    }

    private void Update()
    {
        
        CalcScreenPos();


    }

    private void InitValues()
    {
        cam = FindAnyObjectByType<Camera>(FindObjectsInactive.Exclude);
        plane = GameObject.FindWithTag("LiveFeed").GetComponent<RawImage>();
        inGameCam = FindAnyObjectByType<InGameCamera>();
        
    }

    /// <summary>
    /// Setter for bird tfor reticle to focus on
    /// </summary>
    public void SetBirdInFocus(GameObject _bird)
    {
        //Debug.Log(_bird.name);
        birdInFocus = _bird;
        birdInFocus.GetComponent<BirdDataHandler>().SetReticle(this);
    }

    private void CalcScreenPos()
    {
        if(birdInFocus == null)
        {
            //prevent set active call every frame
           if(reticleImg.enabled == true)
                reticleImg.enabled = false;
            //Return, do not calculate anything if no bird in focus
            return;
        }


        Vector3 _worldScreenPos = cam.WorldToViewportPoint(birdInFocus.transform.position);

        Vector2 _pos2D = new Vector2(_worldScreenPos.x, _worldScreenPos.y);

        Vector2 _tmp = plane.rectTransform.sizeDelta;
        pos = new Vector2(_pos2D.x * _tmp.x, _pos2D.y * _tmp.y);
        
        Vector3 _localPos = reticle.localPosition;
        //If out of camera's view, turn reticle off
        if (pos.x > maxX || pos.x < minX || pos.y > maxY || pos.y < minY || CheckBirdBlocked())
        {
            reticleImg.enabled = false;
            return;
        }
        else
        {
            //Prevent set active call every frame
            if(reticleImg.enabled == false)
            {
                reticleImg.enabled = true;
            }
        }

        
         reticle.position = pos;
        //Debug.Log("Pos on image = " + _pos2D);

    }


    /// <summary>
    /// Use raycast to determine if the bird in focus is being blocked by anything
    /// Return true if blocked, false if not
    /// </summary>
    private bool CheckBirdBlocked()
    {
        //There must be a more performant way of doing this
        RaycastHit _hit;
        LayerMask _mask = LayerMask.GetMask("Bird", "Player", "InGameCam");
        if (Physics.Linecast(birdInFocus.transform.position, cam.gameObject.transform.position, out _hit, ~_mask, QueryTriggerInteraction.Ignore))
        {
            
            Debug.Log("blocked by " + _hit.collider.gameObject.name);
            return true;
        }
       
        return false;
    } //END CheckBirdBlocked()

    public void DeleteThisReticle()
    {
        if (!isOriginal)
        {
            //Try to delete the reticle clone
            try
            {
                inGameCam.GetReticles().Remove(this.gameObject.GetComponent<RawImage>());
                Destroy(this.gameObject);
            }
            //If already been deleted, do nothing
            catch (MissingReferenceException e)
            {
                Debug.Log("Failed to delete reticle, object does not exist anymore " + birdInFocus);
            }

        }
        //This is the original reticle
        else
        {

            if(inGameCam.GetSubjects().Count > 0)
            {
                //Check if bird alredy has an assigned reticle
                if(inGameCam.GetSubjects()[0].GetComponent<BirdDataHandler>().GetReticle() == null)
                {
                    birdInFocus.GetComponent<BirdDataHandler>().SetReticle(null);
                    birdInFocus = inGameCam.GetSubjects()[0];
                    inGameCam.GetSubjects()[0].GetComponent<BirdDataHandler>().SetReticle(this);
                }
               
            }
            else
            {
                //DO not delete original reticle, just disable
                birdInFocus = null;
                reticleImg.enabled = false;
            }
                
                
        }
        
    }
   
}
