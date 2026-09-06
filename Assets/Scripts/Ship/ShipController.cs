using UnityEngine;
using System.Collections.Generic;

public class ShipController : MonoBehaviour
{
    public GameObject targetGO;
    public MovableObject targetMO;
    public bool fireing = false;
    List<MissleLauncher> torpedoes = new List<MissleLauncher>();

    protected MovableObject movableObject;
    
    public void setTarget(MovableObject target)
    {
        targetMO = target;
        targetGO = targetMO.gameObject;
    }
    
    public void setTarget(GameObject target)
    {
        targetGO = target;
        targetMO = targetGO.GetComponent<MovableObject>();
        Debug.Log($"Setted target: {targetGO}");
    }

    protected virtual void Start()
    {
        this.GetComponents<MissleLauncher>(torpedoes);
        movableObject = GetComponent<MovableObject>();
    }

    public void Fire()
    {
        if (targetGO != null)
        {
            foreach (var t in torpedoes)
            {
                t.Fire(targetGO);
            }
        }
    }

    protected virtual void Update()
    {
        // Синхронизируем targetGO с target из MovableObject
        if (movableObject != null && movableObject.targetMO != null)
        {
            if (targetGO != movableObject.targetMO.gameObject)
            {
                targetGO = movableObject.targetMO.gameObject;
                Debug.Log($"[ShipController] Цель обновлена: {targetGO.name}");
            }
        }

        if (fireing)
        {
            Fire();
        }
    }
}