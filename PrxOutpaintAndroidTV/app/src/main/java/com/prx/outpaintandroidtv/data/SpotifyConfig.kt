package com.prx.outpaintandroidtv.data

import com.prx.outpaintandroidtv.BuildConfig

object SpotifyConfig {
    // Loaded from local.properties via BuildConfig
    const val CLIENT_ID = BuildConfig.SPOTIFY_CLIENT_ID
    
    // TODO: Replace with your Vercel URL after deploying
    // Example: "https://your-app.vercel.app/callback"
    const val REDIRECT_URI = "https://YOUR-VERCEL-URL.vercel.app/callback"
    
    const val SCOPES = "user-read-playback-state user-read-currently-playing"
}

