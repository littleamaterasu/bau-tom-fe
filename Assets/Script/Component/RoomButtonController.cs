using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System;

public class RoomButtonController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text roomIdText;
    [SerializeField]
    private TMP_Text currentPlayersText;
    [SerializeField]
    private Button roomButton;

    private string roomId;
    private int currentPlayers;
    private ClientWebSocket ws;

    public void Setup(string roomId, int currentPlayers, ClientWebSocket ws)
    {
        this.roomId = roomId;
        this.currentPlayers = currentPlayers;
        this.ws = ws;
        roomIdText.text = roomId;
        currentPlayersText.text = currentPlayers.ToString() + "/4";

        roomButton.onClick.AddListener(SendJoinRoomRequest);
    }

    private async void SendJoinRoomRequest()
    {
        if (ws == null || ws.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        string username = PlayerPrefs.GetString("username", "Guest"); // Lấy tên người chơi từ PlayerPrefs

        string message = $"{{\"eventName\":\"join_room\",\"data\":{{\"roomId\":\"{roomId}\",\"username\":\"{username}\"}}}}";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

        Debug.Log("Sent join_room request: " + message);
    }
}
