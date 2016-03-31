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
	float4 light = tex2D(LIGHT, pos) * 1.05f;

	light.rgb = clamp(light.rgb, 0, 1);

	orig.rgb -= (1 - light.rgb);

	float bnw = (orig.r + orig.g + orig.b) / 3;

	if (light.r < .9)
	{
		float f = 2.698 * light.r * light.r - 1.317 * light.r;

		orig.rgb = orig.rgb * f + (bnw * (1 - f));
	}

	orig.r *= (
		- 2.6
		* orig.r * orig.r * orig.r 
		+ 3.2  
		* orig.r * orig.r 
		- .395
		* orig.r
		+ .825);

	orig.g *= (
		- 2.46
		* orig.g * orig.g * orig.g
		+ 2.769
		* orig.g * orig.g
		- .11
		* orig.g
		+ .85);


	//orig.rgb = orig.r * orig.rgb + (1 - orig.r) * light.rgb;

    return orig + .05;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 LightAdder();
    }
}
