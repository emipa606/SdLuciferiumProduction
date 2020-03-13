using System;
using System.Linq;
using System.Xml;
using Verse;

namespace sd_luciprod
{
	// Token: 0x02000009 RID: 9
	internal class PatchOperationFindMod : PatchOperation
	{
		// Token: 0x06000032 RID: 50 RVA: 0x00003600 File Offset: 0x00002600
		protected override bool ApplyWorker(XmlDocument xml)
		{
			return !this.modName.NullOrEmpty() && ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.Name == this.modName);
		}

		// Token: 0x04000019 RID: 25
		private string modName;
	}
}
