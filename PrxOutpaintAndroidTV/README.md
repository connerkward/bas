# PrxOutpaint Android TV - Spotify Media Player

A minimal Android TV app that displays the currently playing track from Spotify on other devices, featuring album art with a blurred background.

## Features

- **Real-time Spotify playback display**: Shows what's playing on your other Spotify devices
- **Minimal UI**: Centered album art (400dp) with blurred background
- **QR Code login**: Scan QR code with phone to authenticate
- **Auto-sync authentication**: TV automatically receives auth code after phone login
- **Crossfade animations**: Smooth transitions when tracks change

## Architecture

### Android App
- **Framework**: Jetpack Compose for TV
- **API Client**: Retrofit + OkHttp
- **Image Loading**: Coil
- **State Management**: ViewModel + StateFlow
- **Storage**: DataStore for secure token storage
- **Polling**: Every 3 seconds for currently playing track

### Authentication Server (Vercel)
- **Platform**: Vercel serverless functions
- **Storage**: Upstash Redis (free tier)
- **Domain**: `auth.proximate.works`
- **Endpoints**:
  - `/callback` - Receives Spotify OAuth callback
  - `/poll` - TV polls for auth code

## Setup

### Prerequisites

1. **Spotify Developer Account**
   - Create app at https://developer.spotify.com/dashboard
   - Add redirect URI: `https://auth.proximate.works/callback`
   - Note your Client ID and Client Secret

2. **Vercel Account** (free)
   - Sign up at https://vercel.com

3. **Upstash Account** (free)
   - Sign up at https://upstash.com
   - Create Redis database
   - Get REST API URL and token

### Android App Setup

1. **Configure credentials**:
   ```properties
   # local.properties
   spotify.client.id=YOUR_CLIENT_ID
   spotify.client.secret=YOUR_CLIENT_SECRET
   ```

2. **Build**:
   ```bash
   ./gradlew assembleDebug
   ```

3. **APK location**:
   ```
   app/build/outputs/apk/debug/app-debug.apk
   ```

### Server Setup

1. **Deploy to Vercel**:
   ```bash
   cd spotify-auth-server
   npx vercel login
   npx vercel --prod
   ```

2. **Add environment variables** (Vercel Dashboard → Settings → Environment Variables):
   - `KV_REST_API_URL` - Your Upstash REST API URL
   - `KV_REST_API_TOKEN` - Your Upstash REST API token

3. **Add custom domain**:
   - Vercel Dashboard → Project → Settings → Domains
   - Add `auth.proximate.works`
   - Update DNS: CNAME `auth` → `cname.vercel-dns.com`

4. **Update Spotify Dashboard**:
   - Add redirect URI: `https://auth.proximate.works/callback`

## Installation on Android TV

### Method 1: ADB (Recommended)

1. **Enable Developer Options**:
   - Settings → About → Click "Build" 7 times
   - Settings → Developer Options → Enable "USB debugging" and "Network debugging"

2. **Connect**:
   ```bash
   # Find TV IP (Settings → Network)
   adb connect YOUR_TV_IP:5555
   
   # Install
   adb install app/build/outputs/apk/debug/app-debug.apk
   ```

### Method 2: USB Drive

1. Copy `app/build/outputs/apk/debug/app-debug.apk` to USB
2. Plug USB into TV
3. Use file manager to install

### Method 3: Network Share

1. Host APK on local network
2. Download on TV via browser
3. Install

## Authentication Flow

1. **TV displays QR code** with Spotify authorization URL (includes session ID)
2. **User scans QR** with phone → opens Spotify login
3. **User approves** → Spotify redirects to `https://auth.proximate.works/callback?code=XXX&state=SESSION_ID`
4. **Server stores code** in Upstash Redis with session ID (expires in 5 minutes)
5. **TV polls** `/poll?session=SESSION_ID` every 2 seconds
6. **Server returns code** → TV exchanges for access token
7. **TV starts polling** Spotify API every 3 seconds for currently playing track

## Key Files

### Android App

- `app/src/main/java/com/prx/outpaintandroidtv/MainActivity.kt` - Main activity, handles auth state
- `app/src/main/java/com/prx/outpaintandroidtv/ui/LoginScreen.kt` - QR code + manual code entry
- `app/src/main/java/com/prx/outpaintandroidtv/ui/MediaPlayerScreen.kt` - Album art display
- `app/src/main/java/com/prx/outpaintandroidtv/ui/MediaPlayerViewModel.kt` - Polls Spotify API
- `app/src/main/java/com/prx/outpaintandroidtv/data/AuthManager.kt` - OAuth flow management
- `app/src/main/java/com/prx/outpaintandroidtv/data/SpotifyRepository.kt` - API calls
- `app/src/main/java/com/prx/outpaintandroidtv/data/NetworkModule.kt` - Retrofit setup
- `app/src/main/java/com/prx/outpaintandroidtv/data/SpotifyConfig.kt` - Configuration

### Server

- `spotify-auth-server/api/callback.js` - Handles Spotify OAuth callback
- `spotify-auth-server/api/poll.js` - TV polls for auth code
- `spotify-auth-server/vercel.json` - Vercel routing config

## Dependencies

### Android
- Jetpack Compose for TV
- Retrofit 2.11.0
- OkHttp 4.12.0
- Coil 2.7.0
- DataStore 1.1.1
- Spotify Auth Library 2.0.0

### Server
- Vercel serverless functions
- Upstash Redis (free tier)

## Configuration

### Spotify Scopes
- `user-read-playback-state`
- `user-read-currently-playing`

### Redirect URI
- Production: `https://auth.proximate.works/callback`
- Configured in `SpotifyConfig.kt`

## Limitations & Notes

- **No audio playback**: App only displays what's playing on other devices
- **TV auth constraints**: Spotify doesn't support seamless TV auth, hence QR code + server approach
- **Polling frequency**: 3 seconds for track updates (configurable in `MediaPlayerViewModel`)
- **Token refresh**: Handled automatically via refresh token

## Troubleshooting

### "KV not configured" error
- Ensure Upstash env vars are set in Vercel
- Redeploy after adding env vars

### Auth code not syncing
- Check Upstash is accessible
- Verify session ID matches between QR and poll
- Check Vercel function logs

### Album art not showing
- Verify Spotify API returns track data
- Check network connectivity
- Verify access token is valid

## Free Tier Limits

- **Vercel**: 100k function calls/month (plenty for personal use)
- **Upstash**: 10k commands/day (plenty for personal use)
- **Spotify API**: No hard limits for user-authenticated calls

## Future Improvements

- [ ] WebSocket for real-time updates (eliminate polling)
- [ ] Track metadata display (artist, title)
- [ ] Playback controls
- [ ] Multiple device selection
- [ ] Custom background images

