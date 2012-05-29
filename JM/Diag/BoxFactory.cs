using System;

namespace JM.Diag
{
    public class BoxFactory
    {
        public ICommbox CreateCommbox(BoxVersion ver)
        {
            switch (ver)
            {
                case BoxVersion.C168:
                    return new C168.Commbox();
                case BoxVersion.W80:
                    return new W80.Commbox();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}