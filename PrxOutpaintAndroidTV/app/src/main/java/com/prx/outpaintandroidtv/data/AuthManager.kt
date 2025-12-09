package com.prx.outpaintandroidtv.data

import android.app.Activity
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.util.Base64
import androidx.browser.customtabs.CustomTabsIntent
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import com.spotify.sdk.android.auth.AuthorizationClient
import com.spotify.sdk.android.auth.AuthorizationRequest
import com.spotify.sdk.android.auth.AuthorizationResponse
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import java.security.MessageDigest
import java.security.SecureRandom

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "auth_prefs")

object AuthManager {
    private const val CLIENT_ID = SpotifyConfig.CLIENT_ID
    private const val REDIRECT_URI = SpotifyConfig.REDIRECT_URI
    private const val SCOPES = SpotifyConfig.SCOPES
    
    private val ACCESS_TOKEN_KEY = stringPreferencesKey("access_token")
    private val REFRESH_TOKEN_KEY = stringPreferencesKey("refresh_token")
    private val CODE_VERIFIER_KEY = stringPreferencesKey("code_verifier")

    private var pendingCodeVerifier: String? = null
    private var currentSessionId: String? = null

    fun generateSessionId(): String {
        val random = SecureRandom()
        val bytes = ByteArray(16)
        random.nextBytes(bytes)
        return Base64.encodeToString(bytes, Base64.URL_SAFE or Base64.NO_WRAP or Base64.NO_PADDING)
    }

    fun getCurrentSessionId(): String? = currentSessionId

    fun generateCodeVerifier(): String {
        val random = SecureRandom()
        val bytes = ByteArray(32)
        random.nextBytes(bytes)
        return Base64.encodeToString(bytes, Base64.URL_SAFE or Base64.NO_WRAP or Base64.NO_PADDING)
    }

    fun generateCodeChallenge(verifier: String): String {
        val bytes = verifier.toByteArray()
        val md = MessageDigest.getInstance("SHA-256")
        val digest = md.digest(bytes)
        return Base64.encodeToString(digest, Base64.URL_SAFE or Base64.NO_WRAP or Base64.NO_PADDING)
    }

    fun getAuthorizationUrl(): String {
        val codeVerifier = generateCodeVerifier()
        pendingCodeVerifier = codeVerifier
        val codeChallenge = generateCodeChallenge(codeVerifier)
        
        // Generate session ID for auto-sync
        currentSessionId = generateSessionId()
        
        return Uri.parse("https://accounts.spotify.com/authorize").buildUpon()
            .appendQueryParameter("client_id", CLIENT_ID)
            .appendQueryParameter("response_type", "code")
            .appendQueryParameter("redirect_uri", REDIRECT_URI)
            .appendQueryParameter("code_challenge_method", "S256")
            .appendQueryParameter("code_challenge", codeChallenge)
            .appendQueryParameter("scope", SCOPES)
            .appendQueryParameter("state", currentSessionId)
            .build()
            .toString()
    }
    
    fun getServerBaseUrl(): String {
        // Extract base URL from redirect URI
        return REDIRECT_URI.substringBeforeLast("/callback")
    }

