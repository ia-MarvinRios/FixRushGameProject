using FixRush;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Car : Vehicle
{
    [Header("Car References")]
    [SerializeField] private Transform _jackRootFront;
    [SerializeField] internal GameObject[] TiresFront;
    [SerializeField] private Transform _jackRootRear;
    [SerializeField] internal GameObject[] TiresRear;

    private Jack _jack;

    internal bool IsJacked = false;

    public override void InitializeVehicle()
    {
        base.InitializeVehicle();

        StartCoroutine(FixCarCoroutine());
    }

    private IEnumerator FixCarCoroutine()
    {
        foreach (var i in IssueTypes)
        {
            switch (i)
            {
                default:
                    yield return new WaitForSeconds(1);
                    break;

                case IIssue.Type.Tires:
                    CurrentIssue = new TireIssue(this);
                    yield return StartCoroutine(CurrentIssue.FixingCoroutine());
                    CurrentIssue.CleanUp();
                    break;

                case IIssue.Type.Dirty:
                    CurrentIssue = new DirtIssue(this);
                    yield return StartCoroutine(CurrentIssue.FixingCoroutine());
                    CurrentIssue.CleanUp();
                    break;
            }
        }

        CurrentIssue = null;

        MoveCarToEndPoint(this);
        yield return new WaitUntil(() => IsFixed);
    }

    private void MoveCarToEndPoint(Vehicle vehicle)
    {
        NetworkHandler.MoveCarToEndPoint(vehicle);
    }

    /// <summary>
    /// Jacks the car. If it's placed front or rear returns 1 or -1. Not Jacked returns 0.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>-1, 0, 1</returns>
    internal int JackCar(PlayerController player)
    {
        if (!player.GrabbedObj.TryGetComponent(out Jack jack)) { return 0; }

        Vector3 dirToReference = (player.transform.position - transform.position).normalized;
        float dot              = Vector3.Dot(transform.forward, dirToReference);

        jack.DisablePicking();
        _jack = jack;

        if (dot >= 0)
        {
            // --- Use Front Wheels ---
            jack.transform.position = _jackRootFront.position;
            jack.transform.rotation = _jackRootFront.rotation;
            // Parenting
            jack.transform.SetParent(_jackRootFront);

            IsJacked = true;

            mAnimator.SetTrigger("JackFront");

            return 1;
        }
        else
        {
            // --- Use Rear Wheels ---
            jack.transform.position = _jackRootRear.position;
            jack.transform.rotation = _jackRootRear.rotation;
            // Parenting
            jack.transform.SetParent(_jackRootRear);

            NetworkHandler.SyncJacked(true);

            mAnimator.SetTrigger("JackBack");

            return -1;
        }
    }

    internal int UnjackCar(PlayerController player)
    {
        if (_jack == null) { return 0; }

        Vector3 dirToReference = (player.transform.position - transform.position).normalized;
        float dot              = Vector3.Dot(transform.forward, dirToReference);

        _jack.PickUp(player);

        _jack    = null;
        NetworkHandler.SyncJacked(false);

        mAnimator.SetTrigger("Base");

        return dot >= 0 ? 1 : -1;
    }
}
