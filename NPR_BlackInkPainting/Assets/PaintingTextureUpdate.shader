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

            float _LastMousePos[2];
            float _MousePos[2];
            float _AvgMouseDir[2];

            float _LastPressure;
            float _Pressure;

            float _InitialInk;

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
                fixed expectedWidth = lerp(_LastPressure, _Pressure, pow(projectLen/deltaDist, 3));

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
                density *= 0.5;
                
                // BUG : Cannot add ink in the exactly same place
                // To prevent from painting pixel repeatly
                if( (lastDist < _LastPressure) == false 
                    || dot(deltaDir, fixed2(_AvgMouseDir[0], _AvgMouseDir[1])) < 0)
                {
                    /*********************
                        TODO:
                        Merge the ink dynamically 
                        (constraint with INK, Water, DirectionChange?)

                     *********************/
                    col.xyz -= fixed3(density, density, density);
                }

                #endif
                return col;
            }
            ENDCG
        }
    }
}
