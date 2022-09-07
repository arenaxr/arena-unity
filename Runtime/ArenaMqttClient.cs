/**
* Open source software under the terms in /LICENSE
* Copyright (c) 2021, The CONIX Research Center. All rights reserved.
*/

using System;
using System.Collections.Generic;
using M2MqttUnity;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ArenaUnity
{
    public class ArenaMqttClient : M2MqttUnityClient
    {
        [Header("ARENA MQTT configuration")]
        [Tooltip("IP address or URL of the host running broker/auth/persist services.")]
        public string hostAddress = "mqtt.arenaxr.org";

        public enum Auth { Anonymous, Google };
        public bool IsShuttingDown { get; internal set; }
        private List<byte[]> eventMessages = new List<byte[]>();

        protected override void Awake()
        {
            base.Awake();
            // initialize arena-specific parameters
            brokerAddress = hostAddress;
            brokerPort = 8883;
            isEncrypted = true;
            sslProtocol = MqttSslProtocols.TLSv1_2;
        }

        protected override void Update()
        {
            base.Update(); // call ProcessMqttEvents()

            if (eventMessages.Count > 0)
            {
                foreach (byte[] msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
        }

        protected virtual void ProcessMessage(byte[] msg)
        {
            Debug.LogFormat("Message received of length: {0}", msg.Length);
        }

        private void StoreMessage(byte[] eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            StoreMessage(message);
        }

        public void Publish(string topic, byte[] payload)
        {
            client.Publish(topic, payload);
        }

        public void Subscribe(string[] topics)
        {
            client.Subscribe(topics, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        }

        public void Unsubscribe(string[] topics)
        {
            client.Unsubscribe(topics);
        }

        protected void OnDestroy()
        {
            Disconnect();
        }

        protected new void OnApplicationQuit()
        {
            IsShuttingDown = true;
            base.OnApplicationQuit();
        }
    }
}
