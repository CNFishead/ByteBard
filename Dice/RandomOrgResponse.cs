public partial class RandomOrgDiceRoller
{
    // DTO to deserialize the Random.org response
    private class RandomOrgResponse
    {
        public RandomOrgResult Result { get; set; }

        public class RandomOrgResult
        {
            public RandomData Random { get; set; }

            public class RandomData
            {
                public int[] Data { get; set; }
            }
        }
    }
}
