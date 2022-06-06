// https://www.oreilly.co.jp/books/9784873118475/
Shader "CustomRenderTexture/GrayScottModelShader"
{
	Properties
	{
		_F("feed", Range(0.0, 0.08)) = 0.022
		_K("kill", Range(0.0, 0.08)) = 0.051
		_Du("delta u", float) = 1.0 // > �}���nU�͋����nV���͂₭�g�U����
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
	float _Dx; // > Dx���傫���Ɓu�傫�ȋ�Ԃ��r���V�~�����[�V��������v�������Ɓu�����ȋ�Ԃ��ׂ����V�~�����[�V��������v
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
		float kill = (_F + _K) * v; // > V�̔Z�x�ɔ�Ⴕ�ė��o���A�[���ɂȂ�Ǝ~�܂�
		float du = _Du * laplacian.x - uvv + feed;
		float dv = _Dv * laplacian.y + uvv - kill;
		u = saturate(u + du * _Dt);
		v = saturate(v + dv * _Dt);
		
		return float4(u, v, 0, 0); // R32G32_FLOAT
	}

	float4 frag_init(v2f_customrendertexture i) : SV_Target
	{
		// > ������Ԃł͋�ԑS�̂ɍ���U�̔Z�x�ƁA�ႢV�̔Z�x��ݒ肵�܂��B
		// > �����Ƀp�^�[���́u��v�ɂȂ�悤�ȁA��r�I�ႢU�̔Z�x�ƍ���V�̔Z�x�̗̈悪����Ƃǂ��Ȃ�ł��傤���H
		// > �����ɂ�V�����݂��A������U+2V=3V�ɂ����U��V�ɕω����Ă������߂ɁAU�͂���Ɍ������AV�͂���ɑ������Ă����܂��B
		// > �����ɁA�g�U�̌��ʂɂ����U�͎��͂��痬�ꍞ�݁AV�͎��͂ɗ���o���Ă������߁A�E�E�E
		// > ���̏�Ԃ�U��V�̔Z�x�̈��肪�ۂ����΁A�����Ɏ��͂ƔZ�x�̈قȂ�Z�W�̃p�^�[�����`������܂��B
		return float4(0.25, 1, 0, 0);
	}
	ENDCG

	SubShader
	{
		// https://docs.unity3d.com/ja/2018.4/Manual/class-CustomRenderTexture.html
		// �J�X�^���e�N�X�`���̃V�F�[�_�[���쐬����Ƃ��ɁA�ȉ��̃X�e�b�v�������K�{�ł��B
		// �E#include �gUnityCustomRenderTexture.cginc�h
		// �E�^����ꂽ���_�V�F�[�_�[ CustomRenderTextureVertexShader ���g�p
		// �E�s�N�Z���V�F�[�_�[�ɗ^����ꂽ���͍\���� v2f_customrendertexture ���g�p
		// ��L�ȊO�́A���[�U�[�̎v���ʂ�Ƀs�N�Z���V�F�[�_�[���쐬�ł��܂��B
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