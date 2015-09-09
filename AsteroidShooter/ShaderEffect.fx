sampler s0;

//Tint is the color set in SpriteBatch.Draw() and coords is screen coordinates
float4 PixelShaderFunction(float4 Tint : COLOR0, float2 coords: TEXCOORD0) : COLOR0
{
	float4 DrawColor = tex2D(s0, coords);

	//DrawColor.rgb += TintColor.rgb;
	DrawColor *= Tint;
	
	return DrawColor;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}