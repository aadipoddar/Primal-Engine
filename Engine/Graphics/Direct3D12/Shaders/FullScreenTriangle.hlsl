struct VSOutput
{
	noperspective float4 Position : SV_Position;
	noperspective float4 UV : TEXCOORD;;
};

VSOutput FullScreenTriangleVS(int uint VextexIdx : SV_VextexID)
{
    VSOutput output;

    // TODO : Write Fullsreeen Triangle Code
    output.Position = float4(0, 0, 0, 1);

    return output;
}