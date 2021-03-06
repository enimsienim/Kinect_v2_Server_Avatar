﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnetLLAPISample;
using UnityEngine;
using Kinect = Windows.Kinect;

[Serializable]
public struct SimpleBody
{
	public List<SimpleJoint> Joints;
}

[Serializable]
public struct SimpleJoint
{
	public Vector3 Position;
	public int TrackingState;

	// Orientation
	public float X;
	public float Y;
	public float Z;
	public float W;
}

public class Body
{
	public Dictionary<Kinect.JointType, Joint> Joints;
}

public class Joint
{
	public Vector3 Position;
	public Kinect.TrackingState TrackingState;
	public Kinect.Vector4 Orientation;
}

public class KinectReceiver : MonoBehaviour
{
	public LLAPINetworkManager NetworkManager;

	public bool IsMirror = true;

	private Body localBody;
	private SimpleBody tmpBody = new SimpleBody();

	public GameObject _Avatar;

	public GameObject Ref;
	public GameObject Master;
	public GameObject Hips;
	public GameObject LeftUpLeg;
	public GameObject LeftLeg;
	public GameObject RightUpLeg;
	public GameObject RightLeg;
	public GameObject Spine1;
	public GameObject Spine2;
	public GameObject LeftShoulder;
	public GameObject LeftArm;
	public GameObject LeftForeArm;
	public GameObject LeftHand;
	public GameObject RightShoulder;
	public GameObject RightArm;
	public GameObject RightForeArm;
	public GameObject RightHand;
	public GameObject Neck;
	public GameObject Head;

	public int ct = 0;

	private void Awake()
	{
		localBody = CreateBody();
		tmpBody = initSimpleBody();
    }

	void Start()
	{
		NetworkManager.OnDataReceived += OnDataReceived;

		Ref = _Avatar.transform.Find( "Character1_Reference" ).gameObject;
		Master = Ref.gameObject.transform.Find( "Master" ).gameObject;
		Hips = Master.gameObject.transform.Find( "Character1_Hips" ).gameObject;
		LeftUpLeg = Hips.transform.Find( "Character1_LeftUpLeg" ).gameObject;
		LeftLeg = LeftUpLeg.transform.Find( "Character1_LeftLeg" ).gameObject;
		RightUpLeg = Hips.transform.Find( "Character1_RightUpLeg" ).gameObject;
		RightLeg = RightUpLeg.transform.Find( "Character1_RightLeg" ).gameObject;
		Spine1 = Hips.transform.Find( "Character1_Spine" ).
			gameObject.transform.Find( "Character1_Spine1" ).gameObject;
		Spine2 = Spine1.transform.Find( "Character1_Spine2" ).gameObject;
		LeftShoulder = Spine2.transform.Find( "Character1_LeftShoulder" ).gameObject;
		LeftArm = LeftShoulder.transform.Find( "Character1_LeftArm" ).gameObject;
		LeftForeArm = LeftArm.transform.Find( "Character1_LeftForeArm" ).gameObject;
		LeftHand = LeftForeArm.transform.Find( "Character1_LeftHand" ).gameObject;
		RightShoulder = Spine2.transform.Find( "Character1_RightShoulder" ).gameObject;
		RightArm = RightShoulder.transform.Find( "Character1_RightArm" ).gameObject;
		RightForeArm = RightArm.transform.Find( "Character1_RightForeArm" ).gameObject;
		RightHand = RightForeArm.transform.Find( "Character1_RightHand" ).gameObject;
		Neck = Spine2.transform.Find( "Character1_Neck" ).gameObject;
		Head = Neck.transform.Find( "Character1_Head" ).gameObject;
	}

	SimpleBody initSimpleBody()
	{
		var sb = new SimpleBody();
		var joints = new List<SimpleJoint>();
		for (int jt = 0; jt <= 24; jt++)
		{
			var joint = new SimpleJoint ();
			joint.Position = Vector3.zero;
			joint.TrackingState = (int)Kinect.TrackingState.NotTracked;
			joint.X = 0.0f;
			joint.Y = 0.0f;
			joint.Z = 0.0f;
			joint.W = 0.0f;
			joints.Add(joint);
		}
		sb.Joints = joints;
		return sb;
	}

	Body CreateBody()
	{
		var body = new Body();
		var joints = new Dictionary<Kinect.JointType, Joint>();
		for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
		{
			var joint = new Joint();
			joint.Position = Vector3.zero;
			joint.TrackingState = Kinect.TrackingState.NotTracked;
			joint.Orientation = new Kinect.Vector4(){X = 0, Y = 0, Z = 0, W = 0};
			joints.Add(jt, joint);
		}
		body.Joints = joints;
		return body;
	}

