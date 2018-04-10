using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PAD_Module {
    // 表示对象在左岸或右岸
    public enum CoastPos {
        CoastLeft,
        CoastRight,
        BoatLeft,
        BoatRight
    }

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

        public GameObject gameobject() {
            return _boat;
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
            if (type == Type.Devil) {
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

        public GameObject gameobject() {
            return _passenger;
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

    public class Mountain {
        public Mountain(Vector3 pos) {
            GameObject.Instantiate(Resources.Load("Prefabs/Mountain", typeof(GameObject)), pos, Quaternion.identity);
        }
    }
}
