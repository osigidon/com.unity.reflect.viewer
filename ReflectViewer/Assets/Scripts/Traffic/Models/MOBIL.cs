using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV5
{
    [CreateAssetMenu(menuName = "CivilFX/TrafficV3/Models/LaneChanging/MOBIL", fileName = "New MOBIL")]
    public class MOBIL : LaneChangingModel
    {       
        public MOBIL() : base (4f, 20f, 0.1f, 0.2f, 0.3f)
        {

        }

        public MOBIL(float _bSafe, float _bSafeMax, float _p, float _bThr, float _bBiasRight) : base (_bSafe, _bSafeMax, _p, _bThr, _bBiasRight)
        {

        }


        public void SetModel(float _bSafe, float _bSafeMax, float _p, float _bThr, float _bBiasRight= 0.05f)
        {
            bSafe = _bSafe;
            bSafeMax = _bSafeMax;
            p = _p;
            bThr = _bThr;
            bBiasRight = _bBiasRight;
        }

        /// <summary>
        /// Generalized MOBIL lane changing decision
        /// with bSafe increasing with decrease vrel=v/v0
        /// </summary>
        /// <param name="vrel">increase bSafe with decreasing vrel</param>
        /// <param name="acc">own acceleration at old lane</param>
        /// <param name="accNew">projected own acceleration at new lane</param>
        /// <param name="accLagNew">projected acceleration of new leader</param>
        /// <param name="toRight">true == to right ; false == to left</param>
        /// <returns>true if lane changing is posible</returns>
        public override bool RealizeLaneChange(float vrel, float acc, float accNew, float accLagNew, bool toRight)
        {
            var signRight = (toRight) ? 1 : -1;
            // safety criterion

            var bSafeActual = vrel * bSafe + (1 - vrel) * bSafeMax;
            //if(accLagNew<-bSafeActual){return false;} //!! <jun19
            //if((accLagNew<-bSafeActual)&&(signRight*this.bBiasRight<41)){return false;}//!!! override safety criterion to really enforce overtaking ban OPTIMIZE
            if (signRight * bBiasRight > 40) {
                return true;
            }

            if (accLagNew < Mathf.Min(-bSafeActual, -Mathf.Abs(bBiasRight))) {
                return false;
            }//!!!

            // incentive criterion
            var dacc = accNew - acc + p * accLagNew //!! new
            + bBiasRight * signRight - bThr;

            // hard-prohibit LC against bias if |bias|>9 m/s^2
            if (bBiasRight * signRight < -9) {
                dacc = -1;
            }
            return (dacc > 0);
        }

        public override bool RespectPriority(float accLag, float accLagNew)
        {
            return (accLag - accLagNew > 0.1f);
        }
    }
}