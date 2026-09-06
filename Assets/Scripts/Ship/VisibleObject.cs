// using AstroFleet;
// using Unity.VisualScripting;
// using UnityEngine;
//
// public class VisibleObject : MonoBehaviour
// {
//     public float activeTime;
//     private bool isActive = false;
//     private int lifetime = 0;
//     RendOrImg roi;
//     
//     void Start()
//     {
//         roi = gameObject.GetComponentInChildren<RendOrImg>();
//         roi.activeImage = false;
//     }
//
//     void Update()
//     {
//         if (TimeController.globalTime > activeTime)
//         {
//             isActive = true;
//             roi.activeImage = true;
//         }
//         
//         if (isActive)
//         {
//             lifetime++;
//         }
//
//         if (lifetime > Defines.SIT)
//         {
//             Destroy(gameObject);
//         }
//     }
//     
// }
