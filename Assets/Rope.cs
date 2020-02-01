using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private NativeArray<RopeSegment> ropeSegments;
    private float segmentLength = 0.1f;
    private int segments = 35;
    public Transform target;
    public bool useTarget;
    Vector3[] ropePositions;
    // Use this for initialization
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Vector3 ropePoint = transform.position;
        ropeSegments = new NativeArray<RopeSegment>(segments, Allocator.Persistent);
        for (int i = 0; i < segments; i++)
        {
            ropeSegments[i] = new RopeSegment(ropePoint);
            ropePoint.y -= segmentLength;
        }
        ropePositions = new Vector3[segments];
    }

    public void RopeUpdate()
    {
        Render();
    }

    public JobHandle SimulateBurst()
    {
        IRopeSimulate ropeSimulation = new IRopeSimulate
        {
            fixedDeltaTime = Time.fixedDeltaTime,
            segments = ropeSegments,
            pos = transform.position
        };
        JobHandle x = ropeSimulation.Schedule(ropeSegments.Length, 16);
        IConstraintJob constraintSim = new IConstraintJob
        {
            segments = ropeSegments,
            pos = transform.position,
            pos2 = target.transform.position,
            useTarget = this.useTarget,
            segmentLength = this.segmentLength
        };
        return constraintSim.Schedule(x);
    }
    void OnDestroy()
    {
        ropeSegments.Dispose();
    }

    private void Render()
    {
        for (int i = 0; i < segments; i++)
        {
            ropePositions[i] = ropeSegments[i].Pos();
        }
        if (!useTarget)
        {
            target.transform.position = ropeSegments[ropeSegments.Length - 1].Pos();
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment
    {
        public float3 posNow;
        public float3 posOld;

        public RopeSegment(Vector3 pos)
        {
            posNow = pos;
            posOld = pos;
        }
        public Vector3 Pos()
        {
            return posNow;
        }
    }
    [BurstCompile(CompileSynchronously = true)]
    public struct IRopeSimulate : IJobParallelFor
    {
        public float3 pos, pos2;
        public NativeArray<RopeSegment> segments;
        public float fixedDeltaTime;
        public void Execute(int index)
        {
            float3 gravity = new float3(0, -1, 0);
            RopeSegment segment = segments[index];
            float3 velocity = segment.posNow - segment.posOld;
            segment.posOld = segment.posNow;
            segment.posNow += velocity;
            segment.posNow += gravity * fixedDeltaTime;
            segments[index] = segment;
        }
    }
    [BurstCompile(CompileSynchronously = true)]
    public struct IConstraintJob : IJob
    {
        public float3 pos, pos2;
        public NativeArray<RopeSegment> segments;
        public bool useTarget;
        public float segmentLength;
        public void Execute()
        {
            RopeSegment firstSegment = this.segments[0];
            firstSegment.posNow = new float3(pos.x, pos.y, pos.z);
            segments[0] = firstSegment;
            if (useTarget)
            {
                RopeSegment endSegment = this.segments[segments.Length - 1];
                endSegment.posNow = new float3(pos2.x, pos2.y, pos2.z);
                segments[segments.Length - 1] = endSegment;
            }
            for (int j = 0; j < 50; j++)
            {
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    RopeSegment firstSeg = segments[i];
                    RopeSegment secondSeg = segments[i + 1];
                    float3 dir = firstSeg.posNow - secondSeg.posNow;
                    float dist = math.sqrt(dir.x * dir.x + dir.y * dir.y + dir.z * dir.z);
                    float error = math.abs(dist - segmentLength);
                    float3 changeDir = float3.zero;
                    if (dist > segmentLength)
                    {
                        changeDir = math.normalize(firstSeg.posNow - secondSeg.posNow);
                    }
                    else if (dist < segmentLength)
                    {
                        changeDir = math.normalize(secondSeg.posNow - firstSeg.posNow);
                    }
                    float3 changeAmount = changeDir * error;
                    if (i > 0)
                    {
                        firstSeg.posNow -= changeAmount * 0.5f;
                        this.segments[i] = firstSeg;
                    }
                    if ((i + 1 < segments.Length - 1) || !useTarget)
                    {
                        secondSeg.posNow += changeAmount * 0.5f;
                        this.segments[i + 1] = secondSeg;
                    }

                }
            }
        }
    }
}