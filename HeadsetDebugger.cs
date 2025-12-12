using TMPro;
using UnityEngine;
using UnityEngine.XR;


namespace NamruVarjoUtilities
{
    public class HeadsetDebugger : MonoBehaviour
    {
        //[Header("SETTINGS")]
        //[SerializeField] private 

        private bool amActive = false;
        public bool AmActive => amActive;

        [Header("REFERENCE [INTERNAL]")]
        private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI tmp_headset;
        [SerializeField] private TextMeshProUGUI tmp_leftController;
        [SerializeField] private TextMeshProUGUI tmp_rightController;


        [Header("REFERENCE [EXTERNAL]")]
        [SerializeField] private NVU_Controller _leftController;
        [SerializeField] private NVU_Controller _rightController;

        [SerializeField] private Transform _trans_cameraOffset;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        void Start()
        {
            TurnOff();

            tmp_leftController.text = "";
            tmp_rightController.text = "";
            tmp_headset.text = "";
        }

        void Update()
        {
            if (!AmActive)
            {
                return;
            }

            if ( !_leftController.AmValid )
            {
                tmp_leftController.text = $"device not valid";
            }
            else
            {
                tmp_leftController.text = _leftController.GetControllerText();

            }


            if ( !_rightController.AmValid )
            {
                tmp_rightController.text = $"device not valid\n";
            }
            else
            {
                tmp_rightController.text = "";
            }
            tmp_rightController.text += _rightController.GetControllerText();

        }

        public void TurnOn()
        {
            _canvas.enabled = true;
            amActive = true;
        }

        public void TurnOff()
        {
            _canvas.enabled = false;
            amActive = false;
        }

        public void ToggleMe()
        {
            if (amActive)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {

        }
#endif
    }
}