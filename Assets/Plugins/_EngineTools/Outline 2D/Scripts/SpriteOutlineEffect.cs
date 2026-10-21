using UnityEngine;

namespace RoamMoon.SimpleOutline
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("Roam Moon/Simple Outline")]
    public class SpriteOutlineEffect : MonoBehaviour
    {
        public const string ShaderName = "Roam Moon/Simple Outline";

        const string OutlineColorName = "_OutlineColor";
        const string OutlineThicknessName = "_OutlineThickness";
        const string AlphaThresholdName = "_AlphaThreshold";
        const string EdgeSoftnessName = "_EdgeSoftness";
        const string DiagonalSamplesName = "_DiagonalSamples";
        const string InnerOutlineName = "_InnerOutline";
        const string OutlineOnlyName = "_OutlineOnly";
        const string PulseIntensityName = "_PulseIntensity";
        const string PulseSpeedName = "_PulseSpeed";
        const string TextureSizeName = "_TextureSize";

        [SerializeField] Shader outlineShader;

        [ColorUsage(true, true)]
        [SerializeField] Color outlineColor = new Color(4f, 1.05f, 0.05f, 1f);

        [Range(0f, 16f)]
        [SerializeField] float thickness = 1f;

        [Range(0f, 1f)]
        [SerializeField] float alphaThreshold = 0.5f;

        [Range(0f, 1f)]
        [SerializeField] float edgeSoftness = 0.5f;

        [SerializeField] bool diagonalSamples = true;
        [SerializeField] bool innerOutline = false;
        [SerializeField] bool outlineOnly = false;
        [SerializeField] bool pulse = true;

        [Range(0f, 20f)]
        [SerializeField] float pulseSpeed = 3f;

        SpriteRenderer spriteRenderer;
        MaterialPropertyBlock propertyBlock;
        Material ownMaterial;
        Sprite lastSprite;
        [SerializeField, HideInInspector] Material previousMaterial;
        [SerializeField, HideInInspector] bool materialSwapped;

        public Color OutlineColor
        {
            get => outlineColor;
            set { outlineColor = value; PushProperties(); }
        }

        public float Thickness
        {
            get => thickness;
            set { thickness = Mathf.Clamp(value, 0f, 16f); PushProperties(); }
        }

        public float AlphaThreshold
        {
            get => alphaThreshold;
            set { alphaThreshold = Mathf.Clamp01(value); PushProperties(); }
        }

        public float EdgeSoftness
        {
            get => edgeSoftness;
            set { edgeSoftness = Mathf.Clamp01(value); PushProperties(); }
        }

        public bool DiagonalSamples
        {
            get => diagonalSamples;
            set { diagonalSamples = value; PushProperties(); }
        }

        public bool InnerOutline
        {
            get => innerOutline;
            set { innerOutline = value; PushProperties(); }
        }

        public bool OutlineOnly
        {
            get => outlineOnly;
            set { outlineOnly = value; PushProperties(); }
        }

        public bool Pulse
        {
            get => pulse;
            set { pulse = value; PushProperties(); }
        }

        public float PulseSpeed
        {
            get => pulseSpeed;
            set { pulseSpeed = Mathf.Clamp(value, 0f, 20f); PushProperties(); }
        }

        void Reset()
        {
            outlineShader = ResolveShader();
        }

        Shader ResolveShader()
        {
            Shader shader = outlineShader != null ? outlineShader : Shader.Find(ShaderName);
            return shader;
        }

        void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (outlineShader == null)
            {
                outlineShader = ResolveShader();
            }

            if (outlineShader == null)
            {
                Debug.LogWarning($"{nameof(SpriteOutlineEffect)}: shader \"{ShaderName}\" not found.", this);
                enabled = false;
                return;
            }

            SwapMaterial();
            PushProperties();
        }

        void OnDisable()
        {
            RestoreMaterial();
        }

        void OnValidate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            spriteRenderer = GetComponent<SpriteRenderer>();
            PushProperties();
        }

        Material GetOutlineMaterial()
        {
            if (outlineShader == null)
            {
                return null;
            }

            if (ownMaterial == null || ownMaterial.shader != outlineShader)
            {
                DestroyOwnMaterial();
                ownMaterial = new Material(outlineShader)
                {
                    name = "SpriteOutlineEffect",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            return ownMaterial;
        }

        void DestroyOwnMaterial()
        {
            if (ownMaterial == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(ownMaterial);
            }
            else
            {
                DestroyImmediate(ownMaterial);
            }

            ownMaterial = null;
        }

        void SwapMaterial()
        {
            Material outlineMaterial = GetOutlineMaterial();

            if (outlineMaterial == null)
            {
                return;
            }

            if (spriteRenderer.sharedMaterial == outlineMaterial)
            {
                return;
            }

            if (!materialSwapped)
            {
                previousMaterial = spriteRenderer.sharedMaterial;
                materialSwapped = true;
            }

            spriteRenderer.sharedMaterial = outlineMaterial;
        }

        void RestoreMaterial()
        {
            if (!materialSwapped || spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sharedMaterial = previousMaterial;
            spriteRenderer.SetPropertyBlock(null);
            materialSwapped = false;
            DestroyOwnMaterial();
        }

        void PushProperties()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();

            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(OutlineColorName, outlineColor);
            propertyBlock.SetFloat(OutlineThicknessName, thickness);
            propertyBlock.SetFloat(AlphaThresholdName, alphaThreshold);
            propertyBlock.SetFloat(EdgeSoftnessName, edgeSoftness);
            propertyBlock.SetFloat(DiagonalSamplesName, diagonalSamples ? 1f : 0f);
            propertyBlock.SetFloat(InnerOutlineName, innerOutline ? 1f : 0f);
            propertyBlock.SetFloat(OutlineOnlyName, outlineOnly ? 1f : 0f);
            propertyBlock.SetFloat(PulseIntensityName, pulse ? 1f : 0f);
            propertyBlock.SetFloat(PulseSpeedName, pulseSpeed);
            propertyBlock.SetVector(TextureSizeName, ReadTextureSize());
            lastSprite = spriteRenderer.sprite;
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        Vector4 ReadTextureSize()
        {
            Sprite sprite = spriteRenderer.sprite;

            if (sprite == null || sprite.texture == null)
            {
                return new Vector4(1f, 1f, 0f, 0f);
            }

            return new Vector4(sprite.texture.width, sprite.texture.height, 0f, 0f);
        }

        void Update()
        {
            if (spriteRenderer != null && spriteRenderer.sprite != lastSprite)
            {
                PushProperties();
            }

    #if UNITY_EDITOR
            if (pulse && !Application.isPlaying)
            {
                UnityEditor.SceneView.RepaintAll();
            }
    #endif
        }
    }
}
