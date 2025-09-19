using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class RadialExpandWithRealCollider_JointSafe : MonoBehaviour
{
    [Header("Refs")]
    public Transform satellitesParent;            // "Küreler" parent
    public string visualChildName = "Visual";     // opsiyonel görsel child adı

    [Header("Motion & Collider")]
    public float expandDistance = 1.0f;           // W basılıyken merkezden ek yarıçap
    public float colliderRadiusMultiplier = 1.5f; // W basılıyken gerçek SphereCollider.radius katı
    public float tweenDuration = 0.45f;
    public Ease ease = Ease.InOutSine;
    public bool keepFlatY = true;                 // Y düzleminde aç

    [Header("Visual (opsiyonel)")]
    public float visualScaleMultiplier = 1.2f;

    struct Sat
    {
        public Transform tr;
        public Rigidbody rb;
        public SphereCollider col;
        public Transform visual;

        public Vector3 baseDir;           // merkezden birim yön
        public float baseOrbitRadius;     // merkez mesafesi
        public float baseColRadius;       // SphereCollider.radius
        public Vector3 baseVisualScale;

        public bool prevKinematic;
        public bool prevDetect;
        public Vector3 prevVel, prevAngVel;

        public Joint[] joints;
        public Rigidbody[] jointsPrevConnectedBody;   // eski connectedBody’ler
        public bool[] jointsPrevAutoConn;             // ConfigurableJoint.autoConfigureConnectedAnchor
    }

    List<Sat> sats = new List<Sat>();
    float t; Tween tTw;

    void Start()
    {
        if (!satellitesParent)
        {
            Debug.LogError("[RadialExpand] satellitesParent atanmalı.");
            enabled = false; return;
        }

        foreach (Transform child in satellitesParent)
        {
            var rb  = child.GetComponent<Rigidbody>();
            var col = child.GetComponent<SphereCollider>();
            if (!rb || !col) continue;

            Vector3 d = (rb.worldCenterOfMass - transform.position);
            if (keepFlatY) d.y = 0f;
            if (d.sqrMagnitude < 1e-6f) d = Vector3.right;
            d.Normalize();

            Transform visual = string.IsNullOrEmpty(visualChildName) ? null : child.Find(visualChildName);
            if (!visual) visual = child;

            var joints = child.GetComponents<Joint>();

            var s = new Sat
            {
                tr = child,
                rb = rb,
                col = col,
                visual = visual,
                baseDir = d,
                baseOrbitRadius = (rb.position - transform.position).magnitude,
                baseColRadius = col.radius,
                baseVisualScale = visual.localScale,
                prevKinematic = rb.isKinematic,
                prevDetect = rb.detectCollisions,
                joints = joints,
                jointsPrevConnectedBody = new Rigidbody[joints.Length],
                jointsPrevAutoConn = new bool[joints.Length]
            };

            rb.interpolation = RigidbodyInterpolation.Interpolate;
            sats.Add(s);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) BeginTransition(true);
        if (Input.GetKeyUp(KeyCode.W))   BeginTransition(false);
    }

    void FixedUpdate()
{
    if (tTw == null || !tTw.IsActive()) return;

    Vector3 cpos = transform.position;

    for (int i = 0; i < sats.Count; i++)
    {
        var s = sats[i];

        // 1) Merkezden radyal konum
        float targetOrbit = Mathf.Lerp(s.baseOrbitRadius, s.baseOrbitRadius + expandDistance, t);
        Vector3 targetPos = cpos + s.baseDir * targetOrbit;
        if (keepFlatY) targetPos.y = s.rb.position.y;
        s.rb.MovePosition(targetPos);

        
        
            // === MOD: Görsel yok -> Aynı Transform büyütülür ===
            // Büyüklük oranı (1..colliderRadiusMultiplier)
            float ratio = Mathf.Lerp(1f, colliderRadiusMultiplier, t);

            // Transform ölçeğini büyüt
            s.visual.localScale = s.baseVisualScale * ratio;

            // Collider.radius'ı SABİT tut (çünkü transform scale zaten collider'ı büyütüyor)
            s.col.radius = s.baseColRadius;
       

        sats[i] = s;
    }

    Physics.SyncTransforms();
}


    void BeginTransition(bool open)
    {
        // a) güvenli moda al: kinematic + collisions off, vel sakla
        for (int i = 0; i < sats.Count; i++)
        {
            var s = sats[i];
            s.prevKinematic = s.rb.isKinematic;
            s.prevDetect    = s.rb.detectCollisions;
            s.prevVel       = s.rb.linearVelocity;
            s.prevAngVel    = s.rb.angularVelocity;

            s.rb.isKinematic = true;
            s.rb.detectCollisions = false;

            // b) joint'leri geçici “kopar”: connectedBody=null, autoConfigure kapat
            for (int j = 0; j < s.joints.Length; j++)
            {
                var jnt = s.joints[j];

                s.jointsPrevConnectedBody[j] = jnt.connectedBody;

                if (jnt is ConfigurableJoint cj)
                {
                    s.jointsPrevAutoConn[j] = cj.autoConfigureConnectedAnchor;
                    cj.autoConfigureConnectedAnchor = false;
                }

                jnt.connectedBody = null; // geçici olarak dünyaya bağlı (free) hale getir
            }

            sats[i] = s;
        }

        float target = open ? 1f : 0f;
        tTw?.Kill();
        tTw = DOVirtual.Float(t, target, tweenDuration, v => t = v)
                .SetEase(ease)
                .OnComplete(EndTransition);
    }

    void EndTransition()
    {
        Vector3 cpos = transform.position;

        for (int i = 0; i < sats.Count; i++)
        {
            var s = sats[i];

            // Yeni hedef anchor (merkezden t’ye göre)
            float targetOrbit = Mathf.Lerp(s.baseOrbitRadius, s.baseOrbitRadius + expandDistance, t);
            Vector3 worldAnchorPos = cpos + s.baseDir * targetOrbit;
            if (keepFlatY) worldAnchorPos.y = s.rb.position.y;

            // Merkezin local uzayındaki anchor
            Vector3 connAnchorLocalToCenter = transform.InverseTransformPoint(worldAnchorPos);

            // jointleri geri tak ve anchor’u güncelle
            for (int j = 0; j < s.joints.Length; j++)
            {
                var jnt = s.joints[j];

                jnt.connectedAnchor = connAnchorLocalToCenter;

                if (jnt is ConfigurableJoint cj)
                {
                    cj.autoConfigureConnectedAnchor = s.jointsPrevAutoConn[j];
                    cj.projectionMode = JointProjectionMode.PositionAndRotation;
                    cj.projectionDistance = Mathf.Max(cj.projectionDistance, 0.05f);
                    cj.projectionAngle = Mathf.Max(cj.projectionAngle, 5f);
                }

                jnt.connectedBody = s.jointsPrevConnectedBody[j]; // eski body geri
            }

            Physics.SyncTransforms();

            // fizik modlarını geri yükle
            s.rb.isKinematic = s.prevKinematic;
            s.rb.detectCollisions = s.prevDetect;

            // patlama azaltmak için hızları sıfırla (istersen yorumlayıp eski hızları geri ver)
          
            // s.rb.velocity = s.prevVel;
            // s.rb.angularVelocity = s.prevAngVel;

            sats[i] = s;
        }
    }
}
