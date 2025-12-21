using UnityEngine;

public class RotatesTowardsCamera : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = this.GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canvas != null && Camera.main != null) canvas.transform.rotation = Quaternion.Slerp(canvas.transform.rotation, Camera.main.transform.rotation, 5f * Time.deltaTime);
    }
}
