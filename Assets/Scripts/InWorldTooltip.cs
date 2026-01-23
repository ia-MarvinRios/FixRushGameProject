using System.Collections;
using UnityEngine;

public class InWorldTooltip : MonoBehaviour
{
    bool _isActive = false;
    [SerializeField] Animator _animator;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public Coroutine ShowTooltip(Transform root)
    {
        transform.position = root.position;
        return StartCoroutine(ShowToolTipCoroutine(root));
    }
    public Coroutine HideTooltip()
    {
        return StartCoroutine(HideTooltipCoroutine());
    }

    IEnumerator ShowToolTipCoroutine(Transform root)
    {
        _animator.SetTrigger("Show");

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        _isActive = true;

        StartCoroutine(FollowRootCoroutine(root));
    }
    IEnumerator HideTooltipCoroutine()
    {
        _animator.SetTrigger("Hide");

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        _isActive = false;

        Destroy(gameObject);
    }
    IEnumerator FollowRootCoroutine(Transform root)
    {
        while (_isActive) {
            transform.position = root.position;
            yield return null;
        }
    }
}
