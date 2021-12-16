using ArenaUnity;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaLogger.Example
{
    public class ArenaLoggerUI : MonoBehaviour
    {
        [Header("User Interface")]
        public InputField consoleInputField;
        public Toggle objectLogsToggle;
        public InputField addressInputField;
        public InputField sceneInputField;
        public Button connectButton;
        public Button disconnectButton;
        public Button logoutButton;
        public Button clearButton;

        private bool updateUI = false;

        public void SetBrokerAddress(string brokerAddress)
        {
            if (addressInputField && !updateUI)
            {
                ArenaClient.Instance.brokerAddress = brokerAddress;
            }
        }

        public void SetBrokerPort(string scene)
        {
            if (sceneInputField && !updateUI)
            {
                ArenaClient.Instance.sceneName = scene;
            }
        }

        public void SetObjectLogs(bool isObjectLogs)
        {
            ArenaClient.Instance.logMqttObjects = isObjectLogs;
        }

        public void SetUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text = msg;
                updateUI = true;
            }
        }

        public void AddUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text += msg + "\n";
                updateUI = true;
            }
        }

        private void UpdateUI()
        {
            if (ArenaClient.Instance == null)
            {
                if (connectButton != null)
                {
                    connectButton.interactable = true;
                    disconnectButton.interactable = false;
                    logoutButton.interactable = false;
                }
            }
            else
            {
                if (logoutButton != null)
                {
                    logoutButton.interactable = ArenaClient.Instance.mqttClientConnected;
                }
                if (disconnectButton != null)
                {
                    disconnectButton.interactable = ArenaClient.Instance.mqttClientConnected;
                }
                if (connectButton != null)
                {
                    connectButton.interactable = !ArenaClient.Instance.mqttClientConnected;
                }
            }
            if (addressInputField != null && connectButton != null)
            {
                addressInputField.interactable = connectButton.interactable;
                addressInputField.text = ArenaClient.Instance.brokerAddress;
            }
            if (sceneInputField != null && connectButton != null)
            {
                sceneInputField.interactable = connectButton.interactable;
                sceneInputField.text = ArenaClient.Instance.sceneName;
            }
            if (objectLogsToggle != null && connectButton != null)
            {
                objectLogsToggle.interactable = connectButton.interactable;
                objectLogsToggle.isOn = ArenaClient.Instance.logMqttObjects;
            }
            if (clearButton != null && connectButton != null)
            {
                clearButton.interactable = connectButton.interactable;
            }
            updateUI = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            SetUiMessage("Ready.");
            updateUI = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (updateUI)
            {
                UpdateUI();
            }
        }
    }
}
