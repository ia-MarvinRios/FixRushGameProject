using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private Image _readyIcon;

    internal string PlayerName 
    {
        get
        {
            return _playerNameText.text;
        }

        set
        {
            _playerNameText.text = value;
        }
    }

    internal bool Ready
    {
        get
        {
            return _readyIcon.enabled;
        }

        set
        {
            _readyIcon.enabled = value;
        }
    }
}
