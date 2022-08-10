using System;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
    internal abstract class BaseDao
    {
        protected static Random Random = new Random(DateTime.Now.Millisecond);
    }
}
