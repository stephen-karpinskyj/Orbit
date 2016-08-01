using System;
using UnityEngine;
using UnityEngine.SceneManagement;

using Random = UnityEngine.Random;

public class Controller : MonoBehaviour
{
    [Serializable]
    private struct ColourPair
    {
        public Color BackgroundColour;
        public Color ForegroundColour;
    }

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float angularSpeed = 2f;

    [SerializeField]
    private ColourPair[] colours;

    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private TrailRenderer trailRenderer;

    private bool isTurning;

    private void Awake()
    {
        this.InitFramerate();
    }

    private void Start()
    {
        this.UpdateColours();
    }

    private void Update()
    {
        this.CheckInput();
    }

    private void FixedUpdate()
    {
        this.CheckInput();

        if (this.isTurning)
        {
            this.transform.localRotation *= Quaternion.AngleAxis(this.angularSpeed * Time.fixedDeltaTime, Vector3.up);
        }

        this.transform.localPosition += this.transform.forward * Time.fixedDeltaTime * this.speed;
    }

    private void CheckInput()
    {
        this.isTurning = Input.anyKey;
    }

    private void InitFramerate()
    {
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS", 30);
    }

    private void ToggleFramerate()
    {
        var fps = PlayerPrefs.GetInt("FPS", 30);
        var newFps = fps == 30 ? 60 : 30;
        Application.targetFrameRate = newFps;
        PlayerPrefs.SetInt("FPS", newFps);

        Debug.Log("FPS set to " + newFps);
    }

    private void UpdateColours()
    {
        var newColours = this.colours[Random.Range(0, this.colours.Length)];

        this.mainCam.backgroundColor = newColours.BackgroundColour;

        this.trailRenderer.material.color = newColours.ForegroundColour;
        this.spriteRenderer.material.color = newColours.ForegroundColour;
    }

    public void UGUI_OnResetButtonPress()
    {
        SceneManager.LoadScene(0);
    }

    public void UGUI_OnFPSToggleButtonPress()
    {
        this.ToggleFramerate();
    }
}
