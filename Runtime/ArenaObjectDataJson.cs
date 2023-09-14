using System;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace ArenaUnity
{
    [Serializable]
    public class ArenaObjectDataJson
    {
        private static string defObjectType = "entity";
        [JsonProperty(PropertyName = "object_type")]
        [Tooltip("3D object type.")]
        public string object_type = defObjectType;

        [JsonProperty(PropertyName = "parent")]
        [Tooltip("Parent's object_id. Child objects inherit attributes of their parent, for example scale and translation.")]
        public string parent;

        [JsonProperty(PropertyName = "position")]
        [Tooltip("3D object position")]
        public ArenaPositionJson position;

        [JsonProperty(PropertyName = "rotation")]
        [Tooltip("3D object rotation in quaternion representation; Right-handed coordinate system. Euler degrees are deprecated in wire message format.")]
        public ArenaRotationJson rotation;

        [JsonProperty(PropertyName = "scale")]
        [Tooltip("3D object scale")]
        public ArenaScaleJson scale;

        [JsonProperty(PropertyName = "animation")]
        [Tooltip("Animate and tween values. ")]
        public ArenaAnimationJson animation;

        [JsonProperty(PropertyName = "armarker")]
        [Tooltip("A location marker (such as an AprilTag, a lightAnchor, or an UWB tag), used to anchor scenes, or scene objects, in the real world.")]
        public ArenaArmarkerJson armarker;

        [JsonProperty(PropertyName = "click-listener")]
        [Tooltip("Object will listen for clicks")]
        public ArenaClickListenerJson clickListener;

        [JsonProperty(PropertyName = "box-collision-listener")]
        [Tooltip("Listen for bounding-box collisions with user camera and hands. Must be applied to an object or model with geometric mesh. Collisions are determined by course bounding-box overlaps")]
        public ArenaBoxCollisionListenerJson boxCollisionListener;

        [JsonProperty(PropertyName = "collision-listener")]
        [Tooltip("Name of the collision-listener, default can be empty string. Collisions trigger click events")]
        public string collisionListener;

        [JsonProperty(PropertyName = "blip")]
        [Tooltip("When the object is created or deleted, it will animate in/out of the scene instead of appearing/disappearing instantly. Must have a geometric mesh.")]
        public ArenaBlipJson blip;

        [JsonProperty(PropertyName = "dynamic-body")]
        [Tooltip("Physics type attached to the object. ")]
        public ArenaDynamicBodyJson dynamicBody;

        [JsonProperty(PropertyName = "goto-landmark")]
        [Tooltip("Teleports user to the landmark with the given name; Requires click-listener")]
        public ArenaGotoLandmarkJson gotoLandmark;

        [JsonProperty(PropertyName = "goto-url")]
        [Tooltip("Goto given URL; Requires click-listener")]
        public ArenaGotoUrlJson gotoUrl;

        [JsonProperty(PropertyName = "hide-on-enter-ar")]
        [Tooltip("Hide object when entering AR. Remove component to *not* hide")]
        public bool? hideOnEnterAr;

        [JsonProperty(PropertyName = "hide-on-enter-vr")]
        [Tooltip("Hide object when entering VR. Remove component to *not* hide")]
        public bool? hideOnEnterVr;

        [JsonProperty(PropertyName = "show-on-enter-ar")]
        [Tooltip("Show object when entering AR. Hidden otherwise")]
        public bool? showOnEnterAr;

        [JsonProperty(PropertyName = "show-on-enter-vr")]
        [Tooltip("Show object when entering VR. Hidden otherwise")]
        public bool? showOnEnterVr;

        [JsonProperty(PropertyName = "impulse")]
        [Tooltip("The force applied using physics. Requires click-listener")]
        public ArenaImpulseJson impulse;

        [JsonProperty(PropertyName = "landmark")]
        [Tooltip("Define entities as a landmark; Landmarks appears in the landmark list and you can move (teleport) to them; You can define the behavior of the teleport: if you will be at a fixed or random distance, looking at the landmark, fixed offset or if it is constrained by a navmesh (when it exists)")]
        public ArenaLandmarkJson landmark;

        [JsonProperty(PropertyName = "material-extras")]
        [Tooltip("Define extra material properties, namely texture encoding, whether to render the material's color and render order. The properties set here access directly Three.js material component. ")]
        public ArenaMaterialExtrasJson materialExtras;

        [JsonProperty(PropertyName = "shadow")]
        [Tooltip("shadow")]
        public ArenaShadowJson shadow;

        [JsonProperty(PropertyName = "sound")]
        [Tooltip("The sound component defines the entity as a source of sound or audio. The sound component is positional and is thus affected by the component's position. ")]
        public ArenaSoundJson sound;

        [JsonProperty(PropertyName = "textinput")]
        [Tooltip("Opens an HTML prompt when clicked. Sends text input as an event on MQTT. Requires click-listener.")]
        public ArenaTextinputJson textinput;

        [JsonProperty(PropertyName = "url")]
        [Tooltip("Model URL. Store files paths under 'store/users/<username>' (e.g. store/users/wiselab/models/factory_robot_arm/scene.gltf); to use CDN, prefix with `https://arena-cdn.conix.io/` (e.g. https://arena-cdn.conix.io/store/users/wiselab/models/factory_robot_arm/scene.gltf)")]
        public string url;

        [JsonProperty(PropertyName = "screenshareable")]
        [Tooltip("Whether or not a user can screenshare on an object")]
        public bool? screenshareable;

        [JsonProperty(PropertyName = "remote-render")]
        [Tooltip("Whether or not an object should be remote rendered [Experimental]")]
        public ArenaRemoteRenderJson remoteRender;

        [JsonProperty(PropertyName = "video-control")]
        [Tooltip("Video Control")]
        public ArenaVideoControlJson videoControl;

        [JsonProperty(PropertyName = "attribution")]
        [Tooltip("Attribution Component. Saves attribution data in any entity.")]
        public ArenaAttributionJson attribution;

        [JsonProperty(PropertyName = "particle-system")]
        [Tooltip("Particle system component for A-Frame. ")]
        public ArenaParticleSystemJson particleSystem;

        [JsonProperty(PropertyName = "spe-particles")]
        [Tooltip("GPU based particle systems in A-Frame. ")]
        public ArenaSpeParticlesJson speParticles;

        [JsonProperty(PropertyName = "buffer")]
        [Tooltip("Transform geometry into a BufferGeometry to reduce memory usage at the cost of being harder to manipulate (geometries only: box, circle, cone, ...).")]
        public bool? buffer;

        [JsonProperty(PropertyName = "jitsi-video")]
        [Tooltip("Apply a jitsi video source to the geometry")]
        public ArenaJitsiVideoJson jitsiVideo;

        [JsonProperty(PropertyName = "material")]
        [Tooltip("The material properties of the object’s surface. ")]
        public ArenaMaterialJson material;

        [JsonProperty(PropertyName = "multisrc")]
        [Tooltip("Define multiple visual sources applied to an object.")]
        public ArenaMultisrcJson multisrc;

        [JsonProperty(PropertyName = "skipCache")]
        [Tooltip("Disable retrieving the shared geometry object from the cache. (geometries only: box, circle, cone, ...).")]
        public bool? skipCache;

        [JsonProperty(PropertyName = "visible")]
        [Tooltip("Wether to render the object.")]
        public bool? visible;
    }

}
