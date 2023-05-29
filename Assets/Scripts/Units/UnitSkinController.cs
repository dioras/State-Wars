using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSkinController : MonoBehaviour
{
    [SerializeField] private List<GameObject> allSetObjects = new List<GameObject>();
    [SerializeField] private List<UnitSet> unitSets = new List<UnitSet>();

    public UnitSet SetUnitSet(bool isUnion){
        DisableAllSetObjects();
        var set = unitSets.Find((_set) => _set.IsUnion == isUnion);
        var setObjects = set.GetUnitSet();
        setObjects.helment.gameObject.SetActive(true);
        setObjects.shield.gameObject.SetActive(true);
        setObjects.sword.gameObject.SetActive(true);

        return set;
    }

    private void DisableAllSetObjects(){
        allSetObjects.ForEach((_obj) => _obj.SetActive(false));
    }
}

[Serializable]
public class UnitSet{
    [SerializeField] private bool isUnion = false;
    [SerializeField] private List<GameObject> helments = new List<GameObject>();
    [SerializeField] private List<GameObject> shields = new List<GameObject>();
    [SerializeField] private List<GameObject> swords = new List<GameObject>();

    public bool IsUnion => isUnion;
    public (GameObject helment, GameObject shield, GameObject sword) GetUnitSet(){
        (GameObject helment, GameObject shield, GameObject sword) result = default;
        result.helment = helments[UnityEngine.Random.Range(0, helments.Count)];
        result.shield = shields[UnityEngine.Random.Range(0, shields.Count)];
        result.sword = swords[UnityEngine.Random.Range(0, swords.Count)];
        return result;
    }
}

