using System.Collections.Generic;
using UnityEngine;

namespace Olallieberry
{
    [RequireComponent(typeof(OWTriggerVolume))]
    public class SunOverrideEffectVolume : MonoBehaviour, SunLightController.ISunOverrider
    {
        public OWTriggerVolume triggerVolume;
        public int priority = 0;
        public float blendSeconds = 0.5f;

        public bool overrideColor;
        public Color color = Color.white;

        public bool overrideIntensity;
        public float intensity = 1f;

        public bool overrideAmbientIntensity;
        public float ambientIntensity = 1f;

        public bool overrideShadowStrength;
        public float shadowStrength = 1f;

        private readonly HashSet<GameObject> _trackedObjects = new HashSet<GameObject>(4);
        private float _currentBlend;

        private void Awake()
        {
            if (triggerVolume == null) triggerVolume = gameObject.GetAddComponent<OWTriggerVolume>();
            triggerVolume.OnEntry += OnEnter;
            triggerVolume.OnExit += OnExit;
        }

        private void OnDestroy()
        {
            triggerVolume.OnEntry -= OnEnter;
            triggerVolume.OnExit -= OnExit;
        }

        public void OnEnable() =>
            SunLightController.RegisterSunOverrider(this, priority);

        public void OnDisable() =>
            SunLightController.UnregisterSunOverrider(this);

        /*public override void OnSectorOccupantsUpdated()
        {
            enabled = _sector.ContainsAnyOccupants(
                DynamicOccupant.Player | DynamicOccupant.Probe
            );
        }*/

        private static bool IsTrackedDetector(GameObject obj)
        {
            return obj.CompareTag("PlayerDetector")
                || obj.CompareTag("ProbeDetector")
                || obj.CompareTag("PlayerCameraDetector");
        }

        public void OnEnter(GameObject hitObj)
        {
            if (hitObj != null && IsTrackedDetector(hitObj))
                _trackedObjects.Add(hitObj);
        }

        public void OnExit(GameObject hitObj)
        {
            if (hitObj != null)
                _trackedObjects.Remove(hitObj);
        }

        public SunLightController.SunOverrideSettings ApplySunOverrides(
            OWCamera camera,
            SunLightController.SunOverrideSettings settings)
        {
            // determine desired target based on whether any tracked detectors are present
            float target = _trackedObjects.Count > 0 ? 1f : 0f;

            if (blendSeconds <= 0f)
            {
                _currentBlend = target;
            }
            else
            {
                // Move towards target over time. Using Time.deltaTime since this is called on pre-cull.
                float maxDelta = Time.deltaTime / blendSeconds;
                _currentBlend = Mathf.MoveTowards(_currentBlend, target, maxDelta);
            }

            float blend = _currentBlend;

            if (overrideColor)
                settings.sunColor = Color.Lerp(settings.sunColor, color, blend);

            if (overrideIntensity)
                settings.sunIntensity = Mathf.Lerp(settings.sunIntensity, intensity, blend);

            if (overrideAmbientIntensity)
                settings.ambientIntensity = Mathf.Lerp(
                    settings.ambientIntensity,
                    ambientIntensity,
                    blend
                );

            if (overrideShadowStrength)
                settings.sunShadowStrength = Mathf.Lerp(
                    settings.sunShadowStrength,
                    shadowStrength,
                    blend
                );

            return settings;
        }
    }
}