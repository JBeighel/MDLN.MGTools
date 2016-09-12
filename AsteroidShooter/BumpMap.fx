sampler s0;
sampler NormalMap;
float2 ObjectPos;


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

float2 CalculateXYMagnitude(float Angle, float Magnitude) {
	float2 Components;

	Components.x = cos(Angle) * Magnitude;
	Components.y = sin(Angle) * Magnitude;

	return Components;
}

//Tint is the color set in SpriteBatch.Draw() and coords is screen coordinates
float4 PixelShaderFunction(float4 TintColor : COLOR0, float2 TexCoord: TEXCOORD0) : COLOR0
{
float2 LightPos;
	LightPos.x = 0;
	LightPos.y = 0;

	float LightAngle = CalculateAngleFromPoints(ObjectPos, LightPos);
	float4 NormalColor = tex2D(NormalMap, TexCoord);

	float2 LightMagn = CalculateXYMagnitude(LightAngle, 1);

	float4 DrawColor = tex2D(s0, TexCoord);

	DrawColor *= TintColor;

	if (NormalColor.g - 0.5 > 0) {
		DrawColor.r += NormalColor.g * LightMagn.x;
		DrawColor.g += NormalColor.g * LightMagn.x;
		DrawColor.b += NormalColor.g * LightMagn.x;
	} else {
		//DrawColor.r -= NormalColor.g * LightMagn.x;
		//DrawColor.g -= NormalColor.g * LightMagn.x;
		//DrawColor.b -= NormalColor.g * LightMagn.x;
	}
	
	return DrawColor;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}