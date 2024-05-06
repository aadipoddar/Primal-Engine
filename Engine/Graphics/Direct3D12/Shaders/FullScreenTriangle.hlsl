struct VSOutput
{
    noperspective float4 Position : SV_Position;
    noperspective float2 UV : TEXCOORD;
};

VSOutput FullScreenTriangleVS(in uint VertexIdx: SV_VertexID) 
{
    VSOutput output;
  
    output.Position = float4(0, 0, 0, 1);
  
    return output;
}
