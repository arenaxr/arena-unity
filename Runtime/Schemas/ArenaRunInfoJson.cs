/**
 * Open source software under the terms in /LICENSE
 * Copyright (c) 2021-2024, Carnegie Mellon University. All rights reserved.
 */

// CAUTION: This file is autogenerated from https://github.com/arenaxr/arena-schemas. Changes made here may be overwritten.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ArenaUnity.Schemas
{
    /// <summary>
    /// Program execution info, added at runtime.
    /// </summary>
    [Serializable]
    public class ArenaRunInfoJson
    {
        [JsonIgnore]
        public readonly string componentName = "run_info";

        // run_info member-fields

        private static string defWebHost = null;
        [JsonProperty(PropertyName = "web_host")]
        [Tooltip("ARENA web host.")]
        public string WebHost = defWebHost;
        public bool ShouldSerializeWebHost()
        {
            // web_host
            return (WebHost != defWebHost);
        }

        private static string defNamespace = null;
        [JsonProperty(PropertyName = "namespace")]
        [Tooltip("ARENA namespace.")]
        public string Namespace = defNamespace;
        public bool ShouldSerializeNamespace()
        {
            // namespace
            return (Namespace != defNamespace);
        }

        private static string defScene = null;
        [JsonProperty(PropertyName = "scene")]
        [Tooltip("ARENA scene.")]
        public string Scene = defScene;
        public bool ShouldSerializeScene()
        {
            // scene
            return (Scene != defScene);
        }

        private static string defRealm = null;
        [JsonProperty(PropertyName = "realm")]
        [Tooltip("ARENA realm.")]
        public string Realm = defRealm;
        public bool ShouldSerializeRealm()
        {
            // realm
            return (Realm != defRealm);
        }

        private static string defFilename = null;
        [JsonProperty(PropertyName = "filename")]
        [Tooltip("Executable filename.")]
        public string Filename = defFilename;
        public bool ShouldSerializeFilename()
        {
            // filename
            return (Filename != defFilename);
        }

        private static string defArgs = null;
        [JsonProperty(PropertyName = "args")]
        [Tooltip("Command line arguments.")]
        public string Args = defArgs;
        public bool ShouldSerializeArgs()
        {
            // args
            return (Args != defArgs);
        }

        private static string defEnv = null;
        [JsonProperty(PropertyName = "env")]
        [Tooltip("Value of ARENA-py environment variables.")]
        public string Env = defEnv;
        public bool ShouldSerializeEnv()
        {
            // env
            return (Env != defEnv);
        }

        private static string defCreateTime = null;
        [JsonProperty(PropertyName = "create_time")]
        [Tooltip("Program creation time.")]
        public string CreateTime = defCreateTime;
        public bool ShouldSerializeCreateTime()
        {
            // create_time
            return (CreateTime != defCreateTime);
        }

        private static string defLastActiveTime = null;
        [JsonProperty(PropertyName = "last_active_time")]
        [Tooltip("Program last publish/receive time.")]
        public string LastActiveTime = defLastActiveTime;
        public bool ShouldSerializeLastActiveTime()
        {
            // last_active_time
            return (LastActiveTime != defLastActiveTime);
        }

        private static string defLastPubTime = null;
        [JsonProperty(PropertyName = "last_pub_time")]
        [Tooltip("Program last publish time.")]
        public string LastPubTime = defLastPubTime;
        public bool ShouldSerializeLastPubTime()
        {
            // last_pub_time
            return (LastPubTime != defLastPubTime);
        }

        private static int? defPubMsgs = null;
        [JsonProperty(PropertyName = "pub_msgs")]
        [Tooltip("Number of published messages.")]
        public int? PubMsgs = defPubMsgs;
        public bool ShouldSerializePubMsgs()
        {
            // pub_msgs
            return (PubMsgs != defPubMsgs);
        }

        private static float? defPubMsgsPerSec = null;
        [JsonProperty(PropertyName = "pub_msgs_per_sec")]
        [Tooltip("Published messages per second avg.")]
        public float? PubMsgsPerSec = defPubMsgsPerSec;
        public bool ShouldSerializePubMsgsPerSec()
        {
            // pub_msgs_per_sec
            return (PubMsgsPerSec != defPubMsgsPerSec);
        }

        private static int? defRcvMsgs = null;
        [JsonProperty(PropertyName = "rcv_msgs")]
        [Tooltip("Number of received messages.")]
        public int? RcvMsgs = defRcvMsgs;
        public bool ShouldSerializeRcvMsgs()
        {
            // rcv_msgs
            return (RcvMsgs != defRcvMsgs);
        }

        private static float? defRcvMsgsPerSec = null;
        [JsonProperty(PropertyName = "rcv_msgs_per_sec")]
        [Tooltip("Received messages per second avg.")]
        public float? RcvMsgsPerSec = defRcvMsgsPerSec;
        public bool ShouldSerializeRcvMsgsPerSec()
        {
            // rcv_msgs_per_sec
            return (RcvMsgsPerSec != defRcvMsgsPerSec);
        }

        private static string defLastRcvTime = null;
        [JsonProperty(PropertyName = "last_rcv_time")]
        [Tooltip("Last message receive time.")]
        public string LastRcvTime = defLastRcvTime;
        public bool ShouldSerializeLastRcvTime()
        {
            // last_rcv_time
            return (LastRcvTime != defLastRcvTime);
        }

        // General json object management
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Debug.LogWarning($"{errorContext.Error.Message}: {errorContext.OriginalObject}");
            errorContext.Handled = true;
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;
    }
}
