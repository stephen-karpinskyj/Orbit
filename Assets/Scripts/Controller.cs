using System;
using System.Collections;
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
        public Color HighlightColour;
    }

    [SerializeField]
    private float speed = 10f;
    
    [SerializeField]
    private float orbitRadius = 0.7f;

    [SerializeField]
    private ControllerPhysics physics;

    [SerializeField]
    private Transform coreTrans;

    [SerializeField]
    private ColourPair[] colours;

    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private SpriteRenderer[] overlaySpriteRenderers;

    [SerializeField]
    private SpriteRenderer cwSpriteRenderer;

    [SerializeField]
    private SpriteRenderer ccwSpriteRenderer;

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
    private Vector3 orbitOrigin;
    private Direction orbitDirection;
    private float percentOrbit;
    private ColourPair currentColour;

    private Vector2 prevPos;
    private bool isWallSliding;

    private static bool IsTapping
    {
        get { return Input.anyKey; }
    }

    private static bool WasTapping { get; set; }

    private bool hasTappedDownsWhileWallSliding;

    private void Awake()
    {
        this.InitFramerate();
    }

    private void Start()
    {
        this.GenerateColours();
        this.GeneratePoint();

        this.StartCoroutine(this.RandomisePointCoroutine());
    }

    private void Update()
    {
        var deltaTime = Time.smoothDeltaTime;
        
        this.CheckInput();

        if (this.hasTappedDownsWhileWallSliding && !IsTapping)
        {
            this.isWallSliding = false;
            this.OnStopWallSlide();
        }

        if (!this.isWallSliding)
        {
            this.UpdatePercentOrbit(this.isOrbiting, deltaTime);

            if (!this.isOrbiting)
            {
                this.UpdateOrbitDirection();
            }

            this.UpdateOrbitOrigin();
        }

        this.UpdateCoreTransform(deltaTime);

        //this.DEBUG_ValidateDistance(deltaTime);

        this.prevPos = this.coreTrans.position;
        WasTapping = IsTapping;
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return;
        }
        #endif
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.orbitOrigin, this.orbitRadius);
    }

    private IEnumerator RandomisePointCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            this.GeneratePoint();
        }
    }

    private void CheckInput()
    {
        var wasOrbiting = this.isOrbiting;
        this.isOrbiting = IsTapping;

        if (this.isOrbiting && !wasOrbiting)
        {
            this.OnStartOrbit();
        }
    }

    private void OnStartOrbit()
    {
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
        this.currentColour = this.colours[Random.Range(0, this.colours.Length)];

        this.mainCam.backgroundColor = this.currentColour.BackgroundColour;

        this.trailRenderer.material.color = this.currentColour.ForegroundColour;
        foreach (var s in this.overlaySpriteRenderers)
        {
           s.color = this.currentColour.ForegroundColour;
        }
        this.cwSpriteRenderer.color = this.currentColour.ForegroundColour;
        this.ccwSpriteRenderer.color = this.currentColour.ForegroundColour;
    }

    private void GeneratePoint()
    {
        this.point.localPosition = new Vector2(Random.Range(-pointBounds.x, pointBounds.x), Random.Range(-pointBounds.y, pointBounds.y));
    }

    private void UpdatePercentOrbit(bool increase, float deltaTime)
    {
        var mag = increase ? this.toOrbitDeltaSpeed : this.fromOrbitDeltaSpeed;
        var delta = mag * deltaTime;
        this.percentOrbit = Mathf.Clamp01(this.percentOrbit + delta);
    }

    private void UpdateOrbitDirection()
    {
        var right = this.coreTrans.right;
        var to = this.point.localPosition - this.coreTrans.localPosition;
        var dot = Vector3.Dot(right, to);

        this.orbitDirection = dot > 0f ? Direction.CW : Direction.CCW;

        this.cwSpriteRenderer.color = this.orbitDirection == Direction.CW ? this.currentColour.HighlightColour : this.currentColour.ForegroundColour;
        this.ccwSpriteRenderer.color = this.orbitDirection == Direction.CCW ? this.currentColour.HighlightColour : this.currentColour.ForegroundColour;
    }

    private void UpdateOrbitOrigin()
    {
        var v = this.coreTrans.up;
        var coreOrigin = (Vector2)this.coreTrans.position;
        var offset = this.orbitRadius;

        if (this.orbitDirection == Direction.CW)
        {
            offset *= -1;
        }

        // Based on: http://answers.unity3d.com/questions/564166/how-to-find-perpendicular-line-in-2d.html
        this.orbitOrigin = coreOrigin + new Vector2(-v.y, v.x) / Mathf.Sqrt(Mathf.Pow(v.x, 2) + Mathf.Pow(v.y, 2)) * offset;
    }

    private void UpdateCoreTransform(float deltaTime)
    {
        var initPos = (Vector2)this.coreTrans.localPosition;

        var posOffset = Vector2.zero;
        var angleOffset = 0f;

        if (this.isWallSliding)
        {
            var forwardDir = (Vector2)this.coreTrans.up;
            var forwardDist = deltaTime * this.speed;
            posOffset += forwardDir * forwardDist;

            if (IsTapping && !WasTapping)
            {
                this.hasTappedDownsWhileWallSliding = true;
            }
        }
        else
        {
            // Add orbit
            {
                var orbitDist = deltaTime * this.speed * this.percentOrbit;
                var orbitDegrees = MathUtility.CircleArcDistanceToAngleOffset(orbitDist, this.orbitRadius);

                if (this.orbitDirection == Direction.CW)
                {
                    orbitDegrees *= -1;
                }

                var orbitDest = initPos.RotateAround(this.orbitOrigin, orbitDegrees);
                var orbitPosOffset = (orbitDest - initPos);

                posOffset += orbitPosOffset;
                angleOffset += orbitDegrees;
            }

            // Add forward
            {
                var forwardDir = (Vector2)this.coreTrans.up;
                var forwardDist = deltaTime * this.speed * (1 - this.percentOrbit);
                var forwardPosOffset = forwardDir * forwardDist;

                posOffset += forwardPosOffset;
            }
        }

        var wasWallSliding = this.isWallSliding;
        this.physics.Step(posOffset.normalized, posOffset.magnitude, angleOffset, IsTapping, this.orbitDirection, ref this.isWallSliding);
        if (!wasWallSliding && this.isWallSliding)
        {
            this.OnStartWallSlide();
        }
    }

    private void OnStartWallSlide()
    {
        this.hasTappedDownsWhileWallSliding = false;
    }

    private void OnStopWallSlide()
    {
        this.percentOrbit = 0f;
    }

    private void DEBUG_ValidateDistance(float deltaTime)
    {
        const string FORMAT = "#.000";

        var targetDist = (deltaTime * this.speed).ToString(FORMAT);
        var travelledDist = Vector2.Distance(this.coreTrans.position, this.prevPos).ToString(FORMAT);

        if (Time.frameCount > 1 && travelledDist != targetDist)
        {
            Debug.LogError("Didn't travel the target distance: " + travelledDist + " / " + targetDist);
            //UnityEditor.EditorApplication.isPaused = true;
        }
        else
        {
            Debug.Log(travelledDist + " / " + targetDist);
        }
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
