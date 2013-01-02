using System.Collections.Generic;
using System.Linq;
using LuceneSearch.Model;

namespace LuceneSearch.Data {
  public static class SampleDataRepository {
    public static SampleData Get(int id) {
      return GetAll().SingleOrDefault(x => x.Id.Equals(id));
    }
    public static List<SampleData> GetAll() {
      return new
        List<SampleData> {
                           new SampleData {Id = 1, Name = "Belgrad", Description = "City in Serbia"},
                           new SampleData {Id = 2, Name = "Moscow", Description = "City in Russia"},
                           new SampleData {Id = 3, Name = "Chicago", Description = "City in USA"},
                           new SampleData {Id = 4, Name = "Mumbai", Description = "City in India"},
                           new SampleData {Id = 5, Name = "Hong-Kong", Description = "City in Hong-Kong"},
                         };
    }
  }
}