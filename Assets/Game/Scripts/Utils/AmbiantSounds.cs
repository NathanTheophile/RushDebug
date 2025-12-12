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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlaySound());
    }

    IEnumerator PlaySound()
    {
        if (audioClips == null || audioClips.Count == 0)
            yield break;

        while (true)
        {
            float lDelay = Mathf.Max(0f, Random.Range(minLength, maxLength));
            AudioClip lClip = audioClips[Random.Range(0, audioClips.Count)];

            Vector3 lPosition = new Vector3(Random.Range(-50, 50), 5, Random.Range(-50, 50));
            Manager_Audio.Instance.PlayAtPosition(lClip, pPosition:lPosition, pMixerGroup:Bus, pVolume:volume);
            yield return new WaitForSeconds(lDelay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
