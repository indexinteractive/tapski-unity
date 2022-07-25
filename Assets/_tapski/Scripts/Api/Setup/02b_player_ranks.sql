-- player_rank function
--
-- Api usage:
--   https://<project_id>.supabase.co/rest/v1/rpc/player_rank?device_id=SOME_ID&lim=3
--
-- Sql Usage:
--  SELECT * FROM player_rank('SOME_ID', 21)
--
--   Result:
--   | device_id | display_name | score | rank |
--   |-----------+--------------+-------+------|
--   | SOME_ID   | Test Player  | 100   | 1    |
--   | ...                                     |
CREATE OR REPLACE FUNCTION player_rank(device_id TEXT, lim NUMERIC) RETURNS TABLE (device_id TEXT, display_name TEXT, score NUMERIC, rank NUMERIC) AS $$
    WITH ranked_players AS (
        SELECT device_id, display_name, score, ROW_NUMBER() OVER (ORDER BY score DESC) AS rank
        FROM highscores
    ), player AS (
        SELECT rank FROM ranked_players
        WHERE device_id = (CASE
          WHEN EXISTS (SELECT device_id FROM highscores WHERE device_id = $1) THEN $1
          ELSE (SELECT device_id FROM highscores ORDER BY score DESC LIMIT 1)
        END)
    )
    SELECT ranked_players.* FROM ranked_players, player
    WHERE ABS(ranked_players.rank - player.rank) <= $2
    ORDER BY ranked_players.rank;
$$ LANGUAGE SQL;

-- After creating/modifying functions you must refresh PostgREST schema cache
NOTIFY pgrst, 'reload schema';