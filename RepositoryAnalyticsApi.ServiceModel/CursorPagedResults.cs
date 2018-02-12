using System.Collections.Generic;

namespace RepositoryAnalyticsApi.ServiceModel
{
    public class CursorPagedResults<T>
    {
        public IEnumerable<T> Results { get; set; }
        public bool MoreToRead { get; set; }
        public string EndCursor { get; set; }
    }
}
