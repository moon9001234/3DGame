using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Side Scroller 3D/Timeline Particle Preview 3D")]
public sealed class TimelineParticlePreview3D : MonoBehaviour, ITimeControl
{
    [SerializeField, HideInInspector] private bool includeInactiveChildren = true;
    [SerializeField, HideInInspector] private float maxPreviewStep = 0.016666668f;

    private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    private bool cacheDirty = true;

    private void OnEnable()
    {
        cacheDirty = true;
    }

    private void OnTransformChildrenChanged()
    {
        cacheDirty = true;
    }

    public void OnControlTimeStart()
    {
        CacheParticleSystems();
        ResetParticles();
    }

    public void SetTime(double time)
    {
        CacheParticleSystems();
        SimulateToTime(Mathf.Max(0f, (float)time));
    }

    public void OnControlTimeStop()
    {
        CacheParticleSystems();
        ResetParticles();
    }

    private void CacheParticleSystems()
    {
        if (!cacheDirty)
            return;

        particleSystems.Clear();
        GetComponentsInChildren(includeInactiveChildren, particleSystems);
        cacheDirty = false;
    }

    private void SimulateToTime(float targetTime)
    {
        ResetParticles();

        float remaining = targetTime;
        float stepSize = Mathf.Max(0.001f, maxPreviewStep);
        while (remaining > 0f)
        {
            float step = Mathf.Min(stepSize, remaining);
            for (int i = 0; i < particleSystems.Count; i++)
            {
                ParticleSystem particle = particleSystems[i];
                if (particle != null)
                    particle.Simulate(step, false, false, false);
            }

            remaining -= step;
        }
    }

    private void ResetParticles()
    {
        for (int i = 0; i < particleSystems.Count; i++)
        {
            ParticleSystem particle = particleSystems[i];
            if (particle == null)
                continue;

            if (particle.useAutoRandomSeed)
            {
                particle.useAutoRandomSeed = false;
                if (particle.randomSeed == 0)
                    particle.randomSeed = 1;
            }

            particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Simulate(0f, false, true, false);
        }
    }
}
