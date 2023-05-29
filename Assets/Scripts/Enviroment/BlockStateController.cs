using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class BlockStateController : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private DependencyContainer dependencyContainerSO = default;
    [SerializeField] private EnviromentStorage enviromentStorageSO = default;

    [Header("Settings")] 
    [SerializeField] private EnviromentType enviromentType = default;

    private List<HexCell> _hexCells = new List<HexCell>();
    private Dictionary<Transform, Transform> objectInCell = new Dictionary<Transform, Transform>();

    private void OnEnable()
    {
        StateGenerator.StateCreatedAction += SetEnviroment;
    }

    private void OnDisable()
    {
        StateGenerator.StateCreatedAction -= SetEnviroment;
    }

    private void OnDestroy()
    {
        dependencyContainerSO = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(Tags.Cell))
        {
            var targetCell = dependencyContainerSO.HexGrid.GetCell(other.transform.position);
            if (_hexCells.Contains(targetCell).Equals(false))
            {
                _hexCells.Add(targetCell);
                targetCell.IsEmpty = true;
                targetCell.IsEnviroment = true;
                targetCell.HexMeshRenderer.transform.position = new Vector3(targetCell.HexMeshRenderer.transform.position.x, 
                                                                                UnityEngine.Random.Range(.1f,.25f), 
                                                                                targetCell.HexMeshRenderer.transform.position.z);
                targetCell.SetRenderer(true);
                targetCell.EnableHighlight(enviromentStorageSO.GetEnviromentSet(enviromentType).CellsColor, .1f);
            }
        }
    }

    private void SetEnviroment()
    {
        if (enviromentType.Equals(EnviromentType.Water))
        {
            _hexCells.ForEach((_cell) => _cell.SetRenderer(false));
        }
        
        var _envrSet = enviromentStorageSO.GetEnviromentSet(enviromentType);
        if (_envrSet != null)
        {
            SetEnviromentForCells(_envrSet);
        }
    }
    
    private void SetEnviromentForCells(EnviromentSet enviromentSet, float offesetY = .5f)
    {
        _hexCells.ForEach((_cell) =>
        {
            if (objectInCell.ContainsKey(_cell.transform).Equals(false))
            {
                var _object = Instantiate(enviromentSet.EnviromentObjects[UnityEngine.Random.Range(0, enviromentSet.EnviromentObjects.Count)]);
                _object.transform.position = _cell.transform.position - new Vector3(0f, offesetY, 0f);
                _object.transform.SetParent(transform);

                if (enviromentSet.RandomRotateChild)
                {
                    _object.transform.GetChild(0).localEulerAngles = new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                }
                
                objectInCell.Add(_cell.transform, _object.transform);
            }
        });
    }
}
