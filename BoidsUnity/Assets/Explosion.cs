using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{

    public float ExplosionDuration = 2.0f;
    public float ImpactRadius = 5.0f;
    public float DestructionRadius = 10.0f;
    public float ImpactColor = 1f;
    public float DestructionColor = 0f;

    private MeshRenderer _meshRenderer;
    private float _explosionTime;

    public bool IsDestruction { get; set; }

    private Color _color;
    public Color Color
    {
        set
        {
            this._color = UnityEngine.Color.Lerp(value, Color.white, IsDestruction? DestructionColor : ImpactColor);
        }
    }

    private float Radius
    {
        get
        {
            return IsDestruction ? DestructionRadius : ImpactRadius;
        }
    }

    // Use this for initialization
    void Start()
    {
        this._meshRenderer = this.GetComponent<MeshRenderer>();
        this._meshRenderer.material.color = this._color;
        this._explosionTime = Time.time;
    }

    public void Explode()
    {
        this._meshRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        var t = (Time.time - this._explosionTime) / this.ExplosionDuration;
        if (t < 1)
        {
            this.transform.localScale = Vector3.one * t * this.Radius;

            var nowColor = this._color;
            nowColor.a = 1 - t;
            this._meshRenderer.material.color = nowColor;
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
