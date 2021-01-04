using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV5
{
    public abstract class LaneChangingModel : ScriptableObject
    {
        [SerializeField]
        protected float bSafe; //safe deceleration [m/s^2] at maximum speed v=v0

        [SerializeField]
        protected float bSafeMax; //safe deceleration[m / s ^ 2]  at speed zero(gen.higher)

        [SerializeField]
        protected float p; //poliness (-0.2 to 1) : default 0.1

        [SerializeField]
        protected float bThr; //lane-changing threshold [m/s^2]

        [SerializeField]
        protected float bBiasRight; //bias [m/s^2] to the right

        public LaneChangingModel(float _bSafe, float _bSafeMax, float _p, float _bThr, float _bBiasRight)
        {
            bSafe = _bSafe;
            bSafeMax = _bSafeMax;
            p = _p;
            bThr = _bThr;
            bBiasRight = _bBiasRight;
        }

        public abstract bool RealizeLaneChange(float vrel, float acc, float accNew, float accLagNew, bool toRight);
        public abstract bool RespectPriority(float accLag, float accLagNew);
    }
}