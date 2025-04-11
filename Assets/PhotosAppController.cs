using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PhotosAppController : MonoBehaviour
{
    [SerializeField] private RawImage imagePreview;
    [SerializeField] private RawImage landscapeImagePreview;

    public void SetPhotos(List<PhotoTaken> photos)
    {
        if (photos.Count > 0)
        {
            var thePhoto = photos.Last();
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
        }
    }
}
