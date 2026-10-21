using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace RoamMoon.SimpleOutline.Editor
{
    [CustomEditor(typeof(SpriteOutlineEffect))]
    [CanEditMultipleObjects]
    public class SpriteOutlineEffectEditor : UnityEditor.Editor
    {
        const string LogoGuid = "2848f4445f289664a9015012d8ad4afe";
        const string LogoFallbackFilter = "logorm t:Texture2D";

        static bool Dark => EditorGUIUtility.isProSkin;

        static Color HeaderBackground => Dark
            ? new Color(0.16f, 0.17f, 0.21f)
            : new Color(0.85f, 0.86f, 0.90f);

        static Color CardBackground => Dark
            ? new Color(0.235f, 0.235f, 0.255f)
            : new Color(0.925f, 0.925f, 0.935f);

        static Color OutlineBorder => Dark
            ? new Color(0.32f, 0.33f, 0.38f)
            : new Color(0.74f, 0.75f, 0.79f);

        static Color Accent => Dark
            ? new Color(0.63f, 0.69f, 0.93f)
            : new Color(0.33f, 0.40f, 0.72f);

        static Color MutedText => Dark
            ? new Color(0.60f, 0.62f, 0.68f)
            : new Color(0.36f, 0.38f, 0.44f);

        static Color TitleText => Dark
            ? new Color(0.90f, 0.91f, 0.94f)
            : new Color(0.12f, 0.13f, 0.16f);

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(BuildBanner());
            root.Add(BuildPipelineWarning());
            root.Add(BuildShaderWarning());

            root.Add(BuildCard("Outline", "outlineColor", "thickness"));
            root.Add(BuildCard("Edge Detection", "alphaThreshold", "edgeSoftness", "diagonalSamples"));
            root.Add(BuildCard("Mode", "innerOutline", "outlineOnly"));
            root.Add(BuildAnimationCard());
            root.Add(BuildAdvanced());

            return root;
        }

        VisualElement BuildBanner()
        {
            var banner = new VisualElement();
            banner.style.flexDirection = FlexDirection.Row;
            banner.style.alignItems = Align.Center;
            banner.style.backgroundColor = HeaderBackground;
            banner.style.marginBottom = 10f;
            SetPadding(banner, 10f, 12f);
            SetRadius(banner, 6f);
            SetBorder(banner, OutlineBorder, 1f);

            Texture2D logo = LoadLogo();

            if (logo != null)
            {
                var badge = new Image
                {
                    image = logo,
                    scaleMode = ScaleMode.ScaleToFit
                };

                badge.style.width = 40f;
                badge.style.height = 40f;
                badge.style.marginRight = 10f;
                banner.Add(badge);
            }

            var titles = new VisualElement();
            titles.style.flexGrow = 1f;
            titles.style.justifyContent = Justify.Center;

            var title = new Label("Sprite Outline");
            title.style.fontSize = 14f;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = TitleText;
            titles.Add(title);

            var subtitle = new Label("Roam Moon");
            subtitle.style.fontSize = 10f;
            subtitle.style.letterSpacing = 1.4f;
            subtitle.style.marginTop = 2f;
            subtitle.style.color = MutedText;
            titles.Add(subtitle);

            banner.Add(titles);

            return banner;
        }

        VisualElement BuildPipelineWarning()
        {
            var box = new HelpBox(
                "HDRP is active. This package targets URP. HDRP has no 2D renderer, so SpriteRenderer output is not supported.",
                HelpBoxMessageType.Warning);

            box.style.marginBottom = 8f;
            box.style.display = IsHdrpActive() ? DisplayStyle.Flex : DisplayStyle.None;
            return box;
        }

        static bool IsHdrpActive()
        {
            RenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline;
            return asset != null && asset.GetType().Name.Contains("HDRenderPipelineAsset");
        }

        VisualElement BuildShaderWarning()
        {
            var box = new HelpBox(
                "Outline Shader is not assigned. Re-add the component, or pick the shader under Advanced.",
                HelpBoxMessageType.Warning);

            box.style.marginBottom = 8f;

            SerializedProperty shaderProperty = serializedObject.FindProperty("outlineShader");

            void Sync()
            {
                bool missing = shaderProperty.objectReferenceValue == null;
                box.style.display = missing ? DisplayStyle.Flex : DisplayStyle.None;
            }

            box.TrackPropertyValue(shaderProperty, _ => Sync());
            Sync();

            return box;
        }

        VisualElement BuildCard(string title, params string[] propertyNames)
        {
            VisualElement card = CreateCard(title);

            foreach (string propertyName in propertyNames)
            {
                card.Add(CreateField(propertyName));
            }

            return card;
        }

        VisualElement BuildAnimationCard()
        {
            VisualElement card = CreateCard("Animation");

            PropertyField pulseField = CreateField("pulse");
            PropertyField speedField = CreateField("pulseSpeed");

            card.Add(pulseField);
            card.Add(speedField);

            SerializedProperty pulseProperty = serializedObject.FindProperty("pulse");

            void Sync()
            {
                speedField.SetEnabled(pulseProperty.boolValue);
                speedField.style.opacity = pulseProperty.boolValue ? 1f : 0.5f;
            }

            speedField.TrackPropertyValue(pulseProperty, _ => Sync());
            Sync();

            return card;
        }

        VisualElement BuildAdvanced()
        {
            var foldout = new Foldout
            {
                text = "Advanced",
                value = false
            };

            foldout.style.marginTop = 2f;
            foldout.style.marginLeft = 2f;
            foldout.Add(CreateField("outlineShader"));

            return foldout;
        }

        VisualElement CreateCard(string title)
        {
            var card = new VisualElement();
            card.style.backgroundColor = CardBackground;
            card.style.marginBottom = 8f;
            SetPadding(card, 8f, 10f);
            SetRadius(card, 5f);
            SetBorder(card, OutlineBorder, 1f);

            var heading = new VisualElement();
            heading.style.flexDirection = FlexDirection.Row;
            heading.style.alignItems = Align.Center;
            heading.style.marginBottom = 7f;

            var bar = new VisualElement();
            bar.style.width = 3f;
            bar.style.height = 11f;
            bar.style.marginRight = 6f;
            bar.style.backgroundColor = Accent;
            SetRadius(bar, 1.5f);
            heading.Add(bar);

            var label = new Label(title.ToUpperInvariant());
            label.style.fontSize = 10f;
            label.style.letterSpacing = 1.2f;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = MutedText;
            heading.Add(label);

            card.Add(heading);

            return card;
        }

        PropertyField CreateField(string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            var field = new PropertyField(property);
            field.AddToClassList("unity-base-field__aligned");
            field.style.marginBottom = 1f;
            return field;
        }

        static Texture2D LoadLogo()
        {
            string path = AssetDatabase.GUIDToAssetPath(LogoGuid);

            if (string.IsNullOrEmpty(path))
            {
                string[] matches = AssetDatabase.FindAssets(LogoFallbackFilter);

                if (matches.Length > 0)
                {
                    path = AssetDatabase.GUIDToAssetPath(matches[0]);
                }
            }

            return string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static void SetPadding(VisualElement element, float vertical, float horizontal)
        {
            element.style.paddingTop = vertical;
            element.style.paddingBottom = vertical;
            element.style.paddingLeft = horizontal;
            element.style.paddingRight = horizontal;
        }

        static void SetRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        static void SetBorder(VisualElement element, Color color, float width)
        {
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
        }
    }
}
