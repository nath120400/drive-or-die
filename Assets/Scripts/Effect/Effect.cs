using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public EffectDescription Description { get; set; }

    private ParticleSystem _particles;
    private AudioSource _audio;

    private void Awake()
    {
        _particles = GetComponentInChildren<ParticleSystem>();
        _audio = GetComponentInChildren<AudioSource>();
    }

    public void Play()
    {
        if (_particles != null)
        {
            _particles.Play();
        }

        if (_audio != null)
        {
            _audio.Play();
        }
    }

    private void Update()
    {
        // The effect is done when its particles are dead and its audio is over
        bool particlesDone = _particles == null || !_particles.IsAlive();
        bool audioDone = _audio == null || !_audio.isPlaying;

        if (particlesDone && audioDone && Description != null)
        {
            Description.Release(this);
        }
    }
}
