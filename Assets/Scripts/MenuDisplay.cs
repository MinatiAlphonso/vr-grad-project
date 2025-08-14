using UnityEngine;
using UnityEngine.InputSystem;

public class MenuDisplay : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private InputActionProperty menuAction;
    [SerializeField] private Camera mainCamera;

    private AudioSource audioSource;
    private Vector3 initialOffset;
    private Quaternion initialRotation;
    void Start()
    {
        if (menuAction == null)
        {
            Debug.LogWarning("Menu action is not assigned. Menu display is disabled.");
            return;
        }
        initialOffset = menu.transform.position - mainCamera.transform.position;
        audioSource = menu.GetComponent<AudioSource>();
        menuAction.action.performed += OnMenuDisplay;
    }

    private void OnMenuDisplay(InputAction.CallbackContext context)
    {
        //set menu's start position to where the player is currently located
        menu.transform.position = mainCamera.transform.position + initialOffset;

        // toggle the menu
        menu.SetActive(!menu.activeSelf);
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}