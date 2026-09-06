using System;
using AstroFleet;
using Unity.VisualScripting;
using UnityEngine;

public class MissleLauncher : BasicModule
{
    public GameObject misslePrefub;
    public const int tubes = 2;
    public int ammo = 6;
    public const int maxAmmo = 25;
    public const float prepareTime = 220f*60f;
    public float readyTime = 0f;
    public float startSpeed = 100f;
    public float angle = 0f;
    
    public override void Start()
    {
        base.Start();
        ammo = Mathf.Clamp(ammo, 0, maxAmmo);
    }

    public void Fire(GameObject targetObject)
    {
        if (TimeController.globalTime >= readyTime && ammo > 0)
        {
            ammo--;
            readyTime = TimeController.globalTime + prepareTime;
            
            GameObject missle = Instantiate(misslePrefub);
            // Debug.Log(mother);
            Vector3 c = (Quaternion.AngleAxis(angle, Vector3.forward) * mother.direction);
            
            Vector3 misslePos = mother.position() + c*Defines.EPSD;
            
            
            // Debug.Log("missle object: " + missle.name);
            // var comp = missle.GetComponent(typeof(Missle));
            // Debug.Log("comp: " + comp);
            
            // Debug.Log(missle.GetComponent<Missle>());
            missle.GetComponent<Missle>().Initialize(misslePos, targetObject, startSpeed*c/Defines.DistScale+mother.speed);
        }
    }
}
