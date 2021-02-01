
using UnityEngine;

namespace CivilFX.UI2
{
    [CreateAssetMenu(fileName ="CheveronSequnce", menuName = "CivilFX/UI2/CheveronSequnce")]
    public class CheveronSequence : ScriptableObject
    {
        public Material material;
        public Texture2D[] sprites;
    }
}