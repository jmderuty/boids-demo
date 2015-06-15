using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient.Cmd
{
    public class Metrics
    {
        private static readonly Metrics _instance = new Metrics();
        public static Metrics Instance
        {
            get
            {
                return _instance;
            }
        }

        private ConcurrentDictionary<string, MetricsRepository> _repositories = new ConcurrentDictionary<string, MetricsRepository>();

        public MetricsRepository GetRepository(string repId)
        {
            return _repositories.GetOrAdd(repId, i => new MetricsRepository());
        }
    }
    public class MetricsRepository
    {
        public void AddSample(ushort shipId, long sample)
        {
            _boidsTimes.AddOrUpdate(shipId, _ => new List<long> { sample }, (_, l) => { l.Add(sample); return l; });
        }
        private ConcurrentDictionary<ushort, List<long>> _boidsTimes = new ConcurrentDictionary<ushort, List<long>>();


        public DataMetrics ComputeMetrics()
        {
            var intervals = new List<int>();
            foreach (var boid in _boidsTimes)
            {
                var values = boid.Value.ToArray();
                boid.Value.Clear();
                for (int i = 0; i < values.Length; i++)
                {
                    intervals.Add((int)values[i]);
                    //intervals.Add((int)(values[i] - values[i - 1]));
                }
            }

            var result = new DataMetrics();
            if (intervals.Any())
            {
                intervals.Sort();

                result.Avg = intervals.Average();
                result.NbSamples = intervals.Count;
                for (int i = 0; i < 11; i++)
                {
                    result.Percentiles[i] = intervals[(i * (result.NbSamples - 1)) / 10];
                }
                result.Percentile99 = intervals[99 * (result.NbSamples - 1) / 100];
             
            }
            return result;
        }
    }

    public class DataMetrics
    {
        public double Avg;
        public int NbSamples;
        public int[] Percentiles = new int[11];
        public int Percentile99;
        
    }
}
