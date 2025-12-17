using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using Varjo.XR;

namespace NamruVarjoUtilities
{
    public class NVU_Controller : MonoBehaviour
    {
        [Header("Select hand")]
        public XRNode XRNode = XRNode.LeftHand;

        [Header("Controllers parts")]
        public GameObject controller;
        public GameObject bodyGameobject;
        public GameObject touchPadGameobject;
        public GameObject menubuttonGameobject;
        public GameObject triggerGameobject;
        public GameObject systemButtonGameobject;
        public GameObject gripButtonGameobject;

        [Header("Controller material")]
        public Material controllerMaterial;

        [Header("Controller button highlight material")]
        public Material buttonPressedMaterial;
        public Material touchpadTouchedMaterial;

        [Header("Visible only for debugging")]
        public bool DBG_isVAlid;
        public bool gripButton;
        public bool primary2DAxisTouch;
        public bool primary2DAxisClick;
        public bool primaryButton;

        private List<InputDevice> devices = new List<InputDevice>();
        private InputDevice device;

        private Quaternion deviceRotation; //Controller rotation
        private Vector3 devicePosition; //Controller position
        private Vector3 deviceAngularVelocity; // Controller angular velocity
        private Vector3 deviceVelocity; // Controller velocity

        #region CONTROLS ===============================================================

        #region TRIGGER INPUT --------------------------------------------
        public bool triggerButton;

        public float trigger;

        private Vector3 triggerRotation; // Controller trigger rotation
        public bool TriggerButton { get { return triggerButton; } }
        public UnityEvent Event_TriggerClicked = new UnityEvent(); //logan added
        private bool flag_triggerEventHasBeenInvokedForThisPress = false; // logan-added

        #endregion

        public bool AmValid { get { return device.isValid; } }
        public bool DeviceIsNull { get { return device == null; } } //Logan added


        public bool GripButton { get { return gripButton; } }


        #region PRIMARY AXIS ----------------------------------------------------------------
        public bool Primary2DAxisTouch { get { return primary2DAxisTouch; } }

        public bool Primary2DAxisClick { get { return primary2DAxisClick; } }

        //Logan added-------------------------------------------
        [SerializeField] private bool flag_primaryAxisClickedThisFrame;
        private Vector2 val_primaryAxis;
        public Vector2 Val_primaryAxis => val_primaryAxis;

        [Tooltip("Point where primary 2d axis value is considered aligned with a direction (higher is more strict)"), SerializeField, Range(0f, 1f)] 
        private float primAxisClickThreshold = 0.6f;
        public bool PrimaryAxisDir_Up { get { return primary2DAxisClick && val_primaryAxis.y > primAxisClickThreshold; } }
        public bool PrimaryAxisDir_Down { get { return primary2DAxisClick && val_primaryAxis.y < -primAxisClickThreshold; } }
        public bool PrimaryAxisDir_Right { get { return primary2DAxisClick && val_primaryAxis.x > primAxisClickThreshold; } }
        public bool PrimaryAxisDir_Left { get { return primary2DAxisClick && val_primaryAxis.x < -primAxisClickThreshold; } }

        public UnityEvent Event_PrimaryAxisClicked = new UnityEvent();
        // end logan added--------------------------------------

        #endregion

        public bool PrimaryButton { get { return primaryButton; } }

        public Vector3 DeviceVelocity { get { return deviceVelocity; } }

        public Vector3 DeviceAngularVelocity { get { return deviceAngularVelocity; } }

        public float Trigger { get { return trigger; } }

        #endregion

        [Header("REFERENCE (EXTERNAL)")]
        [SerializeField] private NVU_SelectPointer _selectPointer;
        public NVU_SelectPointer SelectPointer => _selectPointer;


        protected virtual void OnEnable()
        {
            //Event_PrimaryAxisClicked = new UnityEvent();

            if (!device.isValid)
            {
                GetDevice();
            }
        }

        protected virtual void OnDisable()
        {
            //Event_PrimaryAxisClicked.RemoveAllListeners(); //I think this may take away it's functionality given through the inspector...
        }

        protected virtual void Start()
        {
            flag_primaryAxisClickedThisFrame = false;
            flag_triggerEventHasBeenInvokedForThisPress = false;
        }

