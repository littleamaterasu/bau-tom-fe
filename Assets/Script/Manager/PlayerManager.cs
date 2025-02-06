using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System;

public class PlayerManager : MonoBehaviour
{
    public List<PlayerInfoController> players; // Danh sách người chơi tối đa 4 người
    public TMP_InputField betInput;
    public List<Button> diceOption;
    public GameObject betUI; // UI Bet
    public Button confirmBet;

    private Dictionary<string, int> playersBalance = new();
    private int betOption = 0;
    private int betAmount = 0;
    private string username;
    private string roomId;
    private ClientWebSocket ws;

    private void Start()
    {
        for (int i = 0; i < 6; ++i)
        {
            int index = i + 1; // Lưu giá trị cố định
            diceOption[i].onClick.AddListener(() => SetBetOption(index));
        }

        confirmBet.onClick.AddListener(() => SendBetRequest());
    }

    public void SetWebSocket(ClientWebSocket ws)
    {
        this.ws = ws;
    }

    public void UpdatePlayersData(List<PlayerData> roomPlayers, string roomId)
    {
        username = PlayerPrefs.GetString("username", "");
        this.roomId = roomId;

        if (roomPlayers == null)
        {
            Debug.LogError("roomPlayers is NULL!");
            return;
        }
        Debug.Log($"Updating Players Data - Room ID: {roomId}, Players Received: {roomPlayers.Count}");

        int playerCount = roomPlayers.Count;
        int totalSlots = players.Count;

        Debug.Log($"Total Slots: {totalSlots}");

        HashSet<string> existingUsers = new(); // Lưu các username có trong roomPlayers

        for (int i = 0; i < totalSlots; i++)
        {
            if (i < playerCount)
            {
                if (roomPlayers[i] == null)
                {
                    Debug.LogError($"roomPlayers[{i}] is NULL!");
                    continue;
                }

                string username = roomPlayers[i].username;
                int newBalance = roomPlayers[i].balance;

                Debug.Log($"Processing player {i} - Username: {username}, Balance: {newBalance}");

                // Kiểm tra balance có tăng không
                if (playersBalance.TryGetValue(username, out int oldBalance))
                {
                    Debug.Log($"Old Balance: {oldBalance}, New Balance: {newBalance}");

                    if (newBalance > oldBalance)
                    {
                        Debug.Log($"Balance increased for {username}, playing animation...");
                        PlayAnimation();
                    }
                }

                Debug.Log("done get balanace");

                // Cập nhật balance mới
                playersBalance[username] = newBalance;
                existingUsers.Add(username);

                if (players[i] == null)
                {
                    Debug.LogError($"players[{i}] is NULL!");
                    continue;
                }

                players[i].Setup(username, newBalance);
                players[i].Show();
            }
            else
            {
                if (players[i] == null)
                {
                    Debug.LogError($"players[{i}] is NULL!");
                    continue;
                }

                players[i].Hide();
            }
        }

        Debug.Log("update ok");
    }


    private async void SendBetRequest()
    {
        if (ws == null || ws.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        if(betOption == 0)
        {
            Debug.Log("Chọn 1 lựa chọn");
            return;
        }

        Debug.Log("bet option " + betOption);

        string message = $"{{\"eventName\":\"gamble\",\"data\":{{\"roomId\":\"{roomId}\",\"username\":\"{username}\",\"amount\":\"{GetBetAmount()}\",\"option\":\"{betOption}\"}}}}";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

        // cài lại các giá trị
        betOption = 0;
        betAmount = 0;

        CloseBetUI();

        Debug.Log("Sent gamble request: " + message);
    }

    public int GetBetAmount()
    {
        if (betInput == null || string.IsNullOrEmpty(betInput.text))
            return 0;

        return int.TryParse(betInput.text, out betAmount) ? betAmount : 0;
    }

    public int GetBetOption()
    {
        return betOption;
    }

    void SetBetOption(int option)
    {
        betOption = option;
    }

    // TODO
    public void PlayAnimation()
    {

    }

    public void SetUserName(string username)
    {
        this.username = username;
    }

    public string GetUserName()
    {
        return username;
    }

    public void CloseBetUI()
    {

        betUI.SetActive(false);
    }

    public void OpenBetUI()
    {
        StartCoroutine(ShowBetUIAfterDelay());
    }

    private IEnumerator ShowBetUIAfterDelay()
    {
        yield return new WaitForSeconds(3);
        betUI.SetActive(true);
    }
}

