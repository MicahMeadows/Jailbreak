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
    [SerializeField] private TextMeshProUGUI camNameText;
    [SerializeField] private GameObject phonePlayerCam;
    [SerializeField] private Button flashlightAppButton;
    [SerializeField] private Button droneControlAppButton;
    [SerializeField] private Button closeDroneControlAppButton;
    [SerializeField] private Button homescreenAppButton;
    [SerializeField] private Button camViewAppButton;
    [SerializeField] private Button closeCamViewAppButton;

    [SerializeField] private GameObject droneControlAppGroup;
    [SerializeField] private GameObject flashlightAppGroup;
    [SerializeField] private GameObject homescreenAppGroup;
    [SerializeField] private GameObject camViewAppGroup;
    List<SecurityCamera> securityCameras = new List<SecurityCamera>();
    int selectedCam = 0;

    public void Start()
    {
        droneControlAppButton.onClick.AddListener(OnDroneControlAppButtonClicked);
        closeDroneControlAppButton.onClick.AddListener(OnCloseDroneControlAppButtonClicked);
        camViewAppButton.onClick.AddListener(OnCamViewAppButtonClicked);
        closeCamViewAppButton.onClick.AddListener(OnCloseCamViewAppButtonClicked);
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

    void OnCloseCamViewAppButtonClicked()
    {
        phonePlayerCam.SetActive(true);
        homescreenAppGroup.SetActive(true);
        camViewAppGroup.SetActive(false);
        DisableAllSecurityCams();
    }

    void OnCamViewAppButtonClicked()
    {
        phonePlayerCam.SetActive(false);
        homescreenAppGroup.SetActive(false);
        camViewAppGroup.SetActive(true);
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
        base.OnNetworkSpawn();
        if (IsServer)
        {
            phonePlayerCam.SetActive(false);
            canvas.SetActive(false);
        } else {
            phonePlayerCam.SetActive(true);
        }    
    }

    void Update()
    {
        if (computerPlayer == null) {
            computerPlayer = GameObject.FindGameObjectWithTag("ComputerPlayer");
        } else {
            transform.parent = computerPlayer.transform;
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
