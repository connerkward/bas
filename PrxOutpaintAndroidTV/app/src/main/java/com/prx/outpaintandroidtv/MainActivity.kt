package com.prx.outpaintandroidtv

import android.content.Intent
import android.net.Uri
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.RectangleShape
import androidx.tv.material3.ExperimentalTvMaterial3Api
import androidx.tv.material3.Surface
import com.prx.outpaintandroidtv.data.AuthManager
import com.prx.outpaintandroidtv.ui.AuthViewModel
import com.prx.outpaintandroidtv.ui.LoginScreen
import com.prx.outpaintandroidtv.ui.MediaPlayerScreen
import com.prx.outpaintandroidtv.ui.theme.PrxOutpaintAndroidTVTheme
import com.spotify.sdk.android.auth.AuthorizationClient
import com.spotify.sdk.android.auth.AuthorizationResponse

class MainActivity : ComponentActivity() {
    private val authViewModel: AuthViewModel by viewModels()
    
    @OptIn(ExperimentalTvMaterial3Api::class)
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        
        setContent {
            PrxOutpaintAndroidTVTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    shape = RectangleShape
                ) {
                    val authState by authViewModel.state.collectAsState()
                    
                    when {
                        authState.accessToken != null -> {
                            MediaPlayerScreen(
                                accessToken = authState.accessToken!!,
                                onLogout = { authViewModel.logout() }
                            )
                        }
                        else -> {
                            LoginScreen(
                                activity = this@MainActivity,
                                onCodeEntered = { code -> 
                                    authViewModel.handleAuthCallback(code)
                                },
                                onLoginComplete = {
                                    // Login complete handled by state flow
                                }
                            )
                }
            }
        }
    }
}

        handleAuthCallback(intent)
    }
    
    override fun onNewIntent(intent: Intent) {
        super.onNewIntent(intent)
        setIntent(intent)
        handleAuthCallback(intent)
    }
    
    private fun handleAuthCallback(intent: Intent) {
        val uri: Uri? = intent.data
        if (uri != null && uri.scheme == "prxoutpaint" && uri.host == "callback") {
            val authorizationCode = AuthManager.extractCodeFromUri(uri)
            if (authorizationCode != null) {
                authViewModel.handleAuthCallback(authorizationCode)
            }
        }
    }
    
    @Deprecated("Deprecated in Java")
    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        super.onActivityResult(requestCode, resultCode, data)
        
        if (requestCode == AuthManager.AUTH_REQUEST_CODE) {
            val response = AuthorizationClient.getResponse(resultCode, data)
            val result = AuthManager.handleAuthResponse(response)
            
            when {
                result?.accessToken != null -> {
                    // Direct token from Spotify app (implicit grant)
                    authViewModel.handleDirectToken(result.accessToken)
                }
                result?.code != null -> {
                    // Code that needs to be exchanged
                    authViewModel.handleAuthCallback(result.code)
                }
            }
        }
    }
}