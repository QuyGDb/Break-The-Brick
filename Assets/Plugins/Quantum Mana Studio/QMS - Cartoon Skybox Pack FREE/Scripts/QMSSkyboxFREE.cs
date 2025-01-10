using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

namespace QMSSkyboxFREE
{
    public class QMSSkyboxFREE : MonoBehaviour
    {
        public List<Material> skyboxMaterials;
        private int currentIndex = 0;

        public TextMeshProUGUI skyboxIndex;
        public TextMeshProUGUI skyboxNameText;
        public TextMeshProUGUI helpText;

        // Start is called before the first frame update
        void Start()
        {
            UpdateSkybox(currentIndex);
            UpdateUI();
        }

        // Update is called once per frame
        void Update()
        {
            HandleKeyboardInput();
            HandleMouseCameraRotation();
        }

        void UpdateSkybox(int index)
        {
            if (skyboxMaterials != null && skyboxMaterials.Count > 0)
            {
                RenderSettings.skybox = skyboxMaterials[index];
                DynamicGI.UpdateEnvironment();
            }
        }

        public void SwitchSkybox(int direction)
        {
            currentIndex = Mathf.Clamp(currentIndex + direction, 0, skyboxMaterials.Count - 1);
            UpdateSkybox(currentIndex);
            UpdateUI();
        }

        public void SwitchFirstOrLastSkybox(int direction)
        {
            if (direction == -1)
                currentIndex = 0;
            else if (direction == 1)
                currentIndex = skyboxMaterials.Count - 1;
            UpdateSkybox(currentIndex);
            UpdateUI();
        }

        void UpdateUI()
        {
            string materialName = skyboxMaterials[currentIndex].name;
            skyboxIndex.text = (currentIndex + 1).ToString() + "/" + skyboxMaterials.Count.ToString();
            skyboxNameText.text = materialName;
        }

        void HandleKeyboardInput()
        {
            if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
                SwitchSkybox(-1);

            if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
                SwitchSkybox(1);

            if (Input.GetKeyUp(KeyCode.Home))
                SwitchFirstOrLastSkybox(-1);

            if (Input.GetKeyUp(KeyCode.End))
                SwitchFirstOrLastSkybox(1);

            if (Input.GetKeyUp(KeyCode.Tab) || Input.GetKeyUp(KeyCode.Space))
                helpText.enabled = !helpText.enabled;
        }

        void HandleMouseCameraRotation()
        {
            // Lock the cursor to the center of the screen and hide it
            if (Input.GetKeyDown(KeyCode.Escape))  // Optional: allow escape to unlock the cursor
            {
                Cursor.lockState = CursorLockMode.None; // Unlock the cursor
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center
                Cursor.visible = false;                   // Hide the cursor
            }

            // Get the mouse movement inputs
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Get the current rotation of the camera
            Vector3 rotation = transform.eulerAngles;

            // Apply the mouse movement to rotate the camera
            rotation.x -= mouseY;  // Invert Y-axis for natural feel
            rotation.y += mouseX;  // Rotate horizontally

            // Apply the rotation to the camera
            transform.eulerAngles = new Vector3(rotation.x, rotation.y, 0f);
        }
    }
}