using System.Reflection;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NamruVarjoUtilities
{
    public class NVU_SelectPointer : MonoBehaviour
    {
        //[Header("STATE")]
        public NVU_ButtonType ControllerClickInput;


        [Header("OPTIONS")]
        
        [Tooltip("Enable this option if you want this pointer to just behave like 'default'.")] 
        public bool SyncAmHotWithLineRendererState = true;

        public float RaycastDistance = 2.5f;
        public LayerMask RaycastTargetMask;
        public string UIButtonTag = "";

        [Header("REFERENCE (INTERNAL)")]
        [SerializeField] private LineRenderer _lineRenderer;
        private Material _lineRendererMaterial;
        public Material LineRendererMaterlai
        {
            get { return _lineRendererMaterial; }
            set { _lineRendererMaterial = value; }
        }


        /// <summary>Whether this pointer is currently allowed to raycast and select. </summary>
        [HideInInspector] public bool AmHot = false;

        public RaycastHit CurrentHit;

        //[Header("PROPERTIES")]
        /// <summary>
        /// Used for outside classes like the NVU_Controller to determine if they should interact with this class 
        /// to do things like react to button presses.
        /// </summary>
        public bool AmValid
        {
            get
            {
                return _lineRenderer.enabled;
            }
        }


        //OTHER
        private float cachedLineLength = 2.5f;
        [SerializeField] private Button currentlySelectedButton;
        public Button CurrentlySelectedButton => currentlySelectedButton;

        public UnityEvent Event_ClickedTarget;

        [Header("DEBUG")]
        [SerializeField, TextArea(1,10)] private string dbg_Class;
        

        void Start()
        {
            _lineRendererMaterial = _lineRenderer.material;

            if( _lineRenderer.positionCount <= 0 )
            {
                Debug.LogError($"NVU ERROR! LineRenderer for Select pointer doesn't have any positions set. Positions" +
                    $"are necessary for determining line length at Start()");
            }
            if (_lineRenderer.positionCount != 2 )
            {
                Debug.LogError($"NVU ERROR! LineRenderer for Select pointer positions array length is '{_lineRenderer.positionCount}' " +
                    $"rather than 2. Positions count of 2 is necessary for determining line length at Start()");
            }
            else
            {
                cachedLineLength = Vector3.Distance( _lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1) );
            }

            if( _lineRenderer.enabled )
            {
                AmHot = true;
            }
            else
            {
                AmHot = false;
            }


            Debug.Log($"mask value: '{RaycastTargetMask.value}'"); 
        }

        void Update()
        {
            if( _lineRenderer.enabled )
            {
                Vector3[] pts = new Vector3[2] { transform.position, transform.position + transform.forward * cachedLineLength };

                _lineRenderer.SetPositions( pts );


            }

            bool somethingWasSelectedOnPrevFrame = CurrentHit.transform != null;
            RaycastHit prevHit = CurrentHit;
            CurrentHit = new RaycastHit();
            if (AmHot)
            {
                if (Physics.Raycast(transform.position, transform.forward, out CurrentHit, RaycastDistance, RaycastTargetMask))
                {
                    if ( !somethingWasSelectedOnPrevFrame )
                    {
                        SelectCurrentTarget_action();
                    }
                }
                else if (somethingWasSelectedOnPrevFrame)
                {
                    DeSelectCurrentTarget_action(prevHit.transform);
                }
            }
            else
            {
                if (currentlySelectedButton != null)
                {
                    currentlySelectedButton = null; //tust to be safe...
                }

                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    EventSystem.current.SetSelectedGameObject(null); //just to be safe...
                }

            }

#if UNITY_EDITOR
            dbg_Class = $"{nameof(cachedLineLength)}: '{cachedLineLength}'\n" +
                    $"{nameof(AmHot)}: '{AmHot}'\n" +
                    $"{nameof(CurrentHit)}: '{CurrentHit}'\n" +
                    $"{nameof(CurrentHit)} trans null: '{CurrentHit.transform == null}'\n" +
                    $"currently selected null: '{EventSystem.current.currentSelectedGameObject == null}'\n" +

                    $"";
#endif
        }

        public void ToggleLineRenderer( bool tgglOn )
        {
            if (tgglOn )
            {
                _lineRenderer.enabled = true;
 
            }
            else
            {
                _lineRenderer.enabled = false;
            }

            if (SyncAmHotWithLineRendererState)
            {
                AmHot = tgglOn;
            }
        }

        public void SetLineRendererLength(float len )
        {
            cachedLineLength = len;
        }

        public void TryInvoke()
        {
            Debug.Log("yo dawg");
        }

        public void SelectCurrentTarget_action()
        {
            Debug.Log("SelectCurrentTarget");

            if ( !string.IsNullOrEmpty(UIButtonTag) && CurrentHit.transform.CompareTag(UIButtonTag) )
            {
                if ( CurrentHit.transform.TryGetComponent<Button>(out currentlySelectedButton) )
                {
                    Debug.Log($"set currently selected button to: '{currentlySelectedButton}'");
                    EventSystem.current.SetSelectedGameObject(currentlySelectedButton.gameObject);
                }
            }
        }

        [ContextMenu("z call forceSelectCurretnButton()")]
        public void forceSelectCurretnButton()
        {
            TryClickCurrentTarget();
        }

        public void DeSelectCurrentTarget_action( Transform trans )
        {
            Debug.Log($"DeSelectCurrentTarget: '{trans.name}'");

            if ( !string.IsNullOrEmpty(UIButtonTag) )
            {
                if (currentlySelectedButton != null)
                {
                    //currentlySelectedButton..Invoke();
                    EventSystem.current.SetSelectedGameObject(null);
                    currentlySelectedButton = null;
                }
            }
        }

        public void TryClickCurrentTarget()
        {
            Debug.Log($"TryClickCurrentTarget()");

            Event_ClickedTarget.Invoke();

            if ( !string.IsNullOrEmpty(UIButtonTag) && currentlySelectedButton != null )
            {
                currentlySelectedButton.onClick.Invoke();
                
            }
        }
    }
}
