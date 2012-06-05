using System;
using System.Collections.Generic;

namespace JM.Diag
{
    public class BoxFactory
    {
        private static BoxFactory instance;
        private Dictionary<BoxVersion, ICommbox> commboxes;
        private BoxVersion version;

        static BoxFactory()
        {
            instance = new BoxFactory();
        }

        private BoxFactory()
        {
            commboxes = new Dictionary<BoxVersion, ICommbox>();
            version = BoxVersion.C168;
        }

        public static BoxFactory Instance
        {
            get { return instance; }
        }

        public BoxVersion Version
        {
            get { return version; }
            set
            {
                version = value;
                if (commboxes.ContainsKey(version))
                {
                    return;
                }

                switch (version)
                {
                    case BoxVersion.C168:
                        commboxes[version] = new C168.Commbox();
                        break;
                    case BoxVersion.W80:
                        commboxes[version] = new W80.Commbox();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public ICommbox Commbox
        {
            get
            {
                if (commboxes.ContainsKey(Version))
                    return commboxes[Version];
                return null;
            }
        }
    }
}