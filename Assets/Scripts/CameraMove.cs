using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
  [SerializeField]
  private InputActionProperty moveAction;

  private float thetaY = 0;
  private float thetaX = 0;

  [SerializeField]
  private float hoizontalArc = 100;

  [SerializeField]
  private float verticalArc = 30;

  [SerializeField]
  private float speed = 0.2f;

  private void Start()
  {
    moveAction.action.performed += OnMove;
  }


  public void OnMove(InputAction.CallbackContext context)
  {
    //InputValue can be anything, so it must be cast
    Vector2 delta = context.ReadValue<Vector2>();

    //estimate motion from mouse speed
    thetaY += delta.x * speed;
    thetaX -= delta.y * speed;

    //apply rotation amount, wiht limits
    thetaY = Mathf.Clamp(thetaY, -hoizontalArc, hoizontalArc);
    thetaX = Mathf.Clamp(thetaX, -verticalArc, verticalArc);
    transform.localRotation = Quaternion.Euler(thetaX, thetaY, 0);
  }

}
