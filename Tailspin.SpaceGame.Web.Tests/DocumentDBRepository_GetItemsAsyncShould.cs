using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using TailSpin.SpaceGame.Web;
using TailSpin.SpaceGame.Web.Models;

namespace Tests
{
    public class DocumentDBRepository_GetItemsAsyncShould
    {
        private IDocumentDBRepository<Score> _scoreRepository;

        [SetUp]
        public void Setup()
        {
            using (Stream scoresData = typeof(IDocumentDBRepository<Score>)
                .Assembly
                .GetManifestResourceStream("Tailspin.SpaceGame.Web.SampleData.scores.json"))
            {
                _scoreRepository = new LocalDocumentDBRepository<Score>(scoresData);
            }
        }

        [TestCase("Milky Way")]
        [TestCase("Andromeda")]
        [TestCase("Pinwheel")]
        [TestCase("NGC 1300")]
        [TestCase("Messier 82")]
        public void CheckCount(string gameRegion)
        {
            int total = GetCountManually(gameRegion);

            Expression<Func<Score, bool>> queryPredicate = score => (score.GameRegion == gameRegion);

            // Fetch the scores.
            Task<int> scoresTask = _scoreRepository.CountItemsAsync(
                queryPredicate // the predicate defined above
            );
            int total1 = scoresTask.Result;
            Assert.AreEqual(total, total1);
        }

        [TestCase("Milky Way")]
        [TestCase("Andromeda")]
        [TestCase("Pinwheel")]
        [TestCase("NGC 1300")]
        [TestCase("Messier 82")]
        public void FetchOnlyRequestedGameRegion(string gameRegion)
        {
            const int PAGE = 0; // take the first page of results
            const int MAX_RESULTS = 10; // sample up to 10 results

            // Form the query predicate.
            // This expression selects all scores for the provided game region.
            Expression<Func<Score, bool>> queryPredicate = score => (score.GameRegion == gameRegion);

            // Fetch the scores.
            Task<IEnumerable<Score>> scoresTask = _scoreRepository.GetItemsAsync(
                queryPredicate, // the predicate defined above
                score => 1, // we don't care about the order
                PAGE,
                MAX_RESULTS
            );
            IEnumerable<Score> scores = scoresTask.Result;

            // Verify that each score's game region matches the provided game region.
            Assert.That(scores, Is.All.Matches<Score>(score => score.GameRegion == gameRegion));
        }

        public int GetCountManually(string gameRegion) 
        {
                        // Form the query predicate.
            // This expression selects all scores for the provided game region.
            Expression<Func<Score, bool>> queryPredicate = score => (score.GameRegion == gameRegion);

            int total = 0;
            int page = 0;
            while (true) {
                // Fetch the scores.
                Task<IEnumerable<Score>> scoresTask = _scoreRepository.GetItemsAsync(
                    queryPredicate, // the predicate defined above
                    score => 1, // we don't care about the order
                    page,
                    100
                );
                int t = 0;
                foreach (var a in scoresTask.Result) ++t;
                if (t==0) break;
                total += t;
                ++ page;
            }
            return total;
        }

    }
}