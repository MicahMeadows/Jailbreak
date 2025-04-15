using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PhotosAppController : MonoBehaviour
{
    // [SerializeField] private PhonePlayer phonePlayer;
    [SerializeField] PhoneMessagesAppController phoneMessagesAppController;
    [SerializeField] private PhoneCameraController phoneCameraController;
    [SerializeField] private RawImage imagePreview;
    [SerializeField] private RawImage landscapeImagePreview;
    [SerializeField] private GameObject thumbnailParent;
    [SerializeField] private GameObject thumnbnailPrefab;
    [SerializeField] private GameObject galleryGroup;
    [SerializeField] private GameObject previewGroup;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject sendButton;
    [SerializeField] private GameObject photosAppGroup;
    private List<PhotoTaken> photos;
    private string contactToText = null;
    private PhotoTaken? activePreview = null;

    void Start()
    {
        sendButton.GetComponent<Button>().onClick.AddListener(() => {
            OnTextButtonClicked();
            galleryGroup.SetActive(true);
            previewGroup.SetActive(false);
            photosAppGroup.SetActive(false);
        });

        sendButton.SetActive(false);

        backButton.onClick.AddListener(() =>
        {
            galleryGroup.SetActive(true);
            previewGroup.SetActive(false);
        });
    }

    private void OnTextButtonClicked()
    {
        if (activePreview != null)
        {
            // phonePlayer.SendTextImage(activePreview.Value, contactToText);
            phoneMessagesAppController.SendTextImage(activePreview.Value, contactToText);
        }
    }

    public void SetEnabled(bool value, string contactToText = null)
    {
        SetPhotos(phoneCameraController.GetPhotos());
        this.contactToText = contactToText;
    }

    public void PreviewPhoto(PhotoTaken photoToPreview)
    {
        activePreview = photoToPreview;
        sendButton.SetActive(contactToText != null);

        if (photoToPreview.isLandscape)
        {
            landscapeImagePreview.texture = photoToPreview.photo;
            landscapeImagePreview.gameObject.SetActive(true);
            imagePreview.gameObject.SetActive(false);
        }
        else
        {
            imagePreview.texture = photoToPreview.photo;
            imagePreview.gameObject.SetActive(true);
            landscapeImagePreview.gameObject.SetActive(false);
        }
        galleryGroup.SetActive(false);
        previewGroup.SetActive(true);
    }

    void SetPhotos(List<PhotoTaken> photos)
    {
        this.photos = photos;
        foreach (Transform child in thumbnailParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var curPhoto in photos)
        {
            var thumbnail = Instantiate(thumnbnailPrefab, thumbnailParent.transform);
            var image = thumbnail.GetComponentInChildren<RawImage>();
            var aspectRatio = thumbnail.GetComponentInChildren<AspectRatioFitter>().aspectRatio = curPhoto.isLandscape ? 16f/10f : 10f/16f;
            image.texture = curPhoto.photo;
            thumbnail.GetComponent<Button>().onClick.AddListener(() => PreviewPhoto(curPhoto));
        }
    }
}
