using UnityEngine;
namespace CivilFX.TrafficV5 {
    public abstract class CarFollowingModel : ScriptableObject
    {
        [SerializeField]
        public float v0; //desired speed [m/s]
        [SerializeField]
        public float T; //desired time gap [s]
        [SerializeField]
        protected float s0; //minimum gap [m]
        [SerializeField]
        protected float a; //minimum acceleration [m/s^2]
        [SerializeField]
        protected float b; //comfortable deceleration [m/s^2]
        [SerializeField]
        protected int alpha_v0; //multiplicator for temporary reduction
        [SerializeField]
        public float speedLimit; //if effective speed limits, speedlimit < v0
        [SerializeField]
        protected float speedMax; //if vehicle restricts speed, speedmax < speedlimit, v0
        [SerializeField]
        public int bMax;

        public float defaultAcc {
            get { return a; }
        }
        public CarFollowingModel(float _v0, float _T, float _s0, float _a, float _b)
        {
            v0 = _v0;
            T = _T;
            s0 = _s0;
            a = _a;
            b = _b;
            alpha_v0 = 1;
            speedLimit = 1000;
            speedMax = 1000;
        }

        public abstract float CalculateAcceleration(float s, float v, float vl, float al);
        public abstract float CalculateAccGiveWay(float sYield, float sPrio, float v, float vPrio, float accOld);
        public abstract CarFollowingModel Clone();
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("v0 ");
            sb.Append(v0);


            return sb.ToString();
        }
    }
}