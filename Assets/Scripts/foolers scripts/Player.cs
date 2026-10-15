using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    private CasinoManager casinoManager;

    [Header("Movement")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private float speed = 5f;

    [Header("Camera")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private float mouseSensitivity = 0.1f;

    [Header("Interaction")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private GameObject interactionPrompt;

    private float cameraPitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Find the CasinoManager in the current scene
        casinoManager = FindFirstObjectByType<CasinoManager>();

        // Lock cursor for first-person camera
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameObject.Find("InteractionPrompt");
    }

    void Update()
    {
        Move();
        Look();
        CheckInteraction();
    }

    private void Move()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 movement =
            transform.right * input.x +
            transform.forward * input.y;

        controller.Move(movement * speed * Time.deltaTime);
    }

    private void Look()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        // Left and right
        transform.Rotate(
            Vector3.up * lookInput.x * mouseSensitivity
        );

        // Up and down
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        playerCamera.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void CheckInteraction()
    {
         interactionPrompt.SetActive(false);
        Debug.DrawRay(
            playerCamera.position,
            playerCamera.forward * interactionDistance,
            Color.red
        );

        if (Physics.Raycast(
            playerCamera.position,
            playerCamera.forward,
            out RaycastHit hit,
            interactionDistance))
        {
            //Debug.Log("Ray hit: " + hit.transform.name);
      


            Transform hitObject = hit.transform;

            bool lookingAtElevator = false;

            if (hitObject.CompareTag("Elevator"))
            {
                lookingAtElevator = true;

            }
            else if (hitObject.parent != null &&
                     hitObject.parent.CompareTag("Elevator"))
            {
                lookingAtElevator = true;
            }

            if (lookingAtElevator)
            {
                Debug.Log("LOOKING AT ELEVATOR");

                interactionPrompt.SetActive(true);


                if (interactAction.action.WasPressedThisFrame())
                {
                    Debug.Log("INTERACT PRESSED ON ELEVATOR");

                    if (casinoManager != null)
                    {
                        casinoManager.UseElevator();
                    }
                    else
                    {
                        Debug.LogError("CasinoManager was not found.");
                    }
                }
            }
        }
    }
}