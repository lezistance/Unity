using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;


public class MoveObject : MonoBehaviour {

	Kinect.CoordinateMapper mapper;
	
	public GameObject bodyManager;
	public Kinect.JointType type; 
	private BodySourceManager bManager;

	public Camera Camera;
	Camera _Camera;
	
	int SensorWidth = 1920;
	int SensorHeight = 1080;
	
	private Dictionary<ulong, GameObject> bBuff = new Dictionary<ulong, GameObject>();
	
	
	// Use this for initialization
	void Start () {
		_Camera = GameObject.Find( Camera.name ).GetComponent<Camera>();
	}
	
	void Update () 
	{
		if ( bodyManager == null ) return;
		bManager = bodyManager.GetComponent<BodySourceManager>();
		
		if ( bManager == null ) return;
		if ( mapper == null ) mapper = bManager.Sensor.CoordinateMapper;
		
		Kinect.Body[] data = bManager.GetData();
		if ( data == null ) return;
	
		List<ulong> trackedIds = new List<ulong>();

		foreach(var body in data)
		{
			if (body == null) continue;
			if(body.IsTracked) trackedIds.Add (body.TrackingId);
		}
		
		List<ulong> knownIds = new List<ulong>(bBuff.Keys);
		
		foreach(ulong trackingId in knownIds)
		{
			if(!trackedIds.Contains(trackingId))
			{
				Destroy(bBuff[trackingId]);
				bBuff.Remove(trackingId);
			}
		}
		
		foreach(var body in data)
		{
			if (body == null) continue;
			if(body.IsTracked && type != null )
			{
				Vector3 pos = GetVector3FromJoint(body.Joints[type]); 

				Renderer r = GetComponentInChildren<Renderer>();
				transform.position = new Vector3( pos.x ,( pos.y + r.bounds.extents.y / 2 ), -2f );

			}
		}
	}
	
	private Vector3 GetVector3FromJoint(Kinect.Joint joint)
	{
		var valid = joint.TrackingState != Kinect.TrackingState.NotTracked;
		
		if ( Camera != null || valid ) {
			// KinectのCamera座標系(3次元)をColor座標系(2次元)に変換する
			var point =mapper.MapCameraPointToColorSpace( joint.Position );
			var point2 = new Vector3( point.X, point.Y, 0 );
			if ( (0<= point2.x) && (point2.x < SensorWidth) && (0 <= point2.y) && (point2.y < SensorHeight) ) {
				// スクリーンサイズで調整(Kinect->Unity)
				point2.x = point2.x * Screen.width / SensorWidth;
				point2.y = point2.y * Screen.height / SensorHeight;
				
				// Unityのワールド座標系(3次元)に変換
				var colorPoint3 = _Camera.ScreenToWorldPoint( point2 );
				
				// 座標の調整
				// Y座標は逆、Z座標は0にする(Xもミラー状態によって逆にする必要あり)
				colorPoint3.y *= -1;
				colorPoint3.z = 0;
				
				return colorPoint3;
			}
		}
		
		// 適当に返す
		return new Vector3( joint.Position.X * 10, joint.Position.Y * 10, 0 );
	}
}
