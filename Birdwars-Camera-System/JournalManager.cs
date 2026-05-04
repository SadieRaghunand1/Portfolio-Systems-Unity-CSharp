using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class JournalManager : MonoBehaviour
{
    public RawImage[] birdImages;
    [SerializeField] private Texture2D defaultTx;
    [SerializeField] private CameraManager camManager;

    string birdName;

    private void OnEnable()
    {
        Debug.Log("Journal actiavetd");
        for(int i = 0; i < birdImages.Length; i++)
        {
            //There's got to be a better way of doing this
            
            if(i == 0)
            {
                birdName = "Bird 1";
                SetJournalImage(birdImages[i], camManager.birdType1Images[0]);
            }
            else if(i == 1)
            {
                birdName = "Bird 2";
                SetJournalImage(birdImages[i], camManager.birdType2Images[0]);
            }
            else
            {
                SetJournalImage(birdImages[i], defaultTx);
            }
        }
    }

    /// <summary>
    /// Set journal image for a bird from the photos taken this session
    /// </summary>
    public void SetJournalImage(RawImage _image, Texture2D _newImageTx)
    {
        _image.texture = _newImageTx;
    } //END SetJournalImage()

    

    
}
