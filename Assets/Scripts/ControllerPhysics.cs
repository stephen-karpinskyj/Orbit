using UnityEngine;

public class ControllerPhysics : MonoBehaviour
{
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
    private float collisionTolerance = 0.05f;

    [SerializeField]
    private Transform coreTrans;

    private Bounds playableWorldBounds;
    private Line[] playableWorldWalls;

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

    public void Step(Vector3 direction, float distance, float angleOffset, bool shouldBounce)
    {        
        // TODO: If tapping, slide along wall, else bounce off wall

        var distanceLeft = distance;
        var rotLeft = angleOffset;

        var pos = this.coreTrans.localPosition;
        var rot = this.coreTrans.localEulerAngles.z;

        while (distanceLeft > 0f)
        {
            var collided = false;
            var targetPos = pos + direction * distanceLeft;
            var targetRot = rot + rotLeft;

            // Check for wall collision
            // FIXME: Need better way to choose which wall collision was with, could be wrong if core goes out of bounds in both x and y (ie. corner) at same time
            if (!this.playableWorldBounds.Contains(targetPos))
            {
                Line collidingWall;
                if (targetPos.x < this.playableWorldBounds.min.x)
                {
                    collidingWall = this.playableWorldWalls[LeftWallIndex];
                }
                else if (targetPos.x > this.playableWorldBounds.max.x)
                {
                    collidingWall = this.playableWorldWalls[RightWallIndex];
                }
                else if (targetPos.y < this.playableWorldBounds.min.y)
                {
                    collidingWall = this.playableWorldWalls[BottomWallIndex];
                }
                else if (targetPos.y > this.playableWorldBounds.max.y)
                {
                    collidingWall = this.playableWorldWalls[TopWallIndex];
                }
                else
                {
                    throw new System.Exception();
                }

                collided = true;

                Vector2 intersection;
                MathUtility.LineSegmentIntersection(pos, targetPos, collidingWall.Start, collidingWall.End, out intersection);

                var bouncePosAmount = Mathf.Max(0f, Vector2.Distance(pos, intersection) - this.collisionTolerance);
                var bouncePosTarget = pos + direction * bouncePosAmount;

                // Bounce
                {
                    // Apply partial position offset
                    distanceLeft -= bouncePosAmount;
                    pos = bouncePosTarget;

                    // Calculate partial rotation offset
                    var bounceDistPercentage = bouncePosAmount / distanceLeft;
                    var bounceRotAmount = bounceDistPercentage * rotLeft;

                    // Reflect rotation against wall + apply partial rotation offset
                    direction = Vector2.Reflect(targetPos - pos, collidingWall.Normal).Rotate(bounceRotAmount).normalized;
                    rotLeft -= bounceRotAmount;
                    rot = Vector2.up.SignedAngle(direction);
                }
            }

            if (!collided)
            {
                distanceLeft = 0f;
                rotLeft = 0f;
                pos = targetPos;
                rot = targetRot;
            }
        }

        this.coreTrans.localPosition = pos;
        this.coreTrans.localEulerAngles = Vector3.forward * rot;
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
        this.playableWorldWalls = new []
        {
            new Line(new Vector2(min.x, max.y), new Vector2(min.x, min.y)), // Left
            new Line(new Vector2(max.x, max.y), new Vector2(min.x, max.y)), // Top
            new Line(new Vector2(max.x, min.y), new Vector2(max.x, max.y)), // Right
            new Line(new Vector2(min.x, min.y), new Vector2(max.x, min.y)), // Bottom
        };
    }
}
