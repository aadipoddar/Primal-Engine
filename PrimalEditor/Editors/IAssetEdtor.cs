using PrimalEditor.Content;

namespace PrimalEditor.Editors
{
    interface IAssetEdtor
    {
        Asset Asset { get; }

        void SetAsset(Asset asset);
    }
}