    suspend fun exchangeCodeForToken(
        context: Context,
        authorizationCode: String
    ): Result<TokenResponse> {
        val codeVerifier = pendingCodeVerifier
            ?: return Result.failure(Exception("No pending code verifier"))
        
        return try {
            val response = NetworkModule.authApi.exchangeCodeForToken(
                grantType = "authorization_code",
                code = authorizationCode,
                redirectUri = REDIRECT_URI,
                clientId = CLIENT_ID,
                codeVerifier = codeVerifier
            )
            
            if (response.isSuccessful && response.body() != null) {
                val tokenResponse = response.body()!!
                saveTokens(context, tokenResponse)
                pendingCodeVerifier = null
                Result.success(tokenResponse)
            } else {
                Result.failure(Exception("Token exchange failed: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun refreshAccessToken(context: Context): Result<String> {
        val refreshToken = getRefreshTokenFlow(context).first()
            ?: return Result.failure(Exception("No refresh token available"))
        
        return try {
            val response = NetworkModule.authApi.refreshToken(
                grantType = "refresh_token",
                refreshToken = refreshToken,
                clientId = CLIENT_ID
            )
            
            if (response.isSuccessful && response.body() != null) {
                val tokenResponse = response.body()!!
                saveAccessToken(context, tokenResponse.accessToken)
                if (tokenResponse.refreshToken != null) {
                    saveRefreshToken(context, tokenResponse.refreshToken)
                }
                Result.success(tokenResponse.accessToken)
            } else {
                Result.failure(Exception("Token refresh failed: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun saveTokens(context: Context, tokenResponse: TokenResponse) {
        context.dataStore.edit { preferences ->
            preferences[ACCESS_TOKEN_KEY] = tokenResponse.accessToken
            tokenResponse.refreshToken?.let {
                preferences[REFRESH_TOKEN_KEY] = it
            }
        }
    }

    private suspend fun saveAccessToken(context: Context, token: String) {
        context.dataStore.edit { preferences ->
            preferences[ACCESS_TOKEN_KEY] = token
        }
    }
    
    suspend fun saveAccessTokenDirect(context: Context, token: String) {
        context.dataStore.edit { preferences ->
            preferences[ACCESS_TOKEN_KEY] = token
        }
    }

    private suspend fun saveRefreshToken(context: Context, token: String) {
        context.dataStore.edit { preferences ->
            preferences[REFRESH_TOKEN_KEY] = token
        }
    }

    fun getAccessToken(context: Context): Flow<String?> {
        return context.dataStore.data.map { preferences ->
            preferences[ACCESS_TOKEN_KEY]
        }
    }

    fun getRefreshTokenFlow(context: Context): Flow<String?> {
        return context.dataStore.data.map { preferences ->
            preferences[REFRESH_TOKEN_KEY]
        }
    }

    suspend fun clearTokens(context: Context) {
        context.dataStore.edit { preferences ->
            preferences.remove(ACCESS_TOKEN_KEY)
            preferences.remove(REFRESH_TOKEN_KEY)
        }
    }

    const val AUTH_REQUEST_CODE = 1337
    
    fun launchAuthFlow(activity: Activity) {
        // Try Spotify app first (opens Spotify for auth)
        val builder = AuthorizationRequest.Builder(
            CLIENT_ID,
            AuthorizationResponse.Type.CODE,
            REDIRECT_URI
        )
        builder.setScopes(SCOPES.split(" ").toTypedArray())
        val request = builder.build()
        
        pendingCodeVerifier = generateCodeVerifier()
        
        AuthorizationClient.openLoginActivity(activity, AUTH_REQUEST_CODE, request)
    }
    
    fun launchAuthFlowFallback(context: Context) {
        // Fallback to browser if Spotify SDK doesn't work
        val authUrl = getAuthorizationUrl()
        
        val browserIntent = Intent(Intent.ACTION_VIEW, Uri.parse(authUrl)).apply {
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        }
        
        try {
            context.startActivity(browserIntent)
        } catch (e: Exception) {
            val customTabsIntent = CustomTabsIntent.Builder().build()
            customTabsIntent.launchUrl(context, Uri.parse(authUrl))
        }
    }
    
    fun handleAuthResponse(response: AuthorizationResponse): AuthResult? {
        return when (response.type) {
            AuthorizationResponse.Type.CODE -> AuthResult(code = response.code)
            AuthorizationResponse.Type.TOKEN -> AuthResult(accessToken = response.accessToken)
            AuthorizationResponse.Type.ERROR -> null
            else -> null
        }
    }
    
    data class AuthResult(
        val code: String? = null,
        val accessToken: String? = null
    )

    fun extractCodeFromUri(uri: Uri): String? {
        return uri.getQueryParameter("code")
    }
}

