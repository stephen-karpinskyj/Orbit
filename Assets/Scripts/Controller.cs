using System;
using UnityEngine;
using UnityEngine.SceneManagement;

using Random = UnityEngine.Random;
using System.Collections;

public class Controller : MonoBehaviour
{
    private enum Direction
    {
        CW = 0,
        CCW,
    }

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
    private Direction initialOrbitDirection = Direction.CW;

    [SerializeField]
    private Transform core;

    [SerializeField]
    private Transform cwOrbit;

    [SerializeField]
    private Transform ccwOrbit;

    [SerializeField]
    private ColourPair[] colours;

    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private TrailRenderer trailRenderer;

    [SerializeField]
    private Transform point;

    [SerializeField]
    private Vector2 pointBounds;

    [SerializeField]
    private float toOrbitDeltaSpeed = 5f;

    [SerializeField]
    private float fromOrbitDeltaSpeed = -20f;

    private bool isOrbiting;
    private Direction orbitDirection;
    private float percentOrbit;

    private void Awake()
    {
        this.InitFramerate();

        this.orbitDirection = this.initialOrbitDirection;
    }

    private void Start()
    {
        this.CheckInput();

        this.GenerateColours();
        this.GeneratePoint();

        this.StartCoroutine(this.RandomisePointCoroutine());
    }
    
    private void Update()
    {
        this.CheckInput();
    }
    
    private void FixedUpdate()
    {
        this.CheckInput();

        this.UpdatePercentOrbit(this.isOrbiting);

        this.UpdateOrbit(this.orbitDirection);
        this.UpdateForward();
    }
    
    private IEnumerator RandomisePointCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            this.GeneratePoint();
        }
    }
    
    private void UpdatePercentOrbit(bool increase)
    {
        var mag = increase ? this.toOrbitDeltaSpeed : this.fromOrbitDeltaSpeed;
        var delta = mag * Time.fixedDeltaTime;
        this.percentOrbit = Mathf.Clamp01(this.percentOrbit + delta);
    }

    private void UpdateOrbit(Direction direction)
    {
        var orbitPoint = direction == Direction.CW ? this.cwOrbit.position : this.ccwOrbit.position;
        var orbitAmount = this.angularSpeed * Time.fixedDeltaTime * this.percentOrbit;

        if (direction == Direction.CCW)
        {
            orbitAmount *= -1;
        }

        this.core.RotateAround(orbitPoint, Vector3.forward, orbitAmount);
    }

    private void UpdateForward()
    {
        this.core.localPosition += this.core.forward * Time.fixedDeltaTime * this.speed * (1 - this.percentOrbit);
    }

    private void CheckInput()
    {
        var wasOrbiting = this.isOrbiting;
        this.isOrbiting = Input.anyKey;

        if (this.isOrbiting && !wasOrbiting)
        {
            this.OnStartOrbit();
        }
    }

    private void OnStartOrbit()
    {
        var right = this.core.right;
        var to = this.point.localPosition - this.core.localPosition;
        var dot = Vector3.Dot(right, to);

        this.orbitDirection = dot > 0f ? Direction.CCW : Direction.CW;
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

    private void GenerateColours()
    {
        var newColours = this.colours[Random.Range(0, this.colours.Length)];

        this.mainCam.backgroundColor = newColours.BackgroundColour;

        this.trailRenderer.material.color = newColours.ForegroundColour;
        this.spriteRenderer.material.color = newColours.ForegroundColour;
    }

    private void GeneratePoint()
    {
        this.point.localPosition = new Vector2(Random.Range(-pointBounds.x, pointBounds.x), Random.Range(-pointBounds.y, pointBounds.y));
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
