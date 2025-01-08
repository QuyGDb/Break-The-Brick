using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public partial class ParticlesController : MonoBehaviour
    {
        [SerializeField] Particle[] particles;

        private static Dictionary<int, Particle> registerParticles = new Dictionary<int, Particle>();

        private static List<ParticleCase> activeParticles = new List<ParticleCase>();
        private static int activeParticlesCount = 0;

        private static List<TweenCase> delayedParticles = new List<TweenCase>();

        public void Initialise()
        {
            // Register particles
            for (int i = 0; i < particles.Length; i++)
            {
                RegisterParticle(particles[i]);
            }

            StartCoroutine(CheckForActiveParticles());
        }

        public static void Clear()
        {
            for(int i = 0; i < delayedParticles.Count; i++)
            {
                delayedParticles[i].KillActive();
            }

            delayedParticles.Clear();

            for (int i = activeParticlesCount - 1; i >= 0; i--)
            {
                activeParticles[i].OnDisable();

                activeParticles.RemoveAt(i);
                activeParticlesCount--;
            }
        }

        private IEnumerator CheckForActiveParticles()
        {
            while (true)
            {
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;

                for (int i = activeParticlesCount - 1; i >= 0; i--)
                {
                    if (activeParticles[i] != null)
                    {
                        if (activeParticles[i].IsForceDisabledRequired())
                            activeParticles[i].ParticleSystem.Stop();

                        if (!activeParticles[i].ParticleSystem.IsAlive())
                        {
                            activeParticles[i].OnDisable();

                            activeParticles.RemoveAt(i);
                            activeParticlesCount--;
                        }
                    }
                    else
                    {
                        activeParticles.RemoveAt(i);
                        activeParticlesCount--;
                    }
                }
            }
        }

        public static ParticleCase ActivateParticle(Particle particle, float delay = 0)
        {
            bool isDelayed = delay > 0;

            ParticleCase particleCase = new ParticleCase(particle, isDelayed);

            if(isDelayed)
            {
                TweenCase delayTweenCase = null;
                
                delayTweenCase = Tween.DelayedCall(delay, () =>
                {
                    particleCase.ParticleSystem.Play();

                    activeParticles.Add(particleCase);
                    activeParticlesCount++;

                    delayedParticles.Remove(delayTweenCase);
                });

                delayedParticles.Add(delayTweenCase);

                return particleCase;
            }

            activeParticles.Add(particleCase);
            activeParticlesCount++;

            return particleCase;
        }

        #region Register
        public static int RegisterParticle(Particle particle)
        {
            int particleHash = particle.ParticleName.GetHashCode();
            if (!registerParticles.ContainsKey(particleHash))
            {
                particle.Initialise();

                registerParticles.Add(particleHash, particle);

                return particleHash;
            }
            else
            {
                Debug.LogError(string.Format("[Particle Controller]: Particle with name {0} already register!"));
            }

            return -1;
        }

        public static int RegisterParticle(string particleName, GameObject particlePrefab)
        {
            return RegisterParticle(new Particle(particleName, particlePrefab));
        }
        #endregion

        #region Play
        public static ParticleCase PlayParticle(string particleName, float delay = 0)
        {
            int particleHash = particleName.GetHashCode();

            if (registerParticles.ContainsKey(particleHash))
            {
                return ActivateParticle(registerParticles[particleHash], delay);
            }

            Debug.LogError(string.Format("[Particles System]: Particle with type {0} is missing!", particleName));

            return null;
        }

        public static ParticleCase PlayParticle(int particleHash, float delay = 0)
        {
            if (registerParticles.ContainsKey(particleHash))
            {
                return ActivateParticle(registerParticles[particleHash], delay);
            }

            Debug.LogError(string.Format("[Particles System]: Particle with hash {0} is missing!", particleHash));

            return null;
        }

        public static ParticleCase PlayParticle(Particle particle, float delay = 0)
        {
            int particleHash = particle.ParticleName.GetHashCode();
            if (registerParticles.ContainsKey(particleHash))
            {
                return ActivateParticle(registerParticles[particleHash], delay);
            }

            Debug.LogError(string.Format("[Particles System]: Particle with hash {0} is missing!", particleHash));

            return null;
        }
        #endregion

        public static int GetHash(string particleName)
        {
            return particleName.GetHashCode();
        }
    }
}

// -----------------
// Particles Controller v1.1
// -----------------

// Changelog
// v 1.1
// • Added custom editor
// • Ring effect scripts moved to separate files
// • Added particle spawn delay
// v 1.0
// • Basic version
