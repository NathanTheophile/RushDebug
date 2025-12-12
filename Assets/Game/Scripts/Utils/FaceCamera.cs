using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform mLookAt;
    private Transform localTrans;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        localTrans = GetComponent<Transform>();   
        mLookAt = Camera.main.transform; 
    }

    // Update is called once per frame
    void Update()
    {
        if(mLookAt)
        {
            localTrans.LookAt(2* localTrans.position - mLookAt.position);
        }
    }
}
