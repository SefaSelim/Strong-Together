using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerSphereTextureSwapper_FromList : MonoBehaviour
{
    public enum MatchMode { ByIndex, ByName, ByGameObject }

    [Header("Eþleþtirme")]
    [SerializeField] MatchMode matchMode = MatchMode.ByIndex;   // i. sphere -> spheres[i]
    [SerializeField] bool useTemplateIfNoMatch = false;         // kapalý: Element0 kopyalanmaz
    [SerializeField] int templateSlotIndex = 0;                  // açarsan hangi slot kopyalansýn

    [Header("Defaults (yalnýzca slot BOÞSA kopyalanýr)")]
    [SerializeField] List<Texture> defaultTextures = new();
    [SerializeField] List<Color> defaultColors = new();
    [SerializeField] List<Material> defaultMaterials = new();

    [SerializeField] bool applyInitialOnStart = true;
    [SerializeField] bool startWithFirstItem = true;    // ilk seçtiðinle baþla
    [SerializeField] bool respectInspectorStartIndex = false;
    [SerializeField] bool staggerInitialIndices = false; // istersen aç

    [Header("Debug (isteðe baðlý)")]
    [SerializeField] bool debugKeys = true;
    [SerializeField] KeyCode testTextureKey = KeyCode.T;
    [SerializeField] KeyCode testColorKey = KeyCode.C;
    [SerializeField] KeyCode testMaterialKey = KeyCode.M;
    [SerializeField] KeyCode testAllKey = KeyCode.Space;

    [Serializable]
    public class SphereSlot
    {
        public string id;                    // ByName için
        public GameObject sphereRoot;        // runtime baðlanýr
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

    void Start() => StartCoroutine(BindFromManagerAndApply());

    IEnumerator BindFromManagerAndApply()
    {
        while (SpheresManager.Instance == null || SpheresManager.Instance.Spheres == null)
            yield return null;     // manager hazýr
        yield return null;         // spawn sonrasý 1 frame bekle

        CollectFromSpheresManager();
        if (applyInitialOnStart) ApplyInitial();
    }

    void Update()
    {
        if (!Application.isPlaying || !debugKeys) return;
        if (Input.GetKeyDown(testTextureKey)) NextAllTextures();
        if (Input.GetKeyDown(testColorKey)) NextAllColors();
        if (Input.GetKeyDown(testMaterialKey)) NextAllMaterials(true);
        if (Input.GetKeyDown(testAllKey)) CycleAll();
    }

    // -------- Toplama (EÞLEÞTÝRME) --------
    public void CollectFromSpheresManager()
    {
        var list = SpheresManager.Instance.Spheres;
        if (list == null) return;

        // Eski slot haritalarý
        var oldByGo = new Dictionary<GameObject, SphereSlot>();
        var oldByName = new Dictionary<string, SphereSlot>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < spheres.Count; i++)
        {
            var s = spheres[i];
            if (s == null) continue;
            if (s.sphereRoot != null && !oldByGo.ContainsKey(s.sphereRoot)) oldByGo.Add(s.sphereRoot, s);
            if (!string.IsNullOrEmpty(s.id) && !oldByName.ContainsKey(s.id)) oldByName.Add(s.id, s);
        }

        // Þablon (kullanýrsan)
        SphereSlot template = (useTemplateIfNoMatch && spheres.Count > 0) ?
            SafeGetTemplate(templateSlotIndex) : null;

        var newList = new List<SphereSlot>(list.Count);

        for (int i = 0; i < list.Count; i++)
        {
            var go = list[i];
            if (go == null) continue;

            SphereSlot slot = null;

            switch (matchMode)
            {
                case MatchMode.ByIndex:
                    if (i < spheres.Count) slot = spheres[i];
                    break;

                case MatchMode.ByName:
                    // önce isimle, yoksa index fallback
                    if (!oldByName.TryGetValue(go.name, out slot) && i < spheres.Count) slot = spheres[i];
                    break;

                case MatchMode.ByGameObject:
                    if (!oldByGo.TryGetValue(go, out slot) && i < spheres.Count) slot = spheres[i];
                    break;
            }

            if (slot == null)
            {
                slot = template != null ? CloneFromTemplate(template) : new SphereSlot();
            }

            // Baðla & renderer’larý topla
            slot.id = string.IsNullOrEmpty(slot.id) ? go.name : slot.id;
            slot.sphereRoot = go;
            slot.renderers.Clear();
            go.GetComponentsInChildren(slot.renderers);

            newList.Add(slot);
        }

        spheres = newList;
        Debug.Log($"[Swapper] Collected {spheres.Count} spheres with match mode: {matchMode}");
    }

    SphereSlot SafeGetTemplate(int idx)
    {
        idx = Mathf.Clamp(idx, 0, Mathf.Max(0, spheres.Count - 1));
        return spheres[idx];
    }

    SphereSlot CloneFromTemplate(SphereSlot t)
    {
        return new SphereSlot
        {
            id = t.id,
            materialElement = t.materialElement,
            texturePropertyOverride = t.texturePropertyOverride,
            colorPropertyOverride = t.colorPropertyOverride,
            textures = new List<Texture>(t.textures),
            colors = new List<Color>(t.colors),
            materials = new List<Material>(t.materials),
            currentTexIndex = t.currentTexIndex,
            currentColorIndex = t.currentColorIndex,
            currentMatIndex = t.currentMatIndex,
            renderers = new List<Renderer>()
        };
    }

    // -------- Baþlangýç uygulamasý --------
    void ApplyInitial()
    {
        for (int i = 0; i < spheres.Count; i++)
        {
            var s = spheres[i];

            // defaults sadece boþsa
            if (s.materials.Count == 0 && defaultMaterials.Count > 0) s.materials.AddRange(defaultMaterials);
            if (s.textures.Count == 0 && defaultTextures.Count > 0) s.textures.AddRange(defaultTextures);
            if (s.colors.Count == 0 && defaultColors.Count > 0) s.colors.AddRange(defaultColors);

            // index seçimi
            if (s.textures.Count > 0)
            {
                if (startWithFirstItem) s.currentTexIndex = 0;
                else if (respectInspectorStartIndex) s.currentTexIndex = Mathf.Clamp(s.currentTexIndex, 0, s.textures.Count - 1);
                else if (staggerInitialIndices) s.currentTexIndex = i % s.textures.Count;
                SetTexture(s, s.textures[s.currentTexIndex]);
            }
            if (s.colors.Count > 0)
            {
                if (startWithFirstItem) s.currentColorIndex = 0;
                else if (respectInspectorStartIndex) s.currentColorIndex = Mathf.Clamp(s.currentColorIndex, 0, s.colors.Count - 1);
                else if (staggerInitialIndices) s.currentColorIndex = i % s.colors.Count;
                SetColor(s, s.colors[s.currentColorIndex]);
            }
            if (s.materials.Count > 0)
            {
                if (startWithFirstItem) s.currentMatIndex = 0;
                else if (respectInspectorStartIndex) s.currentMatIndex = Mathf.Clamp(s.currentMatIndex, 0, s.materials.Count - 1);
                else if (staggerInitialIndices) s.currentMatIndex = i % s.materials.Count;
                SetMaterial(s, s.materials[s.currentMatIndex], false);
            }
        }
    }

    // -------- Helpers --------
    MaterialPropertyBlock GetBlock(Renderer r)
    {
        if (!_blocks.TryGetValue(r, out var b)) { b = new MaterialPropertyBlock(); _blocks[r] = b; }
        return b;
    }
    string GetTexturePropName(Material m, string overrideName)
    { if (!string.IsNullOrEmpty(overrideName)) return overrideName; if (m != null) { if (m.HasProperty("_BaseMap")) return "_BaseMap"; if (m.HasProperty("_MainTex")) return "_MainTex"; } return "_MainTex"; }
    string GetColorPropName(Material m, string overrideName)
    { if (!string.IsNullOrEmpty(overrideName)) return overrideName; if (m != null) { if (m.HasProperty("_BaseColor")) return "_BaseColor"; if (m.HasProperty("_Color")) return "_Color"; } return "_Color"; }
    Material GetMaterialAt(Renderer r, int idx)
    { var mats = r.sharedMaterials; if (mats == null || mats.Length == 0) return r.sharedMaterial; idx = Mathf.Clamp(idx, 0, mats.Length - 1); return mats[idx]; }

    // -------- Uygulayýcýlar --------
    void SetTexture(SphereSlot s, Texture tex)
    {
        if (tex == null) return;
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
        if (mat == null) return;
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

    // -------- Public API --------
    public void SetOneTexture(int i, Texture tex) { if (ValidSphere(i)) SetTexture(spheres[i], tex); }
    public void NextOneTexture(int i) { if (!ValidSphere(i)) return; var s = spheres[i]; if (s.textures.Count == 0) return; s.currentTexIndex = (s.currentTexIndex + 1) % s.textures.Count; SetTexture(s, s.textures[s.currentTexIndex]); }
    public void SetOneColor(int i, Color col) { if (ValidSphere(i)) SetColor(spheres[i], col); }
    public void NextOneColor(int i) { if (!ValidSphere(i)) return; var s = spheres[i]; if (s.colors.Count == 0) return; s.currentColorIndex = (s.currentColorIndex + 1) % s.colors.Count; SetColor(s, s.colors[s.currentColorIndex]); }
    public void ApplyMaterial(int i, int mIdx, bool reapplyProps = true) { if (!ValidSphere(i)) return; var s = spheres[i]; if (s.materials.Count == 0) return; s.currentMatIndex = Mathf.Clamp(mIdx, 0, s.materials.Count - 1); SetMaterial(s, s.materials[s.currentMatIndex], reapplyProps); }
    public void NextOneMaterial(int i, bool reapplyProps = true) { if (!ValidSphere(i)) return; var s = spheres[i]; if (s.materials.Count == 0) return; s.currentMatIndex = (s.currentMatIndex + 1) % s.materials.Count; SetMaterial(s, s.materials[s.currentMatIndex], reapplyProps); }

    public void NextAllTextures() { for (int i = 0; i < spheres.Count; i++) NextOneTexture(i); }
    public void NextAllColors() { for (int i = 0; i < spheres.Count; i++) NextOneColor(i); }
    public void NextAllMaterials(bool reapplyProps = true) { for (int i = 0; i < spheres.Count; i++) NextOneMaterial(i, reapplyProps); }
    public void CycleAll() { NextAllTextures(); NextAllColors(); NextAllMaterials(true); }
}
