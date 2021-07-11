using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerMovementFPS : MonoBehaviour
{
    [SerializeField] KeyCode jumpKey = KeyCode.Space;

    public CharacterController controller;
    public float speed = 12f;
    
    //Apparently mixing character controller and rigidbody is a bad idea, so instead to get
    //things like gravity we introduce the velocity vector which is updated.
    public float gravity = -9.81f;
    Vector3 velocity;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public float jumpHeight = 6f;
    bool isGrounded;
    
    InputDevice device;

    // Start is called before the first frame update
    void Start()
    {
        var leftHandDevices = new List<InputDevice>();   
        InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
        if(leftHandDevices.Count == 1)
        {
            device = leftHandDevices[0];
            Debug.Log("Found left hand device");
        }
        else {
            Debug.Log("No left hand devices!!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        isGrounded = GetComponent<CharacterController>().isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; //"Set to zero" but actually force player to indeed hit the bottom
        }

        bool jumpValue;
        if ( isGrounded &&
            (   
                Input.GetKeyDown(jumpKey) ||
                (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out jumpValue) && jumpValue)
            )
        ){
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //local coordinates based direction we want to move
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
