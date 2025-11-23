using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    public void InputLogin(string login)
    {
        Debug.Log(login.ToString());
        PlayerSettings.Instance.SetLogin(login);
    }

    public void ClickConnect()
    {
        Debug.Log("ClickConnect");
        if (string.IsNullOrEmpty(PlayerSettings.Instance.Login)) return;
        SceneManager.LoadScene("Game");
    }
}
