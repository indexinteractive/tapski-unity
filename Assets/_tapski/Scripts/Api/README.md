# Supabase API

The classes in this folder represent the REST API endpoints of our Supabase API tables.
In order to interact with these APIs, a Supabase project must be created and appropriate tables as described in this guide.

## 1. Project

A Supabase project should be created according to the [Supabase documentation](https://supabase.com/docs/).

Once a project is active and initialized, there are several `.sql` files within the [Setup](./Setup/) folder that should be executed in the `SQL Editor`.

When a Supabase project is created, an active [postgREST](https://postgrest.org/en/latest/api.html) api layer is automatically instantiated on top.

## Highscores table

### [01_highscores.sql](./Setup/01_highscores.sql)

Describes the table that stores the highscores. Users are uniquely identified using `SystemInfo.deviceUniqueIdentifier`, which is stored in the `device_id` column.

## Stored Procedures

### [02_create_user.sql](./Setup/02_create_user.sql)

Since the application does not perform user authentication, when a user first runs the game we simply add a new entry to the highscores table by calling this stored procedure.

### [02b_player_ranks.sql](./Setup/02b_player_ranks.sql)

This stored procedure returns a set of players surrounding the current player, ranked by score.

The `lim` parameter in this stored procedure specifies the number of players to return *before and after* the player id sent. For this reason, if the view can hold 10 rows, the `lim` parameter should be set to 20, to accomodate the edge cases where the player is at the top or bottom of the list.

### [02c_fn_set_score.sql](./Setup/02c_fn_set_score.sql)

This file contains two functions:

- `score_checksum` is the definition for a checksum that is ran when our score is submitted, validating the client before updating the table

- `set_score` is called via api by the game client when the [game round ends](../../../_tapski/Scripts/UI/GameOver.cs#74)

## Scoring Checksum

As this is an important aspect of the scoring system, it is described separately in this file.

The checksum system is defined by two parts:

- The `score_checksum` function, mentioned above
- The `checksum` function defined in [HighscoresApi.cs](../../../_tapski/Scripts/Api/HighscoresApi.cs#17)

The implementations of these checksum functions MUST PRODUCE THE SAME OUTPUT, otherwise the highscores table will not be updated. This verification is made to ensure the client is not cheating, and for this reason the implementation of these functions is NOT PROVIDED IN THIS REPOSITORY.

Apologies for the use of all-caps, the internet never learned how to read 💁‍♀️

## Permissions

Additional permissions should be set in order to prevent cheating. These permissions are not described by this document and are left to the user.