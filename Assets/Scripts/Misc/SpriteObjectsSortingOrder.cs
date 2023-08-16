using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteObjectsSortingOrder : MonoBehaviour
{
    public string ID => gameObject.name + gameObject.transform.childCount;
    [SerializeField] private List<SpriteRenderPos> _positionKeys = new List<SpriteRenderPos>();
    [SerializeField] private List<MeshRenderPos> _positionKeysMesh = new List<MeshRenderPos>();
    [SerializeField] private List<ParticleRenderPos> _positionKeysParticle = new List<ParticleRenderPos>();

    private float zPos;

    protected void Awake()
    {
        foreach (SpriteRenderer spr in GetComponentsInChildren<SpriteRenderer>(true))
        {
            SpriteRenderPos pos = new SpriteRenderPos();
            pos.Sprite = spr;
            pos.Offset = spr.sortingOrder;
            
            _positionKeys.Add(pos);
        }
        
        foreach (MeshRenderer spr in GetComponentsInChildren<MeshRenderer>(true))
        {
            MeshRenderPos pos = new MeshRenderPos();
            pos.Sprite = spr;
            pos.Offset = spr.sortingOrder;
            
            _positionKeysMesh.Add(pos);
        }
        
        foreach (ParticleSystemRenderer spr in GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            ParticleRenderPos pos = new ParticleRenderPos();
            pos.Sprite = spr;
            pos.Offset = spr.sortingOrder;
            
            _positionKeysParticle.Add(pos);
        }

        zPos = transform.position.z;
        UpdateSortingOrders();
    }

    protected void LateUpdate()
    {
        if (Math.Abs(zPos - transform.position.z) < 0.0001f) return;
        
        zPos = transform.position.z;
        UpdateSortingOrders();
    }

    public void UpdateSortingOrders()
    {
        foreach (SpriteRenderPos spr in _positionKeys)
        {
            spr.Sprite.sortingOrder = (int) ((transform.position.z - spr.Offset) * -100);
        }
        
        foreach (MeshRenderPos spr in _positionKeysMesh)
        {
            spr.Sprite.sortingOrder = (int) ((transform.position.z - spr.Offset) * -100);
        }
        
        foreach (ParticleRenderPos spr in _positionKeysParticle)
        {
            spr.Sprite.sortingOrder = (int) ((transform.position.z - spr.Offset) * -100);
        }
    }
}

[Serializable]
struct SpriteRenderPos
{
    public int Offset;
    public SpriteRenderer Sprite;
}

[Serializable]
struct MeshRenderPos
{
    public int Offset;
    public MeshRenderer Sprite;
}

[Serializable]
struct ParticleRenderPos
{
    public int Offset;
    public ParticleSystemRenderer Sprite;
}
