Shader "Custom/PaintingTextureUpdate"
{
    Properties
    {
        _Tex("InputTex", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex InitCustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma multi_compile _ PAINTING_ON
            #include "UnityCustomRenderTexture.cginc"

            sampler2D _PaintingTexture;
            sampler2D _Tex;

            float _StrokeTime;

            float _LastMousePos[2];
            float _MousePos[2];
            float _AvgMouseDir[2];

            float _LastPressure;
            float _Pressure;

            float _LastInk;
            float _Ink;

            fixed4 frag (v2f_init_customrendertexture  i) : SV_Target
            {
                
                fixed4 col = tex2D(_PaintingTexture, i.texcoord);

                #ifdef PAINTING_ON
                float2 ScreenPos = float2(
                    _CustomRenderTextureWidth * i.texcoord.x,
                    _CustomRenderTextureHeight * i.texcoord.y
                );

                fixed2 curMouseDir = ScreenPos - float2(_MousePos[0], _MousePos[1]);
                fixed2 lastMouseDir = ScreenPos - float2(_LastMousePos[0], _LastMousePos[1]);
                fixed2 deltaDir = normalize(lastMouseDir - curMouseDir);

                fixed curDist = length(curMouseDir);
                fixed lastDist = length(lastMouseDir);
                fixed deltaDist = distance(curMouseDir, lastMouseDir);
                
                fixed projectLen = dot(lastMouseDir, deltaDir);
                fixed2 projectOnMouseDelta = projectLen * deltaDir;
                fixed2 normalOnMouseDelta = lastMouseDir - projectOnMouseDelta;
                fixed expectedWidth = lerp(_LastPressure, _Pressure, pow(projectLen / deltaDist, 2));

                fixed density = 0;
                if(     dot(lastMouseDir, deltaDir) > 0
                    &&  dot(curMouseDir, deltaDir) < 0
                    &&  length(normalOnMouseDelta) < expectedWidth)
                {
                    density = 1;
                }
                else if(curDist < _Pressure){
                    density = 1;
                }
                
                // To prevent from painting pixel repeatly
                /* 
                    Within one of the following condition
                    1.  Not in the last brush zone
                    2.  The first rendering in the stroke (time == 0)
                    3.  For forth-back stroke to accept painting repeatly
                */
                if( (lastDist < _LastPressure) == false 
                    || _StrokeTime == 0
                    || dot(deltaDir, fixed2(_AvgMouseDir[0], _AvgMouseDir[1])) < -0.5)
                {
                    /*********************
                        TODO:
                        Merge the ink dynamically 
                        (constraint with INK, Water, DirectionChange?)

                     *********************/
                    fixed projectRatio = (projectLen) / (deltaDist + _Pressure);
                    if(projectRatio >= 0)
                        density *= lerp(_LastInk, _Ink, projectRatio);
                    //density *= _LastInk;
                
                    col.xyz -= fixed3(density, density, density);
                }

                #endif
                return col;
            }
            ENDCG
        }
    }
}
