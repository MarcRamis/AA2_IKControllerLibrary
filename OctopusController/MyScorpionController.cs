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
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange = 7f;

        float deltaGradient = 0.01f;
        float learningRate = 25f;
        float StopThreshold = 0.1f;
        float[] angles = new float[6];
        Vector3[] initialAngles = new Vector3[6];
        Vector3[] StartOffset = new Vector3[6];
        Vector3[] Axis = new Vector3[6];
        float[] minAngle = new float[6];
        float[] maxAngle = new float[6];

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];
        Transform[] auxFutureBases = new Transform[6];

        private Vector3[,] copy;
        private float[,] distances;

        private bool[] legArrived = new bool[6];

        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            
            //Legs init
            _legs = new MyTentacleController[LegRoots.Length];
            distances = new float[LegRoots.Length, 3];
            copy = new Vector3[LegRoots.Length, 4];
            
            legTargets = LegTargets;
            legFutureBases = LegFutureBases;
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
                legArrived[i] = false;
                for (int j = 0; j < _legs[i].Bones.Length - 1; j++)
                {
                    distances[i,j] = Vector3.Distance(_legs[i].Bones[j].transform.position, _legs[i].Bones[j + 1].transform.position);
                }
            }
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation

            // Axis direction to move
            Axis[0] = new Vector3(0f, 0f, 1f);
            Axis[1] = new Vector3(1f, 0f, 0f);
            Axis[2] = new Vector3(1f, 0f, 0f);
            Axis[3] = new Vector3(1f, 0f, 0f);
            Axis[4] = new Vector3(1f, 0f, 0f);
            Axis[5] = new Vector3(0f, 0f, 0f);

            // Angle clamps
            minAngle[0] = -90;
            maxAngle[0] = 90;
            
            minAngle[1] = -90;
            maxAngle[1] = 0;
            
            minAngle[2] = -100;
            maxAngle[2] = 0;
            
            minAngle[3] = -90;
            maxAngle[3] = 0;
            
            minAngle[4] = -50;
            maxAngle[4] = 10;
            
            minAngle[5] = 0f;
            maxAngle[5] = 0f;

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                StartOffset[i] = _tail.Bones[i].localPosition * 0.32622f;
                angles[i] = GetAngle(Axis[i], _tail.Bones[i]);
                angles[i] = Mathf.Clamp(angles[i],minAngle[i],maxAngle[i]);
                initialAngles[i] = new Vector3(0f,0f,0f);
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
            updateLegPos();
            updateLegs();
            if (Vector3.Distance(tailTarget.position, _tail.Bones[_tail.Bones.Length - 1].position) < animationRange)
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
            for(int i = 0; i < _legs.Length; i++)
            {
                if(Vector3.Distance(_legs[i].Bones[0].position, legFutureBases[i].position) > 0.8f && !legArrived[i])
                {
                    legArrived[i] = true;
                    auxFutureBases[i] = legFutureBases[i];     
                }
                if (legArrived[i])
                {
                    _legs[i].Bones[0].position = Vector3.Lerp(_legs[i].Bones[0].position, auxFutureBases[i].position, 55f * Time.deltaTime);

                    if (Vector3.Distance(_legs[i].Bones[0].position, auxFutureBases[i].position) < 0.1f)
                    {
                        legArrived[i] = false;
                    }
                }
            }
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            if (DistanceFromTarget(tailTarget.position, angles) < StopThreshold)
                return;

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                float gradient = CalculateGradient(tailTarget.position, angles, i, deltaGradient);
                angles[i] -= learningRate * gradient; // Iteration step

                angles[i] = Mathf.Clamp(angles[i], minAngle[i], maxAngle[i]);

                if (Axis[i].x == 1) 
                    _tail.Bones[i].localEulerAngles = new Vector3(angles[i], initialAngles[i].y, initialAngles[i].z);
                else if (Axis[i].y == 1) 
                    _tail.Bones[i].localEulerAngles = new Vector3(initialAngles[i].x, angles[i], initialAngles[i].z);
                else if (Axis[i].z == 1) 
                    _tail.Bones[i].localEulerAngles = new Vector3(initialAngles[i].x, initialAngles[i].y, angles[i]);

                if (DistanceFromTarget(tailTarget.position, angles) < StopThreshold)
                    return;
            }
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {
            
            for (int i = 0; i < _legs.Length; i++)
            {
                for (int j = 0; j < _legs[i].Bones.Length; j++)
                {
                    copy[i,j] = new Vector3(_legs[i].Bones[j].position.x, _legs[i].Bones[j].position.y, _legs[i].Bones[j].position.z);
                }
            }
            for (int i = 0; i < _legs.Length; i++)
            {
                float targetRootDist = Vector3.Distance(copy[i,0], legTargets[i].position);
                float tempDistance = 0;
                for(int j = 0; j < _legs[i].Bones.Length - 1; j++)
                {
                    tempDistance += distances[i, j];
                }
                // Update joint positions
                if (targetRootDist <= tempDistance)
                {
                    // The target is reachable
                    while (Vector3.Distance(copy[i,_legs[i].Bones.Length - 1], legTargets[i].position) > 0.1f)
                    {
                        // STAGE 1: FORWARD REACHING
                        for (int j = _legs[i].Bones.Length - 1; j >= 0; j--)
                        {
                            if (j == _legs[i].Bones.Length - 1)
                            {
                                copy[i,j] = new Vector3(legTargets[i].position.x, legTargets[i].position.y, legTargets[i].position.z);
                            }
                            else
                            {
                                Vector3 tempVec = Vector3.Normalize(copy[i, j] - copy[i, j + 1]) * Vector3.Distance(_legs[i].Bones[j].position, _legs[i].Bones[j + 1].position) + copy[i, j + 1];
                                copy[i, j] = new Vector3(tempVec.x, tempVec.y, tempVec.z);
                            }
                        }
                        // STAGE 2: BACKWARD REACHING
                        for (int j = 0; j < _legs[i].Bones.Length; j++)
                        {
                            if (j == 0)
                            {
                                Vector3 tempVec = _legs[i].Bones[0].position;
                                copy[i, j] = new Vector3(tempVec.x, tempVec.y, tempVec.z);
                            }
                            else
                            {
                                Vector3 tempVec = Vector3.Normalize(copy[i, j] - copy[i, j - 1]) * Vector3.Distance(_legs[i].Bones[j].position, _legs[i].Bones[j - 1].position) + copy[i, j - 1];
                                copy[i, j] = new Vector3(tempVec.x, tempVec.y, tempVec.z);
                            }
                        }
                    }
                }

                // Update original joint rotations
                for (int j = 0; j <= _legs[i].Bones.Length - 2; j++)
                {
                    Vector3 vector1 = Vector3.Normalize(_legs[i].Bones[j + 1].position - _legs[i].Bones[j].position);
                    Vector3 vector2 = Vector3.Normalize(copy[i, j + 1] - copy[i, j]);
                    float angle = Vector3.Angle(vector1, vector2);
                    Vector3 axis = Vector3.Cross(vector1, vector2);
                    _legs[i].Bones[j].transform.Rotate(axis, angle, Space.World);
                }
            }

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
            Vector3 prevPoint = _tail.Bones[0].position;

            // Takes object initial rotation into account
            Quaternion rotation = Quaternion.identity;

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

        #endregion
    }
}
