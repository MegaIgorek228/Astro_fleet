using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject flagshipPrefab;
    public GameObject shipPrefab;
    public GameObject enemyPrefab;

    public int shipCount = 2;
    public float r = 1.5f;
    public int enemyCount = 4;
    public float enemyRange = 12f;
    
    void Start()
    {
        var flagship = Instantiate(flagshipPrefab, new Vector3(0, 2*r, 0), Quaternion.identity); 
        flagship.name = "Flagship";
        float angl = 360f/(shipCount+1);
        for (int i = 0; i < shipCount; i++)
        {
            var ship = Instantiate(shipPrefab, new Vector3(Mathf.Sin(angl*i)*r, 2*r+Mathf.Cos(angl*i)*r, 0), Quaternion.identity);
            ship.name = "Ship " + i;
        }
        
        for (int i = 0; i < enemyCount; i++)
        {
            angl = Random.value * 360f;
            var enemy = Instantiate(enemyPrefab, new Vector3(Mathf.Sin(angl)*enemyRange, 2*r+Mathf.Cos(angl)*enemyRange, 0), Quaternion.identity);
            enemy.name = "Enemy " + i;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
