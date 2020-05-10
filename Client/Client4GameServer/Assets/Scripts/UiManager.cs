using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    public GameObject StartMenu;

    public InputField UsernameField;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log("Networking Client already exists, destroying overload!");
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        StartMenu.SetActive(false);
        UsernameField.interactable = false;
        Client.Instance.ConnectToServer();
    }
}