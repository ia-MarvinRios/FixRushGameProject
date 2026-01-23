using FixRushGame;
using System.Collections;
using UnityEngine;

public class Car : Vehicle
{
    [Header("Car Settings")]
    [SerializeField] Vector3[] _gatoRoots;
    [SerializeField] float _gatoCheckerRadius = 0.3f;
    [SerializeField] IssueType[] _issues;
    [SerializeField] Vector3[] _wheelRoots;
    [Space(10)]
    [Header("Gizmos Settings")]
    [SerializeField] bool _showGatoRoots = false;
    [SerializeField] bool _showWheelRoots = false;
    [SerializeField] float _wheelGizmoRadius = 0.3f;

    internal Vector3[] WheelRoots { get { return _wheelRoots; } }

    public override void Initialize()
    {
        StartCoroutine(FixCarCoroutine());
    }

    IEnumerator FixCarCoroutine()
    {
        foreach(var i in _issues)
        {
            switch (i)
            {
                default:
                    yield return new WaitForSeconds(1);
                    break;

                case IssueType.Tires:
                    TireIssue tIssue = new TireIssue(this);
                    yield return StartCoroutine(tIssue.FixingCoroutine());
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the nearest GatoRoot taking a given position as a reference.
    /// </summary>
    /// <param name="refPos"></param>
    /// <returns>Nearest Vector3 "Gato Root".</returns>
    public Vector3 GetNearestGatoRoot(Vector3 refPos) { return GetNearestObjFromArray(refPos, _gatoRoots); }

    private void OnDrawGizmosSelected()
    {
        if (_showGatoRoots)
        {
            Gizmos.color = Color.darkRed;
            foreach(Vector3 gatoPos in _gatoRoots)
            {
                Gizmos.DrawWireSphere(transform.position + gatoPos, _gatoCheckerRadius);
            }
        }
        if (_showWheelRoots)
        {
            Gizmos.color = Color.white;
            foreach (Vector3 wheel in _wheelRoots)
            {
                Gizmos.DrawWireSphere(transform.position + wheel, _wheelGizmoRadius);
            }
        }
    }
}
