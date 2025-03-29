using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class IL : MonoBehaviour
{
    [SerializeField] private List<Texture2D> _images = new List<Texture2D>();
    [SerializeField] private List<Sprite> _sprites = new List<Sprite>();
    [SerializeField] private GameObject _iconPrefab;
    [SerializeField] private ListImage _listImage;

    private int _currentImageIndex;

    private void Start() => _listImage.ClearImageList();

    public void LoadImages(string imagePath) => StartCoroutine(LoadImagesRoutine(imagePath));

    private IEnumerator LoadImagesRoutine(string imagePath)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, "config", "image", Path.GetFileName(imagePath)) + ".png";

        yield return new WaitUntil(() => File.Exists(fullPath));

        Texture2D loadedTexture = LoadTextureFromFile(fullPath);
        if (loadedTexture != null)
        {
            AddNewImage(loadedTexture);
        }
    }

    private Texture2D LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Image file not found at path: {filePath}");
            return null;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load image: {e.Message}");
            return null;
        }
    }

    private void AddNewImage(Texture2D texture)
    {
        Debug.Log(texture);
        _images.Add(texture);

        Sprite newSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            Vector2.zero
        );

        _sprites.Add(newSprite);
        _listImage.AddToImageList(newSprite); // “еперь используем правильное им€ метода
        _currentImageIndex++;
    }
}