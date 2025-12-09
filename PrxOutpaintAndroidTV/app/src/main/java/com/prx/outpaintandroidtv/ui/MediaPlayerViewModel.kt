package com.prx.outpaintandroidtv.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.prx.outpaintandroidtv.data.CurrentlyPlayingResponse
import com.prx.outpaintandroidtv.data.SpotifyRepository
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class MediaPlayerState(
    val track: CurrentlyPlayingResponse? = null,
    val isLoading: Boolean = false,
    val error: String? = null
)

class MediaPlayerViewModel(
    private val repository: SpotifyRepository
) : ViewModel() {

    private val _state = MutableStateFlow(MediaPlayerState())
    val state: StateFlow<MediaPlayerState> = _state.asStateFlow()

    init {
        startPolling()
    }

    private fun startPolling() {
        viewModelScope.launch {
            while (true) {
                fetchCurrentlyPlaying()
                delay(3000) // Poll every 3 seconds
            }
        }
    }

    private suspend fun fetchCurrentlyPlaying() {
        _state.value = _state.value.copy(isLoading = true, error = null)
        repository.getCurrentlyPlaying()
            .onSuccess { response ->
                _state.value = _state.value.copy(
                    track = response,
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

    companion object {
        fun createFactory(
            repository: SpotifyRepository
        ): ViewModelProvider.Factory = object : ViewModelProvider.Factory {
            @Suppress("UNCHECKED_CAST")
            override fun <T : ViewModel> create(modelClass: Class<T>): T {
                return MediaPlayerViewModel(repository) as T
            }
        }
    }
}

