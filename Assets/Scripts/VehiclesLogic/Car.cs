using FixRushGame;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct GatoRoot
{
    public Vector3 Position;
    public Vector3 RotationPivot;
    public Vector3 EulerRotation;
    public GameObject Object;
}

[System.Serializable]
public struct WheelRoot
{
    public int LinkedGatoRootID;
    public Vector3 Position;
}

public class Car : Vehicle
{
    [Header("Car Settings")]
    [SerializeField] GatoRoot[] _gatoRoots;
    [SerializeField] float _gatoCheckerRadius = 0.3f;
    [SerializeField] WheelRoot[] _wheelRoots;
    [Space(10)]
    [Header("Gizmos Settings")]
    [SerializeField] bool _showGatoRoots = false;
    [SerializeField] Vector3 _gatoRootSizeReference = Vector3.one;
    [SerializeField] bool _showWheelRoots = false;
    [SerializeField] float _wheelGizmoRadius = 0.3f;

    internal GatoRoot[] GatoRoots {  get { return _gatoRoots; } }
    internal WheelRoot[] WheelRoots { get { return _wheelRoots; } }
    internal bool IsGatoed { get; private set; }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public override void Initialize()
    {
        IsGatoed = false;
        StartCoroutine(FixCarCoroutine());
    }

    IEnumerator FixCarCoroutine()
    {
        foreach(var i in Issues)
        {
            switch (i)
            {
                default:
                    yield return new WaitForSeconds(1);
                    break;

                case IssueType.Tires:
                    TireIssue tIssue = new TireIssue(this);
                    yield return StartCoroutine(tIssue.FixingCoroutine());
                    yield return new WaitUntil(()=>!IsGatoed);
                    tIssue = null;
                    break;

                case IssueType.Dirty:
                    DirtIssue dIssue = new DirtIssue(this);
                    yield return StartCoroutine(dIssue.FixingCoroutine());
                    dIssue = null;
                    break;
            }
        }

        yield return StartCoroutine(VManager.Instance.MoveToEndPoint(Agent, this));
    }

    /// <summary>
    /// Places the gato on the nearest GatoRoot Position.
    /// </summary>
    /// <param name="player"></param>
    public void PlaceGato(AuxPlayer player, int gatoRootIndex)
    {
        if (player.GrabbedObj == null || IsGatoed)
            return;

        if (!player.GrabbedObj.CompareTag("Gato") || gatoRootIndex < 0)
            return;

        // Cache
        ref GatoRoot root = ref _gatoRoots[gatoRootIndex];
        GameObject gato = player.GrabbedObj;

        // Place gato
        root.Object = gato;
        player.GrabbedObj = null;

        Transform gatoT = gato.transform;
        gatoT.position = transform.TransformPoint(root.Position);
        gatoT.eulerAngles = root.EulerRotation;

        // --- Rotate Car ---
        Vector3 pivotWorld = transform.TransformPoint(root.RotationPivot);
        Vector3 dir = pivotWorld - transform.position;

        float sizeY = gato.GetComponent<BoxCollider>().size.y / 2;
        float distance = dir.magnitude;

        float angle = Mathf.Asin(sizeY / distance) * Mathf.Rad2Deg;
        float sign = Mathf.Sign(Vector3.Dot(dir, transform.forward));

        Model.RotateAround(pivotWorld, Vector3.right, angle * sign);

        // Set gato pickable
        Pickable pickable = gato.GetComponent<Pickable>();
        pickable.ReleaseOwnership();
        pickable.IsPickable = true;
        gato.GetComponent<Gato>().SetUpGato(this, gatoRootIndex);

        IsGatoed = true;

        Debug.Log($"Gato placed. Pivot: {pivotWorld} | Angle: {angle * sign}");
    }

    public void RemoveGato(int gatoRootIndex)
    {
        if (!IsGatoed || gatoRootIndex < 0)
            return;

        ref GatoRoot root = ref _gatoRoots[gatoRootIndex];
        if (root.Object == null)
            return;

        // --- Rotate Car back ---
        Vector3 pivotWorld = transform.TransformPoint(root.RotationPivot);
        Vector3 dir = pivotWorld - transform.position;

        float sizeY = root.Object.GetComponent<BoxCollider>().size.y / 2;
        float distance = dir.magnitude;

        float angle = Mathf.Asin(sizeY / distance) * Mathf.Rad2Deg;
        float sign = Mathf.Sign(Vector3.Dot(dir, transform.forward));

        Model.RotateAround(pivotWorld, Vector3.right, -angle * sign);

        // Release gato
        root.Object = null;
        IsGatoed = false;

        Debug.Log($"Gato removed. Pivot: {pivotWorld} | Angle: {-angle * sign}");
    }

    /// <summary>
    /// Gets the nearest GatoRoot taking a given position as a reference.
    /// </summary>
    /// <param name="refPos"></param>
    /// <returns>Nearest Vector3 "Gato Root".</returns>
    public int GetNearestGatoRootIndex(Vector3 refPos)
    {
        int nearestIndex = -1;
        float minDist = float.MaxValue;

        for (int i = 0; i < _gatoRoots.Length; i++)
        {
            float dist = (transform.TransformPoint(_gatoRoots[i].Position) - refPos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private void OnDrawGizmosSelected()
    {
        if (_showGatoRoots)
        {
            foreach(GatoRoot gatoRoot in _gatoRoots)
            {
                Gizmos.color = Color.darkRed;
                Gizmos.DrawWireCube(transform.position + gatoRoot.Position, _gatoRootSizeReference);

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position + gatoRoot.RotationPivot, _gatoCheckerRadius * 0.7f);
            }
        }
        if (_showWheelRoots)
        {
            Gizmos.color = Color.white;
            foreach (WheelRoot wheel in _wheelRoots)
            {
                Gizmos.DrawWireSphere(transform.position + wheel.Position, _wheelGizmoRadius);
            }
        }
    }
}
