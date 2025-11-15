using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DMonoStereo.Core.Models;

public partial record Album
{
    [NotMapped]
    public double? AverageTrackRating
    {
        get
        {
            var ratings = Tracks?
                .Where(track => track.Rating.HasValue)
                .Select(track => (double)track.Rating!.Value)
                .ToList();

            if (ratings == null || ratings.Count == 0)
            {
                return null;
            }

            return ratings.Average();
        }
    }
}


