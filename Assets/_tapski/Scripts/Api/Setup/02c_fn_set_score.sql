-- checksum function to verify against input data
-- inspired by https://community.clickteam.com/threads/78708-Protecting-Leaderboard-scores
CREATE OR REPLACE FUNCTION score_checksum(score INTEGER) RETURNS INTEGER AS $$
  SELECT 1;
$$ LANGUAGE SQL;

-- scoring function that uses the checksum defined above to determine if the score is valid
--
-- Api usage:
--   POST https://<project_id>.supabase.co/rest/v1/rpc/set_score
--     {
--       "id": "",
--       "score": "",
--       "checksum": ""
--     }
--
-- Sql Usage:
--   SELECT * FROM set_score('SOME_ID', 999, 2346)
--
CREATE OR REPLACE FUNCTION set_score(id TEXT, value INTEGER, checksum INTEGER) RETURNS INTEGER AS $$
BEGIN
  IF $2 > COALESCE((SELECT score FROM highscores WHERE device_id = $1), 0) THEN
    IF score_checksum($2) = $3 THEN
      UPDATE highscores SET score = $2 WHERE device_id = $1;
      RETURN $2;
    END IF;
  END IF;
  RETURN COALESCE((SELECT score FROM highscores WHERE device_id = $1), 0);
END;
$$ LANGUAGE plpgsql SECURITY DEFINER
SET search_path = public,pg_catalog,pg_temp;

-- After creating/modifying functions you must refresh PostgREST schema cache
NOTIFY pgrst, 'reload schema';