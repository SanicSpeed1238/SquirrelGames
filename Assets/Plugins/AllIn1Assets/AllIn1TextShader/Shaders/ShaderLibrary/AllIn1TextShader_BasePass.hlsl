#ifndef ALLIN1TEXT_BASE_PASS_INCLUDED
#define ALLIN1TEXT_BASE_PASS_INCLUDED

FragmentData AllIn1TextShaderVertex(VertexData v)
{
	FragmentData o;
	
	INITIALIZE_FRAGMENT_DATA(o)
	
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	//TMP flags fake bold (no bold typeface in the font's weight table) by negating the scale it packs: in uv2.y up to TextMeshPro 3.0, in uv0.w
	//since uGUI 2.0, whose uv2 holds the face texture mapping (read there, its bottom vertices looked bold). Real bold glyphs come from another font asset
#if UNITY_VERSION >= 202320
	float bold = step(v.texcoord0.w, 0);
#else
	float bold = step(v.texcoord1.y, 0);
#endif
	float faceDilate = _CustomFaceDilate + bold * 0.2;

	uint vid = GetVertexIndex(v);

	/* Expanding mesh */
	ExtraInfo extraInfo = extraInfoBuffer[vid];
	float2 charUVnormalized = extraInfo.uvNormalized;

	//Margin per axis in rect space, so narrow glyphs get the same horizontal room as wide ones.
	//Pushed along the glyph's own axes (diagonalVector.zw = its right side, refreshed with the buffer) so rotated glyphs keep their shape,
	//and transformed as a direction so the text's rotation and scale carry over
	float2 quadSizeRS = max(abs(extraInfo.diagonalVector.xy), 0.00001);
	float2 quadSizeTexels = float2(extraInfo.uvCorners.y - extraInfo.uvCorners.x, extraInfo.uvCorners.w - extraInfo.uvCorners.z) * float2(_TextureWidth, _TextureHeight);
	//characterCenter.w: 1 glyph, 2 underline or strikethrough slice, 3 mark slice, plus 3 for Extrude's back copies. Line slices sit edge to edge along their run,
	//so they only grow across it; marks keep TMP's box; a back copy grows like its source
	float quadKind = extraInfo.characterCenter.w;
	float shapeKind = GetBaseQuadKind(quadKind);
	float2 kindMask = shapeKind > 2.5 ? float2(0.0, 0.0) : (shapeKind > 1.5 ? float2(0.0, 1.0) : float2(1.0, 1.0));
	bool drawsFace;
	bool drawsBehind;
	GetQuadLayers(quadKind, drawsFace, drawsBehind);
	float2 meshMargin = CalculateMeshMargin(quadSizeRS, quadSizeTexels, extraInfo.characterAspectRatio, faceDilate, drawsFace, drawsBehind);
	meshMargin *= kindMask;
	float2 cornerSign = sign(charUVnormalized - 0.5);
	float2 glyphRight = extraInfo.diagonalVector.zw;
	glyphRight = dot(glyphRight, glyphRight) > 0.0001 ? glyphRight : float2(1.0, 0.0);
	float2 glyphUp = float2(-glyphRight.y, glyphRight.x);

	float2 cornerMargin = meshMargin;
#if defined(_EXTRUDE_ON)
	if (drawsBehind)
	{
		cornerMargin += max(0.0, cornerSign * CalculateExtrudeMargin(glyphRight, quadSizeRS, quadSizeTexels)) * kindMask;
	}
#endif

	float2 marginOffsetRS = glyphRight * (cornerSign.x * cornerMargin.x) + glyphUp * (cornerSign.y * cornerMargin.y);
	v.vertex.xyz += mul(matrix_rectToCanvas, float4(marginOffsetRS, 0.0, 0.0)).xyz;

	//Corners sit at 0 or 1, so this moves each one out by its own margin and keeps the atlas mapping for one sided growth
	charUVnormalized += cornerSign * cornerMargin / quadSizeRS;
	SET_CHAR_UV_NORMALIZED(o, charUVnormalized);
	
	
	float2 charUVNormalized = extraInfoBuffer[vid].uvNormalized;
	SET_UV_NORMALIZED_RAW(o, charUVNormalized);
	
	
	float3 originalVertex = v.vertex.xyz;
	
	float timingSeed = timingSeedsBuffer[vid];
	float3 shaderTime = GET_SHADER_TIME(o) + timingSeed;
	v.vertex = ApplyVertexEffects(v.vertex, shaderTime, timingSeed, extraInfo.characterCenter.xyz, charUVnormalized, glyphRight, quadSizeRS);
	
	
	float3 vertexDiff = v.vertex.xyz - originalVertex;
	
	
	
	/* Important: The vertex displacement must be uniform across all vertices to ensure
		that the rectOrigin recalculation is correct */
	float2 rectOrigin = bottomLeftRect.xy + vertexDiff.xy;

	float4 clipPos = OBJECT_TO_CLIP_SPACE(v);
#if !defined(_EXTRUDE_ON)
	//Back copies only draw Extrude, so a layer without it collapses them to a point and rasterizes nothing
	clipPos = quadKind > 3.5 ? float4(0.0, 0.0, 0.0, 1.0) : clipPos;
#endif

	float3 viewDirWS = normalize(GetViewDirWS(v.vertex.xyz)); 
	
	float bias = CalculateMainBias(faceDilate);
	
	float4 faceColor = v.color * _FaceColor;
	faceColor.rgb *= faceColor.a;

	o.pos = clipPos;

	float2 faceUVRaw = v.texcoord0.xy;
	
	float2 faceUV = faceUVRaw;

	float2 rectUV = GetRectUV(v.vertex.xyz, rectOrigin);
	
	float pixelWorldSize = (unity_OrthoParams.y * 2.0) / _ScreenParams.y;
	float pixelSizeMask = smoothstep(0.05, 0.2, pixelWorldSize);
	
	
	SET_RECT_UV(o, rectUV)

	SET_FACE_COLOR(o, faceColor)

	SET_FACE_UV(o, faceUV)
	
	SET_BIAS(o, bias)

	SET_FACE_DILATE(o, faceDilate)

	SET_VIEWDIR_WS(o, viewDirWS)

	SET_POSITION_OS(o, v.vertex.xyz)
	
	SET_VERTEX_ID(o, vid);
	
	SET_PIXEL_SIZE_MASK(o, pixelSizeMask)
	
	
	float2 texelSize = float2(1.0 / _TextureWidth, 1.0 / _TextureHeight);
	SET_TEXEL_SIZE(o, texelSize)
	
	float3 vertexVS = mul(UNITY_MATRIX_MV, v.vertex).xyz;
	SET_VERTEX_VS(o, vertexVS)
	
	return o;
}

