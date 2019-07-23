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

    public enum RidingStates
    {
        REGULAR,
        GOOFY,
        REGULAR_BLIND,
        GOOFY_BLIND,
        REGULAR_WRAPPED,
        GOOFY_WRAPPED,
        CRASHED
    }



    [Header("Physics")]
    public float speed;
    public float steerSpeed;

    public float ropeLength;
    
    public LayerMask groundedMask;

    public PlayerStates PlayerState = PlayerStates.RIDING;
    public RidingStates RidingState = RidingStates.REGULAR;
    public bool grounded;


    float allowedSidewaysDrift;
    Vector3 moveVector;
    Rigidbody rb;
    Vector3 inputVector;


    public bool IsGrounded()
    {
        PlayerStates prevState = PlayerState;
        Debug.DrawRay(transform.position, Vector3.down * 0.6f, Color.red);
        RaycastHit hit;
        grounded = Physics.SphereCast(transform.position, 0.4f, Vector3.down, out hit, 0.6f, groundedMask);

        if(grounded == false)
        {
            PlayerState = PlayerStates.AERIAL;
        }
        else if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            if (prevState == PlayerStates.AERIAL)
                Land();
            PlayerState = PlayerStates.RIDING;
        }
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Kicker"))
        {
            PlayerState = PlayerStates.KICKER;
        }
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Rail"))
        {
            PlayerState = PlayerStates.RAIL;
        } 

        return grounded && hit.transform.gameObject.layer == LayerMask.NameToLayer("Water");
    }

    void Land()
    {
        Vector3 travelDirection = rb.velocity;
        travelDirection.y = 0;

        Vector3 lookDirection = transform.forward;
        lookDirection.y = 0;

        if(Vector3.Angle(travelDirection, lookDirection) < 30)
        {
            RidingState = RidingStates.REGULAR;
        } else if (Vector3.Angle(travelDirection, -lookDirection) < 30)
        {
            RidingState = RidingStates.REGULAR_BLIND;
        } else
        {
            RidingState = RidingStates.CRASHED;
            Debug.Log("CRASH");
            rb.constraints = RigidbodyConstraints.None;
        }
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
        switch (PlayerState)
        {
            case PlayerStates.RIDING:
                moveVector.x = inputVector.x * steerSpeed;
                moveVector.z += Mathf.Abs(inputVector.x * steerSpeed)* Time.deltaTime * 2f;
            break;

            case PlayerStates.AERIAL:
                moveVector.x = rb.velocity.x;
                transform.RotateAround(transform.position, transform.up, inputVector.x * 360 * Time.deltaTime);
            break;

            case PlayerStates.KICKER:
                moveVector.x = rb.velocity.x;
                transform.RotateAround(transform.position, transform.up, inputVector.x * 360 * Time.deltaTime);
                break;

        }
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
        } else if (inputVector.x > 0 && PlayerState == PlayerStates.RIDING)
        {
            if(transform.position.x >= ropeLength * 0.75)
            {
                moveVector.x += (ropeLength * 0.75f) - transform.position.x;
            }
        } else if (inputVector.x < 0 && PlayerState == PlayerStates.RIDING)
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.position = new Vector3(0, 0.6f, -5);
            rb.velocity = Vector3.zero;
            RidingState = RidingStates.REGULAR;
            PlayerState = PlayerStates.RIDING;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        inputVector = Vector3.zero;
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.z = Input.GetAxis("Vertical");

        if (moveVector == Vector3.zero)
            moveVector = rb.velocity;
        

        
        IsGrounded();

        if (RidingState == RidingStates.CRASHED)
            return;

        Move();
        Steer();
        Dampen();


        if(PlayerState == PlayerStates.RIDING)
        {
            if (RidingState == RidingStates.REGULAR)
                transform.LookAt(transform.position + new Vector3(rb.velocity.x, 0, rb.velocity.z));
            else if (RidingState == RidingStates.REGULAR_BLIND)
                transform.LookAt(transform.position - new Vector3(rb.velocity.x, 0, rb.velocity.z));
        }
            


        if(moveVector != Vector3.zero)
        rb.velocity = moveVector;

        Debug.DrawRay(transform.position, moveVector, Color.black, 20);

        moveVector = Vector3.zero;
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
            Vector3 velocity = rb.velocity;
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
