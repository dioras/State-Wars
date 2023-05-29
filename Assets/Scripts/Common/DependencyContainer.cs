using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/DependencyContainer", fileName = "DependencyContainer")]
public class DependencyContainer : ScriptableObject
{
    [SerializeField] private int countUnionHex = 0;
    [SerializeField] private int countEnemyHex = 0;
    [SerializeField] private int paintAvialable = 0;
    [SerializeField] private int paintAvialableEnemy = 0;

    #region getter/setter 
    public PedestalController PedestalContainer { get; set; }
    public UnitsController UnitsController { get; set; }
    public UnitType UnionCurrencyHero { get; set; }
    public Transform CameraViewStatePoint { get; set; }
    public Vector3 CurrencyStatePlayerMap { get; set; }
    public Vector3 BoarderTextureOffset { get; set; }
    public bool InGame { get; set; }
    public bool BaseControlledUnion { get; set; }
    public GamePanelController GamePanelController { get; set; }
    public BaseController UnionBase { get; set; }
    public BaseController EnemyBase { get; set; }
    public HexGrid HexGrid { get; set; }
    public int CountUnionHex { get => countUnionHex; set { 
            countUnionHex = value;
            UpdateHexCount();
        } 
    }
    public int CountEnemyHex { get => countEnemyHex; set { 
            countEnemyHex = value;
            UpdateHexCount();
        } 
    }
    public int PaintAvialable { get => paintAvialable; set {
            if (value > 20) {
                value = 20;
                if (InGame && BaseControlledUnion)
                {
                    PedestalController.AutoSpawnAction?.Invoke();
                }
            }
            paintAvialable = value;
            PaintGenerator.UpdatePaintValueAction?.Invoke();
        } 
    }
    public int PaintAvialableEnemy { get => paintAvialableEnemy; set {
            if (value > 20) value = 20;
            paintAvialableEnemy = value;
        }
    }
    public Transform FirstUnitTransformPedestal { get; set; }
    public List<UnitController> UnitControllers { get; set; }
    #endregion
    private void UpdateHexCount()
    {
        GamePanelController.UpdateProgressBar();
    }
    public static List<T> Shuffle<T>(List<T> list)
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < list.Count; i++)
        {
            int k = rnd.Next(0, i);
            T value = list[k];
            list[k] = list[i];
            list[i] = value;
        }
        return list;
    }
}
