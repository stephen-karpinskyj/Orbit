using System.Collections.Generic;
using UnityEngine;

public class ControllerPhysics : MonoBehaviour
{
    private const float NearZero = 0.001f;

    private const int LeftWallIndex = 0;
    private const int TopWallIndex = 1;
    private const int RightWallIndex = 2;
    private const int BottomWallIndex = 3;

    private struct Line
    {
        public Vector2 Start;
        public Vector2 End;
        public Vector2 Mid;
        public Vector2 Normal;

        public Line(Vector2 start, Vector2 end)
        {
            this.Start = start;
            this.End = end;

            var v = (end - start).normalized;
            this.Mid = start + v * ((end - start).magnitude / 2);
            this.Normal = new Vector2(-v.y, v.x);
        }
    }

    [SerializeField]
    private RectTransform canvasRect;

    [SerializeField]
    private float radius = 0.5f;

    [SerializeField]
    private Transform coreTrans;

    private Bounds playableWorldBounds;
    private List<Line> playableWorldWalls;

    private Line currentSlideWall;
    private Direction currentWallSlideDirection;

    private void Start()
    {
        this.UpdateWorldSize();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, this.radius);

        if (this.playableWorldWalls != null)
        {
            foreach (var w in this.playableWorldWalls)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(w.Start, w.End);

                //Gizmos.color = Color.cyan;
                //Gizmos.DrawLine(w.Mid, w.Mid + w.Normal);
            }
        }
    }

    public void Step(Vector3 direction, float distance, float angleOffset, bool isTapping, Direction wallSlideDirection, ref bool isWallSliding)
    {
        var distLeft = distance;
        var rotLeft = angleOffset;

        var currPos = this.coreTrans.localPosition;
        var currRot = this.coreTrans.localEulerAngles.z;

        while (distLeft > NearZero)
        {
            if (isWallSliding)
            {
                currPos = this.StepWallSlideTranslation(currPos, distLeft);
                currRot = this.StepWallSlideRotation();
                distLeft = 0;
            }
            else
            {
                var targetPos = currPos + direction * distLeft;
                var targetRot = currRot + rotLeft;

                Line collisionWall;
                Vector2 collisionPoint;

                var collided = this.CheckForPlayableWorldCollision(currPos, targetPos, out collisionWall, out collisionPoint);

                if (collided)
                {
                    // Calculate partial position offset
                    var collisionPosAmount = Mathf.Max(0f, Vector2.Distance(currPos, collisionPoint) - NearZero);
                    var collisionPosTarget = currPos + direction * collisionPosAmount;

                    // Apply partial position offset
                    distLeft -= collisionPosAmount;
                    currPos = collisionPosTarget;

                    if (isTapping)
                    {
                        isWallSliding = true;
                        this.currentSlideWall = collisionWall;
                        this.currentWallSlideDirection = wallSlideDirection;
                        currRot = this.StepWallSlideRotation();
                        rotLeft = 0;
                    }
                    else
                    {
                        // Calculate partial rotation offset
                        var bounceDistPercentage = collisionPosAmount / distLeft;
                        var bounceRotAmount = bounceDistPercentage * rotLeft;

                        // Reflect rotation against wall + apply partial rotation offset
                        direction = Vector2.Reflect(targetPos - currPos, collisionWall.Normal).Rotate(bounceRotAmount).normalized;
                        rotLeft -= bounceRotAmount;
                        currRot = Vector2.up.SignedAngle(direction);
                    }
                }
                else
                {
                    distLeft = 0f;
                    rotLeft = 0f;
                    currPos = targetPos;
                    currRot = targetRot;
                }
            }
        }

        this.coreTrans.localPosition = currPos;
        this.coreTrans.localEulerAngles = Vector3.forward * currRot;
    }

    private bool CheckForPlayableWorldCollision(Vector2 currentPos, Vector2 targetPos, out Line collisionWall, out Vector2 collisionPoint)
    {
        collisionWall = default(Line);
        collisionPoint = Vector2.zero;

        if (this.playableWorldBounds.Contains(targetPos))
        {
            return false;
        }

        // FIXME: Need better way to choose which wall collision was with, could be wrong if core goes out of bounds in both x and y (ie. corner) at same time
        if (targetPos.x < this.playableWorldBounds.min.x)
        {
            collisionWall = this.playableWorldWalls[LeftWallIndex];
        }
        else if (targetPos.x > this.playableWorldBounds.max.x)
        {
            collisionWall = this.playableWorldWalls[RightWallIndex];
        }
        else if (targetPos.y < this.playableWorldBounds.min.y)
        {
            collisionWall = this.playableWorldWalls[BottomWallIndex];
        }
        else if (targetPos.y > this.playableWorldBounds.max.y)
        {
            collisionWall = this.playableWorldWalls[TopWallIndex];
        }
        else
        {
            throw new System.Exception();
        }

        MathUtility.LineSegmentIntersection(currentPos, targetPos, collisionWall.Start, collisionWall.End, out collisionPoint);
        return true;
    }

    private Vector2 StepWallSlideTranslation(Vector2 position, float distance)
    {
        var distLeft = distance;

        var currPos = position;

        var nextPos = MathUtility.NearestPointOnFiniteLine(this.currentSlideWall.Start, this.currentSlideWall.End, position);
        distLeft -= Vector2.Distance(currPos, nextPos);
        currPos = nextPos;

        while (distLeft > NearZero)
        {
            var direction = this.currentSlideWall.Normal.Rotate(this.currentWallSlideDirection == Direction.CW ? 90 : -90).normalized;
            var targetPos = currPos + direction * distLeft;

            nextPos = MathUtility.NearestPointOnFiniteLine(this.currentSlideWall.Start, this.currentSlideWall.End, targetPos);
            distLeft -= Vector2.Distance(currPos, nextPos);
            currPos = nextPos;

            var distOutsideBounds = this.playableWorldBounds.SqrDistance(targetPos);
            if (distOutsideBounds > Mathf.Epsilon)
            {
                this.currentSlideWall = this.GetNextSlideWall(this.currentWallSlideDirection);
            }
        }

        return currPos;
    }

    private float StepWallSlideRotation()
    {
        return Vector2.up.SignedAngle(this.currentSlideWall.Normal);
    }

    private Line GetNextSlideWall(Direction direction)
    {
        var index = this.playableWorldWalls.IndexOf(this.currentSlideWall);
        index += direction == Direction.CW ? 1 : -1;

        if (index < 0)
        {
            index = this.playableWorldWalls.Count - 1;
        }
        else if (index > this.playableWorldWalls.Count - 1)
        {
            index = 0;
        }

        return this.playableWorldWalls[index];
    }

    private void UpdateWorldSize()
    {
        var worldSize = this.canvasRect.sizeDelta;
        worldSize.x *= this.canvasRect.localScale.x;
        worldSize.y *= this.canvasRect.localScale.y;

        var diameter = this.radius * 2;
        var playableWorldSize = new Vector2(worldSize.x - diameter, worldSize.y - diameter);
        this.playableWorldBounds = new Bounds(Vector3.zero, playableWorldSize);

        var min = this.playableWorldBounds.min;
        var max = this.playableWorldBounds.max;
        this.playableWorldWalls = new List<Line>
        {
            new Line(new Vector2(min.x, max.y), new Vector2(min.x, min.y)), // LeftWallIndex
            new Line(new Vector2(max.x, max.y), new Vector2(min.x, max.y)), // TopWallIndex
            new Line(new Vector2(max.x, min.y), new Vector2(max.x, max.y)), // RightWallIndex
            new Line(new Vector2(min.x, min.y), new Vector2(max.x, min.y)), // BottomWallIndex
        };
    }
}
