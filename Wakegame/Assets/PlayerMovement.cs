using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerStates
    {
        RIDING,
        RAIL,
        KICKER,
        AERIAL
    }



    [Header("Physics")]
    public float speed;
    public float steerSpeed;

    public float ropeLength;

    public LayerMask dampeningMask;

    public PlayerStates PlayerState = PlayerStates.RIDING;
    public bool grounded;


    float allowedSidewaysDrift;
    Vector3 moveVector;
    Rigidbody rb;
    Vector3 inputVector;


    public bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector3.down * 0.6f, Color.red);
        RaycastHit hit;
        grounded = Physics.Raycast(transform.position, Vector3.down * 0.5f, out hit, 0.5f, dampeningMask);

        return grounded;
    }


    // Start is called before the first frame update
    void Start()
    {
        allowedSidewaysDrift = ropeLength * 0.75f;
        rb = GetComponent<Rigidbody>();
        moveVector = rb.velocity;
    }


    void Move()
    {
        moveVector.z = moveVector.z < speed ? speed : moveVector.z;
    }

    void Steer()
    {
        moveVector.x = inputVector.x * steerSpeed;
    }

    void Dampen()
    {
        if(inputVector.x == 0)
        {
            if(transform.position.x >= 1)
            {
                moveVector.x = -1;
            } else if (transform.position.x <= -1)
            {
                moveVector.x = 1;
            }
        } else if (inputVector.x > 0)
        {
            if(transform.position.x >= ropeLength * 0.75)
            {
                moveVector.x += (ropeLength * 0.75f) - transform.position.x;
            }
        } else if (inputVector.x < 0)
        {
            if (transform.position.x <= -ropeLength * 0.75)
            {
                moveVector.x -= (ropeLength * 0.75f) - -transform.position.x;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        inputVector = Vector3.zero;
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.z = Input.GetAxis("Vertical");

        PlayerState = IsGrounded() ? PlayerStates.RIDING : PlayerStates.AERIAL;

        Move();
        Steer();
        Dampen();

        switch (PlayerState)
        {
            case PlayerStates.RIDING:
                transform.LookAt(transform.position + new Vector3(rb.velocity.x, 0, rb.velocity.z));
            break;

            case PlayerStates.AERIAL:
                transform.LookAt(transform.position + new Vector3(rb.velocity.x, 0, rb.velocity.z));
            break;


        }

        rb.velocity = moveVector;
        Debug.DrawRay(transform.position, moveVector, Color.black, 20);
    }


    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), rb.velocity.ToString());
    }



    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Kicker")
        {
            Debug.DrawRay(transform.position, rb.velocity, Color.cyan, 20);
            Vector3 velocity = moveVector;
            Quaternion oldRot = transform.rotation;

            ContactPoint hit = collision.contacts[0];
            Debug.DrawRay(transform.position, -hit.normal, Color.green, 10);

            PlayerState = PlayerStates.KICKER;

            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            velocity = transform.forward * velocity.magnitude;
            transform.rotation = oldRot;
            moveVector = velocity;
            Debug.DrawRay(transform.position, moveVector, Color.black, 20);

        }
    }

}
