using UnityEngine;

public class SpawnItems : MonoBehaviour
{
    [SerializeField] private int numberForEasyLevel = 10;
    [SerializeField] private int numberForDifficultLevel = 5;
    private int totalNumber;
    [SerializeField] private GameObject item;
    [SerializeField] private GameObject player;
    [SerializeField] private float step = 5f; // distance between each item

    private GameObject[] spawnedItems;

    void Start()
    {
        totalNumber = numberForEasyLevel + numberForDifficultLevel;

        if(item != null && player != null)
        {
            // gems will be placed at (0, 0, Z)
            float startZ = player.transform.position.z + step;
            spawnedItems = new GameObject[totalNumber];
            for (int i = 0; i < totalNumber; i++)
            {
                float z_pos = startZ + i * step;
                spawnedItems[i] = Instantiate(item, new Vector3(0, 0, z_pos), Quaternion.identity);
                spawnedItems[i].transform.parent = transform;
                
                if(i >= numberForEasyLevel) // difficulty increases
                {
                    spawnedItems[i].AddComponent<Blink>();
                }
            }
        }
        else
        {
            Debug.Log("Missing references!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