	void OnDataReceived(object o, LLAPINetworkEventArgs e)
	{
		SimpleBody simpleBody = new SimpleBody();

		string output;
		var data = e.data;
		using (var inStream = new MemoryStream(data))
		using (var bigStream = new System.IO.Compression.GZipStream(inStream, System.IO.Compression.CompressionMode.Decompress))
		using (var reader = new StreamReader(bigStream))
		{
			output = reader.ReadToEnd();
		}

		simpleBody = JsonUtility.FromJson<SimpleBody>(output);
//		Debug.Log ("output");
		Debug.Log (DistinguishSimpleBody(simpleBody));
		Debug.Log (output);
//		Debug.Log ("simpleBody");
//		Debug.Log (DistinguishSimpleBody(simpleBody));
//		Debug.Log ("count");
//		Debug.Log (simpleBody.Joints.Count);

		if (DistinguishSimpleBody(tmpBody) == 1)
		{
			simpleBody = CombineSimpleBody (tmpBody, simpleBody);
			Debug.Log ("SB1 SB2 combined");
			tmpBody = initSimpleBody();
			//Debug.Log(1);
		}
		else if (DistinguishSimpleBody(tmpBody) == 2)
		{
			simpleBody = CombineSimpleBody (simpleBody, tmpBody);
			Debug.Log ("SB1 SB2 combined");
			tmpBody = initSimpleBody();
			//Debug.Log(2);
		}
		else if (DistinguishSimpleBody(tmpBody) == 0)
		{
			tmpBody = simpleBody;
			//Debug.Log(0);
		}
		Debug.Log ("Count combine");
		Debug.Log (simpleBody.Joints.Count);

		// localBodyへsimpleBodyの値を代入
		if (simpleBody.Joints.Count == 25) {
			for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
			{
				var j = localBody.Joints[jt];
				var sj = simpleBody.Joints[(int)jt];
				j.Position = sj.Position;
				j.TrackingState = (Kinect.TrackingState)sj.TrackingState;
				j.Orientation = ToVector4 (sj.X/10000, sj.Y/10000, sj.Z/10000, sj.W/10000);
			}
		}

		// Bodyデータを取得する
		var joints = localBody.Joints;
		var comp = Quaternion.FromToRotation (new Vector3(0, 0, 0), Vector3.up);

		Quaternion SpineBase;
		Quaternion SpineMid;
		Quaternion SpineShoulder;
		Quaternion ShoulderLeft;
		Quaternion ShoulderRight;
		Quaternion ElbowLeft;
		Quaternion WristLeft;
		Quaternion HandLeft;
		Quaternion ElbowRight;
		Quaternion WristRight;
		Quaternion HandRight;
		Quaternion KneeLeft;
		Quaternion AnkleLeft;
		Quaternion KneeRight;
		Quaternion AnkleRight;

		// 鏡
		if ( IsMirror ) {
			SpineBase = joints[Kinect.JointType.SpineBase].Orientation.ToMirror().ToQuaternion( comp );
			SpineMid = joints[Kinect.JointType.SpineMid].Orientation.ToMirror().ToQuaternion( comp );
			SpineShoulder = joints[Kinect.JointType.SpineShoulder].Orientation.ToMirror().ToQuaternion( comp );
			ShoulderLeft = joints[Kinect.JointType.ShoulderRight].Orientation.ToMirror().ToQuaternion( comp );
			ShoulderRight = joints[Kinect.JointType.ShoulderLeft].Orientation.ToMirror().ToQuaternion( comp );
			ElbowLeft = joints[Kinect.JointType.ElbowRight].Orientation.ToMirror().ToQuaternion( comp );
			WristLeft = joints[Kinect.JointType.WristRight].Orientation.ToMirror().ToQuaternion( comp );
			HandLeft = joints[Kinect.JointType.HandRight].Orientation.ToMirror().ToQuaternion( comp );
			ElbowRight = joints[Kinect.JointType.ElbowLeft].Orientation.ToMirror().ToQuaternion( comp );
			WristRight = joints[Kinect.JointType.WristLeft].Orientation.ToMirror().ToQuaternion( comp );
			HandRight = joints[Kinect.JointType.HandLeft].Orientation.ToMirror().ToQuaternion( comp );
			KneeLeft = joints[Kinect.JointType.KneeRight].Orientation.ToMirror().ToQuaternion( comp );
			AnkleLeft = joints[Kinect.JointType.AnkleRight].Orientation.ToMirror().ToQuaternion( comp );
			KneeRight = joints[Kinect.JointType.KneeLeft].Orientation.ToMirror().ToQuaternion( comp );
			AnkleRight = joints[Kinect.JointType.AnkleLeft].Orientation.ToMirror().ToQuaternion( comp );
		}
		// そのまま
		else {
			SpineBase = joints[Kinect.JointType.SpineBase].Orientation.ToQuaternion( comp );
			SpineMid = joints[Kinect.JointType.SpineMid].Orientation.ToQuaternion( comp );
			SpineShoulder = joints[Kinect.JointType.SpineShoulder].Orientation.ToQuaternion( comp );
			ShoulderLeft = joints[Kinect.JointType.ShoulderLeft].Orientation.ToQuaternion( comp );
			ShoulderRight = joints[Kinect.JointType.ShoulderRight].Orientation.ToQuaternion( comp );
			ElbowLeft = joints[Kinect.JointType.ElbowLeft].Orientation.ToQuaternion( comp );
			WristLeft = joints[Kinect.JointType.WristLeft].Orientation.ToQuaternion( comp );
			HandLeft = joints[Kinect.JointType.HandLeft].Orientation.ToQuaternion( comp );
			ElbowRight = joints[Kinect.JointType.ElbowRight].Orientation.ToQuaternion( comp );
			WristRight = joints[Kinect.JointType.WristRight].Orientation.ToQuaternion( comp );
			HandRight = joints[Kinect.JointType.HandRight].Orientation.ToQuaternion( comp );
			KneeLeft = joints[Kinect.JointType.KneeLeft].Orientation.ToQuaternion( comp );
			AnkleLeft = joints[Kinect.JointType.AnkleLeft].Orientation.ToQuaternion( comp );
			KneeRight = joints[Kinect.JointType.KneeRight].Orientation.ToQuaternion( comp );
			AnkleRight = joints[Kinect.JointType.AnkleRight].Orientation.ToQuaternion( comp );
		}

		// 関節の回転を計算する
		Quaternion q = transform.rotation;
		transform.rotation = Quaternion.identity;

		var comp2 = Quaternion.AngleAxis (-90, new Vector3 (0, 1, 0)) *
			Quaternion.AngleAxis (-0, new Vector3 (0, 0, 1));

		Spine1.transform.rotation = SpineMid * comp2;
		RightArm.transform.rotation = ElbowRight * comp2;
		RightForeArm.transform.rotation = WristRight * Quaternion.AngleAxis (90, new Vector3 (0, 1, 0)) *
			Quaternion.AngleAxis (-0, new Vector3 (0, 0, 1));
		RightHand.transform.rotation = HandRight * comp2;

		LeftArm.transform.rotation = ElbowLeft * comp2;
		LeftForeArm.transform.rotation = WristLeft * comp2;
		LeftHand.transform.rotation = HandLeft * comp2;

		RightUpLeg.transform.rotation = KneeRight * comp2;
		RightLeg.transform.rotation = AnkleRight * comp2;

		RightArm.transform.rotation = ElbowRight * Quaternion.AngleAxis (90, new Vector3 (0, 1, 0));
		RightForeArm .transform.rotation = WristRight * Quaternion.AngleAxis (90, new Vector3 (0, 1, 0));

		LeftUpLeg.transform.rotation = KneeLeft * Quaternion.AngleAxis (0, new Vector3 (0, 0, 1));
		LeftLeg.transform.rotation = AnkleLeft * Quaternion.AngleAxis (0, new Vector3 (0, 0, 1));
		LeftUpLeg.transform.rotation = KneeLeft * Quaternion.AngleAxis (180, new Vector3 (0, 1, 0));
		LeftLeg.transform.rotation = AnkleLeft * Quaternion.AngleAxis (180, new Vector3 (0, 1, 0));
		RightUpLeg.transform.rotation = KneeRight * Quaternion.AngleAxis (0, new Vector3 (0, 1, 0));
		RightLeg.transform.rotation = AnkleRight * Quaternion.AngleAxis (0, new Vector3 (0, 1, 0));

		// モデルの回転を設定する
		transform.rotation = q;

		// モデルの位置を移動する
		var pos = localBody.Joints[Kinect.JointType.SpineMid].Position;
		Ref.transform.position = new Vector3( -pos.x, pos.y, -pos.z );
	}

	// 受け取ったSimpleBodyを判別
	int DistinguishSimpleBody(SimpleBody sb)
	{
//		Debug.Log ("disting");
//		Debug.Log (sb.Joints.Count);
		if (sb.Joints.Count == 13)	// 13 = SimpleBody1の関節の数
		{
			return 1;
		}
		else if (sb.Joints.Count == 12)	// 12 = SimpleBody2の関節の数
		{
			return 2;
		}
		else
		{
			return 0;
		}
	}

	SimpleBody CombineSimpleBody(SimpleBody sb1, SimpleBody sb2)
	{
		for (int jt = 0; jt < sb2.Joints.Count; jt++)
		{
			sb1.Joints.Add(sb2.Joints[jt]);
		}

		return sb1;
	}

	Kinect.Vector4 ToVector4(float x, float y, float z, float w)
	{
		return new Kinect.Vector4()
		{
			X = x,
			Y = y,
			Z = z,
			W = w
		};
	}
}