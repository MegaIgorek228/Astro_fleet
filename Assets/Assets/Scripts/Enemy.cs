using System;
using System.Linq;
using AstroFleet;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AdaptivePerformance;
using Vector2 = System.Numerics.Vector2;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class Enemy : ShipController
{
    public float minRadius = 0.3f;
    public float maxRadius = 0.4f;
    public float angle = 20f;
    public void setTargetWaypoint()
    {
        Vector3 tp = targetMO.getLightPositionFrom(gameObject.transform.position);
        float r = Random.Range(minRadius, maxRadius);
        float a = Random.Range(-angle, angle)+Vector3.Angle(tp, gameObject.transform.position);
        movableObject.setTargetWaypoint(new Vector3(Mathf.Sin(a)*r, Mathf.Cos(a)*r));
    }
    
    protected override void Start()
    {
        base.Start();
        movableObject.isEnemy = true;
        movableObject.isFlagship = false;
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (targetMO == null || targetGO == null || TimeController.globalTicks % 1731 == 0)
        {
            Debug.Log($"Choosing target, {targetGO}, {targetMO}");
            ShipController[] allWithScript1 = FindObjectsByType<ShipController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            List<GameObject> result = new List<GameObject>();

            foreach (ShipController s1 in allWithScript1)
            {
                if (s1.GetComponent<Missle>() == null && s1.GetComponent<Enemy>() == null)
                {
                    result.Add(s1.gameObject);
                }
            }
            GameObject targ = null;
            float d = 99999;
            if (result.Count() < 1)
            {
                return;
            }
            foreach (GameObject go in result)
            {
                float dist = (go.GetComponent<MovableObject>().getLightPositionFrom(gameObject.transform.position) -
                              gameObject.transform.position).magnitude;
                if (dist < d)
                {
                    targ = go;
                    d = dist;
                }
            }
            setTarget(targ);
        }
        if (targetMO != null && targetGO != null)
        {
            if (TimeController.globalTicks % 1000 == 0)
            {
                Debug.Log("Setting target waypoint");
                setTargetWaypoint();
            }

            if ((targetMO.getLightPositionFrom(gameObject.transform.position) - gameObject.transform.position)
                .magnitude < maxRadius * 2)
            {
                Fire();
            }
        }
    }
}
