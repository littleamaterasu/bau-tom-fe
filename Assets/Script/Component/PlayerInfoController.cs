using TMPro;
using UnityEngine;

public class PlayerInfoController : MonoBehaviour
{
    public TMP_Text username;
    public TMP_Text balance;

    public void Setup(string username, int balance)
    {
        this.username.text = username;
        this.balance.text = balance.ToString();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
