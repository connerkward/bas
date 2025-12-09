package com.prx.outpaintandroidtv.data

import retrofit2.Response
import retrofit2.http.Field
import retrofit2.http.FormUrlEncoded
import retrofit2.http.POST

interface AuthApi {
    @POST("api/token")
    @FormUrlEncoded
    suspend fun exchangeCodeForToken(
        @Field("grant_type") grantType: String,
        @Field("code") code: String,
        @Field("redirect_uri") redirectUri: String,
        @Field("client_id") clientId: String,
        @Field("code_verifier") codeVerifier: String
    ): Response<TokenResponse>

    @POST("api/token")
    @FormUrlEncoded
    suspend fun refreshToken(
        @Field("grant_type") grantType: String,
        @Field("refresh_token") refreshToken: String,
        @Field("client_id") clientId: String
    ): Response<TokenResponse>
}

