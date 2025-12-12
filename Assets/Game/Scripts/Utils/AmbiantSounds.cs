using System.Collections;
using System.Collections.Generic;
using Rush.Game.Core;
using UnityEngine;

public class AmbiantSounds : MonoBehaviour
{
    [SerializeField] List<AudioClip> audioClips= new List<AudioClip>();
    [SerializeField] float minLength, maxLength;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlaySound());
    }

    IEnumerator PlaySound()
    {
        float delay = Random.Range(minLength, maxLength);
        AudioClip lClip = audioClips[Random.Range(0, audioClips.Count)];
                Debug.Log("Je joue " + lClip.name);

        Vector3 lPosition = new Vector3(Random.Range(-50, 50), 5, Random.Range(-50, 50));
        Manager_Audio.Instance.PlayAtPosition(lClip, lPosition, pMixerGroup:"Ambiant");
        yield return new WaitForSeconds(delay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
