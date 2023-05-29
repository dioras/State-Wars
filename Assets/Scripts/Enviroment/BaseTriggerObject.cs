using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTriggerObject : MonoBehaviour
{
    [SerializeField] private List<HexCell> hexCells = new List<HexCell>();

    public List<HexCell> GetHexCells => hexCells;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(Tags.Cell))
        {
            var _cell = other.GetComponent<HexCell>();
            if (_cell.IsEmpty) return;
            if (hexCells.Contains(_cell) == false)
            {
                hexCells.Add(_cell);
            }
        }
    }
}
