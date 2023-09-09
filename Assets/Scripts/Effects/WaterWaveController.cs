using System.Collections.Generic;
using UnityEngine;

namespace Effects
{
    public class WaterWaveController : MonoBehaviour
    {
        private List<Vector4> m_waveSources = new List<Vector4>();
        private List<Vector4> m_waveParameters = new List<Vector4>();
        private List<Vector4> waveLifeTimes = new List<Vector4>();

        [SerializeField] private float m_waveDistance = 1f;
        [SerializeField] private float m_waveAmplitude = 1f;
        [SerializeField] private float m_waveSpeed = 1f;
        [SerializeField] private float m_waveDecay = 1f;
        [SerializeField] private float m_waveLifeTime = 5f;

        private Material m_waterMaterial;

        void Start() {
            m_waterMaterial = GetComponent<Renderer>().sharedMaterial;
        }

        void Update() {
            if (m_waveSources.Count > 0)
            {
                m_waterMaterial.SetVectorArray("_WaveSources", m_waveSources);
                m_waterMaterial.SetVectorArray("_WaveParametersArray", m_waveParameters);
                m_waterMaterial.SetVectorArray("_WaveLifeTimes", waveLifeTimes);
            }

            m_waterMaterial.SetInt("_NumWaveSources", m_waveSources.Count);

            for (int i = waveLifeTimes.Count - 1; i >= 0; i--)
            {
                if (Time.time > waveLifeTimes[i].x)
                {
                    m_waveSources.RemoveAt(i);
                    m_waveParameters.RemoveAt(i);
                    waveLifeTimes.RemoveAt(i);
                }
            }
        }

        public void CreateWave(Vector3 position) {
            m_waveSources.Add(position);
            m_waveParameters.Add(new Vector4(m_waveDistance, m_waveAmplitude, m_waveSpeed, m_waveDecay));
            waveLifeTimes.Add(new Vector4(Time.time + m_waveLifeTime, 0, 0, 0));
        }
    }
}