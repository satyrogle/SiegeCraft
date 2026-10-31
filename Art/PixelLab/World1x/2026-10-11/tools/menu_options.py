"""The three premise menu looks under the real title overlay (strip, logo, list), shown 2x.
Reuses the overlay code in sprint_art/compose_options.py. Usage: menu_options.py <composer> <out> <image>..."""
import sys
from PIL import Image

source = open(sys.argv[1]).read()
exec(source[:source.index("towns = [")])
images = [Image.open(p).convert("RGBA") for p in sys.argv[3:]]
sheet([title_overlay(im) for im in images], 2, sys.argv[2])
