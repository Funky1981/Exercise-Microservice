namespace Exercise.API
{
    public static class MapEndpoints

    {
        public static void MapExerciseEndpoints(this WebApplication app)
        {
            app.MapGet("/exercises/{bodyPart}", async (string bodyPart, IHttpClientFactory httpClientFactory) =>
            {
                var client = httpClientFactory.CreateClient("ExerciseApi");
                var response = await client.GetAsync($"/exercises/{bodyPart}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return Results.Content(content, "application/json");
            });
        }
    }
}
