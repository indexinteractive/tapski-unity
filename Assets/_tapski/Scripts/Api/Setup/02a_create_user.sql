-- create_user function
--
-- Api usage:
--   POST https://<project_id>.supabase.co/rest/v1/rpc/create_user
--   {
--     "device_id": "",
--     "display_name": "",
--   }
--
CREATE OR REPLACE FUNCTION create_user(id TEXT, name TEXT) RETURNS BOOLEAN AS $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM highscores WHERE device_id = $1) THEN
    INSERT INTO highscores (device_id, display_name, score) VALUES ($1, $2, 0);
    RETURN true;
  END IF;
  RETURN false;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- After creating/modifying functions you must refresh PostgREST schema cache
NOTIFY pgrst, 'reload schema';