using UnityEngine;

public enum CollectibleType
{
    Gem, Key, Trophy
}
public class CollectItem : MonoBehaviour
{
    private Inventory inventory;

    [SerializeField] private CollectibleType type;

    private AudioSource audioSource;
    private bool isCollected = false;

    [SerializeField] private GameObject collectBurst; // set only for gems
    [SerializeField] private GameObject mazeManager; // set only for key

    void Start()
    {
        inventory = FindAnyObjectByType<Inventory>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // prevent double collect
        if (isCollected)
        {
            return;
        }
        isCollected = true;

        inventory.AddItem(type);

        //display maze only when key is collected
        if (type == CollectibleType.Key && mazeManager != null) 
        {
            foreach (Transform child in mazeManager.transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        if (collectBurst != null) 
        { 
            GameObject particleSystem = Instantiate(collectBurst, transform.position, Quaternion.identity);
            Destroy(particleSystem, 2f);
        }

        Blink blinkScript = GetComponent<Blink>();
        if (blinkScript != null)
        {
            blinkScript.enabled = false;
        }

        if (audioSource != null)
        {
            audioSource.Play();

            // disable the colliders
            Collider parentCollider = GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = false;
            }

            // disable the renderer in parents if present
            Renderer parentRenderer = GetComponent<Renderer>();
            if (parentRenderer != null)
            {
                parentRenderer.enabled = false;
            }

            // disable the renderer in children if present
            Renderer childRenderer = GetComponentInChildren<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.enabled = false;
            }

            Destroy(gameObject, audioSource.clip.length);
        }
        else
        {
            // if no sound, then destroy immediately
            Destroy(gameObject);
        }
    }
}