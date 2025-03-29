using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class GitHubDownloader : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private Text statusText;

    private const string BaseUrl = "https://raw.githubusercontent.com/maylave/unity/main/";
    private const string ConfigListUrl = BaseUrl + "config/listId.json";
    private const string ImagesBaseUrl = BaseUrl + "config/image/";
    private const string JsonConfigBaseUrl = BaseUrl + "config/Items/";

    private readonly List<ListItem> _itemsData = new List<ListItem>();
    private int _totalFilesToDownload;
    private int _downloadedFilesCount;

    private void Awake()
    {
        InitializeDirectories();
    }

    private void InitializeDirectories()
    {
        string configPath = Path.Combine(Application.persistentDataPath, "config");
        string imagePath = Path.Combine(configPath, "image");
        string itemsConfigPath = Path.Combine(configPath, "Items");

        Directory.CreateDirectory(configPath);
        Directory.CreateDirectory(imagePath);
        Directory.CreateDirectory(itemsConfigPath);
    }

    public void StartDownload()
    {
        SetLoadingActive(true);
        StartCoroutine(DownloadAllFiles());
    }

    private IEnumerator DownloadAllFiles()
    {
        yield return StartCoroutine(DownloadConfigList());

        if (_itemsData.Count > 0)
        {
            _totalFilesToDownload = _itemsData.Count * 2; // JSON + Image for each item
            _downloadedFilesCount = 0;

            foreach (var item in _itemsData)
            {
                yield return StartCoroutine(DownloadItemFiles(item));
            }

            UpdateStatus("Загрузка завершена!");
            yield return new WaitForSeconds(1f);
            LoadNextScene();
        }
    }

    private IEnumerator DownloadConfigList()
    {
        UpdateStatus("Загрузка списка файлов...");

        using (var request = UnityWebRequest.Get(ConfigListUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonText = request.downloadHandler.text.Trim();
                _itemsData.Clear();
                _itemsData.AddRange(JsonConvert.DeserializeObject<List<ListItem>>(jsonText));

                string localConfigPath = Path.Combine(Application.persistentDataPath, "config", "listId.json");
                File.WriteAllText(localConfigPath, jsonText);
            }
            else
            {
                Debug.LogError($"Ошибка загрузки списка: {request.error}");
                UpdateStatus("Ошибка загрузки списка!");
                yield return new WaitForSeconds(2f);
            }
        }
    }

    private IEnumerator DownloadItemFiles(ListItem item)
    {
        string fileName = item.name;

        // Download JSON
        yield return StartCoroutine(DownloadFile(
            JsonConfigBaseUrl + fileName + ".json",
            Path.Combine("config", "Items", fileName + ".json"),
            isTexture: false
        ));

        // Download Image
        yield return StartCoroutine(DownloadFile(
            ImagesBaseUrl + fileName + ".png",
            Path.Combine("config", "image", fileName + ".png"),
            isTexture: true
        ));
    }

    private IEnumerator DownloadFile(string url, string localRelativePath, bool isTexture)
    {
        string fileName = Path.GetFileName(localRelativePath);
        UpdateStatus($"Загрузка {fileName}...");

        UnityWebRequest request = isTexture
            ? UnityWebRequestTexture.GetTexture(url)
            : UnityWebRequest.Get(url);

        using (request)
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string fullLocalPath = Path.Combine(Application.persistentDataPath, localRelativePath);

                if (isTexture)
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    File.WriteAllBytes(fullLocalPath, texture.EncodeToPNG());
                }
                else
                {
                    File.WriteAllText(fullLocalPath, request.downloadHandler.text);
                }

                _downloadedFilesCount++;
                UpdateProgress();
            }
            else
            {
                Debug.LogError($"Ошибка загрузки {fileName}: {request.error}");
            }
        }
    }

    private void UpdateStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    private void UpdateProgress()
    {
        if (_totalFilesToDownload == 0) return;

        float progress = (float)_downloadedFilesCount / _totalFilesToDownload;

        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"{progress * 100:F0}%";
    }

    private void SetLoadingActive(bool active)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(active);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        string configPath = Path.Combine(Application.persistentDataPath, "config", "listId.json");

        if (File.Exists(configPath))
        {
            string jsonText = File.ReadAllText(configPath);
            _itemsData.AddRange(JsonConvert.DeserializeObject<List<ListItem>>(jsonText));
            LoadNextScene();
        }
        else
        {
            StartDownload();
        }
    }
}