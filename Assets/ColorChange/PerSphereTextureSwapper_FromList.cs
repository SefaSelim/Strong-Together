using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerSphereTextureSwapper_FromList : MonoBehaviour
{
    [Header("Defaults (yalnýzca slot BOÞSA kopyalanýr)")]
    [SerializeField] List<Texture> defaultTextures = new();
    [SerializeField] List<Color> defaultColors = new();
    [SerializeField] List<Material> defaultMaterials = new();

    [SerializeField] bool applyInitialOnStart = true;
    [SerializeField] bool staggerInitialIndices = true;

    [Header("Template kopyalama (eþleþme olmazsa)")]
    [Tooltip("Eþleþme bulunamazsa, editörde doldurduðun bir slotu þablon olarak kopyala.")]
    [SerializeField] bool useTemplateIfNoMatch = true;
    [Tooltip("Þablon olarak kullanýlacak eski slot indexi. -1 ise ilk dolu slot otomatik seçilir.")]
    [SerializeField] int templateSlotIndex = -1;

    [Header("Debug (isteðe baðlý)")]
    [SerializeField] bool debugKeys = true;
    [SerializeField] KeyCode testTextureKey = KeyCode.T;
    [SerializeField] KeyCode testColorKey = KeyCode.C;
    [SerializeField] KeyCode testMaterialKey = KeyCode.M;
    [SerializeField] KeyCode testAllKey = KeyCode.Space;

    [Serializable]
    public class SphereSlot
    {
        public string id;
        public GameObject sphereRoot;
        public List<Renderer> renderers = new();

        [Header("Opsiyonel sýra listeleri (boþsa defaults kullanýlýr)")]
        public List<Texture> textures = new(); [HideInInspector] public int currentTexIndex = 0;
        public List<Color> colors = new(); [HideInInspector] public int currentColorIndex = 0;
        public List<Material> materials = new(); [HideInInspector] public int currentMatIndex = 0;

        [Tooltip("Renderer birden çok materyal slotuna sahipse hangi index (0=ilk)")]
        public int materialElement = 0;

        [Header("Shader property override (gerekirse)")]
        public string texturePropertyOverride = ""; // URP:_BaseMap, Built-in:_MainTex
        public string colorPropertyOverride = ""; // URP:_BaseColor, Built-in:_Color
    }

    [SerializeField] public List<SphereSlot> spheres = new();
    readonly Dictionary<Renderer, MaterialPropertyBlock> _blocks = new();

    // ==================== Lifecycle ====================
    void Start()
    {
        StartCoroutine(BindFromManagerAndApply());
    }

    IEnumerator BindFromManagerAndApply()
    {
        while (SpheresManager.Instance == null || SpheresManager.Instance.Spheres == null)
            yield return null;  // manager hazýr olsun
        yield return null;      // spawn 1 frame sonra dolsun

        CollectFromSpheresManager();

        if (applyInitialOnStart)
            ApplyInitial();
    }

    void Update()
    {
        if (!Application.isPlaying || !debugKeys) return;

        if (Input.GetKeyDown(testTextureKey)) NextAllTextures();
        if (Input.GetKeyDown(testColorKey)) NextAllColors();
        if (Input.GetKeyDown(testMaterialKey)) NextAllMaterials(true);
        if (Input.GetKeyDown(testAllKey)) CycleAll();
    }

    // ==================== Toplama (MERGE + TEMPLATE) ====================
    public void CollectFromSpheresManager()
    {
        var list = SpheresManager.Instance.Spheres;
        if (list == null) return;

        // 1) Eski slotlarý map'le
        var oldByGo = new Dictionary<GameObject, SphereSlot>();
        var oldByName = new Dictionary<string, SphereSlot>(StringComparer.OrdinalIgnoreCase);
        SphereSlot template = null;

        for (int i = 0; i < spheres.Count; i++)
        {
            var s = spheres[i];
            if (s == null) continue;

            if (s.sphereRoot != null && !oldByGo.ContainsKey(s.sphereRoot))
                oldByGo.Add(s.sphereRoot, s);

            if (!string.IsNullOrEmpty(s.id) && !oldByName.ContainsKey(s.id))
                oldByName.Add(s.id, s);

            // Þablon seçimi: ya belirli index, ya da ilk dolu slot
            if (useTemplateIfNoMatch)
            {
                bool isTemplateIndex = (templateSlotIndex >= 0 && i == templateSlotIndex);
                bool looksFilled = (s.textures.Count > 0 || s.colors.Count > 0 || s.materials.Count > 0);
                if ((template == null && isTemplateIndex) || (template == null && templateSlotIndex < 0 && looksFilled && s.sphereRoot == null))
                    template = s;
            }
        }

        // 2) Yeni listeyi oluþtur
        var newList = new List<SphereSlot>();

        for (int i = 0; i < list.Count; i++)
        {
            var go = list[i];
            if (go == null) continue;

            SphereSlot slot = null;

            // a) Ayný GameObject referansý ile eþleþ
            if (!oldByGo.TryGetValue(go, out slot))
            {
                // b) Ýsimle eþleþ (id veya GameObject adý)
                oldByName.TryGetValue(go.name, out slot);
            }

            if (slot == null)
            {
                // c) Þablondan kopyala
                if (template != null)
                {
                    slot = CloneFromTemplate(template);
                }
                else
                {
                    // d) Sýfýrdan
                    slot = new SphereSlot();
                }
            }

            // her hâlükârda bu sphere'a baðla ve renderer'larý topla
            slot.id = string.IsNullOrEmpty(slot.id) ? go.name : slot.id;
            slot.sphereRoot = go;
            slot.renderers.Clear();
            go.GetComponentsInChildren(slot.renderers);

            // default'lar sadece TAMAMEN boþsa sonra eklenecek (ApplyInitial içinde)
            newList.Add(slot);
        }

        spheres = newList;
        Debug.Log($"[PerSphereTextureSwapper_FromList] Collected {spheres.Count} spheres (merge+template).");
    }

    SphereSlot CloneFromTemplate(SphereSlot t)
    {
        return new SphereSlot
        {
            id = t.id,
            sphereRoot = null, // yeni root birazdan atanacak
            materialElement = t.materialElement,
            texturePropertyOverride = t.texturePropertyOverride,
            colorPropertyOverride = t.colorPropertyOverride,

            // derin kopya (list referans paylaþma!)
            textures = new List<Texture>(t.textures),
            colors = new List<Color>(t.colors),
            materials = new List<Material>(t.materials),

            currentTexIndex = t.currentTexIndex,
            currentColorIndex = t.currentColorIndex,
            currentMatIndex = t.currentMatIndex,

            renderers = new List<Renderer>() // sonra dolduracaðýz
        };
    }

    // ==================== Baþlangýç Uygulamasý ====================
    void ApplyInitial()
    {
        for (int i = 0; i < spheres.Count; i++)
        {
            var s = spheres[i];

            // Defaults SADECE tamamen boþsa
            if (s.materials.Count == 0 && defaultMaterials.Count > 0) s.materials.AddRange(defaultMaterials);
            if (s.textures.Count == 0 && defaultTextures.Count > 0) s.textures.AddRange(defaultTextures);
            if (s.colors.Count == 0 && defaultColors.Count > 0) s.colors.AddRange(defaultColors);

            if (staggerInitialIndices)
            {
                if (s.textures.Count > 0) s.currentTexIndex = i % s.textures.Count;
                if (s.colors.Count > 0) s.currentColorIndex = i % s.colors.Count;
                if (s.materials.Count > 0) s.currentMatIndex = i % s.materials.Count;
            }

            if (s.materials.Count > 0) SetMaterial(s, s.materials[s.currentMatIndex], false);
            if (s.textures.Count > 0) SetTexture(s, s.textures[s.currentTexIndex]);
            if (s.colors.Count > 0) SetColor(s, s.colors[s.currentColorIndex]);
        }
    }

    // ==================== Helpers ====================
    MaterialPropertyBlock GetBlock(Renderer r)
    {
        if (!_blocks.TryGetValue(r, out var b)) { b = new MaterialPropertyBlock(); _blocks[r] = b; }
        return b;
    }
    string GetTexturePropName(Material m, string overrideName)
    {
        if (!string.IsNullOrEmpty(overrideName)) return overrideName;
        if (m != null)
        { if (m.HasProperty("_BaseMap")) return "_BaseMap"; if (m.HasProperty("_MainTex")) return "_MainTex"; }
        return "_MainTex";
    }
    string GetColorPropName(Material m, string overrideName)
    {
        if (!string.IsNullOrEmpty(overrideName)) return overrideName;
        if (m != null)
        { if (m.HasProperty("_BaseColor")) return "_BaseColor"; if (m.HasProperty("_Color")) return "_Color"; }
        return "_Color";
    }
    Material GetMaterialAt(Renderer r, int idx)
    {
        var mats = r.sharedMaterials;
        if (mats == null || mats.Length == 0) return r.sharedMaterial;
        idx = Mathf.Clamp(idx, 0, mats.Length - 1);
        return mats[idx];
    }

    // ==================== Uygulayýcýlar (slot = bir sphere) ====================
    void SetTexture(SphereSlot s, Texture tex)
    {
        if (tex == null || s.renderers == null) return;

        foreach (var r in s.renderers)
        {
            if (r == null) continue;
            var prop = GetTexturePropName(GetMaterialAt(r, s.materialElement), s.texturePropertyOverride);
            var block = GetBlock(r);
            r.GetPropertyBlock(block, s.materialElement);
            block.SetTexture(prop, tex);
            r.SetPropertyBlock(block, s.materialElement);
        }
    }

    void SetColor(SphereSlot s, Color col)
    {
        if (s.renderers == null) return;

        foreach (var r in s.renderers)
        {
            if (r == null) continue;
            var prop = GetColorPropName(GetMaterialAt(r, s.materialElement), s.colorPropertyOverride);
            var block = GetBlock(r);
            r.GetPropertyBlock(block, s.materialElement);
            block.SetColor(prop, col);
            r.SetPropertyBlock(block, s.materialElement);
        }
    }

    void SetMaterial(SphereSlot s, Material mat, bool reapplyProps)
    {
        if (mat == null || s.renderers == null) return;

        foreach (var r in s.renderers)
        {
            if (r == null) continue;
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) mats = new Material[1];
            int idx = Mathf.Clamp(s.materialElement, 0, mats.Length - 1);
            mats[idx] = mat;
            r.sharedMaterials = mats;

            if (reapplyProps)
            {
                if (s.textures.Count > 0) SetTexture(s, s.textures[s.currentTexIndex]);
                if (s.colors.Count > 0) SetColor(s, s.colors[s.currentColorIndex]);
            }
        }
    }

    bool ValidSphere(int index) => index >= 0 && index < spheres.Count;

    // ==================== Public API ====================
    public void SetOneTexture(int sphereIndex, Texture tex) { if (ValidSphere(sphereIndex)) SetTexture(spheres[sphereIndex], tex); }
    public void NextOneTexture(int sphereIndex) { if (!ValidSphere(sphereIndex)) return; var s = spheres[sphereIndex]; if (s.textures.Count == 0) return; s.currentTexIndex = (s.currentTexIndex + 1) % s.textures.Count; SetTexture(s, s.textures[s.currentTexIndex]); }

    public void SetOneColor(int sphereIndex, Color col) { if (ValidSphere(sphereIndex)) SetColor(spheres[sphereIndex], col); }
    public void NextOneColor(int sphereIndex) { if (!ValidSphere(sphereIndex)) return; var s = spheres[sphereIndex]; if (s.colors.Count == 0) return; s.currentColorIndex = (s.currentColorIndex + 1) % s.colors.Count; SetColor(s, s.colors[s.currentColorIndex]); }

    public void ApplyMaterial(int sphereIndex, int materialIndex, bool reapplyProps = true)
    { if (!ValidSphere(sphereIndex)) return; var s = spheres[sphereIndex]; if (s.materials.Count == 0) return; s.currentMatIndex = Mathf.Clamp(materialIndex, 0, s.materials.Count - 1); SetMaterial(s, s.materials[s.currentMatIndex], reapplyProps); }
    public void NextOneMaterial(int sphereIndex, bool reapplyProps = true)
    { if (!ValidSphere(sphereIndex)) return; var s = spheres[sphereIndex]; if (s.materials.Count == 0) return; s.currentMatIndex = (s.currentMatIndex + 1) % s.materials.Count; SetMaterial(s, s.materials[s.currentMatIndex], reapplyProps); }

    public void NextAllTextures() { for (int i = 0; i < spheres.Count; i++) NextOneTexture(i); }
    public void NextAllColors() { for (int i = 0; i < spheres.Count; i++) NextOneColor(i); }
    public void NextAllMaterials(bool reapplyProps = true) { for (int i = 0; i < spheres.Count; i++) NextOneMaterial(i, reapplyProps); }
    public void CycleAll() { NextAllTextures(); NextAllColors(); NextAllMaterials(true); }

    [ContextMenu("Recollect From SpheresManager")]
    public void RecollectFromManagerNow()
    {
        CollectFromSpheresManager();
        if (applyInitialOnStart) ApplyInitial();
    }
}
