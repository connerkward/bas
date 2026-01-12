package com.prx.outpaintandroidtv.ui

import android.graphics.RenderEffect
import android.graphics.Shader
import android.os.Build
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.asComposeRenderEffect
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.tv.material3.Button
import androidx.tv.material3.ExperimentalTvMaterial3Api
import androidx.tv.material3.Text
import coil.compose.AsyncImage
import coil.request.ImageRequest
import com.prx.outpaintandroidtv.data.NetworkModule
import com.prx.outpaintandroidtv.data.SpotifyRepository

@OptIn(ExperimentalTvMaterial3Api::class)
@Composable
fun MediaPlayerScreen(
    accessToken: String,
    onLogout: () -> Unit,
    viewModel: MediaPlayerViewModel = viewModel(
        factory = MediaPlayerViewModel.createFactory(
            SpotifyRepository(NetworkModule.spotifyApi, accessToken)
        )
    )
) {
    val state by viewModel.state.collectAsState()
    val track = state.track?.item
    val context = LocalContext.current

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
    ) {
        // Background image (only show if album art available)
        track?.album?.images?.firstOrNull()?.url?.let { backgroundImageUrl ->
            AsyncImage(
                model = ImageRequest.Builder(context)
                    .data(backgroundImageUrl)
                    .crossfade(500)
                    .build(),
                contentDescription = null,
                modifier = Modifier
                    .fillMaxSize()
                    .alpha(0.3f)
                    .then(
                        if (Build.VERSION.SDK_INT >= 31) {
                            Modifier.graphicsLayer {
                                renderEffect = RenderEffect
                                    .createBlurEffect(50f, 50f, Shader.TileMode.CLAMP)
                                    .asComposeRenderEffect()
                            }
                        } else {
                            Modifier
                        }
                    ),
                contentScale = ContentScale.Crop
            )
        }

        // Album art in center
        track?.album?.images?.firstOrNull()?.url?.let { imageUrl ->
            AsyncImage(
                model = ImageRequest.Builder(context)
                    .data(imageUrl)
                    .crossfade(500)
                    .build(),
                contentDescription = track.name ?: "Album art",
                modifier = Modifier
                    .size(400.dp)
                    .align(Alignment.Center),
                contentScale = ContentScale.Fit
            )
        }
        
        // Logout button in top-right corner
        Button(
            onClick = onLogout,
            modifier = Modifier
                .align(Alignment.TopEnd)
                .padding(24.dp)
        ) {
            Text("Logout")
        }
    }
}

