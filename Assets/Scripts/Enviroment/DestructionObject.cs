using System.Collections.Generic;
using UnityEngine;

public class DestructionObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isBroke = default;

    [Header("Components")]
    [SerializeField] private GameObject newObject = default;
    [SerializeField] private List<GameObject> brokenObjects = new List<GameObject>();

    private List<Rigidbody> brokeRigidbodys = new List<Rigidbody>();
    private Dictionary<Rigidbody, SavePosition> savePositions = new Dictionary<Rigidbody, SavePosition>();

    #region get/set
    public bool IsBroked { get => isBroke; }
    #endregion

    private void Awake()
    {
        brokeRigidbodys.Clear();
        brokenObjects.ForEach((obj) => brokeRigidbodys.Add(obj.GetComponent<Rigidbody>()));
        SaveDefaultPosition();
    }

    private void SaveDefaultPosition()
    {
        savePositions.Clear();
        brokeRigidbodys.ForEach((_brokenObject) =>
        {
            var transform = _brokenObject.transform;
            savePositions.Add(_brokenObject, new SavePosition(transform.localPosition, transform.localEulerAngles));
        });
    }

    public void Broke()
    {
        newObject.SetActive(false);
        ParticleController.PlayParticleAction?.Invoke(transform.position + Vector3.up * 3f, ParticleType.BaseDestroted);

        brokenObjects.ForEach((obj) => obj.SetActive(true));
        brokeRigidbodys.ForEach((rigidbody) => rigidbody.AddExplosionForce(20, transform.position, 20f));

        isBroke = true;
        VibrationController.Vibrate(20);
    }

    public void Restore()
    {
        newObject.SetActive(true);
        brokenObjects.ForEach((obj) => obj.SetActive(false));
        isBroke = false;
        
        foreach (var sPosKey in savePositions.Keys)
        {
            var transformBrokeObject = sPosKey.transform;
            transformBrokeObject.localPosition = savePositions[sPosKey].Position;
            transformBrokeObject.localEulerAngles = savePositions[sPosKey].Euler;
        }
    }
}

public struct SavePosition
{
    public Vector3 Position;
    public Vector3 Euler;

    public SavePosition(Vector3 pos, Vector3 euler)
    {
        Position = pos;
        this.Euler = euler;
    }
}
