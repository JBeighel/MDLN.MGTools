sampler s0;
sampler NormalMap;
float2 ObjectPos;
float2 LightPos;

float PI = 3.141593;

float CalculateAngleFromPoints(float2 PointOrigin, float2 PointOffset) {
	float LenX = PointOffset.x - PointOrigin.x;
	float LenY = PointOrigin.y - PointOffset.y;

	if ((PointOffset.x >= PointOrigin.x) && (PointOffset.y >= PointOrigin.y)) { //Top right quadrant
		return atan(LenY / LenX);
	} else if ((PointOffset.x < PointOrigin.x) && (PointOffset.y >= PointOrigin.y)) { //Top left quadrant
		return PI + atan(LenY / LenX);
	} else if ((PointOffset.x < PointOrigin.x) && (PointOffset.y < PointOrigin.y)) { //Bottom left quadrant
		return atan(LenY / LenX) + PI;
	} else { //Bottom right quadrant
		return PI + atan(LenY / LenX) + PI;
	}
}

//Tint is the color set in SpriteBatch.Draw() and coords is screen coordinates
float4 PixelShaderFunction(float4 TintColor : COLOR0, float2 TexCoord: TEXCOORD0) : COLOR0
{
	float LightAngle = CalculateAngleFromPoints(ObjectPos, LightPos);
	float4 NormalColor = tex2D(NormalMap, TexCoord);

	float4 DrawColor = tex2D(s0, TexCoord);

	DrawColor *= TintColor;

	DrawColor += NormalColor;
	
	return DrawColor;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}