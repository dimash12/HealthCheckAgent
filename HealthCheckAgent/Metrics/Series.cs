using System.Collections.Generic;
using System.Linq;

namespace HealthCheckAgent.Metrics
{
    public class Series
    {
        public Series(long posixTimeStamp, int count)
        {
            this.points.Add(new[] { posixTimeStamp, count }.ToList());
        }

        public string metric { get; set; }
        public List<List<long>> points { get; set; } = new List<List<long>>();

        public string type { get; set; }
        public string tags { get; set; }
        public string host { get; set; }
    }
}