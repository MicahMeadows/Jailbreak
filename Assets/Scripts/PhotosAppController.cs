using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PhotosAppController : MonoBehaviour
{
    [SerializeField] private PhoneCameraController phoneCameraController;
    [SerializeField] private RawImage imagePreview;
    [SerializeField] private RawImage landscapeImagePreview;
    [SerializeField] private GameObject thumbnailParent;
    [SerializeField] private GameObject thumnbnailPrefab;
    [SerializeField] private GameObject galleryGroup;
    [SerializeField] private GameObject previewGroup;
    [SerializeField] private Button backButton;
    private List<PhotoTaken> photos;

    void Start()
    {
        backButton.onClick.AddListener(() =>
        {
            galleryGroup.SetActive(true);
            previewGroup.SetActive(false);
        });
    }

    public void SetEnabled(bool value)
    {
        SetPhotos(phoneCameraController.GetPhotos());
    }

    public void PreviewPhoto(int idx)
    {
        if (photos.Count > idx)
        {
            var thePhoto = photos[idx];
            if (thePhoto.isLandscape)
            {
                landscapeImagePreview.texture = thePhoto.photo;
                landscapeImagePreview.gameObject.SetActive(true);
                imagePreview.gameObject.SetActive(false);
            }
            else
            {
                imagePreview.texture = thePhoto.photo;
                imagePreview.gameObject.SetActive(true);
                landscapeImagePreview.gameObject.SetActive(false);
            }
            galleryGroup.SetActive(false);
            previewGroup.SetActive(true);
        }
    }

    void SetPhotos(List<PhotoTaken> photos)
    {
        this.photos = photos;
        foreach (Transform child in thumbnailParent.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < photos.Count; i++)
        {
            var thumbnail = Instantiate(thumnbnailPrefab, thumbnailParent.transform);
            var image = thumbnail.GetComponent<RawImage>();
            image.texture = photos[i].photo;
            int index = i;
            thumbnail.GetComponent<Button>().onClick.AddListener(() => PreviewPhoto(index));
        }
    }
}
