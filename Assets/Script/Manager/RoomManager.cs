using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private string roomId;

    public void SetRoomId(string roomId)
    {
        this.roomId = roomId;
    }

    public string GetRoomId()
    {
        return roomId; 
    }
}
