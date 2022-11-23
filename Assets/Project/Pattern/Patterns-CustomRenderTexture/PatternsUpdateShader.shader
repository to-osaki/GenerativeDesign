Shader "CustomRenderTexture/PatternsUpdateShader"
{

	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendSrc("Blend Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendDst("Blend Dst", Float) = 0
	}

	CGINCLUDE
#include "UnityCustomRenderTexture.cginc"

	sampler2D _MainTex;

	float4 frag(v2f_customrendertexture i) : SV_Target
	{
		float4 color = tex2D(_SelfTexture2D, i.globalTexcoord.xy);
		return color;
	}

	float4 frag_decal(v2f_customrendertexture i) : SV_Target
	{
		float4 color = tex2D(_MainTex, i.localTexcoord.xy);
		return color;
	}
	ENDCG

	SubShader
	{
		Blend [_BlendSrc] [_BlendDst]

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
			Name "Decal"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag_decal
			ENDCG
		}
	}
}