using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    //
    public class PropertyDefToContentSourceMapping : IVaultDbEntity
    {
        public long PropertyDefID { get; set; } // bigint, not null

        public long ContentSourceID { get; set; } // bigint, not null

        public string ContentSourcePropertyMoniker { get; set; } // nvarchar(255), not null

        public bool CreateNew { get; set; } // bit, not null

        public string Classification { get; set; } // nvarchar(20), null

        public string CSPDefType { get; set; } // nvarchar(20), not null

        public string DisplayName { get; set; } // nvarchar(100), not null

        public string MappingDirection { get; set; } // nvarchar(20), not null

        public string FriendlyName { get; set; } // nvarchar(60), not null

        public bool IsSystem { get; set; } // bit, not null

        public long GetId()
        {
            return PropertyDefID;
        }

        public string GetSelectString()
        {
            return "SELECT ECPM.PropertyDefID, ECPM.ContentSourceID, ECPM.ContentSourcePropertyMoniker, ECPM.CreateNew, CSPD.Classification, CSPD.CSPDefType, CSPD.DisplayName, ECPM.MappingDirection, PD.FriendlyName, PD.IsSystem " +
                        "FROM EntityClassPropertyDefContentSourceMapping ECPM " +
                            "INNER JOIN ContentSourcePropertyDef CSPD ON ECPM.ContentSourcePropertyMoniker = CSPD.ContentSourcePropertyMoniker AND ECPM.ContentSourceID = CSPD.ContentSourceID " +
                            "INNER JOIN PropertyDef PD ON ECPM.PropertyDefID = PD.PropertyDefID " +
                            "WHERE ECPM.EntityClassID = 8";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }

        // method to verify that the property can be written as a UDP
        static public bool VerifyWriteableUDP(VaultCacheOld vaultCache, PropertyDef propertyDef, long contentSourceId)
        {
            bool bWriteable = true;
            if (vaultCache._propertyDefToContentSourceMappingsByPropertyDefIds.Contains(propertyDef.PropertyDefID))
            {
                var propDefToContSrcMapList = vaultCache._propertyDefToContentSourceMappingsByPropertyDefIds[propertyDef.PropertyDefID];
                var writeableMaps = propDefToContSrcMapList.Where(pdtcsm => pdtcsm.ContentSourceID == contentSourceId && !pdtcsm.IsSystem && pdtcsm.MappingDirection == "Write").Select(pdtcsm => pdtcsm);

                // if there are none, set the bWriteable to false
                if (!writeableMaps.Any())
                {
                    bWriteable = false;
                }
            }

            return (bWriteable);
        }

    }
}
