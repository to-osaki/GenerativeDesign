// https://www.oreilly.co.jp/books/9784873118475/
Shader "CustomRenderTexture/GrayScottModelShader"
{
	Properties
	{
		_F("feed", Range(0.0, 0.08)) = 0.022
		_K("kill", Range(0.0, 0.08)) = 0.051
		_Du("delta u", float) = 1.0 // > 抑制系Uは興奮系Vよりはやく拡散する
		_Dv("delta v", float) = 0.5
		_Dx("delta x", float) = 1
		_Dt("delta t", float) = 1
	}

	CGINCLUDE
	#include "UnityCustomRenderTexture.cginc"

	float _F;
	float _K;
	// Parameters
	float _Du;
	float _Dv;
	float _Dx; // > Dxが大きいと「大きな空間を荒くシミュレーションする」小さいと「小さな空間を細かくシミュレーションする」
	float _Dt;

	float4 calc_laplacian4(float4 self, float2 uv)
	{
		float w = 1.0 / _CustomRenderTextureWidth;
		float h = 1.0 / _CustomRenderTextureHeight;
		float4 uv1 = tex2D(_SelfTexture2D, uv - float2(w, 0));
		float4 uv2 = tex2D(_SelfTexture2D, uv + float2(w, 0));
		float4 uv3 = tex2D(_SelfTexture2D, uv - float2(0, h));
		float4 uv4 = tex2D(_SelfTexture2D, uv + float2(0, h));
		float4 laplacian = (uv1 + uv2 + uv3 + uv4) - self * 4;
		return laplacian;
	}

	float4 frag_update(v2f_customrendertexture i) : SV_Target
	{
		// feed U : U + 2V = 3V
		// kill V : V = P
		float4 self = tex2D(_SelfTexture2D, i.globalTexcoord); // need double buffering 
		float4 laplacian = calc_laplacian4(self, i.globalTexcoord) / (_Dx * _Dx);

		float u = self.x;
		float v = self.y;
		float uvv = u * v * v;
		float feed = _F * (1 - u);
		float kill = (_F + _K) * v; // > Vの濃度に比例して流出し、ゼロになると止まる
		float du = _Du * laplacian.x - uvv + feed;
		float dv = _Dv * laplacian.y + uvv - kill;
		u = saturate(u + du * _Dt);
		v = saturate(v + dv * _Dt);
		
		return float4(u, v, 0, 0); // R32G32_FLOAT
	}

	float4 frag_init(v2f_customrendertexture i) : SV_Target
	{
		// > 初期状態では空間全体に高いUの濃度と、低いVの濃度を設定します。
		// > ここにパターンの「種」になるような、比較的低いUの濃度と高いVの濃度の領域があるとどうなるでしょうか？
		// > そこにはVが存在し、反応式U+2V=3VによってUがVに変化していくために、Uはさらに減少し、Vはさらに増加していきます。
		// > 同時に、拡散の効果によってUは周囲から流れ込み、Vは周囲に流れ出していくため、・・・
		// > この状態でUとVの濃度の安定が保たれれば、そこに周囲と濃度の異なる濃淡のパターンが形成されます。
		return float4(0.25, 1, 0, 0);
	}
	ENDCG

	SubShader
	{
		// https://docs.unity3d.com/ja/2018.4/Manual/class-CustomRenderTexture.html
		// カスタムテクスチャのシェーダーを作成するときに、以下のステップだけが必須です。
		// ・#include “UnityCustomRenderTexture.cginc”
		// ・与えられた頂点シェーダー CustomRenderTextureVertexShader を使用
		// ・ピクセルシェーダーに与えられた入力構造体 v2f_customrendertexture を使用
		// 上記以外は、ユーザーの思い通りにピクセルシェーダーを作成できます。
		Pass
		{
			Name "Update"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag_update
			ENDCG
		}
		Pass
		{
			Name "Init"
			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag_init
			ENDCG
		}
	}
}