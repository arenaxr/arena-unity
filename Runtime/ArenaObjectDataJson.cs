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
        public string ObjectType = defObjectType;

        [JsonProperty(PropertyName = "parent")]
        [Tooltip("Parent's object_id. Child objects inherit attributes of their parent, for example scale and translation.")]
        public string Parent;

        [JsonProperty(PropertyName = "position")]
        [Tooltip("3D object position")]
        public ArenaPositionJson Position;

        [JsonProperty(PropertyName = "rotation")]
        [Tooltip("3D object rotation in quaternion representation; Right-handed coordinate system. Euler degrees are deprecated in wire message format.")]
        public ArenaRotationJson Rotation;

        [JsonProperty(PropertyName = "scale")]
        [Tooltip("3D object scale")]
        public ArenaScaleJson Scale;

        [JsonProperty(PropertyName = "animation")]
        [Tooltip("Animate and tween values. ")]
        public ArenaAnimationJson Animation;

        [JsonProperty(PropertyName = "armarker")]
        [Tooltip("A location marker (such as an AprilTag, a lightAnchor, or an UWB tag), used to anchor scenes, or scene objects, in the real world.")]
        public ArenaArmarkerJson Armarker;

        [JsonProperty(PropertyName = "click-listener")]
        [Tooltip("Object will listen for clicks")]
        public ArenaClickListenerJson ClickListener;

        [JsonProperty(PropertyName = "box-collision-listener")]
        [Tooltip("Listen for bounding-box collisions with user camera and hands. Must be applied to an object or model with geometric mesh. Collisions are determined by course bounding-box overlaps")]
        public ArenaBoxCollisionListenerJson BoxCollisionListener;

        [JsonProperty(PropertyName = "collision-listener")]
        [Tooltip("Name of the collision-listener, default can be empty string. Collisions trigger click events")]
        public string CollisionListener;

        [JsonProperty(PropertyName = "blip")]
        [Tooltip("When the object is created or deleted, it will animate in/out of the scene instead of appearing/disappearing instantly. Must have a geometric mesh.")]
        public ArenaBlipJson Blip;

        [JsonProperty(PropertyName = "dynamic-body")]
        [Tooltip("Physics type attached to the object. ")]
        public ArenaDynamicBodyJson DynamicBody;

        [JsonProperty(PropertyName = "goto-landmark")]
        [Tooltip("Teleports user to the landmark with the given name; Requires click-listener")]
        public ArenaGotoLandmarkJson GotoLandmark;

        [JsonProperty(PropertyName = "goto-url")]
        [Tooltip("Goto given URL; Requires click-listener")]
        public ArenaGotoUrlJson GotoUrl;

        [JsonProperty(PropertyName = "hide-on-enter-ar")]
        [Tooltip("Hide object when entering AR. Remove component to *not* hide")]
        public bool HideOnEnterAr;

        [JsonProperty(PropertyName = "hide-on-enter-vr")]
        [Tooltip("Hide object when entering VR. Remove component to *not* hide")]
        public bool HideOnEnterVr;

        [JsonProperty(PropertyName = "show-on-enter-ar")]
        [Tooltip("Show object when entering AR. Hidden otherwise")]
        public bool ShowOnEnterAr;

        [JsonProperty(PropertyName = "show-on-enter-vr")]
        [Tooltip("Show object when entering VR. Hidden otherwise")]
        public bool ShowOnEnterVr;

        [JsonProperty(PropertyName = "impulse")]
        [Tooltip("The force applied using physics. Requires click-listener")]
        public ArenaImpulseJson Impulse;

        [JsonProperty(PropertyName = "landmark")]
        [Tooltip("Define entities as a landmark; Landmarks appears in the landmark list and you can move (teleport) to them; You can define the behavior of the teleport: if you will be at a fixed or random distance, looking at the landmark, fixed offset or if it is constrained by a navmesh (when it exists)")]
        public ArenaLandmarkJson Landmark;

        [JsonProperty(PropertyName = "material-extras")]
        [Tooltip("Define extra material properties, namely texture encoding, whether to render the material's color and render order. The properties set here access directly Three.js material component. ")]
        public ArenaMaterialExtrasJson MaterialExtras;

        [JsonProperty(PropertyName = "shadow")]
        [Tooltip("shadow")]
        public ArenaShadowJson Shadow;

        [JsonProperty(PropertyName = "sound")]
        [Tooltip("The sound component defines the entity as a source of sound or audio. The sound component is positional and is thus affected by the component's position. ")]
        public ArenaSoundJson Sound;

        [JsonProperty(PropertyName = "textinput")]
        [Tooltip("Opens an HTML prompt when clicked. Sends text input as an event on MQTT. Requires click-listener.")]
        public ArenaTextinputJson Textinput;

        [JsonProperty(PropertyName = "url")]
        [Tooltip("Model URL. Store files paths under 'store/users/<username>' (e.g. store/users/wiselab/models/factory_robot_arm/scene.gltf); to use CDN, prefix with `https://arena-cdn.conix.io/` (e.g. https://arena-cdn.conix.io/store/users/wiselab/models/factory_robot_arm/scene.gltf)")]
        public string Url;

        [JsonProperty(PropertyName = "screenshareable")]
        [Tooltip("Whether or not a user can screenshare on an object")]
        public bool Screenshareable;

        [JsonProperty(PropertyName = "remote-render")]
        [Tooltip("Whether or not an object should be remote rendered [Experimental]")]
        public ArenaRemoteRenderJson RemoteRender;

        [JsonProperty(PropertyName = "video-control")]
        [Tooltip("Video Control")]
        public ArenaVideoControlJson VideoControl;

        [JsonProperty(PropertyName = "attribution")]
        [Tooltip("Attribution Component. Saves attribution data in any entity.")]
        public ArenaAttributionJson Attribution;

        [JsonProperty(PropertyName = "particle-system")]
        [Tooltip("Particle system component for A-Frame. ")]
        public ArenaParticleSystemJson ParticleSystem;

        [JsonProperty(PropertyName = "spe-particles")]
        [Tooltip("GPU based particle systems in A-Frame. ")]
        public ArenaSpeParticlesJson SpeParticles;

        [JsonProperty(PropertyName = "buffer")]
        [Tooltip("Transform geometry into a BufferGeometry to reduce memory usage at the cost of being harder to manipulate (geometries only: box, circle, cone, ...).")]
        public bool Buffer;

        [JsonProperty(PropertyName = "jitsi-video")]
        [Tooltip("Apply a jitsi video source to the geometry")]
        public ArenaJitsiVideoJson JitsiVideo;

        [JsonProperty(PropertyName = "material")]
        [Tooltip("The material properties of the object’s surface. ")]
        public ArenaMaterialJson Material;

        [JsonProperty(PropertyName = "multisrc")]
        [Tooltip("Define multiple visual sources applied to an object.")]
        public ArenaMultisrcJson Multisrc;

        [JsonProperty(PropertyName = "skipCache")]
        [Tooltip("Disable retrieving the shared geometry object from the cache. (geometries only: box, circle, cone, ...).")]
        public bool SkipCache;
    }

}
