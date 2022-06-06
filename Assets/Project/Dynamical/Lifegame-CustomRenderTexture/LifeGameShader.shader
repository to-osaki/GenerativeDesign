Shader "CustomRenderTexture/LifeGameShader"
{

	Properties
	{
	}

	CGINCLUDE
#include "UnityCustomRenderTexture.cginc"

	float4 frag(v2f_customrendertexture i) : SV_Target
	{
		float du = 1.0 / _CustomRenderTextureWidth;
		float dv = 1.0 / _CustomRenderTextureHeight;
		float2 dluv = float2(du, 0);
		float2 duuv = float2(du, dv);
		float2 druv = float2(0, dv);
		float2 dduv = float2(-du, dv);
		float2 uv = i.globalTexcoord;
		float4 color = tex2D(_SelfTexture2D, uv); // need double buffering 

		float4 self = color;
		float4 v1 = tex2D(_SelfTexture2D, uv - duuv);
		float4 v2 = tex2D(_SelfTexture2D, uv + duuv);
		float4 v3 = tex2D(_SelfTexture2D, uv - dluv);
		float4 v4 = tex2D(_SelfTexture2D, uv + dluv);
		float4 v5 = tex2D(_SelfTexture2D, uv - druv);
		float4 v6 = tex2D(_SelfTexture2D, uv + druv);
		float4 v7 = tex2D(_SelfTexture2D, uv - dduv);
		float4 v8 = tex2D(_SelfTexture2D, uv + dduv);
		float4 total = (v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8);

		float4 c =
			self * (1 - round(clamp(abs(total - 2.5), 0, 1))) +
			(1 - self) * (1 - round(clamp(abs(total - 3), 0, 1)));
		return c;
	}

	float4 frag_touch(v2f_customrendertexture i) : SV_Target
	{
		return float4(1, 1, 1, 1);
	}
	float4 frag_touch_r(v2f_customrendertexture i) : SV_Target
	{
		return float4(1, 0, 0, 1);
	}
	float4 frag_touch_g(v2f_customrendertexture i) : SV_Target
	{
		return float4(0, 1, 0, 1);
	}
	float4 frag_touch_b(v2f_customrendertexture i) : SV_Target
	{
		return float4(0, 0, 1, 1);
	}
	ENDCG

	SubShader
	{
		Pass
		{
			Name "Update"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag
			ENDCG
		}

		Pass
		{
			Name "Touch"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag_touch
			ENDCG
		}
		Pass
		{
			Name "TouchR"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag_touch_r
			ENDCG
		}
		Pass
		{
			Name "TouchG"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag_touch_g
			ENDCG
		}
		Pass
		{
			Name "TouchB"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag_touch_b
			ENDCG
		}
	}
}