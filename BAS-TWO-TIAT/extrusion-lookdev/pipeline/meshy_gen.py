#!/usr/bin/env python3
"""Direct image-to-3D via fal Meshy-6 (the best/likeliest single-image-to-3D on fal).
Submits one relief image, polls the fal queue, downloads the GLB into depths/<key>/meshy.glb.
A separate headless render (glb_depth.html via Playwright) turns the GLB into a front-facing
depth map for the comparison studio. Meshy is closed/hosted — no practical local equivalent.

Usage: python meshy_gen.py --key selfmade --src ../depths/selfmade/source.jpg
"""
import sys, json, time, base64, argparse, pathlib, urllib.request

HERE = pathlib.Path(__file__).resolve().parent
DEPTHS = HERE.parent / "depths"

def env(k):
    p = pathlib.Path.home() / "dev/central/.env"
    for line in p.read_text().splitlines():
        line = line.strip()
        if line.startswith(k + "="): return line.split("=", 1)[1].strip().strip('"').strip("'")
    raise RuntimeError(f"{k} not in central/.env")

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--key", required=True)
    ap.add_argument("--src", required=True)
    a = ap.parse_args()
    out = DEPTHS / a.key; out.mkdir(parents=True, exist_ok=True)
    key = env("FAL_KEY")
    img = pathlib.Path(a.src).read_bytes()
    data_uri = "data:image/jpeg;base64," + base64.b64encode(img).decode()
    ep = "fal-ai/meshy/v6/image-to-3d"
    hdr = {"Authorization": f"Key {key}", "Content-Type": "application/json"}
    # submit to the queue
    req = urllib.request.Request(f"https://queue.fal.run/{ep}",
        data=json.dumps({"image_url": data_uri, "should_texture": False,
                         "symmetry_mode": "off", "target_polycount": 50000}).encode(), headers=hdr)
    sub = json.loads(urllib.request.urlopen(req, timeout=120).read())
    rid = sub["request_id"]; print("submitted", rid, flush=True)
    status_url = sub.get("status_url", f"https://queue.fal.run/{ep}/requests/{rid}/status")
    resp_url   = sub.get("response_url", f"https://queue.fal.run/{ep}/requests/{rid}")
    t0 = time.time()
    while True:
        st = json.loads(urllib.request.urlopen(urllib.request.Request(status_url, headers=hdr), timeout=60).read())
        s = st.get("status")
        print(f"  {int(time.time()-t0)}s {s}", flush=True)
        if s == "COMPLETED": break
        if s in ("FAILED", "ERROR"): raise RuntimeError(f"meshy failed: {st}")
        time.sleep(10)
    res = json.loads(urllib.request.urlopen(urllib.request.Request(resp_url, headers=hdr), timeout=120).read())
    glb = (res.get("model_glb") or {}).get("url") or (res.get("model_urls") or {}).get("glb")
    if not glb: raise RuntimeError(f"no glb in response: {str(res)[:400]}")
    dest = out / "meshy.glb"
    dest.write_bytes(urllib.request.urlopen(glb, timeout=300).read())
    print(f"wrote {dest} ({dest.stat().st_size//1024} KB) in {int(time.time()-t0)}s", flush=True)

if __name__ == "__main__":
    main()
