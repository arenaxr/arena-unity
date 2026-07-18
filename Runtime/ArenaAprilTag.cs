/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using ArenaUnity.AprilTag;
using UnityEngine;

namespace ArenaUnity
{
    /// <summary>
    /// Detects ARENA-standard AprilTag 36h11 markers via a device camera and uses
    /// tag #0 as the physical scene-origin anchor to relocalize the ARENA scene root
    /// in XR. Attach this component to the XR rig or main camera GameObject.
    ///
    /// Supported platforms: Linux x86-64 (native plugin included).
    /// For Windows, macOS, iOS, and Android builds see Runtime/AprilTag/Plugin/README.md.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://docs.arenaxr.org")]
    public class ArenaAprilTag : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("Physical size of the printed AprilTag in meters.")]
        [Min(0.001f)]
        public float tagSize = 0.15f;

        [Tooltip("Tag ID used as the ARENA scene origin (default: 0).")]
        [Min(0)]
        public int originTagId = 0;

        [Tooltip("Decimation factor: higher values improve speed at the cost of detection accuracy (1–4).")]
        [Range(1, 4)]
        public int decimation = 2;

        [Tooltip("Index of the WebCamTexture device to use for detection. -1 uses the first available camera.")]
        public int webCamIndex = -1;

        [Tooltip("Desired webcam resolution width (0 = device default).")]
        public int webCamWidth = 1280;

        [Tooltip("Desired webcam resolution height (0 = device default).")]
        public int webCamHeight = 720;

        [Header("Relocalization")]
        [Tooltip("Transform to reposition when the origin tag is detected. Leave empty to use ArenaClientScene root.")]
        public Transform sceneRoot;

        [Tooltip("Smooth relocalization over multiple detections using exponential moving average.")]
        public bool smoothing = true;

        [Tooltip("Smoothing factor (0=no update, 1=instant snap). Used only when Smoothing is enabled.")]
        [Range(0.01f, 1f)]
        public float smoothingFactor = 0.2f;

        /// <summary>The most recently detected tag poses (all tag IDs in current frame).</summary>
        public System.Collections.Generic.IEnumerable<TagPose> DetectedTags
          => _detector?.DetectedTags ?? System.Linq.Enumerable.Empty<TagPose>();

        /// <summary>Whether the origin tag was detected in the last frame.</summary>
        public bool OriginTagDetected { get; private set; }

        WebCamTexture _webCam;
        TagDetector _detector;
        Color32[] _buffer;

        void Start()
        {
            InitWebCam();
        }

        void OnDestroy()
        {
            _detector?.Dispose();
            _detector = null;

            if (_webCam != null)
            {
                _webCam.Stop();
                Destroy(_webCam);
                _webCam = null;
            }
        }

        void LateUpdate()
        {
            if (_webCam == null || !_webCam.didUpdateThisFrame)
                return;

            EnsureDetector();
            RunDetection();
        }

        void InitWebCam()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogWarning("[ArenaAprilTag] No webcam devices found.");
                return;
            }

            int idx = (webCamIndex >= 0 && webCamIndex < devices.Length)
                ? webCamIndex : 0;

            _webCam = new WebCamTexture(devices[idx].name, webCamWidth, webCamHeight);
            _webCam.Play();
            Debug.Log($"[ArenaAprilTag] Using webcam: {devices[idx].name}");
        }

        void EnsureDetector()
        {
            int w = _webCam.width;
            int h = _webCam.height;
            if (w <= 0 || h <= 0) return;

            if (_detector == null)
            {
                _detector = new TagDetector(w, h, decimation);
                _buffer = new Color32[w * h];
                Debug.Log($"[ArenaAprilTag] Detector initialized ({w}x{h}, decimation={decimation}).");
            }
        }

        void RunDetection()
        {
            if (_detector == null) return;

            _webCam.GetPixels32(_buffer);

            var cam = Camera.main ?? GetComponentInChildren<Camera>(true) ?? GetComponentInParent<Camera>();
            if (cam == null)
            {
                Debug.LogWarning("[ArenaAprilTag] No camera found for FOV calculation.");
                return;
            }

            var fov = cam.fieldOfView * Mathf.Deg2Rad;
            _detector.ProcessImage(_buffer, fov, tagSize);

            OriginTagDetected = false;
            foreach (var tag in _detector.DetectedTags)
            {
                if (tag.ID == originTagId)
                {
                    OriginTagDetected = true;
                    ApplyRelocalization(cam, tag);
                    break;
                }
            }
        }

        /// <summary>
        /// Relocalize the ARENA scene root so that the detected origin tag maps to (0, 0, 0)
        /// with identity rotation in Unity world space.
        ///
        /// Coordinate notes:
        ///   - <c>tag.Position</c> is the tag-center position in Unity camera space (right-handed,
        ///     Y-up after the keijiro coordinate fixup applied in PoseEstimationJob).
        ///   - <c>tag.Rotation</c> is the tag-frame orientation in Unity camera space.
        ///   - The scene root transform is set so the tag appears at the world origin.
        /// </summary>
        void ApplyRelocalization(Camera cam, TagPose tag)
        {
            Transform root = ResolveSceneRoot();
            if (root == null) return;

            // Build the tag's world-space pose from the camera's current world transform.
            Vector3 tagWorldPos = cam.transform.position + cam.transform.rotation * tag.Position;
            Quaternion tagWorldRot = cam.transform.rotation * tag.Rotation;

            // We want the tag to sit at the world origin with identity rotation.
            // Find the transform that maps tagWorldPos/tagWorldRot → (0, 0, 0) / identity:
            //   newRootRot * tagWorldRot = Identity  =>  newRootRot = Inverse(tagWorldRot)
            //   newRootRot * tagWorldPos + newRootPos = 0  =>  newRootPos = -Inverse(tagWorldRot) * tagWorldPos
            Quaternion invTagRot = Quaternion.Inverse(tagWorldRot);
            Vector3 targetPos = -(invTagRot * tagWorldPos);
            Quaternion targetRot = invTagRot;

            if (smoothing)
            {
                root.position = Vector3.Lerp(root.position, targetPos, smoothingFactor);
                root.rotation = Quaternion.Slerp(root.rotation, targetRot, smoothingFactor);
            }
            else
            {
                root.position = targetPos;
                root.rotation = targetRot;
            }
        }

        Transform ResolveSceneRoot()
        {
            if (sceneRoot != null) return sceneRoot;
            if (ArenaClientScene.Instance != null) return ArenaClientScene.Instance.transform;
            Debug.LogWarning("[ArenaAprilTag] No sceneRoot assigned and ArenaClientScene not found.");
            return null;
        }
    }
}
