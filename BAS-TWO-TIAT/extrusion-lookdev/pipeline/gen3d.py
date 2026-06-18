#!/usr/bin/env python3
"""Generic single-image-to-3D via fal queue API. Mirrors meshy_gen.py's logic.
Submits source image (base64 data URI), polls queue, downloads GLB, writes sidecar entry.json.

Usage: python gen3d.py --endpoint fal-ai/hunyuan-3d/v3.1/pro/image-to-3d --key hunyuan3d \
        --label "Hunyuan3D-Pro v3.1 (fal) -> front depth" --src ../depths/selfmade/source.jpg
"""
import json, time, base64, argparse, pathlib, urllib.request, urllib.error

HERE = pathlib.Path(__file__).resolve().parent
OUT = HERE.parent / "depths" / "selfmade"

def env(k):
    p = pathlib.Path.home() / "dev/central/.env"
    for line in p.read_text().splitlines():
        line = line.strip()
        if line.startswith(k + "="): return line.split("=", 1)[1].strip().strip('"').strip("'")
    raise RuntimeError(f"{k} not in central/.env")

def find_glb(res):
    # try common shapes
    cands = [
        (res.get("model_glb") or {}).get("url"),
        (res.get("model_urls") or {}).get("glb"),
        (res.get("model_mesh") or {}).get("url"),
        (res.get("mesh") or {}).get("url"),
        res.get("model_glb_url"),
    ]
    for c in cands:
        if c: return c
    # deep scan: any value that is a .glb url
    def scan(o):
        if isinstance(o, dict):
            for v in o.values():
                r = scan(v)
                if r: return r
        elif isinstance(o, list):
            for v in o:
                r = scan(v)
                if r: return r
        elif isinstance(o, str) and o.startswith("http") and o.split("?")[0].endswith(".glb"):
            return o
        return None
    return scan(res)

def fal_upload(path, key):
    """Upload a local file to fal storage, return its CDN url (endpoints like Tripo reject data URIs)."""
    init = urllib.request.Request("https://rest.alpha.fal.ai/storage/upload/initiate?storage_type=fal-cdn-v3",
        data=json.dumps({"file_name": pathlib.Path(path).name, "content_type": "image/jpeg"}).encode(),
        headers={"Authorization": f"Key {key}", "Content-Type": "application/json"})
    j = json.loads(urllib.request.urlopen(init, timeout=60).read())
    put = urllib.request.Request(j["upload_url"], data=pathlib.Path(path).read_bytes(),
        headers={"Content-Type": "image/jpeg"}, method="PUT")
    urllib.request.urlopen(put, timeout=120)
    return j["file_url"]

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--endpoint", required=True)
    ap.add_argument("--key", required=True)
    ap.add_argument("--label", required=True)
    ap.add_argument("--src", help="local image (base64 data URI)")
    ap.add_argument("--src-url", help="hosted image URL (some endpoints reject data URIs)")
    ap.add_argument("--upload", action="store_true", help="upload --src to fal storage and use the CDN url")
    ap.add_argument("--image-field", default="image_url", help="body key for the input image")
    ap.add_argument("--extra", default="{}", help="extra JSON body params (overrides defaults)")
    a = ap.parse_args()
    OUT.mkdir(parents=True, exist_ok=True)
    key = env("FAL_KEY")
    if a.src_url:
        image_ref = a.src_url
    elif a.upload:
        image_ref = fal_upload(a.src, key); print("uploaded ->", image_ref, flush=True)
    else:
        img = pathlib.Path(a.src).read_bytes()
        image_ref = "data:image/jpeg;base64," + base64.b64encode(img).decode()
    ep = a.endpoint
    hdr = {"Authorization": f"Key {key}", "Content-Type": "application/json"}
    body = {a.image_field: image_ref}
    body.update(json.loads(a.extra))

    def submit(b):
        req = urllib.request.Request(f"https://queue.fal.run/{ep}",
            data=json.dumps(b).encode(), headers=hdr)
        return json.loads(urllib.request.urlopen(req, timeout=120).read())

    try:
        sub = submit(body)
    except urllib.error.HTTPError as e:
        detail = e.read().decode()
        print(f"SUBMIT {e.code}: {detail[:800]}", flush=True)
        # 422 -> try stripping the texture toggles in case they aren't valid
        if e.code == 422:
            body2 = {a.image_field: image_ref}
            body2.update(json.loads(a.extra))
            print("retrying with minimal body {image + extra only}", flush=True)
            sub = submit(body2)
        else:
            raise
    rid = sub["request_id"]; print("submitted", rid, flush=True)
    status_url = sub.get("status_url", f"https://queue.fal.run/{ep}/requests/{rid}/status")
    resp_url   = sub.get("response_url", f"https://queue.fal.run/{ep}/requests/{rid}")
    t0 = time.time()
    while True:
        st = json.loads(urllib.request.urlopen(urllib.request.Request(status_url, headers=hdr), timeout=60).read())
        s = st.get("status")
        print(f"  {int(time.time()-t0)}s {s}", flush=True)
        if s == "COMPLETED": break
        if s in ("FAILED", "ERROR"): raise RuntimeError(f"failed: {json.dumps(st)[:500]}")
        time.sleep(10)
    res = json.loads(urllib.request.urlopen(urllib.request.Request(resp_url, headers=hdr), timeout=120).read())
    print("RESPONSE KEYS:", list(res.keys()), flush=True)
    glb = find_glb(res)
    if isinstance(glb, dict): glb = glb.get("url")          # some endpoints wrap {url,...}
    if not glb:
        raise RuntimeError(f"no glb in response: {json.dumps(res)[:600]}")
    print("GLB URL:", glb, flush=True)
    elapsed = int(time.time() - t0)
    dest = OUT / f"{a.key}.glb"
    dest.write_bytes(urllib.request.urlopen(glb, timeout=600).read())
    sidecar = OUT / f"{a.key}.entry.json"
    sidecar.write_text(json.dumps({
        "engine": a.key, "file": f"{a.key}.png", "kind": "image-to-3d",
        "model": a.label, "elapsed_s": elapsed,
    }))
    print(f"WROTE {dest} ({dest.stat().st_size//1024} KB) in {elapsed}s", flush=True)
    print(f"WROTE {sidecar}", flush=True)

if __name__ == "__main__":
    main()
