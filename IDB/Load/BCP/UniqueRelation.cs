using System.Collections.Generic;

namespace IDB.Load.BCP
{
    public class UniqueRelation
    {
        public long ParentId { get; }
        public long ChildId { get; }

        public UniqueRelation(long parentId, long childId)
        {
            ParentId = parentId;
            ChildId = childId;
        }
    }

    public class UniqueRelationComparer : IEqualityComparer<UniqueRelation>
    {
        public bool Equals(UniqueRelation x, UniqueRelation y)
        {
            return y != null && x != null && x.ParentId == y.ParentId && x.ChildId == y.ChildId;
        }

        public int GetHashCode(UniqueRelation obj)
        {
            return $"{obj.ParentId}{obj.ChildId}".GetHashCode();
        }
    }
}
