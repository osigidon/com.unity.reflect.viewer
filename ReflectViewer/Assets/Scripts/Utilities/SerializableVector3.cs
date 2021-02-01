//https://answers.unity.com/questions/956047/serialize-quaternion-or-vector3.html

using UnityEngine;

namespace CivilFX.Generic2 {

    [System.Serializable]
    public class SerializableVector3
    {

        /// <summary>
        /// x component
        /// </summary>
        public float x;

        /// <summary>
        /// y component
        /// </summary>
        public float y;

        /// <summary>
        /// z component
        /// </summary>
        public float z;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rX"></param>
        /// <param name="rY"></param>
        /// <param name="rZ"></param>
        public SerializableVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }

        public SerializableVector3(Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", x, y, z);
        }

        /// <summary>
        /// Automatic conversion from SerializableVector3 to Vector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator Vector3(SerializableVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }

        /// <summary>
        /// Automatic conversion from Vector3 to SerializableVector3
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public static implicit operator SerializableVector3(Vector3 rValue)
        {
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }

        public static Vector3[] SerializableVector3toVector3(SerializableVector3[] rValue)
        {
            if (rValue == null)
            {
                return null;
            }

            Vector3[] lValue = new Vector3[rValue.Length];
            for (int i=0; i<rValue.Length; i++)
            {
                lValue[i] = rValue[i];
            }
            return lValue;
        }
    }
}
