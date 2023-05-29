using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ParticleType particleType = default;

    [Header("Components")]
    private new ParticleSystem particleSystem = default;

    private ParticleSystemRenderer particleSystemRenderer = default;

    #region get/set
    public bool IsBusy { get; set; }
    public ParticleType Type => particleType;
    public ParticleSystemRenderer ParticleSystemRenderer => particleSystemRenderer;
    #endregion

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
    }

    public void Play()
    {
        particleSystem.Play();
        StartCoroutine(WaitToDeadParticle(particleSystem.main.duration));
    }

    private IEnumerator WaitToDeadParticle(float duration)
    {
        IsBusy = true;
        yield return new WaitForSecondsRealtime(duration);
        IsBusy = false;
    }
}

