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
        Transform[] _currentTarget;

        float []_theta;

        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        bool isBallHit = false;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin {  set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }
        

        public void TestLogging(string objectName)
        {

           
            Debug.Log("Project made by Alex Alcaide Arroyes & Marc Ramis Caldes. Hello, I am initializing my Octopus Controller in object "+objectName);

            
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            for (int i = 0;  i  < tentacleRoots.Length; i++)
            {
                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i],TentacleMode.TENTACLE);
                //TODO: initialize any variables needed in ccd
            }

            _currentTarget = randomTargets;
            //_currentTarget = _randomTargets;
            //TODO: use the regions however you need to make sure each tentacle stays in its region
            _theta = new float[_tentacles[0].Bones.Length];
        }

              
        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot() {
            //TODO. what happens here?
            Debug.Log("Shoot");
            isBallHit = true;
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            SetTentacleTarget();
            update_ccd();
        }

        #endregion

        #region private and internal methods
        //todo: add here anything that you need
        private Quaternion GetSwing(Quaternion _rot)
        {
            return _rot * Quaternion.Inverse(GetTwist(_rot));
        }

        private Quaternion GetTwist(Quaternion _rot)
        {
            return new Quaternion(0, _rot.y, 0, _rot.w).normalized;
        }

        void update_ccd() {

            for (int i = 0; i < _tentacles.Length; i++)
            {
                for (int j = _tentacles[i].Bones.Length - 2; j >= 0; j--)
                {
                    Vector3 r1 = _tentacles[i].Bones[_tentacles[i].Bones.Length - 1].position - _tentacles[i].Bones[j].position;
                    Vector3 r2 = _currentTarget[i].position - _tentacles[i].Bones[j].position;

                    float angle = 0f;
                    Vector3 axis = Vector3.zero;
                    angle = Mathf.Acos(Vector3.Dot(r1.normalized, r2.normalized)) * Mathf.Rad2Deg;
                    axis = Vector3.Cross(r1, r2).normalized;

                    _theta[j] = Mathf.Clamp(angle, _swingMin, _swingMax);

                    if (Math.Cos(angle) < 0.9999f) 
                    {
                        // Start rotation
                        _tentacles[i].Bones[j].Rotate(axis, angle, Space.World);
                        _tentacles[i].Bones[j].localRotation.ToAngleAxis(out angle, out axis);

                        // Descomposition
                        Quaternion swing = GetSwing(Quaternion.AngleAxis(angle, axis));
                        Quaternion twist = GetTwist(Quaternion.AngleAxis(angle, axis));

                        // Twist rotation
                        twist.ToAngleAxis(out angle, out axis);
                        // Twist constraints
                        float tempTwistAngle = Mathf.Clamp(angle, _twistMin, _twistMax);
                        // Twist rotation with constraints
                        twist = Quaternion.AngleAxis(tempTwistAngle, axis);

                        // Swing rotation
                        swing.ToAngleAxis(out angle, out axis);
                        // Swing constraints
                        _theta[j] = Mathf.Clamp(angle, _swingMin, _swingMax);
                        // Swing rotation with constraints
                        swing = Quaternion.AngleAxis(_theta[j], axis);

                        Quaternion result = swing * twist;
                        _tentacles[i].Bones[j].localRotation = result;
                    }
                }
            }
        }

        void SetTentacleTarget()
        {
            if (isBallHit)
            {
                if (_currentRegion.localPosition.z == -15f)
                {
                    _currentTarget[0] = _target;
                }
                else if (_currentRegion.localPosition.z == -5f)
                {
                    _currentTarget[1] = _target;
                }
                else if (_currentRegion.localPosition.z == 5f)
                {
                    _currentTarget[2] = _target;
                }
                else if(_currentRegion.localPosition.z == 15f) 
                {
                    _currentTarget[3] = _target;
                }
            }
        }

        #endregion






    }
}
