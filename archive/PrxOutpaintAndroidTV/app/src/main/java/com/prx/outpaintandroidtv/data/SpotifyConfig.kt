package com.prx.outpaintandroidtv.data

import com.prx.outpaintandroidtv.BuildConfig

object SpotifyConfig {
    // Loaded from local.properties via BuildConfig
    const val CLIENT_ID = BuildConfig.SPOTIFY_CLIENT_ID
    
    const val REDIRECT_URI = "https://auth.proximate.works/callback"
    
    const val SCOPES = "user-read-playback-state user-read-currently-playing"
}

