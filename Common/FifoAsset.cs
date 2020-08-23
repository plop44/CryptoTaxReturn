using System;
using System.Collections.Generic;

namespace Common
{
    public sealed class FifoAsset : Asset
    {
        public FifoAsset(AssetName name) : base(name)
        {
        }

        protected override int GetCostIndex()
        {
            return 0;
        }
    }
}