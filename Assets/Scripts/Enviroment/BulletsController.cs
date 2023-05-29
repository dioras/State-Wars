using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BulletsController : MonoBehaviour
{
    [Header("Base")] 
    [SerializeField] private GroupingStorage groupingStorageSO = default;

    [Header("Settings")] 
    [SerializeField] private int startPoolSize = 30;
    
    [Header("Components")]
    [SerializeField] private BulletObject bulletPrefab = default;
    
    [Header("Actions")] 
    public static Action<Vector3, Vector3, float, GroupingType, Action> ShootBulletAction = default;
    
    private List<BulletObject> bullets = new List<BulletObject>();

    private void OnEnable()
    {
        ShootBulletAction += ShootBullet;
    }

    private void OnDisable()
    {
        ShootBulletAction -= ShootBullet;
    }

    private void Awake()
    {
        CreateBulletPool();
    }

    private void ShootBullet(Vector3 from, Vector3 to, float duration, GroupingType groupingType, Action callback)
    {
        var freeBullet = bullets.Find((_bullet) => _bullet.gameObject.activeSelf == false);
        if (freeBullet == null)
        {
            freeBullet = AddBullet();
        }
        freeBullet.gameObject.SetActive(true);
        freeBullet.Config(groupingStorageSO.GetGrouping(groupingType).ColorStickmans);
        freeBullet.transform.position = from;
        freeBullet.transform.DOMove(to, duration).SetEase(Ease.Flash).OnComplete(() =>
        {
            freeBullet.gameObject.SetActive(false);
            callback?.Invoke();
        });
    }
    
    private void CreateBulletPool()
    {
        for (int i = 0; i <= startPoolSize; i++)
        {
            AddBullet();
        }
    }

    private BulletObject AddBullet()
    {
        var newBulet = Instantiate(bulletPrefab, transform);
        newBulet.transform.localPosition = Vector3.zero;
        newBulet.transform.eulerAngles = Vector3.zero;
        newBulet.gameObject.SetActive(false);
        bullets.Add(newBulet);

        return newBulet;
    }
}
