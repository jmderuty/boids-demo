using UnityEngine;
using System.Collections;

public class BoidBehavior : MonoBehaviour {
    public Color Color { get; set; }
    public GameObject ExplosionPrefab;

	// Use this for initialization
	void Start () {
		var vertices = new Vector3[]{ new Vector3 (0,1, 0), new Vector3 (3,0, 0), new Vector3 (0, -1, 0)};
		var triangles = new int[]{0,1,2};

		var mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
		mesh.vertices = vertices;
		mesh.triangles = triangles;

        GetComponent<MeshRenderer>().material.color = this.Color;
	}

    public void Explode(bool destroyed)
    {
        var explosionObj = (GameObject)GameObject.Instantiate(ExplosionPrefab, this.transform.position, Quaternion.identity);
        var explosion = explosionObj.GetComponent<Explosion>();
        explosion.IsDestruction = destroyed;
        explosion.Color = this.Color;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
