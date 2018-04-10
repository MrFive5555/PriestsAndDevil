using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PAD_Module;
using ActionManagement;

namespace PAD_View {
    public class View {
        static public Vector3 leftCoastPos = new Vector3(-11.23f, -0.93f, 4.18f);
        static public Vector3 rightCoastPos = new Vector3(14.85f, -0.6f, 3.74f);
        static public Vector3 riverPos = new Vector3(0, 0.5f, 10);
        static public Vector3 Mountain = new Vector3(130, 27, 69);

        // 船的位置
        static private Vector3 boatRight = new Vector3(5.5f, 1.25f, 0);
        static private Vector3 deltaFromRight = new Vector3(-11f, 0, 0);
        // 乘客在船上的位置
        static private Vector3 passengerLeft(int onBoat) {
            return new Vector3(-(6.5f - 2 * onBoat), 2, 0);
        }
        static private Vector3 passengerRight(int onBoat) {
            return new Vector3((4.5f + 2 * onBoat), 2, 0);
        }
        // 乘客在岸上的位置
        static private Vector3 passengerOnCoast(int index, Passenger.Type type, bool onLeft) {
            if (onLeft == true) {
                int x = type == Passenger.Type.Devil
                                ? -(10 + 2 * (index % 3))
                                : -(10 + 2 * (index % 3));
                int z = type == Passenger.Type.Devil
                    ? 1
                    : -1;
                return new Vector3(x, 3.5f, z);
            } else {
                int x = type == Passenger.Type.Devil
                                ? (10 + 2 * (index % 3))
                                : (10 + 2 * (index % 3));
                int z = type == Passenger.Type.Devil
                    ? 1
                    : -1;
                return new Vector3(x, 3.5f, z);
            }
        }
        // 上船位置
        static private Vector3 aboardPoint(bool isLeft) {
            return new Vector3(isLeft ? -8 : 8, 3.5f, 0);
        }

        // 初始化时调用的绘图函数
        static public void StaticRender(Passenger[] _passenger, Boat _boat) {
            for(int i=0; i!=6;++i) {
                _passenger[i].setPosition(passengerOnCoast(i, _passenger[i].type,false));
            }
            _boat.setPostion(boatRight);
        }
        // =======================================================================================
        // 被分离出来的动作
        // 过河动作要被执行时船已经到达对岸
        public class Action_BoatCross : Action {
            public static Action getAction(Passenger[] _p, Boat _b, float _during, Callback callback) {
                Action_BoatCross ac = CreateInstance<Action_BoatCross>();
                ac._passenger = _p;
                ac._boat = _b;
                ac.during = _during;
                ac.callback = callback;
                return ac;
            }
            Passenger[] _passenger;
            Boat _boat;
            private float during; // 过河时间

            public override void Start() {
                from = _boat.gameobject().transform.position;
                del = _boat.CoastPos == CoastPos.BoatLeft ? deltaFromRight : -deltaFromRight;

                for(int i = 0; i != 6; ++i) {
                    if(_passenger[i].CoastPos==CoastPos.BoatLeft || _passenger[i].CoastPos==CoastPos.BoatRight) {
                        if(p1 == -1) {
                            p1 = i;
                            p1From = _passenger[i].gameobject().transform.position;
                        } else {
                            p2 = i;
                            p2From = _passenger[i].gameobject().transform.position;
                        }
                    }
                }
            }
            private float time = 0;

            private Vector3 from;          // 船的起点
            private int p1 = -1, p2 = -1;  // 乘客序号
            private Vector3 p1From;        // 乘客起点
            private Vector3 p2From;
            Vector3 del;

            public override void Update() {
                if (time * Time.deltaTime < during) {
                    // 船
                    _boat.setPostion(from + time * Time.deltaTime * del / during);
                    // 乘客
                    _passenger[p1].setPosition(p1From + time * Time.deltaTime * del / during);
                    if(p2 != -1) {
                        _passenger[p2].setPosition(p2From + time * Time.deltaTime * del / during);
                    }
                    ++time;
                } else {
                    _boat.setPostion(from + del);
                    _passenger[p1].setPosition(p1From + del);
                    if (p2 != -1) {
                        _passenger[p2].setPosition(p2From + del);
                    }
                    callback.call();
                    destroy = true;
                }
            }
        }

        public class Action_MoveTo : Action {
            private GameObject obj;
            private Vector3 from;
            private Vector3 Del;
            private Vector3 to;
            private float during;

            private int time = 0;

            public static Action getAction(GameObject _obj, Vector3 dist, float _during, Callback callback) {
                Action_MoveTo ac = ScriptableObject.CreateInstance<Action_MoveTo>();
                ac.to = dist;
                ac.during = _during;
                ac.obj = _obj;
                ac.callback = callback;
                return ac;
            }
            public override void Start() {
                from = obj.transform.position;
                Del = to - from;
            }
            public override void Update() {
                if (time * Time.deltaTime < during) {
                    obj.transform.position = from + time * Time.deltaTime / during * Del;
                    ++time;
                } else {
                    obj.transform.position = to;
                    callback.call();
                    destroy = true;
                }
            }
        }

        public class Action_Aboard {
            public static Action getAction(int _i, Passenger[] _passenger, Boat _boat, Callback callback) {
                List<Action> list = new List<Action>();
                list.Add(Action_MoveTo.getAction(
                    _passenger[_i].gameobject(),
                    aboardPoint(_boat.CoastPos == CoastPos.BoatLeft),
                    0.2f,
                    callback
                ));
                list.Add(Action_MoveTo.getAction(
                    _passenger[_i].gameobject(), 
                    dist(_passenger, _boat), 
                    0.2f, 
                    callback
                ));
                Action ac = SequenceAction.getAction(list, callback);
                return ac;
            }

            private static Vector3 dist(Passenger[] _passenger, Boat _boat) {
                int onBoat = 0;
                for (int i = 0; i != 6; ++i) {
                    if (_passenger[i].CoastPos == CoastPos.BoatLeft || _passenger[i].CoastPos == CoastPos.BoatRight) {
                        ++onBoat;
                    }
                }
                return _boat.gameobject().transform.position + new Vector3(-3 + 2 * onBoat, 0.75f, 0);
            }
        }

        public class Action_Ashore {
            public static Action getAction(int _i, Passenger[] _passenger, Boat _boat, Callback callback) {
                List<Action> list = new List<Action>();
                list.Add(Action_MoveTo.getAction(
                    _passenger[_i].gameobject(),
                    aboardPoint(_boat.CoastPos == CoastPos.BoatLeft),
                    0.2f,
                    callback
                ));

                list.Add(Action_MoveTo.getAction(
                    _passenger[_i].gameobject(), 
                    passengerOnCoast(_i, _passenger[_i].type, _passenger[_i].CoastPos == CoastPos.CoastLeft), 
                    0.5f, 
                    callback
                ));

                for(int i=0; i!=6; ++i) {
                    if(_passenger[i].CoastPos == CoastPos.BoatLeft || _passenger[i].CoastPos == CoastPos.BoatRight) {
                        list.Add(Action_MoveTo.getAction(
                            _passenger[i].gameobject(),
                            _boat.gameobject().transform.position + new Vector3(-1, 0.75f, 0),
                            0.2f,
                            callback
                        ));
                    }
                }
                Action ac = SequenceAction.getAction(list, callback);
                return ac;
            }
        }
    }
}