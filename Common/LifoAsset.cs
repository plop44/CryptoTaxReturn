namespace Common
{
    public sealed class LifoAsset : Asset
    {
        public LifoAsset(AssetName name) : base(name)
        {
        }

        protected override int GetCostIndex()
        {
            return PreviousCosts.Count - 1;
        }
    }
}