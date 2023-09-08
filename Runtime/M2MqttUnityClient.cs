/*
The MIT License (MIT)

Copyright (c) 2018 Giovanni Paolo Vigano'

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

/// <summary>
/// Adaptation for Unity of the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// modified to run on UWP (also tested on Microsoft HoloLens).
/// </summary>
namespace M2MqttUnity
{
    /// <summary>
    /// Generic MonoBehavior wrapping a MQTT client, using a double buffer to postpone message processing in the main thread.
    /// </summary>
    public class M2MqttUnityClient : MonoBehaviour
    {
        [Header("MQTT broker configuration")]
        [Tooltip("IP address or URL of the host running the broker")]
        protected string brokerAddress = null;
        [Tooltip("Port where the broker accepts connections")]
        protected int brokerPort = 1883;
        [Tooltip("Use encrypted connection")]
        protected bool isEncrypted = false;
        [Tooltip("SSL protocol to use when isEncrypted is true.")]
        protected MqttSslProtocols sslProtocol = MqttSslProtocols.TLSv1_0;
        [Tooltip("Wether to verify the encryption certificate")]
        protected bool verifyCertificate = true;
        [Header("Connection parameters")]
        [Tooltip("Connection to the broker is delayed by the the given milliseconds")]
        protected int connectionDelay = 500;
        [Tooltip("Connection timeout in milliseconds")]
        protected int timeoutOnConnection = MqttSettings.MQTT_CONNECT_TIMEOUT;
        [Tooltip("Connect on startup")]
        protected bool autoConnect = false;
        [Tooltip("UserName for the MQTT broker. Keep blank if no user name is required.")]
        protected string mqttUserName = null;
        [Tooltip("Password for the MQTT broker. Keep blank if no password is required.")]
        protected string mqttPassword = null;

        protected bool willFlag = false;
        protected string willTopic = null;
        protected string willMessage = null;

        /// <summary>
        /// Wrapped MQTT client
        /// </summary>
        protected MqttClient client;

        private List<MqttMsgPublishEventArgs> messageQueue1 = new List<MqttMsgPublishEventArgs>();
        private List<MqttMsgPublishEventArgs> messageQueue2 = new List<MqttMsgPublishEventArgs>();
        private List<MqttMsgPublishEventArgs> frontMessageQueue = null;
        private List<MqttMsgPublishEventArgs> backMessageQueue = null;
        private bool mqttClientConnectionClosed = false;
        public bool mqttClientConnected { get; private set; } = false;

        /// <summary>
        /// Event fired when a connection is successfully established
        /// </summary>
        public event Action ConnectionSucceeded;
        /// <summary>
        /// Event fired when failing to connect
        /// </summary>
        public event Action ConnectionFailed;

        /// <summary>
        /// Connect to the broker using current settings.
        /// </summary>
        public virtual void Connect()
        {
            if (client == null || !client.IsConnected)
            {
                StartCoroutine(DoConnect());
            }
        }

        /// <summary>
        /// Disconnect from the broker, if connected.
        /// </summary>
        public virtual void Disconnect()
        {
            if (client != null)
            {
                StartCoroutine(DoDisconnect());
            }
        }

        /// <summary>
        /// Override this method to take some actions before connection (e.g. display a message)
        /// </summary>
        protected virtual void OnConnecting()
        {
            Debug.LogFormat("Connecting to broker on {0}:{1}...\n", brokerAddress, brokerPort.ToString());
        }

        /// <summary>
        /// Override this method to take some actions if the connection succeeded.
        /// </summary>
        protected virtual void OnConnected()
        {
            Debug.LogFormat("Connected to {0}:{1}...\n", brokerAddress, brokerPort.ToString());

            if (ConnectionSucceeded != null)
            {
                ConnectionSucceeded();
            }
        }

        /// <summary>
        /// Override this method to take some actions if the connection failed.
        /// </summary>
        protected virtual void OnConnectionFailed(string errorMessage)
        {
            Debug.LogWarning("Connection failed.");
            if (ConnectionFailed != null)
            {
                ConnectionFailed();
            }
        }

        /// <summary>
        /// Override this method to subscribe to MQTT topics.
        /// </summary>
        protected virtual void SubscribeTopics()
        {
        }

        /// <summary>
        /// Override this method to unsubscribe to MQTT topics (they should be the same you subscribed to with SubscribeTopics() ).
        /// </summary>
        protected virtual void UnsubscribeTopics()
        {
        }

        /// <summary>
        /// Disconnect before the application quits.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            CloseConnection();
        }

        /// <summary>
        /// Initialize MQTT message queue
        /// Remember to call base.Awake() if you override this method.
        /// </summary>
        protected virtual void Awake()
        {
            frontMessageQueue = messageQueue1;
            backMessageQueue = messageQueue2;
        }

        /// <summary>
        /// Connect on startup if autoConnect is set to true.
        /// </summary>
        protected virtual void Start()
        {
            if (autoConnect)
            {
                Connect();
            }
        }

        /// <summary>
        /// Override this method for each received message you need to process.
        /// </summary>
        protected virtual void DecodeMessage(string topic, byte[] message)
        {
            Debug.LogFormat("Message received on topic: {0}", topic);
        }

        /// <summary>
        /// Override this method to take some actions when disconnected.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        /// <summary>
        /// Override this method to take some actions when the connection is closed.
        /// </summary>
        protected virtual void OnConnectionLost()
        {
            Debug.LogWarning("CONNECTION LOST!");
        }

        /// <summary>
        /// Processing of income messages and events is postponed here in the main thread.
        /// Remember to call ProcessMqttEvents() in Update() method if you override it.
        /// </summary>
        protected virtual void Update()
        {
            ProcessMqttEvents();
        }

        protected virtual void ProcessMqttEvents()
        {
            // process messages in the main queue
            SwapMqttMessageQueues();
            ProcessMqttMessageBackgroundQueue();
            // process messages income in the meanwhile
            SwapMqttMessageQueues();
            ProcessMqttMessageBackgroundQueue();

            if (mqttClientConnectionClosed)
            {
                mqttClientConnectionClosed = false;
                OnConnectionLost();
            }
        }

        private void ProcessMqttMessageBackgroundQueue()
        {
            if (backMessageQueue == null) return;
            try
            {
                foreach (MqttMsgPublishEventArgs msg in backMessageQueue)
                {
                    if (msg == null) continue;
                    DecodeMessage(msg.Topic, msg.Message);
                }
                backMessageQueue.Clear();
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }

        /// <summary>
        /// Swap the message queues to continue receiving message when processing a queue.
        /// </summary>
        private void SwapMqttMessageQueues()
        {
            frontMessageQueue = frontMessageQueue == messageQueue1 ? messageQueue2 : messageQueue1;
            backMessageQueue = backMessageQueue == messageQueue1 ? messageQueue2 : messageQueue1;
        }

        private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs msg)
        {
            frontMessageQueue.Add(msg);
        }

        private void OnMqttConnectionClosed(object sender, EventArgs e)
        {
            // Set unexpected connection closed only if connected (avoid event handling in case of controlled disconnection)
            mqttClientConnectionClosed = mqttClientConnected;
            mqttClientConnected = false;
        }

        /// <summary>
        /// Connects to the broker using the current settings.
        /// </summary>
        /// <returns>The execution is done in a coroutine.</returns>
        private IEnumerator DoConnect()
        {
            // wait for the given delay
            yield return new WaitForSecondsRealtime(connectionDelay / 1000f);
            // leave some time to Unity to refresh the UI
            yield return new WaitForEndOfFrame();

            // create client instance
            if (client == null)
            {
                try
                {
#if (!UNITY_EDITOR && UNITY_WSA_10_0 && !ENABLE_IL2CPP)
                    client = new MqttClient(brokerAddress, brokerPort, isEncrypted, isEncrypted ? sslProtocol: MqttSslProtocols.None);
#else
                    if (verifyCertificate)
                    {
                        client = new MqttClient(brokerAddress, brokerPort, isEncrypted, null, null, isEncrypted ? sslProtocol : MqttSslProtocols.None);
                    }
                    else
                    {
                        client = new MqttClient(brokerAddress, brokerPort, isEncrypted, null, null, isEncrypted ? sslProtocol : MqttSslProtocols.None, SelfSignedCertValidationCallback);
                    }
#endif
                }
                catch (Exception e)
                {
                    client = null;
                    Debug.LogErrorFormat("CONNECTION FAILED! {0}", e.ToString());
                    OnConnectionFailed(e.Message);
                    yield break;
                }
            }
            else if (client.IsConnected)
            {
                yield break;
            }
            OnConnecting();

            // leave some time to Unity to refresh the UI
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            client.Settings.TimeoutOnConnection = timeoutOnConnection;
            string clientId = "unity-" + Guid.NewGuid().ToString();
            try
            {
                if (willFlag)
                {
                    client.Connect(clientId, mqttUserName, mqttPassword, false, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, willFlag, willTopic, willMessage, true, MqttMsgConnect.KEEP_ALIVE_PERIOD_DEFAULT);
                }
                else
                {
                    client.Connect(clientId, mqttUserName, mqttPassword);
                }
            }
            catch (Exception e)
            {
                client = null;
                Debug.LogErrorFormat("Failed to connect to {0}:{1}:\n{2}", brokerAddress, brokerPort, e.ToString());
                OnConnectionFailed(e.Message);
                yield break;
            }
            if (client.IsConnected)
            {
                client.ConnectionClosed += OnMqttConnectionClosed;
                // register to message received
                client.MqttMsgPublishReceived += OnMqttMessageReceived;
                mqttClientConnected = true;
                OnConnected();
            }
            else
            {
                OnConnectionFailed("CONNECTION FAILED!");
            }
        }


        private bool SelfSignedCertValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Regex regex = new Regex(@"CN\s*=\s*(?<name>\*?\.?\w*\.?\w+)");
            try
            {
                var match = regex.Match(certificate.Subject);
                string host = match.Groups["name"].Value;
                Debug.LogWarning($"Excepting server certificate without verification for MQTT on {host}!");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Excepting server certificate without verification for MQTT! Read cert host: {ex.Message}");
                Console.WriteLine();
            }
            return true;
        }

        private IEnumerator DoDisconnect()
        {
            yield return new WaitForEndOfFrame();
            CloseConnection();
            OnDisconnected();
        }

        private void CloseConnection()
        {
            mqttClientConnected = false;
            if (client != null)
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }
                client.MqttMsgPublishReceived -= OnMqttMessageReceived;
                client.ConnectionClosed -= OnMqttConnectionClosed;
                client = null;
            }
        }

#if ((!UNITY_EDITOR && UNITY_WSA_10_0))
        private void OnApplicationFocus(bool focus)
        {
            // On UWP 10 (HoloLens) we cannot tell whether the application actually got closed or just minimized.
            // (https://forum.unity.com/threads/onapplicationquit-and-ondestroy-are-not-called-on-uwp-10.462597/)
            if (focus)
            {
                Connect();
            }
            else
            {
                CloseConnection();
            }
        }
#endif
    }
}
