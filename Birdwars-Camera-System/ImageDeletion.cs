using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageDeletion : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private RawImage[] images;

    private int currentBirdIdxShown = 0; //Each bird type will have its own index it correposnds with, this is whatever is showing now/last one shown if closed

    private void Start()
    {
        //Test 
        LoadImages(cameraManager.birdType1Images);


      
    }
    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LoadImages(List<Texture2D> _textures)
    {
        //Clear images already displayed
        for (int i = 0; i < images.Length; i++)
        {
            images[i].texture = null;
        }

            //Load new images
        for (int i = 0; i < images.Length; i++)
        {
            if(i >= _textures.Count)
            {
               break;
            }
            images[i].texture = _textures[i];
        }
    }

    /// <summary>
    /// Holds deletion function
    /// </summary>
    /// <param name="_displayIdx">The index of the actual image sprite on screen</param>
    public void DeletionWrapper(int _displayIdx)
    {
        //Get data to be passed into deletion function
        string _path = cameraManager.listOfAllPaths[currentBirdIdxShown][_displayIdx];
        Texture2D _imgTexture = cameraManager.listOfAllTextures[currentBirdIdxShown][_displayIdx];

        //Check if file exists
        if(File.Exists(_path) == false)
        {
            return;
        }

        DeleteImage(_path, _imgTexture, cameraManager.listOfAllTextures[currentBirdIdxShown], currentBirdIdxShown);
    }

    /// <summary>
    /// pass in parameter via code not editor in wrapper function, delete image from texture list and files
    /// </summary>
    /// <param name="_path">Path directing to that image</param>
    /// <param name="_texture">Textur that needs to be deleted, check against list of textures and remove??</param>
    /// <param name="_textureList">List to delete from</param>
    ///<param name="_imageIdx">Index of image this is at</param>
    void DeleteImage(string _path, Texture2D _texture, List<Texture2D> _textureList, int _imageIdx)
    {
        //Note - later add "Are you sure?" screen

        File.Delete(_path);
        images[_imageIdx].texture = null;


    } //END DeleteImage()

    /// <summary>
    /// Call on page turn in deletion menu
    /// </summary>
    /// <param name="_inc">1 if move forward, -1 if move back</param>
    public void MoveToNextPage(int _inc)
    {
        if(currentBirdIdxShown + _inc < 0 || currentBirdIdxShown + _inc >= cameraManager.listOfAllTextures.Count)
        {
            Debug.Log("Cannot turn page");
            return;
        }
        currentBirdIdxShown += _inc;
        Debug.Log(currentBirdIdxShown);
        LoadImages(cameraManager.listOfAllTextures[currentBirdIdxShown]);
    }
}
