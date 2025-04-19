using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhonePlayer : NetworkBehaviour
{
    [SerializeField] private Slider batterySlider;
    [SerializeField] private Image sliderFill;
    [SerializeField] private Image chargingIcon;
    private GameObject computerPlayer = null;
    private GameObject cube;
    private GameObject canvas;
    [SerializeField] private PhoneCameraController phoneCameraController;
    [SerializeField] private PhotosAppController photosAppController;
    [SerializeField] private PhoneMessagesAppController phoneMessageAppController;
    public PhoneCallAppController phoneCallController;
    public PhoneAudioManager phoneAudioManager;
    [SerializeField] private TextMeshProUGUI camNameText;
    // [SerializeField] private GameObject phonePlayerCam;

    [SerializeField] private Button photosAppButton;
    [SerializeField] private Button closePhotosAppButton;
    [SerializeField] private Button backPhotosAppButton;
    [SerializeField] private Button flashlightAppButton;
    [SerializeField] private Button droneControlAppButton;
    [SerializeField] private Button closeDroneControlAppButton;
    [SerializeField] private Button securityCamViewAppButton;
    [SerializeField] private Button closeSecurityCamViewAppButton;
    [SerializeField] private Button phoneCamViewAppButton;
    [SerializeField] private Button closePhoneCamViewAppButton;
    [SerializeField] private Button messagesAppButton;
    [SerializeField] private Button closeMessagesAppButton;

    [SerializeField] private GameObject photosAppGroup;
    
    [SerializeField] private GameObject droneControlAppGroup;
    [SerializeField] private GameObject homescreenAppGroup;
    [SerializeField] private GameObject securityCamViewAppGroup;
    [SerializeField] private GameObject phoneCamViewAppGroup;
    [SerializeField] private GameObject messagesAppGroup;
    [SerializeField] private RawImage securityCamViewImage;
    [SerializeField] private RawImage droneCamViewImage;

    [SerializeField] private Button lvl1Btn;
    List<SecurityCamera> securityCameras = new List<SecurityCamera>();
    int selectedCam = 0;

    private Dictionary<string, Action> rpcCallbacks = new Dictionary<string, Action>();

    public void Start()
    {

        photosAppButton.onClick.AddListener(OnPhotosAppButtonClicked);
        closePhotosAppButton.onClick.AddListener(OnClosePhotosAppButtonClicked);
        droneControlAppButton.onClick.AddListener(OnDroneControlAppButtonClicked);
        closeDroneControlAppButton.onClick.AddListener(OnCloseDroneControlAppButtonClicked);
        securityCamViewAppButton.onClick.AddListener(OnSecurityCamViewAppButtonClicked);
        closeSecurityCamViewAppButton.onClick.AddListener(OnCloseSecurityCamViewAppButtonClicked);
        closePhoneCamViewAppButton.onClick.AddListener(OnClosePhoneCamViewAppButtonClicked);
        phoneCamViewAppButton.onClick.AddListener(OnPhoneCamViewAppButtonClicked);
        flashlightAppButton.onClick.AddListener(OnFlashlightAppButtonClicked);
        messagesAppButton.onClick.AddListener(OnMessagesAppButtonClicked);
        closeMessagesAppButton.onClick.AddListener(OnCloseMessagesAppButtonClicked);

        lvl1Btn.onClick.AddListener(OnPressLevel1Btn);
    }

    // Send new image data to server to add to player state for now
    // This may or may not be saved later. This does not include the actual render texture just the path
    [ServerRpc(RequireOwnership=false, Delivery = RpcDelivery.Reliable)]
    public void AddImageData_ServerRPC(string json)
    {

        Debug.Log("got new photo data: " + json);
        var photo = JsonUtility.FromJson<PhotoJSON>(json);
        var player = computerPlayer.GetComponent<Player>();
        var photos = player.currentPlayerState.Photos;
        if (photos == null)
        {
            photos = new List<PhotoJSON>();
            player.currentPlayerState.Photos = photos;
        }
        player.currentPlayerState.Photos.Add(photo);
    }

    public void SaveNewText(MessageTextJSON message, string contact)
    {
        if (computerPlayer != null)
        {
            var player = computerPlayer.GetComponent<Player>();
            var messageGroup = player.currentPlayerState.MessageGroups.FirstOrDefault(g => g.ContactName == contact);

            if (messageGroup == null)
            {
                messageGroup = new MessageGroupJSON()

                {
                    ContactName = contact,
                    Texts = new List<MessageTextJSON>() { }

                };
                player.currentPlayerState.MessageGroups.Add(messageGroup);
            }

            messageGroup.Texts ??= new List<MessageTextJSON>();
            messageGroup.Texts.Add(message);
        }
    }


    [ClientRpc(Delivery = RpcDelivery.Reliable)]
    public void RestorePlayerState_ClientRPC(string json) // TODO: this will break because of RPC max size later
    {
        Debug.Log("restoring player state: " + json);
        PlayerStateJSON playerState = JsonUtility.FromJson<PlayerStateJSON>(json);
        var restoredPhotos = new List<PhotoTaken>() { };
        foreach (var photo in playerState.Photos)
        {
            var tex2d = PhoneCameraController.LoadTextureFromFile(photo.ImagePath);
            var newPhoto = new PhotoTaken()
            {
                imagePath = photo.ImagePath,
                isLandscape = photo.IsLandscape,
                photoTargets = photo.PhotoTargets,
                imageId = photo.ImageId,
                photo = tex2d,
            };
            restoredPhotos.Add(newPhoto);
        }
        phoneCameraController.SetPhotosTaken(restoredPhotos);


        var restoredMessageGroups = new List<MessageGroup>() {};
        foreach (var messageGroup in playerState.MessageGroups)
        {
            var newMessageGroup = new MessageGroup()
            {
                ContactName = messageGroup.ContactName,
                Texts = new List<Message>()
            };
            
            foreach (var message in messageGroup.Texts)
            {
                var thisPhoto = restoredPhotos.FirstOrDefault(p => p.imageId == message.ImageId);
                Debug.Log("restoring message: " + message.MessageText + " with imageId: " + thisPhoto.imageId + " and image: " + thisPhoto.photo + " path: " + thisPhoto.imagePath);
                var newMessage = new Message()
                {
                    MessageText = message.MessageText == "" ? "Image" : message.MessageText,
                    Image = thisPhoto.photo,
                    IsOutgoing = message.IsOutgoing,
                    IsLandscapeImage = message.IsLandscapeImage,
                };
                newMessageGroup.Texts.Add(newMessage);
                
            }

            restoredMessageGroups.Add(newMessageGroup);
        }

        phoneMessageAppController.SetMessageGroups(restoredMessageGroups);
    }

    public void SendIncomingText(string message, string fromContact)
    {
        if (IsServer)
        {
            phoneMessageAppController.SendIncomingText_ClientRPC(message, fromContact);
        }
    }

    public void OnTextReceived(Action<NetworkTextMessage> handler)
    {
        phoneMessageAppController.OnTextReceived(handler);
    }

    public void OffTextReceived(Action<NetworkTextMessage> handler)
    {
        phoneMessageAppController.OffTextReceived(handler);
    }

    public void CreateIncomingCall(string name, Action onPickup)
    {
        string sequenceId = Guid.NewGuid().ToString();
        rpcCallbacks[sequenceId] = onPickup;

        phoneCallController.ShowCallPopup_ClientRPC(name, sequenceId);
        phoneAudioManager.PlayAudio("incoming-ring", true);
    }

    public void HangupCall()
    {
        Debug.Log("PhonePlayerController: HangupCall");
        phoneCallController.HangupCall_ClientRPC();
    }

    public void PickupIncomingCall(string sequenceId)
    {
        phoneAudioManager.StopAudio("incoming-ring");

        if (rpcCallbacks.TryGetValue(sequenceId, out var callback))
        {
            callback?.Invoke();
            rpcCallbacks.Remove(sequenceId);
        }
    }

    [ServerRpc(RequireOwnership=false, Delivery = RpcDelivery.Reliable)]
    public void ChangeLevel_ServerRPC(string scene, ServerRpcParams rpcParams = default)
    {
        Debug.Log("ChangeLevel_ServerRPC called");
        NetworkManager.Singleton.SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    void OnPhotosAppButtonClicked()
    {
        homescreenAppGroup.SetActive(false);
        photosAppGroup.SetActive(true);
        photosAppController.SetEnabled(true);
    }

    void OnClosePhotosAppButtonClicked()
    {
        photosAppGroup.SetActive(false);
        homescreenAppGroup.SetActive(true);
        photosAppController.SetEnabled(false);
    }

    void OnPressLevel1Btn()
    {
        Debug.Log("Level 1 button pressed");
        ChangeLevel_ServerRPC("GasStation");
    }


    public void OpenMessagesAppFromPopup(string callerId)
    {
        messagesAppGroup.SetActive(true);
        homescreenAppGroup.SetActive(false);

        phoneMessageAppController.OpenMessages(callerId);
    }

    void OnMessagesAppButtonClicked()
    {
        Debug.Log("Messages app button clicked");
        
        // phoneMessageAppController.SetMessageGroups();

        messagesAppGroup.SetActive(true);
        homescreenAppGroup.SetActive(false);
    }

    void OnCloseMessagesAppButtonClicked()
    {
        homescreenAppGroup.SetActive(true);
        messagesAppGroup.SetActive(false);
    }

    void OnFlashlightAppButtonClicked()
    {
        ToggleFlashlight();
    }

    void OnPhoneCamViewAppButtonClicked()
    {
        // phonePlayerCam.SetActive(false);
        homescreenAppGroup.SetActive(false);
        phoneCamViewAppGroup.SetActive(true);
        phoneCameraController.SetEnabled(true);
        // phoneCameraController.ResetGyroOffset();
    }

    void OnCloseDroneControlAppButtonClicked()
    {
        homescreenAppGroup.SetActive(true);
        droneControlAppGroup.SetActive(false);
        SetDroneState(false);
    }

    void OnDroneControlAppButtonClicked()
    {
        homescreenAppGroup.SetActive(false);
        droneControlAppGroup.SetActive(true);
        SetDroneState(true);
    }

    void OnClosePhoneCamViewAppButtonClicked()
    {
        // phonePlayerCam.SetActive(true);
        homescreenAppGroup.SetActive(true);
        phoneCamViewAppGroup.SetActive(false);
        phoneCameraController.SetEnabled(false);
    }

    void OnCloseSecurityCamViewAppButtonClicked()
    {
        // phonePlayerCam.SetActive(true);
        homescreenAppGroup.SetActive(true);
        securityCamViewAppGroup.SetActive(false);
        DisableAllSecurityCams();
    }

    void OnSecurityCamViewAppButtonClicked()
    {
        // phonePlayerCam.SetActive(false);
        homescreenAppGroup.SetActive(false);
        securityCamViewAppGroup.SetActive(true);
        SetSecurityCam();
    }

    void DisableAllSecurityCams()
    {
        for (int i = 0; i < securityCameras.Count; i++)
        {
            securityCameras[i].SetActive(false);
        }
    }

    void SetSecurityCam()
    {
        if (securityCameras.Count == 0)
        {
            camNameText.text = "No cameras available";
            securityCamViewImage.texture = null;
        }
        else
        {
            camNameText.text = securityCameras[selectedCam].GetCamName();
            securityCamViewImage.texture = securityCameras[selectedCam].GetCamTexture();

            for (int i = 0; i < securityCameras.Count; i++)
            {
                securityCameras[i].SetActive(selectedCam == i);
            }
        }

        
    }

    void SetDroneState(bool value)
    {
        var drone = GameObject.FindGameObjectWithTag("Drone").GetComponent<DroneControl>();
        drone.SetDroneCamActive(value);
        if (value == true)
        {
            droneCamViewImage.texture = drone.GetDroneRenderTexture();
        }
    }

    public void OnNextCam()
    {
        selectedCam += 1;
        if (selectedCam >= securityCameras.Count)
        {
            selectedCam = securityCameras.Count - 1;
        }
        SetSecurityCam();
    }

    public void OnPrevCam()
    {
        selectedCam -= 1;
        if (selectedCam < 0)
        {
            selectedCam = 0;
        }
        SetSecurityCam();
    }

    public void ToggleFlashlight()
    {
        if (!IsServer)
        {
            Debug.Log("ToggleFlashlight called on client");
            computerPlayer.GetComponent<Player>().ToggleFlashlight_ServerRPC();
        }
    }

    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            if (!IsServer)
            {
                securityCameras = GameObject.FindGameObjectsWithTag("SecurityCam").ToList().Select(cam => cam.GetComponent<SecurityCamera>()).ToList();
                selectedCam = 0;
                SetSecurityCam();
                Debug.Log("Client PhonePlayer loaded into scene.");
            }
            else
            {
                Debug.Log("Server PhonePlayer loaded into scene.");
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;

        Debug.Log("Network spawn called!");
        cube = transform.Find("Cube").gameObject;
        canvas = GetComponentInChildren<Canvas>().gameObject;
        securityCameras = GameObject.FindGameObjectsWithTag("SecurityCam").ToList().Select(cam => cam.GetComponent<SecurityCamera>()).ToList();

        computerPlayer = GameObject.FindGameObjectWithTag("ComputerPlayer");
        phoneAudioManager = computerPlayer.GetComponent<Player>().phoneAudioManager;
        var playerComponent = computerPlayer.GetComponent<Player>();
        transform.parent = playerComponent.phonePlayerParent.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        base.OnNetworkSpawn();
        if (IsServer)
        {
            canvas.SetActive(false);
            // phonePlayerCam.SetActive(false);
            phoneCameraController.SetEnabled(false);
        } else {
            // phonePlayerCam.SetActive(true);
            phoneCameraController.SetEnabled(false);
            computerPlayer.GetComponent<Player>().RequestClientStateRestore_ServerRPC();
        }
    }

    void Update()
    {
        if (batterySlider)
        {
            var batteryValue = SystemInfo.batteryLevel;
            batterySlider.value = batteryValue;
            var charging = SystemInfo.batteryStatus == BatteryStatus.Charging;
            chargingIcon.gameObject.SetActive(charging);
            if (charging)
            {
                sliderFill.color = Color.blue;
            }
            else
            {
                if (batteryValue > .6)
                {
                    sliderFill.color = Color.green;
                }
                else if (batteryValue > .2)
                {
                    sliderFill.color = Color.yellow;
                }
                else
                {
                    sliderFill.color = Color.red;
                }
            }
        }
        if (!IsServer)
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                ToggleFlashlight();
            }
        }
    }
}
