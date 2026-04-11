using UnityEngine;

public abstract class Pilot : MonoBehaviour
{
    //===========================================
    // struct/enum
    //===========================================
    [System.Serializable]
    public struct PilotInfo
    {
        public int team;
        public bool invincible;
        public bool tgt;

    }

    //===========================================
    // Methods
    //===========================================
    public abstract void Attach(Vehicle target);
    public abstract void SetLeaderSystem(LeaderSystem leaderSystem);
    public abstract void Release();

    //===========================================
    // Variable & GetSet Methods
    //===========================================
    public void SetInfomation(PilotInfo _infomation) { infomation = _infomation; }
    public int Team { get { return infomation.team; } private set { infomation.team = value; } }

    [SerializeField] protected PilotInfo infomation;
}
