using UnityEngine;
using System.Collections;

public class BoidBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var vertices = new Vector3[]{ new Vector3 (-0.35f, 0, 0), new Vector3 (0,1, 0), new Vector3 (0.35f, 0, 0)};
		var triangles = new int[]{0,1,2};

		var mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
		mesh.vertices = vertices;
		mesh.triangles = triangles;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
