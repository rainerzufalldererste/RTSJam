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

float2 screenres = float2(1280,720);

const float3 mod = (1. / 20.);
const float3 mod2 = (1. / 12.);

float4 LightAdder(float2 pos : TEXCOORD0) : COLOR0
{
	float4 orig = tex2D(TS,pos);
	float4 light = tex2D(LIGHT, pos) * 1.05f;
	
	//int2 rpos = (int2)(pos * screenres);

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

	orig.rgb = clamp(orig.rgb, 0, 1);

	return orig;
}

float4 Dither(float2 pos : TEXCOORD0) : COLOR0
{
	float4 orig = tex2D(TS,pos);
	float4 light = tex2D(LIGHT,pos);

	orig.rgb -= (1 - light.rgb);

	float bnw = (orig.r + orig.g + orig.b) / 3;
	if (light.r < .9)
	{
		float f = 2.698 * light.r * light.r - 1.317 * light.r;

		orig.rgb = orig.rgb * f + (bnw * (1 - f));
	}
	orig.rgb = clamp(orig.rgb, .25, 1);

	int2 rpos = (int2)(pos * screenres);

	float3 diff = fmod(orig.rgb, mod2);

	float4 outp = 1;

	if ((rpos.x % 4 <= 1 && rpos.y % 4 >= 2) || (rpos.x % 4 >= 2 && rpos.y % 4 <= 1))
	{
		outp.rgb = diff;
	}
	else
	{
		outp.rgb = -diff;
	}

	return outp;
}

float4 Dither2(float2 pos : TEXCOORD0) : COLOR0
{
	float4 orig = tex2D(TS,pos);
	float4 light = tex2D(LIGHT, pos);

	orig.rgb -= (1 - light.rgb);

	float bnw = (orig.r + orig.g + orig.b) / 3;
	if (light.r < .9)
	{
		float f = 2.698 * light.r * light.r - 1.317 * light.r;

		orig.rgb = orig.rgb * f + (bnw * (1 - f));
	}
	orig.rgb = clamp(orig.rgb, .25, 1);

	int2 rpos = (int2)(pos * screenres);

	float3 diff = fmod(orig.rgb, mod);

	float4 outp = 1;

	if ((rpos.x % 10 <= 4 && rpos.y % 10 >= 5) || (rpos.x % 10 >= 5 && rpos.y % 10 <= 4))
	{
		outp.rgb = diff;
	}
	else
	{
		outp.rgb = -diff;
	}

	return outp;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 LightAdder();
    }
	pass Pass2
	{
		PixelShader = compile ps_2_0 Dither();
	}
	pass Pass3
	{
		PixelShader = compile ps_2_0 Dither2();
	}
}
