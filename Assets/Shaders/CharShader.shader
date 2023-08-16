Shader "Somian/Unlit/Transparent" {
Properties {
    _Color ("Main Color (A=Opacity)", Color) = (1,0,1,1)
    _MainTex ("Base (A=Opacity)", 2D) = ""
}
Category {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" }
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    
    SubShader
    {
        Tags { "Queue"="Transparent" "PreviewType"="Plane" }
 
        LOD 100
 
        ZWrite Off
        Blend One OneMinusSrcAlpha
 
        Pass
        {
            Color [_Color] 
            SetTexture [_MainTex] { combine texture * constant ConstantColor[_Color] }
        }
    }
    
}
}