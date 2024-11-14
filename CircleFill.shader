Shader "Custom/CircleFill"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" { }
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _Rotation("Rotation", Float) = 0
        _Inverse("Inverse", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
        }

        Cull Off //禁用背面剔除
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Transparency;
            float _Rotation;
            float _Inverse;

            //source: https://stackoverflow.com/a/3451607/3987342
            float remap(float value, float2 i, float2 o)
            {
                return o.x + (value - i.x) * (o.y - o.x) / (i.y - i.x);
            }
            
            //rotate uvs using radians
			//source: https://forum.unity.com/threads/rotation-of-texture-uvs-directly-from-a-shader.150482/
            float2 rotateuv(float2 uv, float2 center, float rotation)
            {
                uv -= center;
                float s = sin(rotation);
                float c = cos(rotation);
                float2x2 rotMat = float2x2(c, -s, s, c);
                rotMat *= .5;
                rotMat += .5;
                rotMat = rotMat * 2 - 1;
                float2 res = mul(uv, rotMat);
                res += center;
                return res;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.x = (step(0, _Inverse) - uv.x) * _Inverse;//_Inverse配置1时,顺时针，配置-1时，逆时针
                half4 texColor = tex2D(_MainTex, i.uv);
                float2 afterRotateUV = rotateuv(uv, float2(0.5, 0.5), radians(_Rotation));
                float x = lerp(-1, 1, afterRotateUV.x);
                float y = lerp(-1, 1, afterRotateUV.y);
                float pi = 3.1415;
                float pi2 = pi * 2;
                float2 fillAmoutRange = float2(0, 1);
                float2 targetRange = float2(0, pi2);
                float processFillAmount = remap(_Transparency, fillAmoutRange, targetRange);
                float currentArc = atan2(y, x);
                float2 arcRange = float2(-pi, pi);
                float processArc = remap(currentArc, arcRange, targetRange);
                float alpha = step(processFillAmount, processArc);
                texColor.a *= alpha;

                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}