public partial class RandomOrgDiceRoller
{
    // DTO to deserialize the Random.org response
    private class RandomOrgResponse
    {
        public string Jsonrpc { get; set; }
        public RandomOrgResult Result { get; set; }
        public string Id { get; set; }

        public class RandomOrgResult
        {
            public RandomData Random { get; set; }
            public int BitsUsed { get; set; }
            public int BitsLeft { get; set; }
            public int RequestsLeft { get; set; }
            public int AdvisoryDelay { get; set; }

            public class RandomData
            {
                public List<int> Data { get; set; }
                public string CompletionTime { get; set; }
            }
        }
    }
}
