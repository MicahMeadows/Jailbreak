using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PhotosAppController : MonoBehaviour
{
    [SerializeField] private RawImage imagePreview;

    public void SetPhotos(List<PhotoTaken> photos)
    {
        if (photos.Count > 0)
        {
            imagePreview.texture = photos.Last().photo;
        }
    }
}
