using System.Collections;
using UnityEngine;

namespace NamruVarjoUtilities
{
    public class NVU_Player : MonoBehaviour
    {
        [Header("REFERENCE - INTERNAL")]
        [SerializeField] private MeshRenderer[] meshRenderers_toDisable;
        [SerializeField] private Transform _trans_RigOffset;


        [Header("UTILITY")]
        [SerializeField, Tooltip("This is for playing the game in the Unity editor without a headset connected")] 
        private bool autoRepositionOnStart = true;
        [SerializeField] private Vector3 v_offsetRepositionOnStart = Vector3.zero;
        [SerializeField] private Quaternion q_offsetRepositionOnStart = Quaternion.identity;

        void Start()
        {
            if( meshRenderers_toDisable != null && meshRenderers_toDisable.Length > 0 )
            {
                for ( int i = 0; i < meshRenderers_toDisable.Length; i++ )
                {
                    meshRenderers_toDisable[i].enabled = false;
                }
            }

            if ( autoRepositionOnStart )
            {
                StartCoroutine( RepositionOffset() ); //Doing a coroutine in order to guarantee this happens after the initial positioning of the xr rig happens
            }
        }

        public void IncreaseOffsetByAmount( float amount )
        {
            _trans_RigOffset.localPosition += Vector3.up * amount;
        }

        public void DecreaseOffsetByAmount(float amount)
        {
            _trans_RigOffset.localPosition += Vector3.down * amount;
        }

        [ContextMenu("z call CaptureRepositionOnStartValues()")]
        public void CaptureRepositionOnStartValues()
        {
            v_offsetRepositionOnStart = _trans_RigOffset.localPosition;
            q_offsetRepositionOnStart = _trans_RigOffset.localRotation;
        }

        [ContextMenu("z call RepositionOffset()")]
        public IEnumerator RepositionOffset()
        {
            yield return new WaitForSeconds(0.5f);
            _trans_RigOffset.localPosition = v_offsetRepositionOnStart;
            _trans_RigOffset.localRotation = q_offsetRepositionOnStart;
        }
    }
}
