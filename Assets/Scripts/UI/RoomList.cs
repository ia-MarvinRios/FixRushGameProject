using System.Collections.Generic;
using UnityEngine;
using FixRush;

public class RoomList : MonoBehaviour
{
    [Header("Room List References")]
    [SerializeField] private Transform _scrollviewContent;
    [SerializeField] private GameObject _roomCardPrefab;

    private List<RoomCard> _activeRoomCards = new List<RoomCard>();

    private void OnEnable()
    {
        PhotonManager.Instance.OnRoomListChanged += UpdateRoomList;
        PhotonManager.Instance.RefreshRoomList();
    }

    private void OnDisable()
    {
        PhotonManager.Instance.OnRoomListChanged -= UpdateRoomList;
    }

    public void UpdateRoomList(List<RoomData> rooms)
    {
        int i = 0;

        foreach (var roomData in rooms)
        {
            RoomCard card;

            if (i < _activeRoomCards.Count)
            {
                // Reuse
                card = _activeRoomCards[i];
                card.gameObject.SetActive(true);
            }
            else
            {
                // Create a new card
                GameObject roomCard = Instantiate(_roomCardPrefab, _scrollviewContent);
                card = roomCard.GetComponent<RoomCard>();
                _activeRoomCards.Add(card);
            }

            card.SetRoomInfo(roomData);

            i++;
        }

        // Deactivate any remaining cards that are not used
        for (int j = i; j < _activeRoomCards.Count; j++)
        {
            _activeRoomCards[j].gameObject.SetActive(false);
        }
    }
}
