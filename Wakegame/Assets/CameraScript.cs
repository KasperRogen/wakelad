using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(0, 
                                        Player.transform.position.y + 4, 
                                        Player.transform.position.z - 6);
    }
}
