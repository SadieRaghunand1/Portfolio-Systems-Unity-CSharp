using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BirdData
{
    public string birdName;
    public int birdID;
    public int habitatID;
    public bool inAviary;

    //More variables like models and stuff to be added soon

    public BirdData() { }

    public BirdData(string _birdName, int _birdID, int _habitatID, bool _inAviary)
    {
        birdName = _birdName;
        birdID = _birdID;
        habitatID = _habitatID;
        inAviary = _inAviary;
    }
    
}
