using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

using ActionManagement;
using PAD_Module;
using PAD_View;

public class Game : ActionManager {
    Controllor _controllor;

    // 船和乘客对象
    Passenger[] _passenger;
    Boat _boat;

    // 按钮的style
    GUIStyle backgroundStyle;
    GUIStyle boxStyle;

    // Use this for initialization
    void Start() {
        // "gameove"和"成功"的提示框
        backgroundStyle = new GUIStyle("box");
        backgroundStyle.alignment = TextAnchor.LowerCenter;
        backgroundStyle.fontSize = 100;
        // 标题
        boxStyle = new GUIStyle("Box");
        boxStyle.fontSize = 20;
        // 初始化对象
        _passenger = new Passenger[6];
        for (int i = 0; i != 3; ++i) {
            _passenger[i] = new Passenger(Passenger.Type.Priest);
        }
        for (int i = 3; i != 6; ++i) {
            _passenger[i] = new Passenger(Passenger.Type.Devil);
        }
        _boat = new Boat();
        new Coast(View.leftCoastPos);
        new Coast(View.rightCoastPos);
        new River(View.riverPos);
        new Mountain(View.Mountain);

        _controllor = new Controllor(_passenger, _boat, this);
    }

    private float clickWaiting = 0;
    // Update is called once per frame
    protected new void Update() {
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
        base.Update();
    }

    void OnGUI() {
        switch (_controllor.state) {
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
    public class Controllor {
        public enum GameState {
            waiting,
            moving,
            win,
            gameover
        }
        public GameState state;

        Passenger[] _passenger;
        Boat _boat;

        ActionManager _actionManager;

        public Controllor(Passenger[] _p, Boat _b, ActionManager am) {
            _passenger = _p;
            _boat = _b;
            _actionManager = am;
            state = GameState.waiting;
            reinitial();
        }

        public void reinitial() {
            for(int i=0; i != 6; ++i) {
                _passenger[i].CoastPos = CoastPos.CoastRight;
            }
            _boat.CoastPos = CoastPos.BoatRight;
            state = GameState.waiting;
            View.StaticRender(_passenger, _boat);
        }

        public void clickObject(string name) { // 点击
            if(state != GameState.waiting) {
                return;
            } else if(Regex.IsMatch(name, "^passenge")) {
                int index = int.Parse(name.Substring(name.Length - 1));
                CoastPos pPos = _passenger[index].CoastPos;
                CoastPos bPos = _boat.CoastPos;

                int onBoat = countOnBoat();

                if(onBoat < 2 && pPos == CoastPos.CoastLeft && bPos == CoastPos.BoatLeft) {
                    _passenger[index].CoastPos = bPos; // 上船
                    aboard(index);
                } else if(onBoat < 2 && pPos == CoastPos.CoastRight && bPos == CoastPos.BoatRight) {
                    _passenger[index].CoastPos = bPos; // 上船
                    aboard(index);
                } else if(pPos == CoastPos.BoatLeft) {
                    _passenger[index].CoastPos = CoastPos.CoastLeft; // 下船
                    ashore(index);
                } else if (pPos == CoastPos.BoatRight) { 
                    _passenger[index].CoastPos = CoastPos.CoastRight; // 下船
                    ashore(index);
                }
            } else if (Regex.IsMatch(name, "^Boat$")) {
                if(countOnBoat() != 0) {
                    if (_boat.CoastPos == CoastPos.BoatLeft) { // 从左岸过河
                        _boat.CoastPos = CoastPos.BoatRight;
                        for (int i = 0; i != 6; ++i) {
                            if (_passenger[i].CoastPos == CoastPos.BoatLeft) {
                                _passenger[i].CoastPos = CoastPos.BoatRight;
                            }
                        }
                    } else { // 从右岸过河
                        _boat.CoastPos = CoastPos.BoatLeft;
                        for (int i = 0; i != 6; ++i) {
                            if (_passenger[i].CoastPos == CoastPos.BoatRight) {
                                _passenger[i].CoastPos = CoastPos.BoatLeft;
                            }
                        }
                    }
                    state = GameState.moving;
                    Action cross = View.Action_BoatCross.getAction(_passenger, _boat, 1, Finish.getInstance(this));
                    _actionManager.RunAction(cross);
                }
            }
        }
        private void aboard(int index) {
            state = GameState.moving;
            Action ac = View.Action_Aboard.getAction(index, _passenger, _boat, Finish.getInstance(this));
            _actionManager.RunAction(ac);
        }
        private void ashore(int index) {
            state = GameState.moving;
            Action ac = View.Action_Ashore.getAction(index, _passenger, _boat, Finish.getInstance(this));
            _actionManager.RunAction(ac);
        }
        private int countOnBoat() {
            int count = 0;
            for(int i=0; i!=6; ++i) {
                if(_passenger[i].CoastPos == CoastPos.BoatLeft 
                    || _passenger[i].CoastPos == CoastPos.BoatRight) {
                    ++count;
                }
            }
            return count;
        }
        // 检查游戏是否结束
        private void checkState() {
            int leftDevil = 0, leftPriest = 0;
            int rightDevil = 0, rightPriest = 0;
            for(int i=0; i != 6; ++i) {
                if(_passenger[i].type == Passenger.Type.Devil) {
                    if(_passenger[i].CoastPos == CoastPos.BoatLeft || _passenger[i].CoastPos == CoastPos.CoastLeft) {
                        ++leftDevil;
                    } else {
                        ++rightDevil;
                    }
                } else {
                    if (_passenger[i].CoastPos == CoastPos.BoatLeft || _passenger[i].CoastPos == CoastPos.CoastLeft) {
                        ++leftPriest;
                    } else {
                        ++rightPriest;
                    }
                }
            }
            if((leftDevil > leftPriest && leftPriest != 0) 
                || (rightDevil > rightPriest && rightPriest != 0)) {
                state = GameState.gameover;
            } else if(leftDevil == 3 && leftPriest == 3) {
                state = GameState.win;
            }
        }

        // 动作的回调函数
        private class Finish : Callback {
            private static Controllor _controllor;
            private static Finish _instance;
            public static Finish getInstance(Controllor _c) {
                if(_instance == null) {
                    _controllor = _c;
                    _instance = new Finish();
                }
                return _instance;
            }
            public void call() {
                _controllor.state = GameState.waiting;
                _controllor.checkState();
            }
        }
    }
}
