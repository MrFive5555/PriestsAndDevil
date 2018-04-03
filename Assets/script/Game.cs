using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Game : MonoBehaviour {
    Controllor _controllor;

    // 按钮的style
    GUIStyle backgroundStyle;
    GUIStyle boxStyle;

    // Use this for initialization
    void Start() {
        _controllor = new Controllor();

        // "gameove"和"成功"的提示框
        backgroundStyle = new GUIStyle("box");
        backgroundStyle.alignment = TextAnchor.LowerCenter;
        backgroundStyle.fontSize = 100;
        // 标题
        boxStyle = new GUIStyle("Box");
        boxStyle.fontSize = 20;
    }

    private float clickWaiting = 0;
    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButton(0) && clickWaiting >= 0.2) {
            clickWaiting = 0;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo)) {
                GameObject gameObj = hitInfo.collider.gameObject;
                _controllor.clickObject(gameObj.name);
            }
        }
        clickWaiting += Time.deltaTime;
        _controllor.setView();
    }

    void OnGUI() {
        switch (_controllor.getState()) {
            case Controllor.GameState.gameover:
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "游戏失败", backgroundStyle);
                break;
            case Controllor.GameState.win:
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "成功", backgroundStyle);
                break;
        }
        float baseX = Screen.width * 0.1f;
        float baseY = Screen.height * 0.3f;
        
        GUI.Box(new Rect(baseX + 0, baseY + 0, 140, 100), "恶魔与牧师", boxStyle);
        if (GUI.Button(new Rect(baseX + 20, baseY + 40, 100, 50), "重新开始")) {
            _controllor.reinitial();
        }
    }

    // =================================================================================================
    // controllor
    // 表示对象在左岸或右岸
    public enum CoastPos {
        CoastLeft,
        CoastRight,
        BoatLeft,
        BoatRight
    }

    public class Controllor {
        public enum GameState {
            waiting,
            moving,
            win,
            gameover
        }
        Passenger[] _passanger;
        Boat _boat;
        private GameState _state;

        public GameState getState() {
            return _state;
        }

        public Controllor() {
            // 初始化对象
            _passanger = new Passenger[6];
            for (int i = 0; i != 3; ++i) {
                _passanger[i] = new Passenger(Passenger.Type.Priest);
            }
            for (int i = 3; i != 6; ++i) {
                _passanger[i] = new Passenger(Passenger.Type.Devil);
            }
            _boat = new Boat();
            new Coast(View.leftCoastPos);
            new Coast(View.rightCoastPos);
            new River(View.riverPos);
            _state = GameState.waiting;
            setView();
        }

        public void reinitial() {
            for(int i=0; i != 6; ++i) {
                _passanger[i].CoastPos = CoastPos.CoastRight;
            }
            _boat.CoastPos = CoastPos.BoatRight;
            _state = GameState.waiting;
        }

        public void clickObject(string name) { // 点击
            if(_state != GameState.waiting) {
                return;
            } else if(Regex.IsMatch(name, "^passenge")) {
                int index = int.Parse(name.Substring(name.Length - 1));
                CoastPos pPos = _passanger[index].CoastPos;
                CoastPos bPos = _boat.CoastPos;

                int onBoat = countOnBoat();

                if(onBoat < 2 && pPos == CoastPos.CoastLeft && bPos == CoastPos.BoatLeft) {
                    _passanger[index].CoastPos = bPos; // 上船
                } else if(onBoat < 2 && pPos == CoastPos.CoastRight && bPos == CoastPos.BoatRight) {
                    _passanger[index].CoastPos = bPos; // 上船
                } else if(pPos == CoastPos.BoatLeft) {
                    _passanger[index].CoastPos = CoastPos.CoastLeft; // 下船
                } else if(pPos == CoastPos.BoatRight) { 
                    _passanger[index].CoastPos = CoastPos.CoastRight; // 下船
                }
            } else if (Regex.IsMatch(name, "^Boat$")) {
                if(countOnBoat() == 0) {
                    return;
                }
                if (_boat.CoastPos == CoastPos.BoatLeft) { // 从左岸过河
                    _boat.CoastPos = CoastPos.BoatRight;
                    for(int i=0; i!=6; ++i) {
                        if(_passanger[i].CoastPos == CoastPos.BoatLeft) {
                            _passanger[i].CoastPos = CoastPos.BoatRight;
                        }
                    }
                } else { // 从右岸过河
                    _boat.CoastPos = CoastPos.BoatLeft;
                    for (int i = 0; i != 6; ++i) {
                        if (_passanger[i].CoastPos == CoastPos.BoatRight) {
                            _passanger[i].CoastPos = CoastPos.BoatLeft;
                        }
                    }
                }
                _state = GameState.moving;
            }
        }
        public void setView() {
            // 当渲染完过河的动画后才把_state设置为可以继续的waiting状态，并进入游戏结算的环节
            if(View.render(_passanger, _boat, _state == GameState.moving)) {
                _state = GameState.waiting;
                checkState();
            }
        }
        private int countOnBoat() {
            int count = 0;
            for(int i=0; i!=6; ++i) {
                if(_passanger[i].CoastPos == CoastPos.BoatLeft 
                    || _passanger[i].CoastPos == CoastPos.BoatRight) {
                    ++count;
                }
            }
            return count;
        }
        private void checkState() {
            int leftDevil = 0, leftPriest = 0;
            int rightDevil = 0, rightPriest = 0;
            for(int i=0; i != 6; ++i) {
                if(_passanger[i].type == Passenger.Type.Devil) {
                    if(_passanger[i].CoastPos == CoastPos.BoatLeft || _passanger[i].CoastPos == CoastPos.CoastLeft) {
                        ++leftDevil;
                    } else {
                        ++rightDevil;
                    }
                } else {
                    if (_passanger[i].CoastPos == CoastPos.BoatLeft || _passanger[i].CoastPos == CoastPos.CoastLeft) {
                        ++leftPriest;
                    } else {
                        ++rightPriest;
                    }
                }
            }
            if((leftDevil > leftPriest && leftPriest != 0) 
                || (rightDevil > rightPriest && rightPriest != 0)) {
                _state = GameState.gameover;
            } else if(leftDevil == 3 && leftPriest == 3) {
                _state = GameState.win;
            }
        }
    }

    // ================================================================================================
    // View 部分
    public class View {
        static public Vector3 leftCoastPos = new Vector3(23, 1.5f, 10);
        static public Vector3 rightCoastPos = new Vector3(-23, 1.5f, 10);
        static public Vector3 riverPos = new Vector3(0, 0.5f, 10);

        // 当render函数渲染完moving的过程后，将会返回一个true，表示游戏可以重新返回waiting的状态
        static public bool render(Passenger[] _passanger, Boat _boat, bool isMoving) {
            if(isMoving) {
                return moving(_passanger, _boat);
            } else {
                standing(_passanger, _boat);
                return true;
            }
        }
        static private Vector3 boatLeft = new Vector3(-5.5f, 1.25f, 0);
        static private Vector3 boatRight = new Vector3(5.5f, 1.25f, 0);
        static private Vector3 passengerLeft(int onBoat) {
            return new Vector3(-(6.5f - 2 * onBoat++), 2, 0);
        }
        static private Vector3 passengerRight(int onBoat) {
            return new Vector3((4.5f + 2 * onBoat++), 2, 0);
        }

        static private float time = 0;
        // 过河后返回true, 另外开始执行函数后，_boat的位置已经到达对岸
        static private bool moving(Passenger[] _passanger, Boat _boat) {
            Vector3 boatFrom = _boat.CoastPos == CoastPos.BoatLeft ? boatRight : boatLeft;
            Vector3 boatTo = _boat.CoastPos == CoastPos.BoatLeft ? boatLeft : boatRight;
            int during = 1; // 过河动画时间
            if(time * Time.deltaTime < during) {
                // 船
                _boat.setPostion(boatFrom + time * Time.deltaTime / during * (boatTo - boatFrom));
                // 乘客
                int onBoat = 0;
                for (int i = 0; i != 6; ++i) {
                    if(_passanger[i].CoastPos != CoastPos.BoatLeft && _passanger[i].CoastPos != CoastPos.BoatRight) {
                        continue;
                    }
                    Vector3 pFrom = _boat.CoastPos == CoastPos.BoatLeft ? passengerRight(onBoat) : passengerLeft(onBoat);
                    Vector3 pTo = _boat.CoastPos == CoastPos.BoatLeft ? passengerLeft(onBoat) : passengerRight(onBoat);
                    _passanger[i].setPosition(pFrom + time * Time.deltaTime / during * (pTo - pFrom));
                    ++onBoat;
                }
                ++time;
                return false;
            } else {
                _boat.setPostion(boatTo);
                time = 0;
                return true;
            }
        }
        // view类不检查是否有多于2个对象在船上，请保证传入数据时船上的乘客不大于两个
        static private void standing(Passenger[] _passanger, Boat _boat) {
            int onBoat = 0;
            int onLeftPriest = 0;
            int onLeftDevil = 0;
            int onRightPriest = 0;
            int onRightDevil = 0;
            // 乘客
            for (int i=0; i!=6; ++i) {
                switch(_passanger[i].CoastPos) {
                    case CoastPos.BoatLeft:
                        _passanger[i].setPosition(new Vector3(-(6.5f - 2 * onBoat++), 2, 0));
                        break;
                    case CoastPos.BoatRight:
                        _passanger[i].setPosition(new Vector3((4.5f + 2 * onBoat++), 2, 0));
                        break;
                    case CoastPos.CoastLeft: {
                        int x = _passanger[i].type == Passenger.Type.Devil 
                            ? -(10 + 2 * onLeftDevil++) 
                            : -(10 + 2 * onLeftPriest++);
                        int z = _passanger[i].type == Passenger.Type.Devil
                            ? 1
                            : -1;
                        _passanger[i].setPosition(new Vector3(x, 3.5f, z));
                        break;
                    }
                    case CoastPos.CoastRight: {
                        int x = _passanger[i].type == Passenger.Type.Devil
                            ? (10 + 2 * onRightDevil++)
                            : (10 + 2 * onRightPriest++);
                        int z = _passanger[i].type == Passenger.Type.Devil
                            ? 1
                            : -1;
                        _passanger[i].setPosition(new Vector3(x, 3.5f, z));
                        break;
                    }
                }
            }
            // 船
            if(_boat.CoastPos == CoastPos.BoatLeft) {
                _boat.setPostion(boatLeft);
            } else {
                _boat.setPostion(boatRight);
            }
        }
    }

    // ================================================================================================
    // Module部分
    public class Boat {
        public CoastPos CoastPos = CoastPos.BoatRight;
        private GameObject _boat;
        public Boat() {
            _boat = GameObject.Instantiate(Resources.Load("Prefabs/Boat", typeof(GameObject))) as GameObject;
            _boat.name = "Boat";
        }

        public void setPostion(Vector3 position) {
            _boat.transform.position = position;
        }
    }
    
    public class Passenger {
        public enum Type {
            Priest,
            Devil
        }
        public Type type;
        public CoastPos CoastPos = CoastPos.CoastRight;
        static private int count = 0;
        GameObject _passenger;
        public Passenger(Type _type) {
            type = _type;
            if(type == Type.Devil) {
                _passenger = GameObject.Instantiate(Resources.Load("Prefabs/Devil", typeof(GameObject))) as GameObject;
            } else {
                _passenger = GameObject.Instantiate(Resources.Load("Prefabs/Priest", typeof(GameObject))) as GameObject;
            }
            _passenger.name = "passenger" + count.ToString();
            ++count;
        }

        public void setPosition(Vector3 pos) {
            _passenger.transform.position = pos;
        }
    }

    public class Coast {
        public Coast(Vector3 pos) {
            GameObject.Instantiate(Resources.Load("Prefabs/Coast", typeof(GameObject)), pos, Quaternion.identity);
        }
    }

    public class River {
        public River(Vector3 pos) {
            GameObject.Instantiate(Resources.Load("Prefabs/River", typeof(GameObject)), pos, Quaternion.identity);
        }
    }
}

