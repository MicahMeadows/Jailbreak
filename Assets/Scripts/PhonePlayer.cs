using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PhonePlayer : NetworkBehaviour
{
    private GameObject computerPlayer = null;
    private GameObject cube;
    private GameObject canvas;
    [SerializeField] private PhoneCameraController phoneCameraController;
    [SerializeField] private TextMeshProUGUI camNameText;
    [SerializeField] private GameObject phonePlayerCam;
    [SerializeField] private Button flashlightAppButton;
    [SerializeField] private Button droneControlAppButton;
    [SerializeField] private Button closeDroneControlAppButton;
    [SerializeField] private Button phoneCamViewAppButton;
    [SerializeField] private Button securityCamViewAppButton;
    [SerializeField] private Button closeSecurityCamViewAppButton;
    [SerializeField] private Button closePhoneCamViewAppButton;

    [SerializeField] private GameObject droneControlAppGroup;
    [SerializeField] private GameObject homescreenAppGroup;
    [SerializeField] private GameObject securityCamViewAppGroup;
    [SerializeField] private GameObject phoneCamViewAppGroup;
    List<SecurityCamera> securityCameras = new List<SecurityCamera>();
    int selectedCam = 0;

    public void Start()
    {
        droneControlAppButton.onClick.AddListener(OnDroneControlAppButtonClicked);
        closeDroneControlAppButton.onClick.AddListener(OnCloseDroneControlAppButtonClicked);
        securityCamViewAppButton.onClick.AddListener(OnCamViewAppButtonClicked);
        closeSecurityCamViewAppButton.onClick.AddListener(OnCloseSecurityCamViewAppButtonClicked);
        closePhoneCamViewAppButton.onClick.AddListener(OnClosePhoneCamViewAppButtonClicked);
        phoneCamViewAppButton.onClick.AddListener(OnPhoneCamViewAppButtonClicked);
        flashlightAppButton.onClick.AddListener(OnFlashlightAppButtonClicked);
    }

    void OnFlashlightAppButtonClicked()
    {
        ToggleFlashlight();
    }

    void OnPhoneCamViewAppButtonClicked()
    {
        phonePlayerCam.SetActive(false);
        homescreenAppGroup.SetActive(false);
        phoneCamViewAppGroup.SetActive(true);
        phoneCameraController.SetEnabled(true);
    }

    void OnCloseDroneControlAppButtonClicked()
    {
        var drone = GameObject.FindGameObjectWithTag("Drone").GetComponent<DroneControl>();
        phonePlayerCam.SetActive(true);
        drone.SetDroneCamActive(false);
        homescreenAppGroup.SetActive(true);
        droneControlAppGroup.SetActive(false);
    }

    void OnDroneControlAppButtonClicked()
    {
        homescreenAppGroup.SetActive(false);
        droneControlAppGroup.SetActive(true);
        var drone = GameObject.FindGameObjectWithTag("Drone").GetComponent<DroneControl>();
        phonePlayerCam.SetActive(false);
        drone.SetDroneCamActive(true);
    }

    void OnClosePhoneCamViewAppButtonClicked()
    {
        phonePlayerCam.SetActive(true);
        homescreenAppGroup.SetActive(true);
        phoneCamViewAppGroup.SetActive(false);
        phoneCameraController.SetEnabled(false);
    }

    void OnCloseSecurityCamViewAppButtonClicked()
    {
        phonePlayerCam.SetActive(true);
        homescreenAppGroup.SetActive(true);
        securityCamViewAppGroup.SetActive(false);
        DisableAllSecurityCams();
    }

    void OnCamViewAppButtonClicked()
    {
        phonePlayerCam.SetActive(false);
        homescreenAppGroup.SetActive(false);
        securityCamViewAppGroup.SetActive(true);
        SetSecurityCam();
    }

    void DisableAllSecurityCams()
    {
        for (int i = 0; i < securityCameras.Count; i++)
        {
            securityCameras[i].EnableCamera(false);
        }
    }

    void SetSecurityCam()
    {
        phonePlayerCam.SetActive(false);
        camNameText.text = securityCameras[selectedCam].GetCamName();
        for (int i = 0; i < securityCameras.Count; i++)
        {
            securityCameras[i].EnableCamera(i == selectedCam);
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
            computerPlayer.GetComponent<Player>().ToggleFlashlight_ServerRPC();
        }
    }

    public override void OnNetworkSpawn()
    {
        cube = transform.Find("Cube").gameObject;
        canvas = GetComponentInChildren<Canvas>().gameObject;
        securityCameras = GameObject.FindGameObjectsWithTag("SecurityCam").ToList().Select(cam => cam.GetComponent<SecurityCamera>()).ToList();

        computerPlayer = GameObject.FindGameObjectWithTag("ComputerPlayer");
        var playerComponent = computerPlayer.GetComponent<Player>();
        transform.parent = playerComponent.phonePlayerParent.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        base.OnNetworkSpawn();
        if (IsServer)
        {
            canvas.SetActive(false);
            phonePlayerCam.SetActive(false);
            phoneCameraController.SetEnabled(false);
        } else {
            phonePlayerCam.SetActive(true);
            phoneCameraController.SetEnabled(false);
        }
    }

    void Update()
    {
        if (!IsServer)
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                ToggleFlashlight();
            }
        }
    }
}
