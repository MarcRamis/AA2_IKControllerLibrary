using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{

    public delegate float ErrorFunction(Vector3 target, float[] solution);

    public struct PositionRotation
    {
        Vector3 position;
        Quaternion rotation;

        public PositionRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        // PositionRotation to Vector3
        public static implicit operator Vector3(PositionRotation pr)
        {
            return pr.position;
        }
        // PositionRotation to Quaternion
        public static implicit operator Quaternion(PositionRotation pr)
        {
            return pr.rotation;
        }
    }

    public class RobotJoint
    {
        // A single 1, which is the axes of movement
        public Vector3 Axis;
        public float MinAngle;
        public float MaxAngle;

        // The offset at resting position
        public Vector3 StartOffset;
    }

    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange = 3f;

        float deltaGradient = 0.01f;
        float learningRate = 10f;
        float StopThreshold = 0.1f;
        float[] angles = new float[6];
        Vector3[] initialAngles = new Vector3[6];
        Vector3[] StartOffset = new Vector3[6];
        Vector3[] Axis = new Vector3[6];

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        
        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            for(int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
            }

        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation

            Axis[0] = new Vector3(1, 0, 0);
            Axis[1] = new Vector3(1, 0, 0);
            Axis[2] = new Vector3(1, 0, 0);
            Axis[3] = new Vector3(1, 0, 0);
            Axis[4] = new Vector3(1, 0, 0);
            Axis[5] = new Vector3(0, 0, 0);

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                StartOffset[i] = _tail.Bones[i].position - _tail.Bones[i].parent.position;
                angles[i] = GetAngle(Axis[i], _tail.Bones[i]);
                initialAngles[i] =_tail.Bones[i].localEulerAngles;
            }

        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
            
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {

        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            if (Vector3.Distance(tailTarget.transform.position, _tail.Bones[_tail.Bones.Length - 1].transform.position) < animationRange)
            {
                updateTail();
            }
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            //
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            //if (DistanceFromTarget(tailTarget.transform.position, angles) < StopThreshold)
            //    return;

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                float gradient = CalculateGradient(tailTarget.transform.position, angles, i, deltaGradient);
                angles[i] -= learningRate * gradient; // Iteration step
                
                if (Axis[i].x == 1) 
                    _tail.Bones[i].transform.localEulerAngles = new Vector3(angles[i], initialAngles[i].y, initialAngles[i].z);
                else if (Axis[i].y == 1) 
                    _tail.Bones[i].transform.localEulerAngles = new Vector3(initialAngles[i].x, angles[i], initialAngles[i].z);
                else if (Axis[i].z == 1) 
                    _tail.Bones[i].transform.localEulerAngles = new Vector3(initialAngles[i].x, initialAngles[i].y, angles[i]);
                
                //if (DistanceFromTarget(tailTarget.transform.position, angles) < StopThreshold)
                //    return;
            }
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {
            
        }
        private float CalculateGradient(Vector3 target, float[] _angles, int i, float delta)
        {
            float angle = _angles[i]; // saves the angle to restore it later

            float f_x = DistanceFromTarget(target, _angles);
            _angles[i] += delta;
            float f_x_plus_d = DistanceFromTarget(target, _angles);

            float gradient = (f_x_plus_d - f_x) / delta;

            _angles[i] = angle; // restoring

            return gradient;
        }

        // Returns the distance from the target, given a solution
        private float DistanceFromTarget(Vector3 target, float[] _angles)
        {
            Vector3 point = ForwardKinematics(_angles);
            return Vector3.Distance(point, target);
        }
        private PositionRotation ForwardKinematics(float[] _angles)
        {
            Vector3 prevPoint = _tail.Bones[0].transform.position;

            // Takes object initial rotation into account
            Quaternion rotation = _tail.Bones[0].transform.localRotation;

            for (int i = 1; i < _tail.Bones.Length; i++)
            {
                rotation *= Quaternion.AngleAxis(_angles[i - 1], Axis[i - 1]);
                Vector3 nextPoint = prevPoint + rotation * StartOffset[i];

                Debug.DrawLine(prevPoint, nextPoint, Color.white);
                prevPoint = nextPoint;
            }


            // The end of the effector
            return new PositionRotation(prevPoint, rotation);
        }

        private float GetAngle(Vector3 axis, Transform bone)
        {
            float angle = 0;
            if (axis.x == 1)
                angle = bone.localEulerAngles.x;
            else if (axis.y == 1)
                angle = bone.localEulerAngles.y;
            else if (axis.z == 1)
                angle = bone.localEulerAngles.z;

            return angle;
        }
        private void SetAngle(Vector3 axis, float _angle, Transform bone)
        {
            float angle = 0;

            //if (axis.x == 1)
            //else if (axis.y == 1)
            //else if (axis.z == 1)
        }

        #endregion
    }
}
