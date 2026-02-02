using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface ILeaderboardRepository
    {
       Task ParseOcrAndSaveAsync(int analysisId);

    Task<List<Leaderboardentry>> GetTopAsync(int n);
}
}
