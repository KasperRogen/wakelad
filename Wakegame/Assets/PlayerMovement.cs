using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Physics")]
    public float speed;
    public float steerSpeed;

    public float ropeLength;

    public LayerMask dampeningMask;



    float allowedSidewaysDrift;
    Vector3 moveVector;
    Rigidbody rb;
    Vector3 inputVector;


    public bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector3.down * 0.5f, Color.red);
        RaycastHit hit;
        bool grounded;
        grounded = Physics.Raycast(transform.position, Vector3.down * 0.5f, out hit, dampeningMask);

        Debug.Log(hit.collider.gameObject.name);

        return grounded;
    }


    // Start is called before the first frame update
    void Start()
    {
        allowedSidewaysDrift = ropeLength * 0.75f;
        rb = GetComponent<Rigidbody>();
    }


    void Move()
    {
        moveVector = rb.velocity;
        moveVector.z = speed;
    }

    void Steer()
    {
        moveVector.x = inputVector.x * steerSpeed;
    }

    void Dampen()
    {
        if (IsGrounded()) {
            Debug.Log("GROUNDED!");
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

        rb.velocity = moveVector;
    }

    // Update is called once per frame
    void Update()
    {
        inputVector = Vector3.zero;
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.z = Input.GetAxis("Vertical");


        Move();
        Steer();
        Dampen();

        transform.LookAt(transform.position + new Vector3(rb.velocity.x, 0, rb.velocity.z));
    }


    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), rb.velocity.ToString());
    }

}
