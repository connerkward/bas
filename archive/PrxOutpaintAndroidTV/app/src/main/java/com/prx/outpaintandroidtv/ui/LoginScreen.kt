package com.prx.outpaintandroidtv.ui

import android.app.Activity
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.tv.material3.Button
import androidx.tv.material3.ExperimentalTvMaterial3Api
import androidx.tv.material3.Text
import com.prx.outpaintandroidtv.data.AuthManager
import com.prx.outpaintandroidtv.data.NetworkModule
import kotlinx.coroutines.delay

@OptIn(ExperimentalTvMaterial3Api::class)
@Composable
fun LoginScreen(
    activity: Activity,
    onCodeEntered: (String) -> Unit,
    onLoginComplete: () -> Unit
) {
    var manualCode by remember { mutableStateOf("") }
    var isPolling by remember { mutableStateOf(false) }
    var status by remember { mutableStateOf("Waiting for login...") }
    
    // Generate auth URL (this also creates session ID)
    val authUrl = remember { AuthManager.getAuthorizationUrl() }
    val sessionId = remember { AuthManager.getCurrentSessionId() }
    val serverBaseUrl = remember { AuthManager.getServerBaseUrl() }
    
    // Poll for code
    LaunchedEffect(sessionId) {
        if (sessionId != null && serverBaseUrl.startsWith("https://")) {
            isPolling = true
            val pollApi = NetworkModule.createPollApi(serverBaseUrl)
            
            while (isPolling) {
                try {
                    val response = pollApi.pollForCode(sessionId)
                    if (response.isSuccessful) {
                        val code = response.body()?.code
                        if (code != null) {
                            status = "Code received! Connecting..."
                            isPolling = false
                            onCodeEntered(code)
                            break
                        }
                    }
                } catch (e: Exception) {
                    // Ignore errors, keep polling
                }
                delay(2000) // Poll every 2 seconds
            }
        }
    }
    
    DisposableEffect(Unit) {
        onDispose { isPolling = false }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black),
        contentAlignment = Alignment.Center
    ) {
        Row(
            modifier = Modifier.fillMaxSize().padding(48.dp),
            horizontalArrangement = Arrangement.SpaceEvenly,
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Left side - QR Code
            Column(
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                Text(
                    text = "1. Scan with your phone",
                    color = Color.White,
                    fontSize = 24.sp
                )
                
                Spacer(modifier = Modifier.height(24.dp))
                
                QrCode(content = authUrl, size = 280)
                
                Spacer(modifier = Modifier.height(16.dp))
                
                Text(
                    text = if (isPolling) "● $status" else status,
                    color = Color(0xFF1DB954),
                    fontSize = 14.sp
                )
            }
            
            // Right side - Manual entry (fallback)
            Column(
                horizontalAlignment = Alignment.CenterHorizontally,
                modifier = Modifier.fillMaxWidth(0.5f)
            ) {
                Text(
                    text = "2. Login on phone & return",
                    color = Color.White,
                    fontSize = 24.sp
                )
                
                Spacer(modifier = Modifier.height(16.dp))
                
                Text(
                    text = "It will connect automatically!",
                    color = Color(0xFF1DB954),
                    fontSize = 16.sp
                )
                
                Spacer(modifier = Modifier.height(32.dp))
                
                Text(
                    text = "— or paste code manually —",
                    color = Color.Gray,
                    fontSize = 12.sp
                )
                
                Spacer(modifier = Modifier.height(16.dp))
                
                BasicTextField(
                    value = manualCode,
                    onValueChange = { manualCode = it },
                    singleLine = true,
                    textStyle = TextStyle(color = Color.White, fontSize = 16.sp),
                    cursorBrush = SolidColor(Color(0xFF1DB954)),
                    keyboardOptions = KeyboardOptions(imeAction = ImeAction.Done),
                    keyboardActions = KeyboardActions(
                        onDone = {
                            if (manualCode.isNotBlank()) {
                                onCodeEntered(manualCode.trim())
                            }
                        }
                    ),
                    modifier = Modifier
                        .fillMaxWidth()
                        .background(Color(0xFF222222), RoundedCornerShape(8.dp))
                        .border(1.dp, Color(0xFF444444), RoundedCornerShape(8.dp))
                        .padding(16.dp),
                    decorationBox = { innerTextField ->
                        Box {
                            if (manualCode.isEmpty()) {
                                Text(
                                    text = "Paste code here...",
                                    color = Color.Gray,
                                    fontSize = 16.sp
                                )
                            }
                            innerTextField()
                        }
                    }
                )
                
                Spacer(modifier = Modifier.height(16.dp))
                
                Button(
                    onClick = {
                        if (manualCode.isNotBlank()) {
                            onCodeEntered(manualCode.trim())
                        }
                    }
                ) {
                    Text("Connect")
                }
            }
        }
    }
}
