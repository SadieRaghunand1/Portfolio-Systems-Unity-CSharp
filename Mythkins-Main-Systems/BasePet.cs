using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePet : MonoBehaviour
{
    public enum Type
    {
        Griffin,
        Kraken,
        Cerberus
    }

    //Actual data specific to this pet
    public Type type;
    public string petName;
    public float affection;
    public Vector3 posInSanctuary;
    [SerializeField] protected int numEver;//What number pet this is in game ever


    [SerializeField] protected PetManager petManager;

    private int saveLines = 5; //How many lines are needed to save data
    protected string saveFile; //File name holding data
    

    // Start is called before the first frame update
    protected void Awake()
    {
        petManager = FindAnyObjectByType<PetManager>();

        
    }
    

    public virtual void Callback_LoadScore(PetManager _petManager)
    {
        //Debug.Log(_petManager);
    }

    public void WrapPetManagerTag(string _petType)
    {
        petManager.GetWhichPet(_petType);
    }

    public virtual string[] ReturnPetDataForSave()
    {
        string[] _petData = new string[saveLines];
        _petData[0] = type.ToString();
        _petData[1] = petName;
        _petData[2] = affection.ToString();
        _petData[3] = transform.position.ToString();
        _petData[4] = numEver.ToString();

        return _petData;
    }

    public void AssignAllPetData(Type _type, string _petName, float _affection, Vector3 _posInSanctuary, int _numEver)
    {
        type = _type;
        petName = _petName;
        affection = _affection;
        posInSanctuary = _posInSanctuary;
        numEver = _numEver;

        transform.position = posInSanctuary;

    }

    /// <summary>
    /// Called when pet is added to sanctuary, not if they are being loaded in but if they are a new pet entirely
    /// </summary>
    public virtual void OnCreation()
    {
        petManager.totalPetsEver++;
        numEver = petManager.totalPetsEver;
    }
}
