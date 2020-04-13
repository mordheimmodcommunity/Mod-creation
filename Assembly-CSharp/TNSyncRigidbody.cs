using TNet;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("TNet/Sync Rigidbody")]
public class TNSyncRigidbody : TNBehaviour
{
    public int updatesPerSecond = 10;

    public bool isImportant;

    private Transform mTrans;

    private Rigidbody mRb;

    private float mNext;

    private bool mWasSleeping;

    private Vector3 mLastPos;

    private Vector3 mLastRot;

    private void Awake()
    {
        mTrans = base.transform;
        mRb = GetComponent<Rigidbody>();
        mLastPos = mTrans.position;
        mLastRot = mTrans.rotation.eulerAngles;
        UpdateInterval();
    }

    private void UpdateInterval()
    {
        mNext = Time.time + ((updatesPerSecond <= 0) ? 0f : (1f / (float)updatesPerSecond));
    }

    private void FixedUpdate()
    {
        if (updatesPerSecond <= 0 || !(mNext < Time.time) || !base.tno.isMine || !TNManager.isInChannel)
        {
            return;
        }
        bool flag = mRb.IsSleeping();
        if (flag && mWasSleeping)
        {
            return;
        }
        UpdateInterval();
        Vector3 position = mTrans.position;
        Vector3 eulerAngles = mTrans.rotation.eulerAngles;
        if (mWasSleeping || position != mLastPos || eulerAngles != mLastRot)
        {
            mLastPos = position;
            mLastRot = eulerAngles;
            if (isImportant)
            {
                base.tno.Send(1, Target.OthersSaved, position, eulerAngles, mRb.velocity, mRb.angularVelocity);
            }
            else
            {
                base.tno.SendQuickly(1, Target.OthersSaved, position, eulerAngles, mRb.velocity, mRb.angularVelocity);
            }
        }
        mWasSleeping = flag;
    }

    [RFC(1)]
    private void OnSync(Vector3 pos, Vector3 rot, Vector3 vel, Vector3 ang)
    {
        mTrans.position = pos;
        mTrans.rotation = Quaternion.Euler(rot);
        mRb.velocity = vel;
        mRb.angularVelocity = ang;
        UpdateInterval();
    }

    private void OnCollisionEnter()
    {
        if (TNManager.isHosting)
        {
            Sync();
        }
    }

    public void Sync()
    {
        if (TNManager.isInChannel)
        {
            UpdateInterval();
            mWasSleeping = false;
            mLastPos = mTrans.position;
            mLastRot = mTrans.rotation.eulerAngles;
            base.tno.Send(1, Target.OthersSaved, mLastPos, mLastRot, mRb.velocity, mRb.angularVelocity);
        }
    }
}
