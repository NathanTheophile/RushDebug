using UnityEngine;

public class RotaBush : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int rotaratio = Random.Range(0, 360);
        Quaternion rota = Quaternion.Euler(90, 0, rotaratio);
        transform.rotation = rota;

        float scaleratio = Random.Range(0.04f, 0.08f);
        transform.localScale = new Vector3(scaleratio, scaleratio, scaleratio);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
