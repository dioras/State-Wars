using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/EnviromentStorage", fileName = "EnviromentStorageSO")]
public class EnviromentStorage : ScriptableObject
{
    [SerializeField] private List<EnviromentSet> _enviromentSets = default;

    public EnviromentSet GetEnviromentSet(EnviromentType enviromentType)
    {
        return _enviromentSets.Find((_envioment) => _envioment.EnviromentType == enviromentType);
    }
}

[Serializable]
public class EnviromentSet
{
    [SerializeField] private EnviromentType enviromentType = default;
    [SerializeField] private Color cellsColor = default;
    [SerializeField] private bool randomRotateChild = false;
    [SerializeField] private List<GameObject> enviromentObjects = new List<GameObject>();

    public EnviromentType EnviromentType => enviromentType;
    public List<GameObject> EnviromentObjects => enviromentObjects;
    public Color CellsColor => cellsColor;
    public bool RandomRotateChild => randomRotateChild;
}