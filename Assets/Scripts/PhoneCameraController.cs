using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public struct PhotoTaken {
    public string imageId;
    public string imagePath;
    public Texture2D photo;
    public List<string> photoTargets;
    public bool isLandscape;
}

public class PhoneCameraController : MonoBehaviour
{
    [SerializeField] private Button takePhotoButton;
    [SerializeField] private RawImage uiRenderImage;
    [SerializeField] private RawImage photoPreview;
    [SerializeField] private GameObject phoneCamera;
    [SerializeField] private PhonePlayer phonePlayer;
    private List<PhotoTaken> photosTaken = new List<PhotoTaken>();
    public float rotationSpeed = 0.2f;
    public float gyroSmoothSpeed = 50f;

    private Vector3 lastPanPosition;
    private bool isPanning;
    private bool isActive = false;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float minVerticalAngle = -60f;
    private float maxVerticalAngle = 60f;

    private bool useGyro = false;
    private Quaternion gyroOffset = Quaternion.identity;
    private RenderTexture renderTexture;

    private float touchStartTime = -1f;
    private bool gyroResetTriggered = false;
    private const float gyroHoldDuration = 1f;
    [SerializeField] private float defaultFOV = 80f;
    [SerializeField] private float minFOV = 30f;
    [SerializeField] private float maxFOV = 130f;
    [SerializeField] private float zoomSpeed = 0.1f;
    private float lastTouchDistance = 0f;
    [SerializeField] private Button resetZoomBtn;
    [SerializeField] private TextMeshProUGUI zoomText;

