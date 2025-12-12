using UnityEngine;

public class RotaGrass : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int ratio = Random.Range(0, 3);
        Quaternion rota = Quaternion.Euler(0, ratio * 90, 0);
        transform.rotation = rota;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
