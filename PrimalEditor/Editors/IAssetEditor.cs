using PrimalEditor.Content;

namespace PrimalEditor.Editors
{
	interface IAssetEditor
    {
        Asset Asset { get; }

        void SetAsset(AssetInfo asset);
    }
}