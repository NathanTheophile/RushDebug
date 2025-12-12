using Rush.Game.Core;
using UnityEngine;

public class AmbiantLoop : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Manager_Audio.Instance.PlayLoop(audioClip, pMixerGroup:"Ambiant");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
