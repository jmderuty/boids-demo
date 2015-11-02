using UnityEngine;
using System.Collections;

public class Canon : MonoBehaviour
{

    private LineRenderer _lineRenderer;
    public float LaserDuration = 1.0f;

    private float _timeLastShot;

    // Use this for initialization
    void Start()
    {
        this._lineRenderer = this.GetComponent<LineRenderer>();
        this._lineRenderer.enabled = false;
        this._lineRenderer.useWorldSpace = true;
    }

    public void Shoot(Transform target, bool hit)
    {
        this._lineRenderer.SetPosition(0, this.transform.position);
        if (hit)
        {
            this._lineRenderer.SetPosition(1, target.position);
        }
        else
        {
            this._lineRenderer.SetPosition(1, target.position  + Vector3.Normalize(target.position - this.transform.position)*1000);
        }
        this._lineRenderer.enabled = true;

        var color = this._lineRenderer.material.color;
        color.a = 1;
        this._lineRenderer.material.color = color;

        this._timeLastShot = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (this._lineRenderer.enabled)
        {
            var elapsed = Time.time - this._timeLastShot;
            if (elapsed > this.LaserDuration)
            {
                this._lineRenderer.enabled = false;
            }
            else
            {
                var color = this._lineRenderer.material.color;
                color.a = (this.LaserDuration - elapsed) / LaserDuration;
                this._lineRenderer.material.color = color;
            }
        }
    }
}
