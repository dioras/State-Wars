using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurrelPedestal : MonoBehaviour
{
    [SerializeField] private MeshRenderer changeMeshRenderer = default;
    private static readonly int MeshColor = Shader.PropertyToID("_Color");
    public Collider Collider { get; private set; }
    public Color ChangeMeshColor
    {
        set => changeMeshRenderer.material.SetColor(MeshColor, value);
    }

    private void Awake()
    {
        Collider = GetComponent<Collider>();
    }
}
