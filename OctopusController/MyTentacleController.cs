using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{

    
    internal class MyTentacleController

    //MAINTAIN THIS CLASS AS INTERNAL
    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        Transform _endEffectorSphere;

        public Transform[] Bones { get => _bones; }

        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            //TODO: add here whatever is needed to find the bones forming the tentacle for all modes
            //you may want to use a list, and then convert it to an array and save it into _bones
            List<Transform> bonesFromRoot = new List<Transform>();

            tentacleMode = mode;
            Transform tmpBone;

            switch (tentacleMode){
                case TentacleMode.LEG:
                    //TODO: in _endEffectorsphere you keep a reference to the base of the leg
                    tmpBone = root.GetChild(0);
                    bonesFromRoot.Add(tmpBone);
                    for (int i = 0; i < 3; i++)
                    {
                        tmpBone = tmpBone.GetChild(1);
                        bonesFromRoot.Add(tmpBone);
                    }
                    _endEffectorSphere = bonesFromRoot.Last<Transform>();
                    _bones = bonesFromRoot.ToArray();
                    break;
                case TentacleMode.TAIL:
                    //TODO: in _endEffectorsphere you keep a reference to the red sphere 
                    tmpBone = root;
                    bonesFromRoot.Add(tmpBone);
                    for(int i = 0; i < 5; i++)
                    {
                        tmpBone = tmpBone.GetChild(1);
                        bonesFromRoot.Add(tmpBone);
                    }
                    _endEffectorSphere = bonesFromRoot.Last<Transform>();
                    _bones = bonesFromRoot.ToArray();
                    break;
                case TentacleMode.TENTACLE:
                    //TODO: in _endEffectorphere you  keep a reference to the sphere with a collider attached to the endEffector
                    tmpBone = root.GetChild(0);
                    tmpBone = tmpBone.GetChild(0);
                    for (int i = 0; i < 51; i++)
                    {
                        tmpBone = tmpBone.GetChild(0);
                        bonesFromRoot.Add(tmpBone);
                    }
                    _endEffectorSphere = bonesFromRoot.Last<Transform>();
                    _bones = bonesFromRoot.ToArray();

                    break;
            }
            return Bones;
        }
    }
}
