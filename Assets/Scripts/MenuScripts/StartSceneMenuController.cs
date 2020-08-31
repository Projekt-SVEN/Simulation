using System.Collections.Generic;
using Assets.Scripts.InputValidation;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    //WET says hello
    public class StartSceneMenuController: MonoBehaviour
    {
        [SerializeField]
        private Color _normalColor = Color.white;

        [SerializeField]
        private Color _errorColor = Color.red;

        [SerializeField]
        private GameObject _mainMenu;

        [SerializeField]
        private GameObject _optionMenu;

        [SerializeField]
        private GameObject _simulationMenu;

        [SerializeField]
        private InputField _roomFile;

        [SerializeField]
        private InputField _usernameInputField;

        [SerializeField]
        private InputField _passwordInputField;

        [SerializeField]
        private InputField _serverAddressInputField;

        [SerializeField]
        private Toggle _requiresAuthenticationToggle;

        [SerializeField]
        private InputField _userCountInputField;

        [SerializeField]
        private Slider _movementSpeed;

        [SerializeField]
        private Slider _zoomSpeed;

        private InputFieldInputChecker<float> CreateScaleInputChecker(InputField inputField, float initialValue)
        {
            return new InputFieldInputChecker<float>(
                inputField: inputField,
                initialValue: initialValue,
                normalColor: _normalColor,
                errorColor: _errorColor,
                inputParser: OptionsManager.ParseFloat,
                validator: OptionsManager.IsValidSize);
        }

        private InputFieldInputChecker<float> CreateTemperatureInputChecker(InputField inputField, float initialValue)
        {
            return new InputFieldInputChecker<float>(
                inputField: inputField,
                initialValue: initialValue,
                normalColor: _normalColor,
                errorColor: _errorColor,
                inputParser: OptionsManager.ParseFloat,
                validator: OptionsManager.IsValidTemperature);
        }

        private InputFieldInputChecker<string> CreateAnyStringInputChecker(InputField inputField, string initialValue)
        {
            return new InputFieldInputChecker<string>(
                inputField: inputField,
                initialValue: initialValue,
                normalColor: _normalColor,
                errorColor: _errorColor,
                inputParser: value => value,
                validator: value => true);
        }

        private InputFieldInputChecker<int?> CreatePositiveIntInputChecker(InputField inputField, int initialValue)
        {
            return new InputFieldInputChecker<int?>(
                inputField: inputField,
                initialValue: initialValue,
                normalColor: _normalColor,
                errorColor: _errorColor,
                inputParser: OptionsManager.ParseInt,
                validator: value => value.HasValue && value.Value > 0);
        }

        private Dictionary<string, IInputChecker> _inputChecker = null;
        
        void Awake()
        {
            _inputChecker = new Dictionary<string, IInputChecker>()
            {
                { OptionsNames.ROOM_FILE, CreateAnyStringInputChecker(_roomFile, OptionsManager.RoomFile) },
                { OptionsNames.USERNAME, CreateAnyStringInputChecker(_usernameInputField, OptionsManager.Username) },
                { OptionsNames.PASSWORD, CreateAnyStringInputChecker(_passwordInputField, OptionsManager.Password) },
                { OptionsNames.SERVER_ADDRESS, CreateAnyStringInputChecker(_serverAddressInputField, OptionsManager.ServerAddress) },
                { OptionsNames.USER_COUNT, CreatePositiveIntInputChecker(_userCountInputField, OptionsManager.UserCount) },
            };

            _requiresAuthenticationToggle.isOn = OptionsManager.RequiresAuthentication;
            _zoomSpeed.value = OptionsManager.CameraZoomSpeed;
            _movementSpeed.value = OptionsManager.MovementSpeed;
        }

        private bool ValidateAndSaveOptions()
        {
            foreach (IInputChecker inputChecker in _inputChecker.Values)
            {
                if (!inputChecker.IsValid)
                    return false;
            }

            OptionsManager.RoomFile = _inputChecker[OptionsNames.ROOM_FILE].GetValueAs<string>();

            OptionsManager.Username = _inputChecker[OptionsNames.USERNAME].GetValueAs<string>();
            OptionsManager.Password = _inputChecker[OptionsNames.PASSWORD].GetValueAs<string>();
            OptionsManager.ServerAddress = _inputChecker[OptionsNames.SERVER_ADDRESS].GetValueAs<string>();
            OptionsManager.RequiresAuthentication = _requiresAuthenticationToggle.isOn;

            OptionsManager.UserCount = _inputChecker[OptionsNames.USER_COUNT].GetValueAs<int>();

            return true;
        }

        public void MainMenu_StartSimulationButton_OnClick()
        {
            _mainMenu.SetActive(false);
            _simulationMenu.SetActive(true);
        }

        public void MainMenu_OptionsButton_OnClick()
        {
            _mainMenu.SetActive(false);
            _optionMenu.SetActive(true);
        }

        public void MainMenu_QuitButton_OnClick()
        {
            Application.Quit();
        }

        public void SimulationMenu_StartSimulationButton_OnClick()
        {
            if (ValidateAndSaveOptions())
            {
                SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            }
        }

        public void SimulationMenu_BackButton_OnClick()
        {
            if (ValidateAndSaveOptions())
            {
                _simulationMenu.SetActive(false);
                _mainMenu.SetActive(true);
            }
        }

        public void OptionsMenu_BackButton_OnClick()
        {
            _optionMenu.SetActive(false);
            _mainMenu.SetActive(true);
        }

        public void MovementSpeed_Slider(Slider slider)
        {
            OptionsManager.MovementSpeed = slider.value;
        }

        public void ZoomSpeed_Slider(Slider slider)
        {
            OptionsManager.CameraZoomSpeed = slider.value;
        }

        public void OpenFileDialogOnClick()
        {
            FileBrowser.SetFilters(false, new FileBrowser.Filter("XML-Schema", ".xml"));
            FileBrowser.OnSuccess onSuccess = new FileBrowser.OnSuccess(SavePath);
            FileBrowser.ShowLoadDialog(onSuccess, null);
        }

        private void SavePath(string[] paths)
        {
            if(paths.GetLength(0) > 0)
            {
                _roomFile.text = paths[0];
                _inputChecker[OptionsNames.ROOM_FILE].ForceUpdate();
            }
        }

    }
}