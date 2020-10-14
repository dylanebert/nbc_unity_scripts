using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Valve.VR.InteractionSystem;

public class DataCollection : MonoBehaviour
{

    [System.Serializable]
    public class ObjectData {
        public int step;
        public float timestamp;
        public string name;
        public string id;
        public bool dynamic;
        public bool visible;
        public float posX;
        public float posY;
        public float posZ;
        public float rotX;
        public float rotY;
        public float rotZ;
        public float rotW;
        public float relPosX;
        public float relPosY;
        public float relPosZ;
        public float relRotX;
        public float relRotY;
        public float relRotZ;
        public float relRotW;
        public float velX;
        public float velY;
        public float velZ;
        public float relVelX;
        public float relVelY;
        public float relVelZ;
        public float boundX;
        public float boundY;
        public float boundZ;
    }

    public Transform head;
    List<GameObject> staticObjects;
    List<GameObject> dynamicObjects;

    StreamWriter writer;
    int timestep = 0;
    bool recording = false;
    float recordStartTime;

    //Initialization
    private void Start() {
        staticObjects = GameObject.FindGameObjectsWithTag("Static").ToList();
        dynamicObjects = GameObject.FindGameObjectsWithTag("Dynamic").ToList();
    }

    void StartRecording() {
        writer = new StreamWriter("/path/to/data.txt");
        Debug.Log("Started recording");
        recording = true;
        recordStartTime = Time.time;
        foreach (GameObject obj in dynamicObjects) {
            if (obj.GetComponent<VelocityEstimator>() != null)
                obj.GetComponent<VelocityEstimator>().BeginEstimatingVelocity();
        }
    }

    void StopRecording() {
        recording = false;
        Debug.Log("Stopped recording");
        foreach (GameObject obj in dynamicObjects) {
            if (obj != null) {
                if (obj.GetComponent<VelocityEstimator>() != null)
                    obj.GetComponent<VelocityEstimator>().FinishEstimatingVelocity();
            }
        }
        writer.Close();
        Application.Quit();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.R)) {
            if(!recording) {
                StartRecording();
            } else {
                StopRecording();
            }
        }

        if (recording) {
            WriteFrame();
            timestep += 1;
        }
    }

    bool IsVisible(Renderer renderer) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            return true;
        else
            return false;
    }

    void WriteFrame() {
        List<ObjectData> frameData = new List<ObjectData>();
        foreach (GameObject obj in dynamicObjects)
            WriteObjectData(obj);
        foreach (GameObject obj in staticObjects)
            WriteObjectData(obj);
    }

    void WriteObjectData(GameObject obj) {
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        bool visible = false;
        if (renderer != null)
            visible = IsVisible(renderer);
        else if (obj.name == "LeftHand" || obj.name == "RightHand" || obj.name == "Head")
            visible = true;
        else
            Debug.LogError("Missing renderer on " + obj.name);
        bool dynamic = obj.tag == "Dynamic";
        Vector3 pos = obj.transform.position;
        Quaternion rot = obj.transform.rotation;
        Vector3 vel;
        if (obj.GetComponent<Hand>() != null) {
            vel = obj.GetComponent<Hand>().GetTrackedObjectVelocity();
        }
        else if (obj.GetComponent<Rigidbody>() != null) {
            vel = obj.GetComponent<Rigidbody>().velocity;
        }
        else if (obj.GetComponent<VelocityEstimator>() != null) {
            vel = obj.GetComponent<VelocityEstimator>().GetVelocityEstimate();
        }
        else {
            if (obj.tag == "Dynamic")
                Debug.LogError("Missing velocity component on " + obj.name);
            vel = Vector3.zero;
        }
        Vector3 relPos = head.InverseTransformDirection(obj.transform.position - head.position);
        Quaternion relRot = Quaternion.Inverse(head.rotation) * obj.transform.rotation;
        Vector3 relVel = head.InverseTransformDirection(vel);
        Vector3 extents = GetBounds(obj).extents;
        ObjectData data = new ObjectData() {
            step = timestep,
            timestamp = ((Time.time - recordStartTime) * 1000),
            name = obj.name,
            id = obj.GetComponent<Tracked>().id,
            visible = visible,
            dynamic = dynamic,
            posX = pos.x,
            posY = pos.y,
            posZ = pos.z,
            rotX = rot.x,
            rotY = rot.y,
            rotZ = rot.z,
            rotW = rot.w,
            velX = vel.x,
            velY = vel.y,
            velZ = vel.z,
            relPosX = relPos.x,
            relPosY = relPos.y,
            relPosZ = relPos.z,
            relRotX = relRot.x,
            relRotY = relRot.y,
            relRotZ = relRot.z,
            relRotW = relRot.w,
            relVelX = relVel.x,
            relVelY = relVel.y,
            relVelZ = relVel.z,
            boundX = extents.x,
            boundY = extents.y,
            boundZ = extents.z
        };
        string json = JsonUtility.ToJson(data);
        writer.WriteLine(json);
    }

    Bounds GetBounds(GameObject obj) {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;
        if(obj.GetComponent<Renderer>() != null) {
            bounds = obj.GetComponent<Renderer>().bounds;
            hasBounds = true;
        }
        for(int i = 0; i < obj.transform.childCount; i++) {
            Renderer renderer = obj.transform.GetChild(i).GetComponent<Renderer>();
            if(renderer != null) {
                if(hasBounds) {
                    bounds.Encapsulate(renderer.bounds);
                } else {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
            }
        }
        return bounds;
    }
}
