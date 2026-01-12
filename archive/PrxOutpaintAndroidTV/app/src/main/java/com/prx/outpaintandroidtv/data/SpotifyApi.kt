package com.prx.outpaintandroidtv.data

import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Header

interface SpotifyApi {
    @GET("v1/me/player/currently-playing")
    suspend fun getCurrentlyPlaying(
        @Header("Authorization") authorization: String
    ): Response<CurrentlyPlayingResponse>
}

