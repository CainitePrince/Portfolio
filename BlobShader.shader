Shader "Custom/BlobShader"
{
    Properties
    {
        _NoiseTex("Noise", 2D) = "white" {}
        _Color("Color", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 300

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 position : POSITION;
                float3 normal: NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangent: TANGENT;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
            };

            sampler2D _NoiseTex;
            sampler2D _Color;
            
            float4 proceduralTexture(float3 uvw)
            {
                // This will draw points.
                float d = max(uvw.x, max(uvw.y, uvw.z));
                return float4(smoothstep(0.88, 0.92, d).xxxx);
            }

            float4 sampleTextureWithFilter(float3 uvw, float3 ddx_uvw, float3 ddy_uvw)
            {
                const int MaxSamples = 10;

                int sx = 1 + int(clamp(4.0 * length(ddx_uvw - uvw), 0.0, float(MaxSamples - 1)));
                int sy = 1 + int(clamp(4.0 * length(ddy_uvw - uvw), 0.0, float(MaxSamples - 1)));

                float4 no = float4(0.0, 0.0, 0.0, 1.0);

                for (int j = 0; j < sy; j++)
                {
                    for (int i = 0; i < sx; i++)
                    {
                        float2 st = float2(float(i), float(j)) / float2(float(sx), float(sy));
                        no += proceduralTexture(uvw + st.x * (ddx_uvw - uvw) + st.y * (ddy_uvw - uvw));
                    }
                }

                no.xyz /= float(sx * sy);

                return no;
            }

            float noise3D(float3 p)
            {
                float a = tex2Dlod(_NoiseTex, float4(p.x, p.z, 0.0, 0.0)).r;
                float b = tex2Dlod(_NoiseTex, float4(p.x, p.y, 0.0, 0.0)).r;
                return lerp(a, b, 0.5);
            }
            
            float blobby_fbm(float3 p)
            {
                float t = 0.0;

                t += noise3D(float3(0.54 * p)) * 0.56;
                t += noise3D(float3(0.81 * p)) * 0.47;
                t += noise3D(float3(1.63 * p)) * 0.23;

                return t;
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                float3 n_rot = mul(unity_ObjectToWorld, v.normal);//  * normal;
                float2 projected_normal = normalize(float2(n_rot.x, n_rot.y));

                float time = _Time.y;

                // Add time to the noise parameters so it's animated
                float displacement = 3.0 * blobby_fbm(0.2 * v.normal + 0.05 * time) + 0.15 * sin(4.0 * v.position.y + 5.0 * time);

                // This creates an expanding and contracting effect over time.
                float breathing = 0.05 * sin(5.0 * time) + 0.8;

                float amplitude = 0.75;
                
                // We start with a sphere and displace the surface along the sphere normal with noise.
                float3 new_p = v.position + amplitude * (v.normal * breathing * displacement);

                // We want to calculate the normal of the displaced surface.
                // We do this by displacing two other points on the sphere and calculating the normal of that triangle.

                // This offset values seems to give reasonable results.
                float offset = 0.01;

                float3 a = normalize(v.position + v.tangent * offset);

                // Apply the displacement to the point using the normal that we would be using at that point.
                float3 normal_a = a;
                displacement = 3.0 * blobby_fbm(0.2 * normal_a + 0.05 * time) + 0.15 * sin(4.0 * a.y + 5.0 * time);
                float3 new_a = a + amplitude * (normal_a * breathing * displacement);
                
                float3 bitangent = normalize(cross(v.normal, v.tangent));
                float3 b = normalize(v.position + bitangent * offset);

                float3 normal_b = b;
                displacement = 3.0 * blobby_fbm(0.2 * normal_b + 0.05 * time) + 0.15 * sin(4.0 * b.y + 5.0 * time);
                float3 new_b = b + amplitude * (normal_b * breathing * displacement);

                // Calculate the normal of the triangle we found.
                float3 new_normal = normalize(cross(new_a - new_p, new_b - new_p));
                
                // The usual transformations.
                o.position = UnityObjectToClipPos(new_p);
                o.normal = UnityObjectToWorldNormal(new_normal);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Lights
                float3 l1 = float3(0.5773502691, 0.5773502691, 0.5773502691);
                float3 l2 = float3(-1.0, 0.0, 0.0);
                float3 l3 = float3(0.5773502691, -0.5773502691, 0.5773502691);

                float time = _Time.y;

                // Animated UVs
                float2 uv1 = float2((sin(0.14 * time + 2.1) * 0.5 + 0.5), 0.0);
                float2 uv2 = float2((sin(0.21 * time) * 0.5 + 0.5), 0.0);
                float2 uv3 = float2(1.0 - (sin(0.33 * time + 4.5) * 0.5 + 0.5), 0.0);

                float3 m1 = tex2D(_Color, uv1).rgb;
                float3 m2 = tex2D(_Color, uv2).rgb;
                float3 m3 = tex2D(_Color, uv3).rgb;

                float nl1 = clamp(dot(i.normal, l1), 0.0, 1.0);
                float nl2 = clamp(dot(i.normal, l2), 0.0, 1.0);
                float nl3 = clamp(dot(i.normal, l3), 0.0, 1.0);

                float3 col = float3(0.0, 0.0, 0.0);
                col = m1 * nl1;
                col += 0.2 * (m2 * nl2);
                col += m3 * nl3;

                // Apply texture filtering
                float w = 1 - (i.uv.x + i.uv.y);
                float3 uvw = float3(i.uv, w);

                // Calculate texture sampling footprint.
                float3 ddx_uvw = uvw + ddx(uvw);
                float3 ddy_uvw = uvw + ddy(uvw);

                // Do the actual sampling and filtering here.
                float4 tex = sampleTextureWithFilter(uvw, ddx_uvw, ddy_uvw);

                float3 spotColor = float3(0.20392156, 0.66274509, 0.99607843);

                float3 src = lerp(col * 5.0, spotColor, tex.r);
                float3 dst = float3(0.89, 0.89, 0.89);

                float3 final_color = lerp(src, dst, 0.5 * (1.0 - tex.r));

                return fixed4(final_color, tex.r);
            }
            ENDCG
        }
    }
}
