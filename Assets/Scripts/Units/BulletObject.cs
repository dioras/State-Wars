using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletObject : MonoBehaviour
{
    [SerializeField] private ParticleSystemRenderer _particleSystemRenderer = default;
    private static readonly int Color = Shader.PropertyToID("_TintColor");

    public void Config(Color color)
    {
        _particleSystemRenderer.material.SetColor(Color, color);
    }
}