float4 AllIn1TextShaderFragment(
	FragmentData i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	EffectsData data = CalculateEffectsData(i); 
	
	
	
	uint vid = (uint) GET_VERTEX_ID(i);
	float4 faceColor = GET_FACE_COLOR(i);
	float bias = GET_BIAS(i);
	float vertexAlpha = faceColor.a;
	float3 positionOS = GET_POSITION_OS(i);

	
	float2 originalRectUV = GET_RECT_UV(i);
	
	float2 characterUVNormalized = GET_CHAR_UV_NORMALIZED(i);
	CREATE_CHARACTER_DATA_WITH_BIAS(mainCharacterData, characterUVNormalized, bias, vid)
	mainCharacterData.pixelSDF = data.pixelTexels / GetAtlasSpreadTexels();
	
	CREATE_CHARACTER_DATA(outline01Data, characterUVNormalized, vid)

	CREATE_CHARACTER_DATA(outline02Data, characterUVNormalized, vid)

	CREATE_CHARACTER_DATA(outline03Data, characterUVNormalized, vid)
	
	mainCharacterData = ApplyUVEffects_FragmentStage(mainCharacterData);
	
	//float2 charUVNormalized = data.characterUVNormalized;
	
	float2 rectUVDiff = mainCharacterData.rectUV - originalRectUV;
	mainCharacterData.uvNormalized += rectUVDiff;
	
	float4 uvCornersInAtlas = extraInfoBuffer[vid].uvCornersInAtlas;
	
	ConfigureCharacterData(
		data.characterUVCorners,
		uvCornersInAtlas,
		bias,
		data.texelSize,
		_CustomOutlineSoftness,
		mainCharacterData
	);
	
	mainCharacterData.uvNormalized += rectUVDiff;
	
	float4 objectColor = faceColor;
	
#if defined(OUTLINE_01_ENABLED)
	ConfigureOutline01(data.characterUVCorners, uvCornersInAtlas, data.texelSize, mainCharacterData, outline01Data);
#endif

#if defined(OUTLINE_02_ENABLED)
	ConfigureOutline02(data.characterUVCorners, uvCornersInAtlas, data.texelSize, mainCharacterData, outline02Data);
#endif

#if defined(OUTLINE_03_ENABLED)
	ConfigureOutline03(data.characterUVCorners, uvCornersInAtlas, data.texelSize, mainCharacterData, outline03Data);
#endif
	
	float4 col = objectColor;
	
	
	float characterBodyAlpha = mainCharacterData.characterMask * _InnerAlpha;
	col.rgb *= characterBodyAlpha;
	col.a = characterBodyAlpha;
	
	bool drawsFace;
	bool drawsBehind;
	GetQuadLayers(data.quadKind, drawsFace, drawsBehind);

	//A back copy starts from nothing: its glyph quad draws the text, after every back copy
	if (drawsFace)
	{
		col = ApplyColorEffects(col, data, mainCharacterData, outline01Data, outline02Data, outline03Data);
		col = ApplyLightingEffects(col, data, mainCharacterData, outline01Data, outline02Data, outline03Data);
		col = ApplyColorEffectsAfterLighting(col, data, mainCharacterData, outline01Data, outline02Data, outline03Data);
	}
	else
	{
		col = float4(0.0, 0.0, 0.0, 0.0);
	}

	if (drawsBehind)
	{
		col = ApplyExtrude(mainCharacterData, data, col);
		col = ApplyShadows(mainCharacterData, data, col);
	}
	col = ApplyAlphaEffects(col, mainCharacterData.rectUV, mainCharacterData);
	
	
	float3 finalRGB = col.rgb * _ColorIntensity;
	float finalAlpha = saturate(col.a * vertexAlpha);
	finalAlpha = Pow_4(finalAlpha);

	//On the alpha, so every blending mode fades out at the mask's edge
#if defined(UNITY_UI_CLIP_RECT)
	finalAlpha *= UIRectClip(GET_POSITION_OS(i).xy);
#endif
	
	
	
#if defined(_BLENDINGMODE_MULTIPLY)
	col.rgb = lerp(1.0, finalRGB, finalAlpha);
#elif defined(_BLENDINGMODE_ADDITIVE)
	col.rgb = finalRGB * finalAlpha;
#else
	col.rgb = finalRGB * finalAlpha;
#endif

	col.a = finalAlpha;

#if defined(UNITY_UI_ALPHACLIP)
	clip(col.a - 0.001);
#endif
	
#if defined(_ALPHA_CUTOFF_ON)
	clip(col.a - _AlphaCutoffValue);
#endif
	
	return col;
}

#endif //ALLIN1TEXT_BASE_PASS_INCLUDED