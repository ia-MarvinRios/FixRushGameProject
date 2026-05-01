using FixRush;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private bool _getPreviousData = true;

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
        _getPreviousData = true;
    }

    private void UpdatePlayerList(List<PlayerData> playersData)
    {
        foreach (PlayerData data in playersData)
        {
            if (data.ActorNumber == PhotonManager.Instance.MyActorNumber)
            {
                foreach (Transform child in _characterBuilderParent)
                {
                    Destroy(child.gameObject);
                }

                if (!_getPreviousData)
                {
                    _playerSettings.Hat = data.HatID;
                    _playerSettings.Body = data.BodyID;

                    if (ColorUtility.TryParseHtmlString("#" + data.SkinColorHex, out Color color))
                    {
                        _playerSettings.SkinColor = color;
                    }
                }

                // Instantiate hat
                Instantiate(_gameContent.Hats[_playerSettings.Hat].Prefab, _characterBuilderParent);

                // Instantiate body and get its mesh renderer
                MeshRenderer mr = Instantiate(
                    _gameContent.Bodies[_playerSettings.Body].Prefab, 
                    _characterBuilderParent
                ).GetComponent<MeshRenderer>();

                // Set skin color creating a new material instance
                if (mr != null)
                {
                    Material newMat = new Material(mr.material);

                    newMat.color = _playerSettings.SkinColor;

                    mr.material = newMat;
                }

                _getPreviousData = false;

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

    private void ApplyUI(RemotePlayerUI container, PlayerData data)
    {
        container.UpdateUI(data);

        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }

        Instantiate(_gameContent.Hats[data.HatID].Prefab, container.transform).layer = 3;

        ProcessRemoteBody(data, container.transform);

        container.InfoPanel.PlayerName = data.PlayerName;
        container.InfoPanel.Ready = data.IsReady;

        Debug.Log($"{container.name}: {data.PlayerName} | data AN: {data.ActorNumber}, container AN: {container.ActorNumber}");
    }

    private void UpdateRemotePlayerUI(PlayerData data)
    {
        RemotePlayerUI emptySlot = null;

        foreach (var container in _remoteContainers)
        {
            if (container.ActorNumber == data.ActorNumber)
            {
                ApplyUI(container, data);
                return;
            }

            if (container.ActorNumber == 0 && emptySlot == null)
            {
                emptySlot = container;
            }
        }

        if (emptySlot != null)
        {
            emptySlot.ActorNumber = data.ActorNumber;
            ApplyUI(emptySlot, data);
        }
    }

    private void RemoveRemotePlayerUI(PlayerData data)
    {
        foreach (RemotePlayerUI container in _remoteContainers)
        {
            if (container.ActorNumber == data.ActorNumber)
            {
                container.ActorNumber = 0;
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
            if (container == null) continue;

            container.ActorNumber = 0;
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
    private void ProcessRemoteBody(PlayerData data, Transform container)
    {
        MeshRenderer mr = Instantiate(
                    _gameContent.Bodies[data.BodyID].Prefab,
                    container.transform
        ).GetComponent<MeshRenderer>();

        mr.gameObject.layer = 3;

        if (mr != null)
        {
            Material newMat = new Material(mr.material);
            Color color;
            if (ColorUtility.TryParseHtmlString("#" + data.SkinColorHex, out color))
            {
                newMat.color = color;
            }
            mr.material = newMat;
        }
    }

    public void SwitchHat(int factor)
    {
        int length = _gameContent.Hats.Length;
        int newIndex = (_playerSettings.Hat + factor + length) % length;

        PhotonManager.Instance.SetIntProperty("hat", newIndex);
    }
    public void SwitchBody(int factor)
    {
        int length = _gameContent.Bodies.Length;
        int newIndex = (_playerSettings.Body + factor + length) % length;

        PhotonManager.Instance.SetIntProperty("body", newIndex);
    }
    public void SetSkinTone(Image skinTone)
    {
        _playerSettings.SkinColor = skinTone.color;
        PhotonManager.Instance.SetStringProperty("skinColorHex", ColorUtility.ToHtmlStringRGBA(skinTone.color));
    }
}
