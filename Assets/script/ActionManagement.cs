using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionManagement {

    // 结束时需要调用callback，并把destory设置成true
    public class Action : ScriptableObject {
        public GameObject gameobject { get; set; }
        public Callback callback;

        public bool enable = true;
        public bool destroy = false; // 当动作完成后，应令destroy为true

        public virtual void Start() {
            throw new System.NotImplementedException();
        }
        public virtual void Update() {
            throw new System.NotImplementedException();
        }
    }

    public class SequenceAction : Action, Callback {
        public List<Action> sequence;
        private int i = 0;

        public static Action getAction(List<Action> _seq, Callback callback) {
            SequenceAction action = ScriptableObject.CreateInstance<SequenceAction>();
            action.sequence = _seq;
            action.callback = callback;
            return action;
        }

        public override void Start() {
            foreach(Action ac in sequence) {
                ac.callback = this;
            }
            if(sequence.Count != 0) {
                sequence[0].Start();
            } else {
                callback.call();
                destroy = true;
            }
        }
        public override void Update() {
            sequence[i].Update();
        }
        // 子动作的回调函数
        public void call() {
            sequence[i].destroy = true;
            if (++i < sequence.Count) {
                sequence[i].Start();
            } else {
                callback.call();
                destroy = true;
            }
        }
    }

    public interface Callback {
        void call();
    }

    public class ActionManager : MonoBehaviour {
        private Dictionary<int, Action> actions = new Dictionary<int, Action>();
        private List<Action> waitingAdd = new List<Action>();
        private List<int> waitingDelete = new List<int>();
  
        protected void Update() {
            foreach (Action ac in waitingAdd) {
                actions[ac.GetInstanceID()] = ac;
            }
            waitingAdd.Clear();
            foreach(KeyValuePair<int, Action> kv in actions) {
                Action ac = kv.Value;
                if(ac.destroy) {
                    waitingDelete.Add(kv.Key);
                } else if(ac.enable) {
                    ac.Update();
                }
            }
            foreach(int key in waitingDelete) {
                Action ac = actions[key];
                actions.Remove(key);
                DestroyObject(ac);
            }
            waitingDelete.Clear();
        }

        public void RunAction(Action action) {
            waitingAdd.Add(action);
            action.Start();
        }
    }
}
