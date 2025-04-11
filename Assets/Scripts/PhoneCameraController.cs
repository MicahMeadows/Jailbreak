using UnityEngine;
using UnityEngine.UI;

public class PhoneCameraController : MonoBehaviour
{
    [SerializeField] private Button takePhotoButton;
    [SerializeField] private RawImage uiRenderImage;
    [SerializeField] private RawImage photoPreview;
    [SerializeField] private GameObject phoneCamera;
    public float rotationSpeed = 0.2f;
    public float gyroSmoothSpeed = 50f; // new: smoothing factor for gyro

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

    void Start()
    {
        
        takePhotoButton.onClick.AddListener(TakePhoto);
    }


    private bool IsDeviceSideways()
    {
        Vector3 gravity = Input.gyro.gravity;

        // Check if the device is tilted ~90 degrees on its side
        // If |x| > |y| then the device is likely in landscape orientation
        return Mathf.Abs(gravity.x) > Mathf.Abs(gravity.y);
    }

    public void TakePhoto()
    {
        bool isLandscape = IsDeviceSideways();
        Debug.Log("took photo in landscape?: " + isLandscape);

        // Save the current active RenderTexture
        RenderTexture currentRT = RenderTexture.active;

        // Set the camera's render texture as the active one
        RenderTexture.active = renderTexture;

        // Create a new Texture2D to hold the screenshot
        Texture2D photo = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        // Read pixels from the active (now renderTexture) buffer
        photo.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        photo.Apply();

        // Restore the previously active RenderTexture
        RenderTexture.active = currentRT;

        // Display the photo in a UI element
        photoPreview.texture = photo;
        photoPreview.enabled = true;
    }


    public void SetEnabled(bool value)
    {
        photoPreview.enabled = false;
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            useGyro = true;
        }
        isActive = value;
        phoneCamera.GetComponent<Camera>().enabled = isActive;

        if (isActive)
        {
            Camera cam = phoneCamera.GetComponent<Camera>();
            renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
            renderTexture.Create();
            cam.targetTexture = renderTexture;
            uiRenderImage.texture = renderTexture;
        }

        if (!isActive)
        {
            xRotation = 0f;
            yRotation = 0f;
            phoneCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
        else if (useGyro)
        {
            // Reset the offset and immediately set camera rotation
            ResetGyroOffset();

            Quaternion deviceRotation = Input.gyro.attitude;
            deviceRotation = Quaternion.Euler(90f, 0f, 0f) *
                            new Quaternion(-deviceRotation.x, -deviceRotation.y, deviceRotation.z, deviceRotation.w);

            Quaternion targetRotation = gyroOffset * deviceRotation;
            phoneCamera.transform.localRotation = targetRotation;
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
    }

    void Update()
    {
        
        if (!isActive) return;

        if (useGyro)
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ResetGyroOffset();
            }

            Quaternion deviceRotation = Input.gyro.attitude;
            deviceRotation = Quaternion.Euler(90f, 0f, 0f) * new Quaternion(-deviceRotation.x, -deviceRotation.y, deviceRotation.z, deviceRotation.w);
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
