sampler TS : register(s0)
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler LIGHT : register(s1)
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

float4 LightAdder(float2 pos : TEXCOORD0) : COLOR0
{
	float4 orig = tex2D(TS,pos);
	float4 light = tex2D(LIGHT, pos);

	orig.rgb -= (1 - light.rgb);

	//light.rgb = (orig.r + orig.g + orig.b) / 3;

	//orig.rgb = orig.r * orig.rgb + (1 - orig.r) * light.rgb;

    return orig;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 LightAdder();
    }
}
