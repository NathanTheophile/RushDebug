using System.Collections;
using System.Collections.Generic;
using Rush.Game.Core;
using UnityEngine;

public class AmbiantSounds : MonoBehaviour
{
    [SerializeField] List<AudioClip> audioClips= new List<AudioClip>();
    [SerializeField] float minLength, maxLength;
    [SerializeField] string Bus;
    [SerializeField, Range(0f, 1f)] float volume = 1f;
    [SerializeField] float startDelay = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlaySound());
    }

    IEnumerator PlaySound()
    {
        if (audioClips == null || audioClips.Count == 0)
            yield break;

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);
            
        while (true)
        {
            float lDelay = Mathf.Max(0f, Random.Range(minLength, maxLength));
            AudioClip lClip = audioClips[Random.Range(0, audioClips.Count)];

            Manager_Audio.Instance.PlayOneShot(lClip, pMixerGroup:Bus, pVolume:volume);
            yield return new WaitForSeconds(lDelay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
