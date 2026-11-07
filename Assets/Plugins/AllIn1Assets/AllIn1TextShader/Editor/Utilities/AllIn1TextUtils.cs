using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace AllIn1Assets.AllIn1TextShader
{
	public static class AllIn1TextUtils
	{
		private const char BLURRED_ATLAS_STAMP_SEPARATOR = '|';

		private const int FONT_ASSET_POINT_SIZE = 280;
		private const int FONT_ASSET_PADDING = 20;
		private const int FONT_ASSET_ATLAS_SIZE = 2048;
		private const int FONT_ASSET_PROBE_POINT_SIZE = 32;

		public static Texture CreateBlurredAtlas(string dstFolder, string fontAtlasName, string blurredFontAtlasName, Texture2D atlasFontTexture, bool uniquePath = false)
		{
			Texture2D originalAtlasCopy = new Texture2D(atlasFontTexture.width, atlasFontTexture.height, TextureFormat.Alpha8, 1, false);
			Graphics.CopyTexture(atlasFontTexture, originalAtlasCopy);

			TextureBlurrerTool blurrerTool = new TextureBlurrerTool();
			Texture2D blurredAtlas = blurrerTool.FastBlur(originalAtlasCopy, 4, 2, TextureFormat.Alpha8);

			Texture res = EditorUtils.SaveTextureAsPNG(dstFolder, blurredFontAtlasName, "Blurred atlas saved!",
				blurredAtlas, FilterMode.Bilinear, TextureImporterType.SingleChannel, TextureWrapMode.Repeat, false, generateUniqueAssetPath: uniquePath,
				importerUserData: GetBlurredAtlasStamp(atlasFontTexture) ?? string.Empty);

			return res;
		}

		//The blurred atlas fontAtlas needs as it is now: current if it was made from the same font, else the one named after the atlas.
		//Rebuilt in place when the font changed since it was made, created when there is none
		public static Texture GetCurrentBlurredAtlas(AllIn1TextShaderConfig config, Texture2D fontAtlas, Texture current)
		{
			string stamp = GetBlurredAtlasStamp(fontAtlas);
			string blurredAtlasName = config.GetBlurredAtlasName(fontAtlas.name);

			Texture res = current;
			if (res == null || !IsBlurredAtlasOf(res, blurredAtlasName, stamp))
			{
				res = config.FindBlurredTextureByFilename(fontAtlas.name);
			}

			if (res == null)
			{
				res = CreateBlurredAtlas(config.FontTexturesFolderPath, fontAtlas.name, blurredAtlasName, fontAtlas);
			}
			else if (!IsBlurredAtlasOf(res, blurredAtlasName, stamp))
			{
				//Another font's atlas has the same name, so this one gets a file of its own
				res = CreateBlurredAtlas(config.FontTexturesFolderPath, fontAtlas.name, blurredAtlasName, fontAtlas, uniquePath: true);
			}
			else if (!IsBlurredAtlasCurrent(res, fontAtlas, stamp, trustUnstamped: true))
			{
				res = RebuildBlurredAtlas(res, fontAtlas);
			}

			return res;
		}

		//Runs after the font asset was imported, so a blurred atlas that predates stamps can't be trusted to match it and is rebuilt too
		public static void RefreshBlurredAtlasesOf(AllIn1TextShaderConfig config, TMP_FontAsset fontAsset)
		{
			Texture2D fontAtlas = fontAsset.atlasTexture;
			string stamp = fontAtlas != null ? GetBlurredAtlasStamp(fontAtlas) : null;
			if (stamp == null) { return; }

			string blurredAtlasName = config.GetBlurredAtlasName(fontAtlas.name);
			string[] guids = AssetDatabase.FindAssets($"{blurredAtlasName} t:Texture2D");
			for (int i = 0; i < guids.Length; i++)
			{
				Texture blurredAtlas = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guids[i]));
				if (blurredAtlas != null && IsBlurredAtlasOf(blurredAtlas, blurredAtlasName, stamp) && !IsBlurredAtlasCurrent(blurredAtlas, fontAtlas, stamp, trustUnstamped: false))
				{
					RebuildBlurredAtlas(blurredAtlas, fontAtlas);
				}
			}
		}

		//Identifies the atlas content a blurred copy is made from: the owning font asset's GUID, then a hash of its glyph layout
		private static string GetBlurredAtlasStamp(Texture2D fontAtlas)
		{
			string fontAssetPath = AssetDatabase.GetAssetPath(fontAtlas);
			TMP_FontAsset fontAsset = string.IsNullOrEmpty(fontAssetPath) ? null : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
			if (fontAsset == null) { return null; }

			Hash128 layoutHash = new Hash128();
			layoutHash.Append(fontAtlas.name);
			layoutHash.Append(fontAtlas.width);
			layoutHash.Append(fontAtlas.height);
			layoutHash.Append(fontAsset.atlasPadding);
			layoutHash.Append((int)fontAsset.atlasRenderMode);
			layoutHash.Append(fontAsset.faceInfo.pointSize);
			for (int i = 0; i < fontAsset.glyphTable.Count; i++)
			{
				Glyph glyph = fontAsset.glyphTable[i];
				GlyphRect rect = glyph.glyphRect;
				GlyphMetrics metrics = glyph.metrics;
				layoutHash.Append(glyph.atlasIndex);
				layoutHash.Append(rect.x);
				layoutHash.Append(rect.y);
				layoutHash.Append(rect.width);
				layoutHash.Append(rect.height);
				layoutHash.Append(metrics.width);
				layoutHash.Append(metrics.height);
				layoutHash.Append(metrics.horizontalBearingX);
				layoutHash.Append(metrics.horizontalBearingY);
			}

			string res = AssetDatabase.AssetPathToGUID(fontAssetPath) + BLURRED_ATLAS_STAMP_SEPARATOR + layoutHash;
			return res;
		}

		private static string GetStampSource(string stamp)
		{
			int separatorIdx = stamp.IndexOf(BLURRED_ATLAS_STAMP_SEPARATOR);
			string res = separatorIdx >= 0 ? stamp.Substring(0, separatorIdx) : stamp;
			return res;
		}

		private static string GetStoredStamp(Texture blurredAtlas)
		{
			AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(blurredAtlas));
			string res = importer != null ? importer.userData : null;
			return res;
		}

		//Blurred atlases made before stamps existed only have their name to go by
		private static bool IsBlurredAtlasOf(Texture blurredAtlas, string blurredAtlasName, string fontAtlasStamp)
		{
			if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(blurredAtlas))) { return false; }

			string storedStamp = GetStoredStamp(blurredAtlas);
			bool res = string.IsNullOrEmpty(storedStamp) || fontAtlasStamp == null
				? blurredAtlas.name == blurredAtlasName
				: GetStampSource(storedStamp) == GetStampSource(fontAtlasStamp);
			return res;
		}

		private static bool IsBlurredAtlasCurrent(Texture blurredAtlas, Texture2D fontAtlas, string fontAtlasStamp, bool trustUnstamped)
		{
			if (fontAtlasStamp == null) { return true; }

			string storedStamp = GetStoredStamp(blurredAtlas);
			bool res = string.IsNullOrEmpty(storedStamp)
				? trustUnstamped && blurredAtlas.width == fontAtlas.width && blurredAtlas.height == fontAtlas.height
				: storedStamp == fontAtlasStamp;
			return res;
		}

		private static Texture RebuildBlurredAtlas(Texture blurredAtlas, Texture2D fontAtlas)
		{
			string path = AssetDatabase.GetAssetPath(blurredAtlas);
			string folder = Path.GetDirectoryName(path).Replace('\\', '/');
			Texture res = CreateBlurredAtlas(folder, fontAtlas.name, Path.GetFileNameWithoutExtension(path), fontAtlas);
			return res;
		}

		public static TMP_FontAsset CreateFontAsset(TMP_Text tmpText)
		{
			AllIn1TextShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);

			var field = typeof(TMP_FontAsset).GetField(
				"m_SourceFontFile_EditorRef",
				BindingFlags.Instance | BindingFlags.NonPublic
			);

			TMP_FontAsset tmpFontAsset = tmpText.font;
			Font font = field.GetValue(tmpFontAsset) as Font;
			if (font == null)
			{
				EditorUtility.DisplayDialog("Create Font Atlas", $"{tmpFontAsset.name} has no Source Font File, so there is no font to build a new atlas from. Assign one in the font asset's Generation Settings and try again.", "Ok");
				return null;
			}

			string fontAssetname = font.name;

			string oldFontAssetPath = AssetDatabase.GetAssetPath(tmpFontAsset);
			string newFontAssetPath = oldFontAssetPath;
			if (!pluginConfig.IsWritableFolder(newFontAssetPath))
			{
				// Two font assets made from the same font file would otherwise share this name and the second would replace the first
				newFontAssetPath = AssetDatabase.GenerateUniqueAssetPath(EditorUtils.CombineAssetPath(pluginConfig.UserTMPFontAssetsFolderPath, $"{fontAssetname}.asset"));
			}

			TMP_FontAsset res = CreateStaticFontAsset(font, newFontAssetPath);
			return res;
		}

		//The point size drops below FONT_ASSET_POINT_SIZE only when the glyphs don't fit in one atlas
		public static TMP_FontAsset CreateStaticFontAsset(Font font, string assetPath, string extraCharacters = null)
		{
			string characters = GetFontAssetCharacters() + extraCharacters;

			//Glyphs the font lacks and glyphs that ran out of atlas space both come back as missing, so a size that surely fits tells them apart
			TMP_FontAsset res = CreateDynamicFontAsset(font, FONT_ASSET_PROBE_POINT_SIZE);
			if (res == null) { return null; }

			res.TryAddCharacters(characters, out string unsupportedCharacters);
			string supportedCharacters = RemoveCharacters(characters, unsupportedCharacters);

			int fittingSize = FONT_ASSET_PROBE_POINT_SIZE;
			int tooBigSize = FONT_ASSET_POINT_SIZE + 1;
			int pointSize = FONT_ASSET_POINT_SIZE;
			while (tooBigSize - fittingSize > 1)
			{
				TMP_FontAsset attempt = CreateDynamicFontAsset(font, pointSize);
				if (attempt.TryAddCharacters(supportedCharacters))
				{
					DestroyFontAsset(res);
					res = attempt;
					fittingSize = pointSize;
				}
				else
				{
					DestroyFontAsset(attempt);
					tooBigSize = pointSize;
				}
				pointSize = (fittingSize + tooBigSize) / 2;
			}

			res.atlasPopulationMode = AtlasPopulationMode.Static;
			res.name = Path.GetFileNameWithoutExtension(assetPath);
			AssetDatabase.CreateAsset(res, assetPath);

			Material textMat = res.material;
			textMat.shader = Shader.Find("TextMeshPro/Distance Field");
			textMat.name = res.name + " Material";

			Texture2D atlasTex = res.atlasTexture;
			atlasTex.name = res.name + " Atlas";

			AssetDatabase.AddObjectToAsset(textMat, res);
			AssetDatabase.AddObjectToAsset(atlasTex, res);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return res;
		}

		private static TMP_FontAsset CreateDynamicFontAsset(Font font, int pointSize)
		{
			TMP_FontAsset res = TMP_FontAsset.CreateFontAsset(
				font: font,
				samplingPointSize: pointSize,
				atlasPadding: FONT_ASSET_PADDING,
				renderMode: UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
				atlasWidth: FONT_ASSET_ATLAS_SIZE,
				atlasHeight: FONT_ASSET_ATLAS_SIZE,
				atlasPopulationMode: AtlasPopulationMode.Dynamic,
				enableMultiAtlasSupport: false);
			return res;
		}

		private static void DestroyFontAsset(TMP_FontAsset fontAsset)
		{
			Object.DestroyImmediate(fontAsset.atlasTexture);
			Object.DestroyImmediate(fontAsset.material);
			Object.DestroyImmediate(fontAsset);
		}

		//Printable ASCII, the no-break space, curly quotes and the ellipsis
		private static string GetFontAssetCharacters()
		{
			string res = string.Empty;
			for (int i = 32; i < 127; i++)
			{
				res += (char)i;
			}
			res += " ‘’“”…";
			return res;
		}

		private static string RemoveCharacters(string characters, string charactersToRemove)
		{
			string res = string.Empty;
			for (int i = 0; i < characters.Length; i++)
			{
				if (charactersToRemove.IndexOf(characters[i]) < 0) { res += characters[i]; }
			}
			return res;
		}

		public static bool IsFontAssetOptimallyConfigured(TMP_FontAsset fontAsset)
		{
			bool validPadding = fontAsset.atlasPadding >= FONT_ASSET_PADDING;
			bool validWidth = fontAsset.atlasWidth == FONT_ASSET_ATLAS_SIZE;
			bool validHeight = fontAsset.atlasHeight == FONT_ASSET_ATLAS_SIZE;

			bool res = validPadding && validWidth && validHeight;

			return res;
		}

		private static readonly string[] RENDER_STATE_PROPERTIES = { "_BlendSrc", "_BlendDst", "_ZTestMode", "_CullingMode", "_ZWrite", "_ColorMask" };

		// InitMaterial gives a cleaned copy a new material's render state, so a duplicate gets its source's blend mode and Advanced settings back
		private static void CopyRenderState(Material source, Material target)
		{
			if (source == null || target == null || source == target) { return; }

			for (int i = 0; i < RENDER_STATE_PROPERTIES.Length; i++)
			{
				string propertyName = RENDER_STATE_PROPERTIES[i];
				if (source.HasProperty(propertyName) && target.HasProperty(propertyName))
				{
					target.SetFloat(propertyName, source.GetFloat(propertyName));
				}
			}

			target.renderQueue = source.renderQueue;
		}

		public static void NewMaterialInAllHierarchy(GameObject go)
		{
			AllIn1TextShaderConfig pluginConfig = PluginRegistry.GetPluginByID<AllIn1TextShaderConfig>(PluginID.ALLIN1_TEXT_SHADER);

			AllIn1TextShaderComponent textShaderComponent = go.GetComponent<AllIn1TextShaderComponent>();
			if (textShaderComponent != null)
			{
				Material sourceMaterial = textShaderComponent.GetCurrentMaterial();
				textShaderComponent.CleanMaterial(pluginConfig.shStandard);
				CopyRenderState(sourceMaterial, textShaderComponent.GetCurrentMaterial());
			}

			// Text layers carry no AllIn1TextShaderComponent, so they get their own copy here, the way the layer inspector creates them
			TextLayerGraphic layerGraphic = go.GetComponent<TextMaterialConfiguratorSlave>() != null ? go.GetComponent<TextLayerGraphic>() : null;
			if (layerGraphic != null && layerGraphic.LayerMaterial != null)
			{
				layerGraphic.LayerMaterial = new Material(layerGraphic.LayerMaterial);
			}

			for(int i = 0; i < go.transform.childCount; i++)
			{
				GameObject goChildren = go.transform.GetChild(i).gameObject;
				NewMaterialInAllHierarchy(goChildren);
			}
		}
	}
}