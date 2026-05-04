using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TakePicture : MonoBehaviour
{
    [SerializeField] InGameCamera inGameCamera;
    [SerializeField] CameraManager camManager;
    [SerializeField] private InventoryManager inventoryManager;
    public KeyCode keyboardKey;

    [Header("Hipfire variables")]
    [SerializeField] RawImage liveFeed;
    [SerializeField] Canvas camCanvas;
    private void Update()
    {

        if (inGameCamera.gameCam.enabled && Input.GetKeyDown(keyboardKey) && camManager.cameraOpen && inventoryManager.camInHand) //Temporary mapping bc needs to be a click
        {
            
            inGameCamera.TakePicture();
            //GetBirdData();        
        }

        //Hipfire, input mapping is temp
        if( Input.GetKeyDown(KeyCode.L) && !camManager.cameraOpen)
        {
            Debug.Log("Hip fire shot");

            camCanvas.enabled = true;
            inGameCamera.TakePicture();
            camCanvas.enabled = false;
        }
    }

   




}
