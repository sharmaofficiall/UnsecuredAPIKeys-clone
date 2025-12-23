using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("PG_CONN")
            ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Database=UnsecuredAPIKeys;Username=postgres;Password=sunny123;Port=5432";

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            var updateCmd = new NpgsqlCommand("UPDATE \"ApplicationSettings\" SET \"Value\"='true' WHERE \"Key\"='AllowScraper';", conn);
            await updateCmd.ExecuteNonQueryAsync();

            var insertCmd = new NpgsqlCommand("INSERT INTO \"ApplicationSettings\" (\"Key\",\"Value\") SELECT 'AllowScraper','true' WHERE NOT EXISTS (SELECT 1 FROM \"ApplicationSettings\" WHERE \"Key\"='AllowScraper');", conn);
            await insertCmd.ExecuteNonQueryAsync();

            var selectCmd = new NpgsqlCommand("SELECT \"Key\", \"Value\" FROM \"ApplicationSettings\" WHERE \"Key\"='AllowScraper';", conn);
            await using (var reader = await selectCmd.ExecuteReaderAsync())
            {
                Console.WriteLine("Result rows:");
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"{reader.GetString(0)} = {reader.GetString(1)}");
                }
            }

            // If a GitHub token is provided via env var, insert it into SearchProviderTokens
            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (!string.IsNullOrWhiteSpace(githubToken))
            {
                Console.WriteLine("GITHUB_TOKEN env var found — inserting into SearchProviderTokens...");
                var insertTokenCmd = new NpgsqlCommand(
                    "INSERT INTO \"SearchProviderTokens\" (\"Token\", \"SearchProvider\", \"IsEnabled\", \"LastUsedUTC\") VALUES (@token, 1, true, NULL);",
                    conn);
                insertTokenCmd.Parameters.AddWithValue("token", githubToken);
                await insertTokenCmd.ExecuteNonQueryAsync();

                var verifyCmd = new NpgsqlCommand("SELECT \"Id\", \"SearchProvider\", \"IsEnabled\", \"LastUsedUTC\" FROM \"SearchProviderTokens\" WHERE \"SearchProvider\"=1 ORDER BY \"Id\" DESC LIMIT 5;", conn);
                await using (var vr = await verifyCmd.ExecuteReaderAsync())
                {
                    Console.WriteLine("Recent GitHub tokens:");
                    while (await vr.ReadAsync())
                    {
                        var lastUsed = vr.IsDBNull(3) ? "NULL" : vr.GetDateTime(3).ToString();
                        Console.WriteLine($"id={vr.GetInt32(0)}, SearchProvider={vr.GetInt32(1)}, IsEnabled={vr.GetBoolean(2)}, LastUsedUTC={lastUsed}");
                    }
                }

                    // Also ensure a set of high-yield search queries exist (insert if not present)
                    var queries = new[] {
                        "\"sk-proj-\" extension:json",
                        "\"sk-proj-\" extension:env",
                        "\"OPENAI_API_KEY\" extension:env",
                        "\"ANTHROPIC_API_KEY\" extension:env",
                        "\"sk-ant-\" extension:json",
                        "\"DEEPSEEK_API_KEY\" extension:env",
                        "\"MISTRAL_API_KEY\" extension:env",
                        "\"GROQ_API_KEY\" extension:env",
                        "\"gsk_\" extension:json",
                        "\"AIza\" extension:json",
                        "\"AIza\" extension:env",
                        "\"GOOGLE_API_KEY\" extension:env",
                        "\"HUGGING_FACE_HUB_TOKEN\" extension:env",
                        "\"CO_API_KEY\" extension:env",
                        "\"PERPLEXITY_API_KEY\" extension:env",
                        "\"STABILITY_KEY\" extension:env",
                        "\"pa-\" extension:json",
                        "\"fw_\" extension:json",
                        "\"octo_\" extension:json",
                        "\"REPLICATE_API_TOKEN\" extension:env"
                    };

                    foreach (var q in queries)
                    {
                        var insertQueryCmd = new NpgsqlCommand(
                            "INSERT INTO \"SearchQueries\" (\"Query\", \"IsEnabled\", \"SearchResultsCount\", \"LastSearchUTC\") SELECT @q, true, 0, now() WHERE NOT EXISTS (SELECT 1 FROM \"SearchQueries\" WHERE \"Query\"=@q);",
                            conn);
                        insertQueryCmd.Parameters.AddWithValue("q", q);
                        await insertQueryCmd.ExecuteNonQueryAsync();
                    }
                    Console.WriteLine("Ensured search queries are present.");
                    // Mark queries as due by setting LastSearchUTC to a distant past value
                    var markDueCmd = new NpgsqlCommand("UPDATE \"SearchQueries\" SET \"LastSearchUTC\" = '1970-01-01' WHERE \"IsEnabled\" = true;", conn);
                    await markDueCmd.ExecuteNonQueryAsync();
                    Console.WriteLine("Marked enabled queries as due (LastSearchUTC set to 1970-01-01).");

                    // Ensure bot continuous-mode and verifier settings exist
                    var upsertSettings = new (string key, string value)[] {
                        ("ScraperContinuousMode", "true"),
                        ("AllowVerifier", "true"),
                        ("VerifierContinuousMode", "true")
                    };

                    foreach (var s in upsertSettings)
                    {
                        var upsertCmd = new NpgsqlCommand(
                            "INSERT INTO \"ApplicationSettings\" (\"Key\", \"Value\") SELECT @k, @v WHERE NOT EXISTS (SELECT 1 FROM \"ApplicationSettings\" WHERE \"Key\"=@k) ;",
                            conn);
                        upsertCmd.Parameters.AddWithValue("k", s.key);
                        upsertCmd.Parameters.AddWithValue("v", s.value);
                        await upsertCmd.ExecuteNonQueryAsync();
                        var updateSettingCmd = new NpgsqlCommand(
                            "UPDATE \"ApplicationSettings\" SET \"Value\"=@v WHERE \"Key\"=@k;",
                            conn);
                        updateSettingCmd.Parameters.AddWithValue("k", s.key);
                        updateSettingCmd.Parameters.AddWithValue("v", s.value);
                        await updateSettingCmd.ExecuteNonQueryAsync();
                    }
                    Console.WriteLine("Enabled continuous-mode and verifier settings in ApplicationSettings.");
            }

                // Print a masked view of recent API keys (do not show full secrets)
                var countCmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"APIKeys\";", conn);
                var totalKeys = (long)await countCmd.ExecuteScalarAsync();
                Console.WriteLine($"APIKeys total count = {totalKeys}");
                var maskedSelect = new NpgsqlCommand(
                    "SELECT \"Id\", (\"ApiType\")::text AS \"ApiType\", (\"Status\")::text AS \"Status\", (\"SearchProvider\")::text AS \"SearchProvider\", \"FirstFoundUTC\", \"LastFoundUTC\", \"TimesDisplayed\", \"ErrorCount\", " +
                    "substring(\"ApiKey\" from 1 for 4) || '...' || substring(\"ApiKey\" from greatest(length(\"ApiKey\")-3,1) for 4) AS \"MaskedApiKey\" " +
                    "FROM \"APIKeys\" ORDER BY \"Id\" DESC LIMIT 20;",
                    conn);
                await using (var mr = await maskedSelect.ExecuteReaderAsync())
                {
                    Console.WriteLine("===MASKED_KEYS_START===");
                    System.IO.File.WriteAllText("masked_keys.txt", "");
                        Console.WriteLine("Recent API keys (masked):");
                        while (await mr.ReadAsync())
                        {
                        var id = mr.GetInt64(0);
                        var apiType = mr.IsDBNull(1) ? "" : mr.GetString(1);
                        var status = mr.IsDBNull(2) ? "" : mr.GetString(2);
                        var provider = mr.IsDBNull(3) ? "" : mr.GetString(3);
                        var firstFound = mr.IsDBNull(4) ? "NULL" : mr.GetDateTime(4).ToString();
                        var lastFound = mr.IsDBNull(5) ? "NULL" : mr.GetDateTime(5).ToString();
                        var times = mr.IsDBNull(6) ? 0 : mr.GetInt32(6);
                        var errors = mr.IsDBNull(7) ? 0 : mr.GetInt32(7);
                        var masked = mr.IsDBNull(8) ? "" : mr.GetString(8);
                        var line = $"Id={id} Provider={provider} Type={apiType} Status={status} FirstFound={firstFound} LastFound={lastFound} Times={times} Errors={errors} Key={masked}";
                        Console.WriteLine(line);
                        System.IO.File.AppendAllText("masked_keys.txt", line + Environment.NewLine);
                    }
                }
                Console.WriteLine("===MASKED_KEYS_END===");

            Console.WriteLine("Done.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
            Console.Error.WriteLine(ex);
            return 2;
        }
    }
}
