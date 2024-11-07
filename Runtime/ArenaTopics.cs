namespace ArenaUnity
{
    public enum ArenaTopicTokens
    {
        REALM = 0,
        TYPE = 1,
        NAMESPACE = 2,
        SCENENAME = 3,
        SCENE_MSGTYPE = 4,
        USER_CLIENT = 5,
        UUID = 6,
        TO_UID = 7,
    };

    public static class ArenaTopicSceneMsgTypes
    {
        public const string
            PRESENCE = "x",
            CHAT = "c",
            USER = "u",
            OBJECTS = "o",
            RENDER = "r",
            ENV = "e",
            PROGRAM = "p",
            DEBUG = "d";
    }


    public readonly struct ArenaTopics
    {
        /**
        * ARENA pubsub topic variables
        * - nameSpace - namespace of the scene
        * - sceneName - name of the scene
        * - userClient - name of the user client per arena-auth (e.g. jdoe_1448081341_web)
        * - idTag - username prefixed with a uuid (e.g. jdoe_1448081341)
        * - userObj - idTag prefixed with camera_ (e.g. camera_jdoe_1448081341)
        */
        public ArenaTopics(
            string realm = "",
            string name_space = "",
            string scenename = "",
            string userclient = "",
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
            userClient = userclient;
            uuid = uuId;
            userObj = userobj;
            objectId = objectid;
            toUid = touid;
            deviceName = devicename;
        }
        public readonly string REALM { get; }
        public readonly string nameSpace { get; }
        public readonly string sceneName { get; }
        public readonly string userClient { get; }
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
        public string SUB_DEVICE                { get { return $"{REALM}/d/{nameSpace}/{deviceName}/#"; } } // All client placeholder
        public string SUB_PROC_REG              { get { return $"{REALM}/proc/reg"; } }
        public string SUB_PROC_CTL              { get { return $"{REALM}/proc/control/{uuid}/#"; } }
        public string SUB_PROC_DBG              { get { return $"{REALM}/proc/debug/{uuid}"; } }
        public string SUB_SCENE_PUBLIC          { get { return $"{REALM}/s/{nameSpace}/{sceneName}/+/+"; } }
        public string SUB_SCENE_PRIVATE         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/+/+/{idTag}/#"; } }
        public string SUB_SCENE_RENDER_PRIVATE  { get { return $"{REALM}/s/{nameSpace}/{sceneName}/r/+/{idTag}/#"; } }

        // PUBLISH
        public string PUB_NETWORK_LATENCY       { get { return "$NETWORK/latency"; } }
        public string PUB_DEVICE                { get { return $"{REALM}/d/{nameSpace}/{deviceName}/{idTag}"; } }
        public string PUB_PROC_REG              { get { return $"{REALM}/proc/reg"; } }
        public string PUB_PROC_CTL              { get { return $"{REALM}/proc/control"; } }
        public string PUB_PROC_DBG              { get { return $"{REALM}/proc/debug/{uuid}"; } }
        public string PUB_SCENE_PRESENCE        { get { return $"{REALM}/s/{nameSpace}/{sceneName}/x/{userClient}/{idTag}"; } }
        public string PUB_SCENE_PRESENCE_PRIVATE{ get { return $"{REALM}/s/{nameSpace}/{sceneName}/x/{userClient}/{idTag}/{toUid}"; } }
        public string PUB_SCENE_CHAT            { get { return $"{REALM}/s/{nameSpace}/{sceneName}/c/{userClient}/{idTag}"; } }
        public string PUB_SCENE_CHAT_PRIVATE    { get { return $"{REALM}/s/{nameSpace}/{sceneName}/c/{userClient}/{idTag}/{toUid}"; } }
        public string PUB_SCENE_USER            { get { return $"{REALM}/s/{nameSpace}/{sceneName}/u/{userClient}/{userObj}"; } }
        public string PUB_SCENE_USER_PRIVATE    { get { return $"{REALM}/s/{nameSpace}/{sceneName}/u/{userClient}/{userObj}/{toUid}"; } } // Need to add face_ privs
        public string PUB_SCENE_OBJECTS         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/o/{userClient}/{objectId}"; } } // All client placeholder
        public string PUB_SCENE_OBJECTS_PRIVATE { get { return $"{REALM}/s/{nameSpace}/{sceneName}/o/{userClient}/{objectId}/{toUid}"; } }
        public string PUB_SCENE_RENDER          { get { return $"{REALM}/s/{nameSpace}/{sceneName}/r/{userClient}/{idTag}"; } }
        public string PUB_SCENE_RENDER_PRIVATE  { get { return $"{REALM}/s/{nameSpace}/{sceneName}/r/{userClient}/{idTag}/-"; } } // To avoid unpriv sub
        public string PUB_SCENE_RENDER_PRI_SERV { get { return $"{REALM}/s/{nameSpace}/{sceneName}/r/{userClient}/-/{toUid}"; } }
        public string PUB_SCENE_ENV             { get { return $"{REALM}/s/{nameSpace}/{sceneName}/e/{userClient}/{idTag}"; } }
        public string PUB_SCENE_ENV_PRIVATE     { get { return $"{REALM}/s/{nameSpace}/{sceneName}/e/{userClient}/{idTag}/-"; } } // To avoid unpriv sub
        public string PUB_SCENE_PROGRAM         { get { return $"{REALM}/s/{nameSpace}/{sceneName}/p/{userClient}/{idTag}"; } }
        public string PUB_SCENE_PROGRAM_PRIVATE { get { return $"{REALM}/s/{nameSpace}/{sceneName}/p/{userClient}/{idTag}/{toUid}"; } }
        public string PUB_SCENE_DEBUG           { get { return $"{REALM}/s/{nameSpace}/{sceneName}/d/{userClient}/{idTag}/-"; } } // To avoid unpriv sub
#pragma warning restore format
    }
}
