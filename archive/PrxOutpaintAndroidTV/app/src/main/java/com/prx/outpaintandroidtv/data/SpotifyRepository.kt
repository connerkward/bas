package com.prx.outpaintandroidtv.data

import retrofit2.Response

class SpotifyRepository(private val api: SpotifyApi, private val accessToken: String) {
    suspend fun getCurrentlyPlaying(): Result<CurrentlyPlayingResponse?> {
        return try {
            val response = api.getCurrentlyPlaying("Bearer $accessToken")
            if (response.isSuccessful) {
                Result.success(response.body())
            } else {
                Result.failure(Exception("Failed to fetch: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}

