using UnityEngine;
using LootLocker.Requests;
using System.Collections;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UnityEvent playerConnected;
    private IEnumerator Start()
    {
        bool connected = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.Log("Error starting LootLocker serssion");
                return;
            }
            Debug.Log("Succesfully lootlocker Session");
            connected = true;
        }
        );
        yield return new WaitUntil(() => connected);
        playerConnected?.Invoke();
    }
}
