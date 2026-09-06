using UnityEngine;

public class BasicModule : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;
    public float armor = 0f;
    public MovableObject mother;

    public virtual void Start()
    {
        mother = this.GetComponent<MovableObject>();
        if (mother == null)
        {
            Debug.LogError(this.gameObject.name + " must has a MovableObject component");
        }
    }
}
