using Rush.Game.Core;
using UnityEngine;

public class AmbiantLoop : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] string Bus;
    [SerializeField, Range(0f, 1f)] float volume = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Manager_Audio.Instance.PlayLoop(audioClip, pVolume:volume, pMixerGroup:Bus);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
