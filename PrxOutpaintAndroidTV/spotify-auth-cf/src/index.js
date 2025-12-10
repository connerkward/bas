export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    
    // Handle CORS
    if (request.method === 'OPTIONS') {
      return new Response(null, {
        headers: {
          'Access-Control-Allow-Origin': '*',
          'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
        }
      });
    }

    // Callback from Spotify
    if (url.pathname === '/callback') {
      const code = url.searchParams.get('code');
      const error = url.searchParams.get('error');
      const state = url.searchParams.get('state');

      if (error) {
        return new Response(errorPage(error), { headers: { 'Content-Type': 'text/html' } });
      }

      if (!code) {
        return new Response(errorPage('No code received'), { headers: { 'Content-Type': 'text/html' } });
      }

      // Store code in KV (expires in 5 minutes)
      if (state) {
        await env.AUTH_CODES.put(`auth:${state}`, code, { expirationTtl: 300 });
      }

      return new Response(successPage(code), { headers: { 'Content-Type': 'text/html' } });
    }

    // Poll endpoint for TV
    if (url.pathname === '/poll') {
      const sessionId = url.searchParams.get('session');
      
      if (!sessionId) {
        return Response.json({ error: 'Missing session' }, {
          headers: { 'Access-Control-Allow-Origin': '*' }
        });
      }

      const code = await env.AUTH_CODES.get(`auth:${sessionId}`);
      
      if (code) {
        await env.AUTH_CODES.delete(`auth:${sessionId}`);
        return Response.json({ code }, {
          headers: { 'Access-Control-Allow-Origin': '*' }
        });
      }

      return Response.json({ code: null }, {
        headers: { 'Access-Control-Allow-Origin': '*' }
      });
    }

    return new Response('Not found', { status: 404 });
  }
};

function errorPage(error) {
  return `<!DOCTYPE html>
<html><head><meta name="viewport" content="width=device-width, initial-scale=1">
<style>body{font-family:-apple-system,sans-serif;background:#121212;color:white;display:flex;justify-content:center;align-items:center;height:100vh;margin:0;text-align:center}h1{color:#ff4444}</style>
</head><body><div><h1>Login Failed</h1><p>${error}</p></div></body></html>`;
}

function successPage(code) {
  return `<!DOCTYPE html>
<html><head><meta name="viewport" content="width=device-width, initial-scale=1">
<style>
*{box-sizing:border-box}
body{font-family:-apple-system,sans-serif;background:#121212;color:white;display:flex;justify-content:center;align-items:center;min-height:100vh;margin:0;padding:20px}
.container{text-align:center;max-width:400px}
.check{font-size:80px;margin-bottom:20px}
h1{color:#1DB954;font-size:28px}
.sub{color:#b3b3b3;font-size:16px;margin-top:10px}
.code-box{background:#282828;border:3px solid #1DB954;border-radius:12px;padding:20px;margin:20px 0;word-break:break-all}
.code{font-family:monospace;font-size:11px;color:#1DB954;user-select:all}
.copy-btn{background:#1DB954;color:black;border:none;padding:15px 40px;border-radius:30px;font-size:16px;font-weight:bold;cursor:pointer}
.copied{color:#1DB954;margin-top:10px;opacity:0;transition:opacity 0.3s}
.copied.show{opacity:1}
</style></head>
<body><div class="container">
<div class="check">✓</div>
<h1>Connected to Spotify!</h1>
<p class="sub">Your TV should connect automatically.<br>If not, copy this code:</p>
<div class="code-box"><div class="code" id="code">${code}</div></div>
<button class="copy-btn" onclick="copyCode()">Copy Code</button>
<div class="copied" id="copied">Copied!</div>
</div>
<script>
function copyCode(){
  navigator.clipboard.writeText(document.getElementById('code').innerText);
  document.getElementById('copied').classList.add('show');
  setTimeout(()=>document.getElementById('copied').classList.remove('show'),2000);
}
</script>
</body></html>`;
}

