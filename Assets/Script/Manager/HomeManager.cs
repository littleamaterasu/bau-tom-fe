using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.Collections;

public class HomeManager : MonoBehaviour
{
    public DiceManager diceManager; // Xúc sắc
    public RoomManager roomManager;
    public PlayerManager playerManager;

    public GameObject roomPrefab;  // Prefab của Button
    public Transform contentPanel; // Content của Scroll View
    public Button addRoom; // Nút tạo phòng
    public GameObject mainGame; // Xúc sắc và bàn chơi
    public GameObject mainUI; // UI phòng

    public Button closeGame; // nút thoát trò chơi
    public Button leaveRoom; // Nút rời phòng


    private ClientWebSocket websocket;
    private Dictionary<string, GameObject> roomButtons = new Dictionary<string, GameObject>();
    
    // Start
    async void Start()
    {
        addRoom.onClick.AddListener(() => StartCoroutine(HandleCreateRoomCooldown()));
        closeGame.onClick.AddListener(() => {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });

        leaveRoom.onClick.AddListener(() => LeaveRoom());

        string savedUrl = PlayerPrefs.GetString("url", ""); // Lấy URL từ PlayerPrefs
        if (string.IsNullOrEmpty(savedUrl))
        {
            Debug.LogError("WebSocket URL is not set in PlayerPrefs!");
            return;
        }

        // Chuyển từ HTTP -> WebSocket (https -> wss)
        string wsUrl = savedUrl.Replace("https://", "wss://").Replace("http://", "ws://");

        websocket = new ClientWebSocket();
        Uri serverUri = new Uri(wsUrl);
        await websocket.ConnectAsync(serverUri, CancellationToken.None);
        
        // Web socket cho player manager
        playerManager.SetWebSocket(websocket);
        StartListening();
    }

    // Kết nối
    async void StartListening()
    {
        byte[] buffer = new byte[1024];

        while (websocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Debug.Log("Received message: " + message);

            HandleMessage(message);
        }
    }

    // Xử lý các tin nhắn
    void HandleMessage(string message)
    {
        Debug.Log($"Message: {message}");
        try
        {
            EventData response = JsonUtility.FromJson<EventData>(message);
 
            // thông tin phòng (chỉ hiển thị khi mainUI đang bật)
            if (response.eventName == "room_list" && mainUI != null && mainUI.activeInHierarchy)
            {
                RoomListData roomListData = JsonUtility.FromJson<RoomListData>(message);
                UpdateRoomList(roomListData.data);
            }
            // nhập phòng thành công
            else if (response.eventName == "join_room_success")
            {
                JoinRoomSuccess joinRoomSuccess = JsonUtility.FromJson<JoinRoomSuccess>(message);
                PlayerRoomData playerRoomData = joinRoomSuccess.data;
                roomManager.SetRoomId(playerRoomData.roomId);
                playerManager.SetUserName(playerRoomData.username);
                SwitchToMainGame();
            } 
            // kết quả xúc sắc của phòng
            else if(response.eventName == "dice_result")
            {
                try
                {
                    // JSON
                    DiceEvent diceEvent = JsonUtility.FromJson<DiceEvent>(message);
                    DiceData diceData = diceEvent.data;

                    diceManager.Action(diceData.face);

                    // đợi xúc sắc lăn xong có kết quả rồi mới hiện betUI
                    playerManager.OpenBetUI();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing 'dice_result' data: {ex.Message}");
                }
            } 
            // thông tin các người chơi
            else if(response.eventName == "room_players" && playerManager != null && playerManager.isActiveAndEnabled)
            {
                try
                {
                    // JSON
                    RoomPlayersEvent roomPlayersEvent = JsonUtility.FromJson<RoomPlayersEvent>(message);
                    RoomPlayersData roomPlayersData = roomPlayersEvent.data;
                    if (roomPlayersData.playerDatas == null) Debug.Log("player null");
                    if (roomPlayersData.roomId == null) Debug.Log("room null");
                    playerManager.UpdatePlayersData(roomPlayersData.playerDatas, roomPlayersData.roomId);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing 'room_players' data: {ex.Message}");
                }
            } 
            // rời phòng thành công
            else if(response.eventName == "leave_room_success")
            {
                SwitchToRoomList();
            } 
            // đặt cược thành công
            else if(response.eventName == "gamble_success")
            {
                playerManager.CloseBetUI();
            }
            // sự kiện khác
            else
            {
                Debug.Log("Nhận data từ chủ đề " + response.eventName);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("JSON Parse Error: " + e.Message);
        }
    }

    // Quản lý phòng
    void UpdateRoomList(List<RoomData> rooms)
    {
        List<string> existingRoomIds = new List<string>(roomButtons.Keys);

        // Xóa các room không còn tồn tại
        foreach (string roomId in existingRoomIds)
        {
            if (!rooms.Exists(r => r.roomId == roomId))
            {
                Destroy(roomButtons[roomId]);
                roomButtons.Remove(roomId);
            }
        }

        // Cập nhật hoặc thêm mới
        foreach (var room in rooms)
        {
            if (roomButtons.ContainsKey(room.roomId))
            {
                // Cập nhật số người chơi
                roomButtons[room.roomId].GetComponent<RoomButtonController>().Setup(room.roomId, room.currentPlayers, websocket);
            }
            else
            {
                GameObject newRoom = Instantiate(roomPrefab, contentPanel);
                newRoom.GetComponent<RoomButtonController>().Setup(room.roomId, room.currentPlayers, websocket);
                roomButtons[room.roomId] = newRoom;
            }
        }
    }

    private async void createRoom()
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        string roomId = GenerateRandomRoomId(12);

        string message = $"{{\"eventName\":\"create_room\",\"data\":{{\"roomId\":\"{roomId}\"}}}}";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await websocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

        Debug.Log("Sent create_room request: " + message);
    }

