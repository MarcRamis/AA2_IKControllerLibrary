using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController 
    {
        
        MyTentacleController[] _tentacles =new  MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];

        float []_theta;

        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin {  set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }
        

        public void TestLogging(string objectName)
        {

           
            Debug.Log("hello, I am initializing my Octopus Controller in object "+objectName);

            
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for (int i = 0;  i  < tentacleRoots.Length; i++)
            {
                
                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i],TentacleMode.TENTACLE);
                //TODO: initialize any variables needed in ccd
            }

            _randomTargets = randomTargets;
            //TODO: use the regions however you need to make sure each tentacle stays in its region
            _theta = new float[_tentacles[0].Bones.Length];
            _swingMin = 0f;
            _swingMax = 180f;
            _twistMin = -45f;
            _twistMax = 45f;
        }

              
        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot() {
            //TODO. what happens here?
            Debug.Log("Shoot");
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
        }




        #endregion


        #region private and internal methods
        //todo: add here anything that you need
        private Quaternion GetSwing(Quaternion rot3)
        {
            //rot3 = Quaternion.AngleAxis(Mathf.Clamp(rot3.w, _swingMin, _swingMax), new Vector3(rot3.x, rot3.y, rot3.z));
            Quaternion swing = Quaternion.Inverse(GetTwist(rot3)) * rot3;
            return swing;
        }

        private Quaternion GetTwist(Quaternion rot3)
        {
            Quaternion twist = Quaternion.AngleAxis(rot3.w, new Vector3(0, rot3.y, 0)).normalized;
            return twist;
        }

        void update_ccd() {

            for (int i = 0; i < _tentacles.Length; i++)
            {
                for (int j = _tentacles[i].Bones.Length - 2; j >= 0; j--)
                {
                    Vector3 r1 = _tentacles[i].Bones[_tentacles[i].Bones.Length - 1].transform.position - _tentacles[i].Bones[j].transform.position;
                    Vector3 r2 = _randomTargets[i].transform.position - _tentacles[i].Bones[j].transform.position;

                    float angle = 0f;
                    Vector3 axis = Vector3.zero;
                    angle = Mathf.Acos(Vector3.Dot(r1.normalized, r2.normalized)) * Mathf.Rad2Deg;
                    axis = Vector3.Cross(r1, r2).normalized;

                    _theta[j] = Mathf.Clamp(angle, -180f, 180f);
                    if (Math.Cos(angle) < 0.9999f) 
                    {
                        _tentacles[i].Bones[j].transform.Rotate(axis,_theta[j],Space.World);
                        //_tentacles[i].Bones[j].transform.localRotation = GetSwing(_tentacles[i].Bones[j].transform.localRotation);
                    }
                }
            }
        }


        

        #endregion






    }
}
