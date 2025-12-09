package com.prx.outpaintandroidtv.ui

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.prx.outpaintandroidtv.data.AuthManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class AuthState(
    val accessToken: String? = null,
    val isLoading: Boolean = false,
    val error: String? = null
)

class AuthViewModel(application: Application) : AndroidViewModel(application) {
    private val _state = MutableStateFlow(AuthState())
    val state: StateFlow<AuthState> = _state.asStateFlow()

    init {
        checkAuthStatus()
    }

    private fun checkAuthStatus() {
        viewModelScope.launch {
            AuthManager.getAccessToken(getApplication()).collect { token ->
                _state.value = _state.value.copy(accessToken = token)
            }
        }
    }

    fun handleAuthCallback(authorizationCode: String) {
        viewModelScope.launch {
            _state.value = _state.value.copy(isLoading = true, error = null)
            AuthManager.exchangeCodeForToken(getApplication(), authorizationCode)
                .onSuccess {
                    _state.value = _state.value.copy(
                        accessToken = it.accessToken,
                        isLoading = false
                    )
                }
                .onFailure { error ->
                    _state.value = _state.value.copy(
                        isLoading = false,
                        error = error.message
                    )
                }
        }
    }
    
    fun handleDirectToken(accessToken: String) {
        viewModelScope.launch {
            // Save the token directly (from implicit grant)
            _state.value = _state.value.copy(accessToken = accessToken, isLoading = false)
            // Persist it
            AuthManager.saveAccessTokenDirect(getApplication(), accessToken)
        }
    }

    fun logout() {
        viewModelScope.launch {
            AuthManager.clearTokens(getApplication())
            _state.value = AuthState()
        }
    }
}

