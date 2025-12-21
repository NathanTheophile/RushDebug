using UnityEngine;

public class AlwaysFaceUp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        transform.rotation *= Quaternion.Inverse(transform.parent.rotation);
        transform.rotation *= Quaternion.LookRotation(Vector3.up);   
    }
}
