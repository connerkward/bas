package com.prx.outpaintandroidtv.data

import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Query

interface PollApi {
    @GET("poll")
    suspend fun pollForCode(@Query("session") sessionId: String): Response<PollResponse>
}

data class PollResponse(
    val code: String?
)

