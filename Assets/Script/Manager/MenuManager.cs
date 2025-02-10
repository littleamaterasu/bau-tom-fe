using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public Button loginButton;
    public Button registerButton;
    public Button submitButton;
    public TMP_Text notification;
    public TMP_InputField urlInput;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    private bool isLoginMode = true; // Mặc định là Login

    void Start()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        loginButton.onClick.AddListener(SetLoginMode);
        registerButton.onClick.AddListener(SetRegisterMode);
        submitButton.onClick.AddListener(SendRequest);
    }

    void SetLoginMode()
    {
        isLoginMode = true;
        notification.text = "Mode: Login";
    }

    void SetRegisterMode()
    {
        isLoginMode = false;
        notification.text = "Mode: Register";
    }

    void SendRequest()
    {
        string url = urlInput.text.Trim();
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            notification.text = "Error: Please fill in all fields!";
            return;
        }

        string endpoint = isLoginMode ? "/login" : "/register";
        StartCoroutine(SendPostRequest(url + endpoint, username, password));
    }

    IEnumerator SendPostRequest(string url, string username, string password)
    {
        // Disable UI
        SetUIState(false);

        // Tạo JSON request
        string jsonData = JsonUtility.ToJson(new UserData(username, password));
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            notification.text = "Success: " + request.downloadHandler.text;

            if (isLoginMode)
            {
                yield return new WaitForSeconds(1f);
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("url", url);
                SceneManager.LoadScene("home"); // Đổi thành tên scene của bạn
            }
        }
        else
        {
            notification.text = "Error: " + request.error;
        }

        // Re-enable UI
        SetUIState(true);
    }

    void SetUIState(bool state)
    {
        loginButton.interactable = state;
        registerButton.interactable = state;
        submitButton.interactable = state;
        urlInput.interactable = state;
        usernameInput.interactable = state;
        passwordInput.interactable = state;
    }

    [System.Serializable]
    public class UserData
    {
        public string username;
        public string password;

        public UserData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
}
