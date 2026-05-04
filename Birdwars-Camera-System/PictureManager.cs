using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manage showing all pictures on UI, deleting pictures, and choosing a top photo
/// </summary>
public class PictureManager : MonoBehaviour
{
    //Might move some stuff into this script

    [SerializeField] private Canvas ui;
    [SerializeField] private List<RawImage> imageSpots; //All spaces for showing images, add and remove as images are added or removed
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private InGameCamera inGameCamera;


    private void Start()
    {
        //Testing
        DeleteImage("Bird 1");
    }


    /// <summary>
    /// Uses same image set to load up bird images when looking at all of the images of a single bird
    /// Will need some kind of UI element set up for this, call when open UI, maybe set up tabs in UI for each bird, and thats how bird to load is passed in?
    /// </summary>
    /// <param name="_birdToLoad">Pass in one of the lists from the camera manager</param>
    public void LoadImages(List<Texture2D> _birdToLoad) 
    {
        //Set count for for loop to larger of the two lists' counts
        int _length = (_birdToLoad.Count < imageSpots.Count) ? imageSpots.Count : _birdToLoad.Count;

        //Set images as the bird photos of this type of bird
        for(int i = 0; i < _length; i++)
        {
            //Depending on if ever setting empty images to disabled
            imageSpots[i].enabled = true;

            //Set texture to photo
            imageSpots[i].texture = _birdToLoad[i];
        }

        //May get rid of this if the empty slots are just black textures, but right now this sets them inactive
        //Make sure any unused
        int _numPhotos = _birdToLoad.Count;
        for(int i = (imageSpots.Count - 1); i > _birdToLoad.Count; i--)
        {
            imageSpots[i].enabled = false;
        }

        
    } //END LoadImages()

    public void DeleteImage(string _birdName)
    {
        //Need to get name of the image to delete
        //Which means that data needs to be loaded in as well
        //Reverse engineer the pixel byte thing??

        string _folderName = Application.persistentDataPath + "/" + _birdName;

        if(Directory.Exists(_folderName) == false)
        {
            Debug.Log("Directory does not exist");
            return;
        }

        //Get all files in this folder
        IEnumerable<string> _files = Directory.EnumerateFiles(_folderName, "*.jpeg", SearchOption.AllDirectories);

        //This is just giving me some sytem.byte message
        int i = 0;
       foreach(var _file in _files)
        {
            Debug.Log(_file[i]);
            i++;
        }

        /*for(int i = 0; i < _files.Count(); i++)
        {
            Debug.Log(_files.ElementAt(i));
        }*/


    } //END DeleteImage()
    
}
