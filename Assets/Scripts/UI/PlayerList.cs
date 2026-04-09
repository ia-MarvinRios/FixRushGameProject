using System.Collections.Generic;
using UnityEngine;

public class PlayerList : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameContent _gameContent;
    [SerializeField] private PlayerSettings _playerSettings;
    [SerializeField] private Transform _characterBuilderParent;
    [SerializeField] private Transform _InWorlCanvas;
    [SerializeField] private GameObject _playerInfoPanelPrefab;
    [SerializeField] private InfoPanel _localPlayerInfoPanel;
    [SerializeField] private RemotePlayerUI[] _remoteContainers;

    private int _hatIndex = -1;
    private int _bodyIndex = -1;

    private void OnEnable()
    {
        PhotonManager.Instance.OnPlayerListChanged += UpdatePlayerList;
        PhotonManager.Instance.OnRemotePlayerLeave += RemoveRemotePlayerUI;
    }

    private void OnDisable()
    {
        PhotonManager.Instance.OnPlayerListChanged -= UpdatePlayerList;
        PhotonManager.Instance.OnRemotePlayerLeave -= RemoveRemotePlayerUI;

        RemoveAllRemotePlayersUI();
    }

    private void UpdatePlayerList(List<PlayerData> playersData)
    {
        foreach (PlayerData data in playersData)
        {
            if (data.Uid == PhotonManager.Instance.MyUserID)
            {
                foreach (Transform child in _characterBuilderParent)
                {
                    Destroy(child.gameObject);
                }

                Instantiate(_gameContent.Hats[data.HatID].Prefab, _characterBuilderParent);
                _hatIndex = data.HatID;
                _playerSettings.Hat = _gameContent.Hats[data.HatID].Prefab;

                Instantiate(_gameContent.Bodies[data.BodyID].Prefab, _characterBuilderParent);
                _bodyIndex = data.BodyID;
                _playerSettings.Body = _gameContent.Bodies[data.BodyID].Prefab;

                SetLayerRecursively(_characterBuilderParent.gameObject, 3);

                if (_localPlayerInfoPanel != null)
                {
                    _localPlayerInfoPanel.PlayerName = data.PlayerName;
                    _localPlayerInfoPanel.Ready = data.IsReady;
                }
                
            }
            else
            {
                UpdateRemotePlayerUI(data);
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void UpdateRemotePlayerUI(PlayerData data)
    {
        foreach (RemotePlayerUI container in _remoteContainers)
        {
            if (container.Uid == data.Uid)
            {
                container.UpdateUI(data);

                foreach (Transform child in container.transform)
                {
                    Destroy(child.gameObject);
                }

                Instantiate(_gameContent.Hats[data.HatID].Prefab, container.transform).layer = 3;
                Instantiate(_gameContent.Bodies[data.BodyID].Prefab, container.transform).layer = 3;

                container.InfoPanel.Ready = data.IsReady;

                return;
            }
            else if (string.IsNullOrEmpty(container.Uid))
            {
                container.Uid = data.Uid;
                container.UpdateUI(data);

                Instantiate(_gameContent.Hats[data.HatID].Prefab, container.transform).layer = 3;
                Instantiate(_gameContent.Bodies[data.BodyID].Prefab, container.transform).layer = 3;

                container.InfoPanel.PlayerName = data.PlayerName;
                container.InfoPanel.Ready = data.IsReady;

                return;
            }
        }
    }

    private void RemoveRemotePlayerUI(PlayerData data)
    {
        foreach (RemotePlayerUI container in _remoteContainers)
        {
            if (container.Uid == data.Uid)
            {
                container.Uid = string.Empty;
                container.BodyID = -1;
                container.HatID = -1;

                container.InfoPanel.PlayerName = "-";
                container.InfoPanel.Ready = false;

                foreach (Transform child in container.transform)
                {
                    Destroy(child.gameObject);
                }

                return;
            }
        }
    }
    private void RemoveAllRemotePlayersUI()
    {
        foreach (RemotePlayerUI container in _remoteContainers)
        {
            container.Uid = string.Empty;
            container.BodyID = -1;
            container.HatID = -1;
            container.InfoPanel.PlayerName = "-";
            container.InfoPanel.Ready = false;

            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void SwitchHat(int factor)
    {
        int length = _gameContent.Hats.Length;
        int newIndex = (_hatIndex + factor + length) % length;

        PhotonManager.Instance.SetIntProperty("hat", newIndex);
    }
    public void SwitchBody(int factor)
    {
        int length = _gameContent.Bodies.Length;
        int newIndex = (_bodyIndex + factor + length) % length;

        PhotonManager.Instance.SetIntProperty("body", newIndex);
    }
}
