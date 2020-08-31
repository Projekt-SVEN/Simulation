using Assets.Scripts;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject _playerInterface;

    [SerializeField]
    private GameObject _menu;

    private bool _menuopen = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_menuopen)
            {
                _playerInterface.SetActive(true);
                _menu.SetActive(false);
            }
            else
            {
                _playerInterface.SetActive(false);
                _menu.SetActive(true);
            }
            _menuopen = !_menuopen;
        }
        else if (!_menuopen)
        {
            Vector2 translation = new Vector2(
                x: Input.GetAxis("Horizontal"),
                y: Input.GetAxis("Vertical"));

            float mouseInput = Input.GetAxis("Mouse ScrollWheel");

            Camera.main.fieldOfView -= mouseInput * OptionsManager.CameraZoomSpeed;

            transform.Translate(translation * OptionsManager.MovementSpeed * Time.deltaTime);
        }
    }

    public void EnabelMovement()
    {
        _menuopen = false;
    }
}
