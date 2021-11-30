﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
  
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;

        //LEGS
        Transform[] legTargets = new Transform[6];
        Transform[] legFutureBases = new Transform[6];
        MyTentacleController[] _legs = new MyTentacleController[6];

        private Vector3[,] copy;
        private float[,] distances;

        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            distances = new float[LegRoots.Length, 5];
            copy = new Vector3[LegRoots.Length, 6];
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
                legTargets[i] = LegTargets[i];
                legFutureBases[i] = LegFutureBases[i];
                Debug.Log("0");
                for (int j = 0; j < _legs[0].Bones.Length - 1; j++)
                {
                    Debug.Log("1");
                    distances[i,j] = Vector3.Distance(_legs[i].Bones[j].transform.position, _legs[i].Bones[j + 1].transform.position);
                }
            }
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {

        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {

        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            updateLegs();
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
                if (targetRootDist > tempDistance)
                {
                    // The target is unreachable
                    //Debug.Log(tempDistance);
                    //Debug.Log(targetRootDist);
                }
                else
                {
                    // The target is reachable
                    //while (TODO)
                    while (Vector3.Distance(copy[i,_legs[i].Bones.Length - 1], legTargets[i].position) > 0.1f)
                    {
                        // STAGE 1: FORWARD REACHING
                        //TODO
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
                        //TODO
                        for (int j = 0; j < _legs[0].Bones.Length; j++)
                        {
                            if (j == 0)
                            {
                                Vector3 tempVec = _legs[i].Bones[0].position;
                                copy[i,j] = new Vector3(tempVec.x, tempVec.y, tempVec.z);
                            }
                            else
                            {
                                Vector3 tempVec = Vector3.Normalize(copy[i, j] - copy[i, j - 1]) * Vector3.Distance(_legs[i].Bones[j].position, _legs[i].Bones[j - 1].position) + copy[i, j - 1];
                                copy[j,j] = new Vector3(tempVec.x, tempVec.y, tempVec.z);
                            }
                        }
                    }
                    //for(int i = 0; i < joints.Length; i++)
                    //{
                    //    joints[i].position = copy[i];
                    //}
                }

                // Update original joint rotations
                for (int j = 0; j <= _legs[i].Bones.Length - 2; j++)
                {
                    //TODO 
                    Vector3 vector1 = Vector3.Normalize(_legs[i].Bones[j + 1].position - _legs[i].Bones[j].position);
                    Vector3 vector2 = Vector3.Normalize(copy[i,j + 1] - copy[i,j]);
                    float angle = Vector3.Angle(vector1, vector2);
                    Vector3 axis = Vector3.Cross(vector1, vector2);
                    _legs[i].Bones[j].transform.Rotate(axis, angle, Space.World);
                }
            }
        }
            
        #endregion
    }
}