    private string GenerateRandomRoomId(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Random random = new System.Random();
        char[] stringChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    private IEnumerator HandleCreateRoomCooldown()
    {
        addRoom.interactable = false; // Vô hiệu hóa nút
        createRoom(); // Tạo phòng
        yield return new WaitForSeconds(5); // Chờ 5 giây
        addRoom.interactable = true; // Kích hoạt lại nút
    }

    async void LeaveRoom()
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        string message = $"{{\"eventName\":\"leave_room\",\"data\":{{\"roomId\":\"{roomManager.GetRoomId()}\", \"username\":\"{playerManager.GetUserName()}\"}}}}";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await websocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

        Debug.Log("Sent leave room request: " + message);
    }

    void SwitchToMainGame()
    {
        mainUI.SetActive(false); 
        mainGame.SetActive(true); 
    }

    void SwitchToRoomList()
    {
        Debug.Log("switch to room list");
        mainGame.SetActive(false);
        mainUI.SetActive(true);
    }

    void OnDestroy()
    {
        if (websocket != null)
        {
            websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }

    
}

// Data
[Serializable]
public class PlayerRoomData
{
    public string username;
    public string roomId;
}
[Serializable]
public class RoomListData
{
    public string eventName;
    public List<RoomData> data;
}
[Serializable]
public class RoomData
{
    public string roomId;
    public int currentPlayers;
}
[Serializable]
public class DiceData
{
    public int face;
}
[Serializable]
public class PlayerData
{
    public string username;
    public int balance;
    
}
[Serializable]
public class RoomPlayersData
{
    public string roomId;
    public List<PlayerData> playerDatas;
    
}

// Event
[Serializable]
public class DiceEvent
{
    public string eventName;
    public DiceData data;
}
[Serializable]
public class RoomPlayersEvent
{
    public string eventName;
    public RoomPlayersData data;
}
[Serializable]
public class JoinRoomSuccess
{
    public string eventName;
    public PlayerRoomData data;
}
[Serializable]
public class EventData
{
    public string eventName;  
    public object data;       
}
