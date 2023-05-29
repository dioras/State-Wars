using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateLevelObject : MonoBehaviour
{
    [SerializeField] private Transform pointBase1 = default;
    [SerializeField] private Transform pointBase2 = default;
    [SerializeField] private Transform pointSight = default;

    public Transform PointBase01 => pointBase1;
    public Transform PointBase02=> pointBase2;
    public Transform PointSight => pointSight;
}
