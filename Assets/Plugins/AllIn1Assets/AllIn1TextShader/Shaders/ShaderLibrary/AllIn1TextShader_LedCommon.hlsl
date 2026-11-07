#ifndef ALLIN1TEXTSHADER_LEDCOMMON
#define ALLIN1TEXTSHADER_LEDCOMMON

//Shared by the text Led effect and the LED panel shader, so a text lights exactly the dot the panel draws in that cell.
//Grid positions are in cell units with integers on the cell corners

//0 at the dot center, 1 where it touches the cell edge. Roundness 1 is a circle, 0 a square
float LedDotDistance(float2 gridPos, float roundness)
{
	float2 local = abs(frac(gridPos) * 2.0 - 1.0);
	return lerp(max(local.x, local.y), length(local), roundness);
}

//Antialiased to one screen pixel at any zoom, softness widens the edge inward in half cell units
float LedDotMask(float dotDistance, float size, float softness)
{
	float aa = max(fwidth(dotDistance), 0.0001);
	return smoothstep(-0.5 * aa, softness + 0.5 * aa, size - dotDistance);
}

float LedCellHash(float2 cellId)
{
	return frac(sin(dot(cellId, float2(12.9898, 78.233))) * 43758.5453);
}

//Whole cells only, so a panel never shows a dot cut by its border
float LedInsideCells(float2 cellId, float2 cells)
{
	float2 inside = step(0.0, cellId) * step(cellId, cells - 1.0);
	return inside.x * inside.y;
}

#endif //ALLIN1TEXTSHADER_LEDCOMMON
