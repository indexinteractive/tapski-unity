## Supabase API Endpoints

The classes in this folder represent the REST API endpoints of our Supabase API tables.

In order to interact with some of these APIs, some functions need to be created

### Player Ranking

```sql
-- Create function for player ranking
CREATE OR REPLACE FUNCTION player_rank(device_id text, lim numeric) RETURNS TABLE (device_id text, display_name text, score numeric, rank numeric) AS $$
    WITH ranked_players AS (
        SELECT device_id, display_name, score, ROW_NUMBER() OVER (ORDER BY score DESC) AS rank
        FROM highscores
    ), player AS (
        SELECT rank FROM ranked_players
        WHERE device_id = $1
    )
    SELECT ranked_players.* FROM ranked_players, player
    WHERE ABS(ranked_players.rank - player.rank) <= $2
    ORDER BY ranked_players.rank;
$$ LANGUAGE SQL;

-- After creating a table or changing its primary key, you must refresh PostgREST schema cache
NOTIFY pgrst, 'reload schema';
```

Usage:

```sql
-- https://<project_id>.supabase.co/rest/v1/rpc/player_rank?device_id=SOME_ID&lim=3
SELECT * FROM player_rank('SOME_ID', 21)

-- Result:
-- | device_id | display_name | score | rank |
-- |-----------+--------------+-------+------|
-- | SOME_ID   | Test Player  | 100   | 1    |
-- | ...                                     |
```