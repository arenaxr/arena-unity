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
            string namespacE = "",
            string scenename = "",
            string username = "",
            string idtag = "",
            string camname = "",
            string uuId = "",
            string userobj = "",
            string objectid = "",
            string touid = "",
            string clientid = ""
            )
            REALM = realm;
            nameSpace = namespacE;
            sceneName = scenename;
            userName = username;
            idTag = idtag;
            camName = camname;
            uuid = uuId;
            userObj = userobj;
            objectId = objectid;
            toUid = touid;
            clientId = clientid;
        }
        public string REALM { get; }
        public string nameSpace { get; }
        public string sceneName { get; }
        public string userName { get; }
        public string idTag { get; }
        public string camName { get; }
        public string uuid { get; }
        public string userObj { get; }
        public string objectId { get; }
        public string toUid { get; }
        public string clientId { get; }

#pragma warning disable format
        // Disable auto-format to keep alignment for readability

         // SUBSCRIBE
        public string SUB_NETWORK               { get { return "$NETWORK"; } }
        public string SUB_CHAT_PUBLIC           { get { return $"{REALM}/c/{nameSpace}/o/#"; } }
        public string SUB_CHAT_PRIVATE          { get { return $"{REALM}/c/{nameSpace}/p/{idTag}/#"; } }
        public string SUB_DEVICE                { get { return $"{REALM}/d/{userName}/#"; } } // All client placeholder
        public string SUB_PROC_REG              { get { return $"{REALM}/proc/reg"; } }
        public string SUB_PROC_CTL              { get { return $"{REALM}/proc/control/{uuid}/#"; } }
        public string SUB_PROC_DBG              { get { return $"{REALM}/proc/debug/{uuid}"; } }
        public string SUB_SCENE_PUBLIC          { get { return $"{REALM}/s/{nameSpace}/{sceneName}/+/+"; } }
        public string SUB_SCENE_PRIVATE         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/+/+/{camName}/#"; } }

        // PUBLISH
        public string PUB_NETWORK_LATENCY       { get { return "$NETWORK/latency"; } }
        public string PUB_CHAT_PUBLIC           { get { return $"{REALM}/c/{nameSpace}/o/{idTag}"; } }
        public string PUB_CHAT_PRIVATE          { get { return $"{REALM}/c/{nameSpace}/p/{toUid}/{idTag}"; } }
        public string PUB_DEVICE                { get { return $"{REALM}/d/{nameSpace}/{sceneName}/{idTag}"; } }
        public string PUB_PROC_REG              { get { return $"{REALM}/proc/reg"; } }
        public string PUB_PROC_CTL              { get { return $"{REALM}/proc/control"; } }
        public string PUB_PROC_DBG              { get { return $"{REALM}/proc/debug/{uuid}"; } }
        public string PUB_SCENE_PRESENCE        { get { return $"{REALM}/s/{nameSpace}/{sceneName}/x/{idTag}"; } }
        public string PUB_SCENE_USER            { get { return $"{REALM}/s/{nameSpace}/{sceneName}/u/{userObj}"; } }
        public string PUB_SCENE_USER_PRIVATE    { get { return $"{REALM}/s/{nameSpace}/{sceneName}/u/{userObj}/{toUid}"; } } // Need to add face_ privs
        public string PUB_SCENE_OBJECTS         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/o/{objectId}"; } } // All client placeholder
        public string PUB_SCENE_OBJECTS_PRIVATE { get { return $"{REALM}/s/{nameSpace}/{sceneName}/o/{objectId}/{toUid}"; } }
        public string PUB_SCENE_RENDER          { get { return $"{REALM}/s/{nameSpace}/{sceneName}/r/{camName}"; } }
        public string PUB_SCENE_RENDER_PRIVATE  { get { return $"{REALM}/s/{nameSpace}/{sceneName}/r/{camName}/-"; } } // To avoid unpriv sub
        public string PUB_SCENE_ENV             { get { return $"{REALM}/s/{nameSpace}/{sceneName}/e/{camName}"; } }
        public string PUB_SCENE_ENV_PRIVATE     { get { return $"{REALM}/s/{nameSpace}/{sceneName}/e/{camName}/-"; } } // To avoid unpriv sub
        public string PUB_SCENE_PROGRAM         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/p/{camName}"; } }
        public string PUB_SCENE_PROGRAM_PRIVATE { get { return $"{REALM}/s/{nameSpace}/{sceneName}/p/{camName}/{toUid}"; } }
        public string PUB_SCENE_DEBUG           { get { return $"{REALM}/s/{nameSpace}/{sceneName}/d/{camName}/-"; } } // To avoid unpriv sub
#pragma warning restore format
    }
}
