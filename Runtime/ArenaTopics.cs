namespace ArenaUnity
{

    public readonly struct ArenaTopics
    {
        /**
        * ARENA pubsub topic variables
        * - nameSpace - namespace of the scene
        * - sceneName - name of the scene
        * - userName - name of the user per arena-auth (e.g. jdoe)
        * - idTag - username prefixed with a uuid (e.g. 1448081341_jdoe)
        * - camName - idTag prefixed with camera_ (e.g. camera_1448081341_jdoe)
        */
        public ArenaTopics(
            string realm = "",
            string name_space = "",
            string scenename = "",
            string idtag = "",
            string uuId = "",
            string userobj = "",
            string objectid = "",
            string touid = "",
            string devicename = ""
            )
        {
            REALM = realm;
            nameSpace = name_space;
            sceneName = scenename;
            idTag = idtag;
            uuid = uuId;
            userObj = userobj;
            objectId = objectid;
            toUid = touid;
            deviceName = devicename;
        }
        public readonly string REALM { get; }
        public readonly string nameSpace { get; }
        public readonly string sceneName { get; }
        public readonly string idTag { get; }
        public readonly string uuid { get; }
        public readonly string userObj { get; }
        public readonly string objectId { get; }
        public readonly string toUid { get; }
        public readonly string deviceName { get; }

#pragma warning disable format
        // Disable auto-format to keep alignment for readability

         // SUBSCRIBE
        public string SUB_NETWORK               { get { return "$NETWORK"; } }
        public string SUB_DEVICE                { get { return $"{REALM}/d/{deviceName}/#"; } } // All client placeholder
        public string SUB_PROC_REG              { get { return $"{REALM}/proc/reg"; } }
        public string SUB_PROC_CTL              { get { return $"{REALM}/proc/control/{uuid}/#"; } }
        public string SUB_PROC_DBG              { get { return $"{REALM}/proc/debug/{uuid}"; } }
        public string SUB_SCENE_PUBLIC          { get { return $"{REALM}/s/{nameSpace}/{sceneName}/+/+"; } }
        public string SUB_SCENE_PRIVATE         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/+/+/{idTag}/#"; } }

        // PUBLISH
        public string PUB_NETWORK_LATENCY       { get { return "$NETWORK/latency"; } }
        public string PUB_DEVICE                { get { return $"{REALM}/d/{deviceName}/{idTag}"; } }
        public string PUB_PROC_REG              { get { return $"{REALM}/proc/reg"; } }
        public string PUB_PROC_CTL              { get { return $"{REALM}/proc/control"; } }
        public string PUB_PROC_DBG              { get { return $"{REALM}/proc/debug/{uuid}"; } }
        public string PUB_SCENE_PRESENCE        { get { return $"{REALM}/s/{nameSpace}/{sceneName}/x/{idTag}"; } }
        public string PUB_SCENE_PRESENCE_PRIVATE{ get { return $"{REALM}/s/{nameSpace}/{sceneName}/x/{idTag}/{toUid}"; } }
        public string PUB_SCENE_CHAT            { get { return $"{REALM}/s/{nameSpace}/{sceneName}/c/{idTag}"; } }
        public string PUB_SCENE_CHAT_PRIVATE    { get { return $"{REALM}/s/{nameSpace}/{sceneName}/c/{idTag}/{toUid}"; } }
        public string PUB_SCENE_USER            { get { return $"{REALM}/s/{nameSpace}/{sceneName}/u/{userObj}"; } }
        public string PUB_SCENE_USER_PRIVATE    { get { return $"{REALM}/s/{nameSpace}/{sceneName}/u/{userObj}/{toUid}"; } } // Need to add face_ privs
        public string PUB_SCENE_OBJECTS         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/o/{objectId}"; } } // All client placeholder
        public string PUB_SCENE_OBJECTS_PRIVATE { get { return $"{REALM}/s/{nameSpace}/{sceneName}/o/{objectId}/{toUid}"; } }
        public string PUB_SCENE_RENDER          { get { return $"{REALM}/s/{nameSpace}/{sceneName}/r/{idTag}"; } }
        public string PUB_SCENE_RENDER_PRIVATE  { get { return $"{REALM}/s/{nameSpace}/{sceneName}/r/{idTag}/-"; } } // To avoid unpriv sub
        public string PUB_SCENE_ENV             { get { return $"{REALM}/s/{nameSpace}/{sceneName}/e/{idTag}"; } }
        public string PUB_SCENE_ENV_PRIVATE     { get { return $"{REALM}/s/{nameSpace}/{sceneName}/e/{idTag}/-"; } } // To avoid unpriv sub
        public string PUB_SCENE_PROGRAM         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/p/{idTag}"; } }
        public string PUB_SCENE_PROGRAM_PRIVATE { get { return $"{REALM}/s/{nameSpace}/{sceneName}/p/{idTag}/{toUid}"; } }
        public string PUB_SCENE_DEBUG           { get { return $"{REALM}/s/{nameSpace}/{sceneName}/d/{idTag}/-"; } } // To avoid unpriv sub
#pragma warning restore format
    }
}
