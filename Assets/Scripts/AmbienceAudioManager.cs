using System.Collections.Generic;
using System.IO.Compression;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class AmbienceAudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioSource> zones;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        zones = new List<AudioSource>(GetComponentsInChildren<AudioSource>());
        foreach (AudioSource z in zones) z.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
