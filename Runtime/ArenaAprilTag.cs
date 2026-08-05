/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

using UnityEngine;
using ArenaUnity.Components;
#if HAS_AR_FOUNDATION
using ArenaUnity.AprilTag;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace ArenaUnity
{
    /// <summary>
    /// Detects ARENA-standard AprilTag 36h11 markers via ARFoundation and uses them 
    /// for scene relocalization (originTagId or static armarkers) and dynamic object tracking.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://docs.arenaxr.org")]
    public class ArenaAprilTag : MonoBehaviour
    {
#if HAS_AR_FOUNDATION
        [Header("AR Foundation")]
        [Tooltip("Optional ARCameraManager for AR passthrough frame capture. Will find one if empty.")]
        public ARCameraManager arCameraManager;

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

        [Header("Relocalization")]
        [Tooltip("Transform to reposition when a static tag is detected. Leave empty to use ArenaClientScene root.")]
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

        TagDetector _detector;
        Color32[] _buffer;
        Camera _camera;

        void Start()
        {
            if (arCameraManager == null)
            {
#if UNITY_2023_1_OR_NEWER
                arCameraManager = FindFirstObjectByType<ARCameraManager>();
#else
                arCameraManager = FindObjectOfType<ARCameraManager>();
#endif
            }

            if (arCameraManager != null)
            {
                arCameraManager.frameReceived += OnARCameraFrameReceived;
            }
            else
            {
                Debug.LogWarning("[ArenaAprilTag] No ARCameraManager found. AprilTag detection is disabled.");
            }

            CacheCamera();
        }

        void OnDestroy()
        {
            if (arCameraManager != null)
            {
                arCameraManager.frameReceived -= OnARCameraFrameReceived;
            }

            _detector?.Dispose();
            _detector = null;
        }

        void CacheCamera()
        {
            _camera = Camera.main
                ?? GetComponentInChildren<Camera>(true)
                ?? GetComponentInParent<Camera>();
            if (_camera == null)
                Debug.LogWarning("[ArenaAprilTag] No camera found; FOV calculation will be unavailable.");
        }

        void OnARCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
                return;

            using (image)
            {
                int w = image.width;
                int h = image.height;
                if (w <= 0 || h <= 0) return;

                if (_detector == null)
                {
                    _detector = new TagDetector(w, h, decimation);
                    Debug.Log($"[ArenaAprilTag] AR Detector initialized ({w}x{h}, decimation={decimation}).");
                }

                if (_camera == null)
                {
                    Debug.LogWarning("[ArenaAprilTag] No camera found for FOV calculation.");
                    return;
                }

                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, w, h),
                    outputDimensions = new Vector2Int(w, h),
                    outputFormat = TextureFormat.RGBA32,
                    // XRCpuImage is Top-Left. ImageConverter flips Y (for WebCamTexture compat).
                    // We must MirrorY here so the final image passed to AprilTag C library remains Top-Left.
                    transformation = XRCpuImage.Transformation.MirrorY
                };

                if (_buffer == null || _buffer.Length != w * h)
                {
                    _buffer = new Color32[w * h];
                }

                var handle = System.Runtime.InteropServices.GCHandle.Alloc(_buffer, System.Runtime.InteropServices.GCHandleType.Pinned);
                try
                {
                    image.Convert(conversionParams, handle.AddrOfPinnedObject(), _buffer.Length * 4);
                }
                finally
                {
                    handle.Free();
                }

                var fov = _camera.fieldOfView * Mathf.Deg2Rad;
                _detector.ProcessImage(_buffer, fov, tagSize);

                OriginTagDetected = false;
                foreach (var tag in _detector.DetectedTags)
                {
                    ProcessTagDetection(tag);
                }
            }
        }

        void ProcessTagDetection(TagPose tag)
        {
            string tagStr = tag.ID.ToString();
            Debug.Log($"[ArenaAprilTag] Detected Tag ID: {tagStr}");

            // 1. Primary relocalization (Origin Tag)
            if (tag.ID == originTagId)
            {
                OriginTagDetected = true;
                ApplyRelocalization(_camera, tag, Vector3.zero, Quaternion.identity);
            }

            // 2. Secondary Support (armarker components)
            if (ArenaArmarker.ActiveMarkers.TryGetValue(tagStr, out var armarker))
            {
                if (armarker.json.Publish)
                {
                    // Publishing raw detections directly to MQTT is reserved for future implementation
                    // matching arena-web-core's pubDetList
                }

                if (!armarker.json.Dynamic)
                {
                    // STATIC tag: use it to reorient the XR rig so the physical tag aligns with the virtual object
                    ApplyRelocalization(_camera, tag, armarker.transform.position, armarker.transform.rotation);
                }
                else
                {
                    // DYNAMIC tag: move the virtual object to match the physical tag
                    Vector3 tagWorldPos = _camera.transform.position + _camera.transform.rotation * tag.Position;
                    Quaternion tagWorldRot = _camera.transform.rotation * tag.Rotation;

                    if (smoothing)
                    {
                        armarker.transform.position = Vector3.Lerp(armarker.transform.position, tagWorldPos, smoothingFactor);
                        armarker.transform.rotation = Quaternion.Slerp(armarker.transform.rotation, tagWorldRot, smoothingFactor);
                    }
                    else
                    {
                        armarker.transform.position = tagWorldPos;
                        armarker.transform.rotation = tagWorldRot;
                    }

                    if (!armarker.json.Publish)
                    {
                        // Publish the updated GameObject transform over MQTT to the scene
                        armarker.PublishTransformUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// Relocalize the scene root (usually the XR Rig) so that the detected physical tag maps to the expected target pose.
        /// </summary>
        void ApplyRelocalization(Camera cam, TagPose tag, Vector3 targetWorldPos, Quaternion targetWorldRot)
        {
            Transform root = ResolveSceneRoot();
            if (root == null) return;

            // Physical tag's current pose in world space
            Vector3 tagWorldPos = cam.transform.position + cam.transform.rotation * tag.Position;
            Quaternion tagWorldRot = cam.transform.rotation * tag.Rotation;

            // We want to find a transformation T that maps the physical tag's current world pose to the target world pose.
            // T_rot * tagWorldRot = targetWorldRot => T_rot = targetWorldRot * Inverse(tagWorldRot)
            // T_rot * tagWorldPos + T_pos = targetWorldPos => T_pos = targetWorldPos - T_rot * tagWorldPos
            
            Quaternion tRot = targetWorldRot * Quaternion.Inverse(tagWorldRot);
            Vector3 tPos = targetWorldPos - (tRot * tagWorldPos);

            // Apply T to the root (XR Rig or Scene Root)
            Quaternion targetRot = tRot * root.rotation;
            Vector3 targetPos = tRot * root.position + tPos;

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
#else
        void Start()
        {
            Debug.LogWarning("[ArenaAprilTag] AR Foundation is required for AprilTag support. Component disabled.");
        }
#endif
    }
}
