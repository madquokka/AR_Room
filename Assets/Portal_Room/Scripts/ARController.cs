using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleARCore;

#if UNITY_EDITOR
using input = GoogleARCore.InstantPreviewInput;
#endif

public class ARController : MonoBehaviour
{


	//Fill this list with the planes that ARCore detected in the current frame.
	private List<TrackedPlane> m_NewTrackedPlanes = new List<TrackedPlane>();

    public GameObject GridPrefab;
    public GameObject Portal;
    public GameObject ARCamera;
    
    //Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //Check ARCore session status
        if(Session.Status != SessionStatus.Tracking)
        {
        	return;
        }

        //The following function will fill m_NewTrackedPlanes with the planes that ARCore detected in the current frame.
        Session.GetTrackables<TrackedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);

        //Instantiate a Grid for each TrackedPlane in m_NewTrackedPlanes
        for (int i=0; i < m_NewTrackedPlanes.Count; ++i)
        {
            GameObject grid = Instantiate(GridPrefab, Vector3.zero, Quaternion.identity, transform);

            //Set the position of grid and modify the vertices of the attached mesh
            grid.GetComponent<GridVisualizer>().Initialize(m_NewTrackedPlanes[i]);
        }

        Touch touch;
        if(Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        } 

        //Check if user touched any of tracked planes
        TrackableHit hit;
        if(Frame.Raycast(touch.position.x, touch.position.y, TrackableHitFlags.PlaneWithinPolygon, out hit))
        {
            //Place the portal on the top of tracked plane i.e touched

            //Enable the portal
            Portal.SetActive(true);

            //Create a new Anchor

            Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

            //Set the position of the portal to be the same as hit position
            Portal.transform.position = hit.Pose.position;
            Portal.transform.rotation = hit.Pose.rotation;

            //Portal faced to the camera
            Vector3 cameraPosition = ARCamera.transform.position;

            //The portal should only rotate around the Y axis
            cameraPosition.y = hit.Pose.position.y;

            //Rotate the portal to face the camera
            Portal.transform.LookAt(cameraPosition, Portal.transform.up);

            //ARCore will keep understanding the world and update the anchors accordingly hence need to attach portal to the anchor
            Portal.transform.parent = anchor.transform;
        }
    }
}