        protected virtual void Update()
        {
            if (!device.isValid)
            {
                GetDevice();
            }

            DBG_isVAlid = device.isValid;

            // Get values for device position, rotation and buttons.
            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out devicePosition))
            {
                transform.localPosition = devicePosition;
            }

            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out deviceRotation))
            {
                transform.localRotation = deviceRotation;
            }

            if (device.TryGetFeatureValue(CommonUsages.trigger, out trigger))
            {
                UpdateControllerVisual();
            }

            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButton))
            {
                UpdateControllerVisual();
            }

            if ( triggerButton )
            {
                if ( !flag_triggerEventHasBeenInvokedForThisPress )
                {
                    flag_triggerEventHasBeenInvokedForThisPress = true;

                    Event_TriggerClicked.Invoke();

                    if( _selectPointer.AmHot && 
                        _selectPointer.ControllerClickInput == NVU_ButtonType.Trigger &&
                        _selectPointer.CurrentHit.transform != null
                    )
                    {
                        _selectPointer.TryClickCurrentTarget();
                    }
                }
            }
            else
            {
                flag_triggerEventHasBeenInvokedForThisPress = false;
            }

            if (device.TryGetFeatureValue(CommonUsages.gripButton, out gripButton))
            {
                UpdateControllerVisual();
            }

            if (device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out primary2DAxisTouch))
            {
                UpdateControllerVisual();
            }

            if ( device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out primary2DAxisClick) )
            {
                UpdateControllerVisual();
            }

            if ( primary2DAxisClick )
            {
                if ( !flag_primaryAxisClickedThisFrame )
                {
                    flag_primaryAxisClickedThisFrame = true;

                    Event_PrimaryAxisClicked.Invoke();
                }
            }
            else
            {
                flag_primaryAxisClickedThisFrame = false;
            }

            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton))
            {
                UpdateControllerVisual();
            }

            //Logan added...
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out val_primaryAxis))
            {
                UpdateControllerVisual();
            }

            device.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out deviceAngularVelocity);

            device.TryGetFeatureValue(CommonUsages.deviceVelocity, out deviceVelocity);
        }

        void GetDevice()
        {
            InputDevices.GetDevicesAtXRNode(XRNode, devices);
            device = devices.FirstOrDefault();
        }

        void UpdateControllerVisual()
        {
            //Set trigger rotation from input
            triggerRotation.Set(trigger * -30f, 0, 0);
            triggerGameobject.transform.localRotation = Quaternion.Euler(triggerRotation);

            //Set controller button inputs
            if (!triggerButton)
            {
                triggerGameobject.GetComponent<MeshRenderer>().material = controllerMaterial;
            }
            else
            {
                triggerGameobject.GetComponent<MeshRenderer>().material = buttonPressedMaterial;
            }

            if (!gripButton)
            {
                gripButtonGameobject.GetComponent<MeshRenderer>().material = controllerMaterial;
            }
            else
            {
                gripButtonGameobject.GetComponent<MeshRenderer>().material = buttonPressedMaterial;
            }

            if (!primary2DAxisTouch)
            {
                touchPadGameobject.GetComponent<MeshRenderer>().material = controllerMaterial;
            }
            else if (primary2DAxisTouch && primary2DAxisClick)
            {
                touchPadGameobject.GetComponent<MeshRenderer>().material = buttonPressedMaterial;
            }
            else if (primary2DAxisTouch)
            {
                touchPadGameobject.GetComponent<MeshRenderer>().material = touchpadTouchedMaterial;
            }

            if (!primaryButton)
            {
                menubuttonGameobject.GetComponent<MeshRenderer>().material = controllerMaterial;
            }
            else
            {
                menubuttonGameobject.GetComponent<MeshRenderer>().material = buttonPressedMaterial;
            }
        }

        public string GetControllerText()
        {
            return
                $"{nameof(devicePosition)}: '{devicePosition}'\n" +
                $"{nameof(deviceRotation)}: '{deviceRotation}'\n" +

                $"<b>Primary Axis--------------</b>\n" +
                $"{nameof(val_primaryAxis)}: '{val_primaryAxis}'\n" +
                $"{nameof(Primary2DAxisTouch)}: '{Primary2DAxisTouch}'\n" +
                $"{nameof(Primary2DAxisClick)}: '{Primary2DAxisClick}'\n" +

                $"<b>Trigger-----------------------</b>\n" +
                $"{nameof(TriggerButton)}: '{TriggerButton}'\n" +
                $"{nameof(Trigger)}: '{Trigger}'\n" +

                $"<b>Trigger-----------------------</b>\n" +
                $"{nameof(primaryButton)}: '{primaryButton}'\n" +

                $"";
        }
    }
}