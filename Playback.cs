using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Valve.VR;
using UnityEngine.XR;

public class Playback : MonoBehaviour
{
    public Camera playbackCam;
    public GameObject player;
    public bool recording;

    StreamReader reader;
    Dictionary<string, GameObject> dict;
    List<Tracked> candidates;
    int currentFrame;

    private void Awake() {
        XRDevice.DisableAutoXRCameraTracking(playbackCam, true);
        player.SetActive(false);
    }

    private void Start() {
        reader = new StreamReader("/path/to/data.txt");
        dict = new Dictionary<string, GameObject>();
        candidates = FindObjectsOfType<Tracked>().ToList();
        foreach (Tracked tracked in candidates)
            if (tracked.GetComponent<Rigidbody>())
                tracked.GetComponent<Rigidbody>().isKinematic = true;
        StartCoroutine(Play());
    }

    void PopulateDict(DataCollection.ObjectData objData) {
        string id = objData.id;
        foreach (Tracked tracked in candidates)
            if (tracked.id == id)
                dict[id] = tracked.gameObject;
    }

    IEnumerator Step(List<DataCollection.ObjectData> frame) {
        foreach(DataCollection.ObjectData objData in frame) {
            GameObject obj = dict[objData.id];
            obj.transform.position = new Vector3(objData.posX, objData.posY, objData.posZ);
            obj.transform.rotation = new Quaternion(objData.rotX, objData.rotY, objData.rotZ, objData.rotW);
        }
        if (recording) {
            ScreenCapture.CaptureScreenshot("/path/to/images/" + currentFrame.ToString() + ".jpg");
        }
        yield return null;
    }

    IEnumerator Play() {
        string line;
        List<DataCollection.ObjectData> frame = new List<DataCollection.ObjectData>();
        while((line = reader.ReadLine()) != null) {
            DataCollection.ObjectData objData = JsonUtility.FromJson<DataCollection.ObjectData>(line);
            if(objData.step == currentFrame) {
                frame.Add(objData);
            } else {
                if (currentFrame == 0)
                    foreach (DataCollection.ObjectData obj in frame)
                        PopulateDict(obj);
                yield return Step(frame);
                frame = new List<DataCollection.ObjectData> {
                    objData
                };
                currentFrame += 1;
            }
        }
        Application.Quit();
    }
}
