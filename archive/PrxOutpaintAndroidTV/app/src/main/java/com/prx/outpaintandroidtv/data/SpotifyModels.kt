package com.prx.outpaintandroidtv.data

import com.google.gson.annotations.SerializedName

data class CurrentlyPlayingResponse(
    @SerializedName("item") val item: Track?,
    @SerializedName("is_playing") val isPlaying: Boolean = false
)

data class Track(
    @SerializedName("id") val id: String,
    @SerializedName("name") val name: String,
    @SerializedName("artists") val artists: List<Artist>,
    @SerializedName("album") val album: Album,
    @SerializedName("duration_ms") val durationMs: Long
)

data class Artist(
    @SerializedName("name") val name: String
)

data class Album(
    @SerializedName("name") val name: String,
    @SerializedName("images") val images: List<Image>
)

data class Image(
    @SerializedName("url") val url: String,
    @SerializedName("width") val width: Int?,
    @SerializedName("height") val height: Int?
)

