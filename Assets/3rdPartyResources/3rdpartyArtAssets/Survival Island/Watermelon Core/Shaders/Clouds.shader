Shader "WMelon/Clouds"
{
	Properties
    {
		_LightColor ("Light Color", Color) = (1, 1, 1, 1)

		[Space]
		_CloudsColor1 ("Clouds Color 1", Color) = (1, 1, 1, 1)
		_CloudsTex1 ("Clouds 1", 2D) = "white" {}
		_CloudsTexSize1 ("Clouds Size 1", Vector) = (1, 1, 0, 0)
		_Velocity1("Velocity 1", Vector) = (0.1, 0.1, 0, 0)
		[Space]
		_CloudsInfluence1("Clouds Influende 1", Range(0, 1)) = 1

		[Space]
		_CloudsColor2 ("Clouds Color 2", Color) = (1, 1, 1, 1)
		_CloudsTex2 ("Clouds 2", 2D) = "white" {}
		_CloudsTexSize2 ("Clouds Size 2", Vector) = (1, 1, 0, 0)
		_Velocity2("Velocity 2", Vector) = (0.1, 0.1, 0, 0)
		_CloudsInfluence2("Clouds Influende 2", Range(0, 1)) = 1
	}

	SubShader
    {
		Blend One Zero
		Lighting Off

		Pass
		{
			Name "CloudShadow"

			HLSLPROGRAM
			
			#include "UnityCustomRenderTexture.cginc"

			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag

			half4 _LightColor;

			sampler2D _CloudsTex1;
			sampler2D _CloudsTex2;

			half4 _CloudsColor1;
			half4 _CloudsColor2;

			half4 _Velocity1;
			half4 _Velocity2;

			half4 _CloudsTexSize1;
			half4 _CloudsTexSize2;

			half _CloudsInfluence1;
			half _CloudsInfluence2;

			float4 frag(v2f_customrendertexture input) : SV_Target
			{
				half4 cloud1 = 1 - tex2D(_CloudsTex1, input.localTexcoord.xy / _CloudsTexSize1 + (_Time.y * _Velocity1.xy) % 1);
				cloud1 = cloud1 * (1 - _CloudsColor1) * _CloudsColor1.a;

				half4 cloud2 = 1 - tex2D(_CloudsTex2, input.localTexcoord.xy / _CloudsTexSize2 + (_Time.y * _Velocity2.xy) % 1);
				cloud2 = cloud2 * (1 - _CloudsColor2) * _CloudsColor2.a;

				return _LightColor - cloud1 * _CloudsInfluence1 - cloud2 * _CloudsInfluence2;//max(0.1f, cloud1 * cloud2);
			}

			ENDHLSL
		}
	}
}