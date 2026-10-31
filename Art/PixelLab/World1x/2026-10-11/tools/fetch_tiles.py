"""Download PixelLab tileset sheets, their metadata and map objects into a folder of their own.
Usage: fetch_tiles.py <out folder> <tileset id>... -- <object id>..."""
import json
import os
import sys
import urllib.request

out = sys.argv[1]
args = sys.argv[2:]
split = args.index("--") if "--" in args else len(args)
tilesets, objects = args[:split], args[split + 1:]
os.makedirs(out, exist_ok=True)


def get(url, path):
    with urllib.request.urlopen(url, timeout=60) as r, open(path, "wb") as f:
        f.write(r.read())
    print("saved", path, os.path.getsize(path))


for tid in tilesets:
    get("https://api.pixellab.ai/mcp/tilesets/%s/image?inline=true" % tid, os.path.join(out, tid + ".png"))
    get("https://api.pixellab.ai/mcp/tilesets/%s/metadata" % tid, os.path.join(out, tid + ".json"))
for oid in objects:
    get("https://api.pixellab.ai/mcp/map-objects/%s/download" % oid, os.path.join(out, "obj_" + oid + ".png"))
for name in os.listdir(out):
    if name.endswith(".json"):
        meta = json.load(open(os.path.join(out, name)))
        tiles = meta.get("tiles") or meta.get("tileset_data", {}).get("tiles") or []
        print(name, "keys", list(meta.keys())[:8], "tiles", len(tiles))
        if tiles:
            print("  first tile", json.dumps(tiles[0])[:400])
