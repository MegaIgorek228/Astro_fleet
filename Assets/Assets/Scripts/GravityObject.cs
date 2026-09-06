using AstroFleet;
using UnityEngine;

public class GravityObject : MonoBehaviour
{
    [SerializeField] private float M = 5e30f;

    public Vector3 getAceleration(Vector3 position)
    {
        float R = Vector3.Distance(transform.position, position)*Defines.DistScale;
        Vector3 a = (transform.position - position).normalized * Defines.G * M / R / R;
        return a;
    }
    
    
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