    void Start()
    {
        Debug.Log("new message");
        takePhotoButton.onClick.AddListener(TakePhoto);
        Debug.Log("Supports Gyro: " + SystemInfo.supportsGyroscope);
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            useGyro = true;
        }
        resetZoomBtn.onClick.AddListener(() =>
        {
            SetZoom(defaultFOV);
        });
    }

    private float FOVToZoom(float fov)
    {
        if (fov > defaultFOV) // Between 130 and 80 — zoomed out
        {
            float t = Mathf.InverseLerp(maxFOV, defaultFOV, fov); // 130 → 80
            return Mathf.Lerp(0.5f, 1f, t);
        }
        else // Between 80 and 30 — zoomed in
        {
            float t = Mathf.InverseLerp(defaultFOV, minFOV, fov); // 80 → 30
            return Mathf.Lerp(1f, 2f, t);
        }
    }

    private float ZoomToFOV(float zoom)
    {
        if (zoom < 1f) // Between 0.5x and 1x — zooming out
        {
            float t = Mathf.InverseLerp(0.5f, 1f, zoom); // 0.5 → 1
            return Mathf.Lerp(maxFOV, defaultFOV, t);    // 130 → 80
        }
        else // Between 1x and 2x — zooming in
        {
            float t = Mathf.InverseLerp(1f, 2f, zoom); // 1 → 2
            return Mathf.Lerp(defaultFOV, minFOV, t);  // 80 → 30
        }
    }





    private string SaveTextureToFile(Texture2D texture, string uid)
    {
        byte[] bytes = texture.EncodeToPNG(); // you could use EncodeToJPG for smaller size
        string fileName = $"photo_{uid}.png";
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log($"📸 Saved photo to: {filePath}");
        return filePath;
    }

    public static Texture2D LoadTextureFromFile(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return tex;
    }



    public void SetZoom(float zoomLevel)
    {
        Camera cam = phoneCamera.GetComponent<Camera>();
        var newFov = Mathf.Clamp(zoomLevel, minFOV, maxFOV);
        cam.fieldOfView = newFov;
        var zoom = FOVToZoom(cam.fieldOfView);
        zoomText.text = $"{zoom:0.0}x";
    }


    private bool IsDeviceSideways()
    {
        Vector3 gravity = Input.gyro.gravity;
        return Mathf.Abs(gravity.x) > Mathf.Abs(gravity.y);
    }

    public List<PhotoTaken> GetPhotos()
    {
        return photosTaken;
    }

    public void SetPhotosTaken(List<PhotoTaken> photos)
    {
        photosTaken = photos;
    }

    List<PhotoTarget> DetectVisibleObjects(Camera cam)
    {
        var visibleTargets = new List<PhotoTarget>();
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        var allTargets = FindObjectsByType<PhotoTarget>(FindObjectsSortMode.None);
        Vector3 rayOrigin = cam.transform.position;

        // Create a mask that excludes the "PlayerHidden" layer
        int playerHiddenLayer = LayerMask.NameToLayer("PlayerHidden");
        int layerMask = ~(1 << playerHiddenLayer); // Invert to ignore it

        foreach (var target in allTargets)
        {
            var colliderTransform = target.GetTargetColliderTransform();
            if (colliderTransform == null) continue;

            if (!target.TryGetComponent<Collider>(out var col)) continue;

            if (!GeometryUtility.TestPlanesAABB(planes, col.bounds)) continue;

            Vector3 rayDirection = (colliderTransform.position - rayOrigin).normalized;
            float distance = Vector3.Distance(rayOrigin, colliderTransform.position);

            // 👇 Skip if beyond max view distance
            if (distance > target.GetMaxDistance()) continue;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, distance, layerMask))
            {
                if (hit.transform == colliderTransform || hit.transform.IsChildOf(colliderTransform))
                {
                    visibleTargets.Add(target);
                    Debug.Log($"✅ Visible Target: {target.GetName()}");
                }
            }
        }

        return visibleTargets;
    }

    public static Texture2D RotateTexture90CounterClockwise(Texture2D original)
    {
        int width = original.width;
        int height = original.height;

        Texture2D rotated = new Texture2D(height, width, original.format, false);
        Color[] originalPixels = original.GetPixels();
        Color[] rotatedPixels = new Color[originalPixels.Length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                rotatedPixels[(height - 1 - y) + x * height] = originalPixels[x + y * width];
            }
        }

        rotated.SetPixels(rotatedPixels);
        rotated.Apply();
        return rotated;
    }

    public void TakePhoto()
    {
        bool isLandscape = IsDeviceSideways();

        var camObjects = DetectVisibleObjects(phoneCamera.GetComponent<Camera>());

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D photo = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        photo.Apply();

        RenderTexture.active = currentRT;

        photoPreview.texture = photo;
        photoPreview.enabled = true;

        if (isLandscape)
        {
            photo = RotateTexture90CounterClockwise(photo);
        }

        var newUid = Guid.NewGuid().ToString();
        string savedPath = SaveTextureToFile(photo, newUid);

        var newPhotoTaken = new PhotoTaken
        {
            imageId = newUid,
            photo = photo,
            imagePath = savedPath,
            isLandscape = isLandscape,
            photoTargets = camObjects.Select(x => x.GetName()).ToList(),
        };

        photosTaken.Add(newPhotoTaken);

        var newPhotoJson = new PhotoJSON()
        {
            ImageId = newUid,
            ImagePath = savedPath,
            IsLandscape = isLandscape,
            PhotoTargets = camObjects.Select(x => x.GetName()).ToList(),
        };

        phonePlayer.AddImageData_ServerRPC(JsonConvert.SerializeObject(newPhotoJson));
    }



    public void SetEnabled(bool value)
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            useGyro = true;
        }

        photoPreview.enabled = false;
        isActive = value;
        phoneCamera.GetComponent<Camera>().enabled = isActive;

        if (isActive)
        {
            Camera cam = phoneCamera.GetComponent<Camera>();

            SetZoom(defaultFOV);

            renderTexture = new RenderTexture(1000, 1600, 16);
            renderTexture.Create();

            cam.targetTexture = renderTexture;
            uiRenderImage.texture = renderTexture;

            ResetGyroOffset();

            Quaternion deviceRotation = Input.gyro.attitude;
            deviceRotation = Quaternion.Euler(90f, 0f, 0f) *
                            new Quaternion(-deviceRotation.x, -deviceRotation.y, deviceRotation.z, deviceRotation.w);

            Quaternion targetRotation = gyroOffset * deviceRotation;
            phoneCamera.transform.localRotation = targetRotation;
        }

        else 
        {
            xRotation = 0f;
            yRotation = 0f;
            phoneCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }

    public void ResetGyroOffset()
    {
        if (!useGyro) return;

        Quaternion rawGyro = Input.gyro.attitude;
        rawGyro = Quaternion.Euler(90f, 0f, 0f) * new Quaternion(-rawGyro.x, -rawGyro.y, rawGyro.z, rawGyro.w);

        Vector3 euler = rawGyro.eulerAngles;
        float yaw = euler.y;

        Quaternion yawOnlyRotation = Quaternion.Euler(0f, yaw, 0f);
        gyroOffset = Quaternion.Inverse(yawOnlyRotation);
        Debug.Log("Gyro offset was reset!");
    }

    void Update()
    {

        if (!isActive) return;

        if (useGyro)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartTime = Time.time;
                    gyroResetTriggered = false;
                }
                else if (touch.phase == TouchPhase.Stationary)
                {
                    if (!gyroResetTriggered && Time.time - touchStartTime >= gyroHoldDuration)
                    {
                        ResetGyroOffset();
                        gyroResetTriggered = true;
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    touchStartTime = -1f;
                    gyroResetTriggered = false;
                }
            }
            else
            {
                touchStartTime = -1f;
                gyroResetTriggered = false;
            } 

            if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

                float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
                float touchDeltaMag = (touch0.position - touch1.position).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                Camera cam = phoneCamera.GetComponent<Camera>();
                float newFOV = cam.fieldOfView + deltaMagnitudeDiff * zoomSpeed;
                SetZoom(newFOV);
            }


            Quaternion deviceRotation = Input.gyro.attitude;
            deviceRotation = Quaternion.Euler(90f, 0f, 0f) *
                             new Quaternion(-deviceRotation.x, -deviceRotation.y, deviceRotation.z, deviceRotation.w);

            Quaternion targetRotation = gyroOffset * deviceRotation;

            phoneCamera.transform.localRotation = Quaternion.Slerp(
                phoneCamera.transform.localRotation,
                targetRotation,
                Time.deltaTime * gyroSmoothSpeed
            );
        }
        else
        {
            Vector2 delta = Vector2.zero;

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                    lastPanPosition = touch.position;
                else if (touch.phase == TouchPhase.Moved)
                {
                    delta = touch.position - (Vector2)lastPanPosition;
                    lastPanPosition = touch.position;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                isPanning = true;
                lastPanPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0) && isPanning)
            {
                delta = (Vector2)(Input.mousePosition - lastPanPosition);
                lastPanPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isPanning = false;
            }

            if (delta != Vector2.zero)
            {
                yRotation += -delta.x * rotationSpeed;
                xRotation += delta.y * rotationSpeed;
                xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

                phoneCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
            }
        }
    }
}
